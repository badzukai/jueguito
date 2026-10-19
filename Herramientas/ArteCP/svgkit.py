# Kit mínimo para dibujar en SVG (estilo caricatura: contorno grueso + degradés suaves)
# y pasarlo a PNG con Chrome en modo headless.
import os, subprocess, html
from concurrent.futures import ThreadPoolExecutor

CHROME = r'C:\Program Files\Google\Chrome\Application\chrome.exe'
OL = '#3a2117'      # color del contorno
OW = 5              # grosor del contorno (px)


_contador = [0]


class Dib:
    def __init__(self, w, h):
        _contador[0] += 1
        self.prefijo = 'd%d_' % _contador[0]
        self.w, self.h = int(round(w)), int(round(h))
        self.defs = []
        self.items = []
        self.n = 0

    # --- degradés -------------------------------------------------
    def grad(self, *colores, vertical=True, x1=None, y1=None, x2=None, y2=None):
        self.n += 1
        gid = '%sg%d' % (self.prefijo, self.n)
        if x1 is None:
            x1, y1, x2, y2 = (0, 0, 0, 1) if vertical else (0, 0, 1, 0)
        stops = ''
        for i, c in enumerate(colores):
            if isinstance(c, tuple):
                off, col = c
            else:
                off, col = i / max(1, len(colores) - 1), c
            stops += '<stop offset="%.3f" stop-color="%s"/>' % (off, col)
        self.defs.append('<linearGradient id="%s" x1="%s" y1="%s" x2="%s" y2="%s">%s</linearGradient>' % (gid, x1, y1, x2, y2, stops))
        return 'url(#%s)' % gid

    def radial(self, *colores, cx=0.5, cy=0.4, r=0.6):
        self.n += 1
        gid = '%sg%d' % (self.prefijo, self.n)
        stops = ''.join('<stop offset="%.3f" stop-color="%s"/>' % (i / max(1, len(colores) - 1), c) for i, c in enumerate(colores))
        self.defs.append('<radialGradient id="%s" cx="%s" cy="%s" r="%s">%s</radialGradient>' % (gid, cx, cy, r, stops))
        return 'url(#%s)' % gid

    # --- formas ---------------------------------------------------
    def _st(self, fill, stroke, sw, op, extra):
        s = 'fill="%s"' % (fill or 'none')
        if stroke:
            s += ' stroke="%s" stroke-width="%s" stroke-linejoin="round" stroke-linecap="round"' % (OL if stroke is True else stroke, sw if sw is not None else OW)
        if op is not None:
            s += ' opacity="%s"' % op
        return s + (' ' + extra if extra else '')

    def rect(self, x, y, w, h, fill, rx=0, stroke=True, sw=None, op=None, extra=''):
        self.items.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" rx="%.1f" %s/>' % (x, y, w, h, rx, self._st(fill, stroke, sw, op, extra)))

    def ell(self, cx, cy, rx, ry, fill, stroke=True, sw=None, op=None, extra=''):
        self.items.append('<ellipse cx="%.1f" cy="%.1f" rx="%.1f" ry="%.1f" %s/>' % (cx, cy, rx, ry, self._st(fill, stroke, sw, op, extra)))

    def circ(self, cx, cy, r, fill, **k):
        self.ell(cx, cy, r, r, fill, **k)

    def path(self, d, fill, stroke=True, sw=None, op=None, extra=''):
        self.items.append('<path d="%s" %s/>' % (d, self._st(fill, stroke, sw, op, extra)))

    def line(self, x1, y1, x2, y2, color=OL, sw=None, op=None):
        self.items.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" stroke-width="%s" stroke-linecap="round"%s/>'
                          % (x1, y1, x2, y2, color, sw if sw is not None else OW, ' opacity="%s"' % op if op is not None else ''))

    def text(self, x, y, t, size, fill, weight='bold', anchor='middle', family='Trebuchet MS, Verdana, sans-serif',
             stroke=None, sw=0, extra=''):
        st = ' stroke="%s" stroke-width="%s" paint-order="stroke"' % (stroke, sw) if stroke else ''
        self.items.append('<text x="%.1f" y="%.1f" font-size="%s" font-weight="%s" text-anchor="%s" font-family="%s" fill="%s"%s %s>%s</text>'
                          % (x, y, size, weight, anchor, family, fill, st, extra, html.escape(t)))

    def raw(self, s):
        self.items.append(s)

    def grupo(self, transform, fn):
        # Dibuja lo que agregue fn() dentro de un <g transform=...>
        antes = len(self.items)
        fn()
        dentro = self.items[antes:]
        del self.items[antes:]
        self.items.append('<g transform="%s">%s</g>' % (transform, ''.join(dentro)))

    def brillo(self, cx, cy, rx, ry, op=0.35, color='#ffffff', rot=0):
        extra = 'transform="rotate(%s %.1f %.1f)"' % (rot, cx, cy) if rot else ''
        self.ell(cx, cy, rx, ry, color, stroke=False, op=op, extra=extra)

    def svg(self):
        return ('<svg xmlns="http://www.w3.org/2000/svg" width="%d" height="%d" viewBox="0 0 %d %d"><defs>%s</defs>%s</svg>'
                % (self.w, self.h, self.w, self.h, ''.join(self.defs), ''.join(self.items)))


def pagina(svg_o_html, w, h):
    return ('<!doctype html><html><head><meta charset="utf-8"><style>html,body{margin:0;padding:0;background:transparent;overflow:hidden}'
            'img{position:absolute}</style></head><body style="width:%dpx;height:%dpx;position:relative">%s</body></html>' % (w, h, svg_o_html))


def render(trabajos, carpeta_tmp, hilos=6):
    # trabajos: lista de (html, w, h, ruta_png)
    os.makedirs(carpeta_tmp, exist_ok=True)

    def uno(i_t):
        i, (contenido, w, h, ruta) = i_t
        hp = os.path.join(carpeta_tmp, 'r%03d.html' % i)
        with open(hp, 'w', encoding='utf-8') as f:
            f.write(contenido)
        perfil = os.path.join(carpeta_tmp, 'perfil%d' % (i % hilos))
        subprocess.run([CHROME, '--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
                        '--default-background-color=00000000', '--user-data-dir=' + perfil, '--no-first-run',
                        '--window-size=%d,%d' % (w, h), '--screenshot=' + ruta, 'file:///' + hp.replace('\\', '/')],
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=120)
        return ruta, os.path.exists(ruta)

    with ThreadPoolExecutor(hilos) as ex:
        res = list(ex.map(uno, enumerate(trabajos)))
    malos = [r for r, ok in res if not ok]
    if malos:
        raise RuntimeError('No se pudieron dibujar: %s' % malos)
    return [r for r, _ in res]
