# Mini librería de pixel art: lienzo RGBA, lectura/escritura PNG sin dependencias.
import zlib, struct


def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


class Img:
    def __init__(self, w, h, fill=(0, 0, 0, 0)):
        self.w, self.h = w, h
        self.p = [list(fill) for _ in range(w * h)]

    def get(self, x, y):
        if 0 <= x < self.w and 0 <= y < self.h:
            return tuple(self.p[y * self.w + x])
        return (0, 0, 0, 0)

    def set(self, x, y, c):
        if c is None:
            return
        if isinstance(c, str):
            c = hexc(c)
        if 0 <= x < self.w and 0 <= y < self.h:
            if len(c) == 4 and c[3] < 255 and c[3] > 0:
                o = self.p[y * self.w + x]
                a = c[3] / 255.0
                oa = o[3] / 255.0
                na = a + oa * (1 - a)
                if na == 0:
                    self.p[y * self.w + x] = [0, 0, 0, 0]
                    return
                r = [int((c[i] * a + o[i] * oa * (1 - a)) / na) for i in range(3)]
                self.p[y * self.w + x] = r + [int(na * 255)]
            else:
                self.p[y * self.w + x] = list(c) if len(c) == 4 else list(c) + [255]

    def rect(self, x, y, w, h, c):
        for j in range(y, y + h):
            for i in range(x, x + w):
                self.set(i, j, c)

    def hline(self, x, y, w, c):
        self.rect(x, y, w, 1, c)

    def vline(self, x, y, h, c):
        self.rect(x, y, 1, h, c)

    def frame(self, x, y, w, h, c):
        self.hline(x, y, w, c)
        self.hline(x, y + h - 1, w, c)
        self.vline(x, y, h, c)
        self.vline(x + w - 1, y, h, c)

    def rrect(self, x, y, w, h, c, r=1):
        # rectángulo con esquinas recortadas
        for j in range(y, y + h):
            for i in range(x, x + w):
                dx = min(i - x, x + w - 1 - i)
                dy = min(j - y, y + h - 1 - j)
                if r >= 1 and dx == 0 and dy == 0:
                    continue
                if r >= 2 and ((dx == 0 and dy <= 1) or (dy == 0 and dx <= 1)):
                    continue
                self.set(i, j, c)

    def ellipse(self, cx, cy, rx, ry, c):
        for j in range(int(cy - ry) - 1, int(cy + ry) + 2):
            for i in range(int(cx - rx) - 1, int(cx + rx) + 2):
                if ((i + 0.5 - cx) / rx) ** 2 + ((j + 0.5 - cy) / ry) ** 2 <= 1.0:
                    self.set(i, j, c)

    def pattern(self, x, y, rows, pal):
        # rows: lista de strings; cada char es una clave de pal ('.' = nada)
        for j, row in enumerate(rows):
            for i, ch in enumerate(row):
                if ch != '.' and ch != ' ':
                    self.set(x + i, y + j, pal[ch])

    def outline(self, c, diag=False):
        # dibuja un contorno alrededor de los píxeles opacos
        src = [row[:] for row in self.p]
        def op(i, j):
            return 0 <= i < self.w and 0 <= j < self.h and src[j * self.w + i][3] > 0
        for j in range(self.h):
            for i in range(self.w):
                if op(i, j):
                    continue
                n = [(1, 0), (-1, 0), (0, 1), (0, -1)]
                if diag:
                    n += [(1, 1), (-1, -1), (1, -1), (-1, 1)]
                if any(op(i + a, j + b) for a, b in n):
                    self.set(i, j, c)

    def paste(self, o, x, y):
        for j in range(o.h):
            for i in range(o.w):
                c = o.get(i, j)
                if c[3] > 0:
                    self.set(x + i, y + j, c)

    def flip_h(self):
        n = Img(self.w, self.h)
        for j in range(self.h):
            for i in range(self.w):
                n.p[j * self.w + (self.w - 1 - i)] = self.p[j * self.w + i][:]
        return n

    def scaled(self, k):
        n = Img(self.w * k, self.h * k)
        for j in range(n.h):
            for i in range(n.w):
                n.p[j * n.w + i] = self.p[(j // k) * self.w + (i // k)][:]
        return n

    def save(self, path):
        raw = b''
        for j in range(self.h):
            raw += b'\x00' + bytes(v for i in range(self.w) for v in self.p[j * self.w + i])
        def chunk(t, d):
            c = struct.pack('>I', len(d)) + t + d
            return c + struct.pack('>I', zlib.crc32(t + d) & 0xffffffff)
        png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', self.w, self.h, 8, 6, 0, 0, 0))
        png += chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b'')
        open(path, 'wb').write(png)


def load(path):
    d = open(path, 'rb').read()
    assert d[:8] == b'\x89PNG\r\n\x1a\n'
    pos = 8
    idat = b''
    plte = None
    trns = None
    while pos < len(d):
        ln = struct.unpack('>I', d[pos:pos + 4])[0]
        t = d[pos + 4:pos + 8]
        cd = d[pos + 8:pos + 8 + ln]
        if t == b'IHDR':
            w, h, bd, ct, _, _, il = struct.unpack('>IIBBBBB', cd)
        elif t == b'PLTE':
            plte = cd
        elif t == b'tRNS':
            trns = cd
        elif t == b'IDAT':
            idat += cd
        pos += 12 + ln
    assert bd == 8 and il == 0, (bd, il)
    bpp = {6: 4, 2: 3, 0: 1, 4: 2, 3: 1}[ct]
    raw = zlib.decompress(idat)
    stride = w * bpp
    out = Img(w, h)
    prev = bytearray(stride)
    i = 0
    for y in range(h):
        f = raw[i]; i += 1
        line = bytearray(raw[i:i + stride]); i += stride
        for x in range(stride):
            a = line[x - bpp] if x >= bpp else 0
            b = prev[x]
            c = prev[x - bpp] if x >= bpp else 0
            if f == 1: line[x] = (line[x] + a) & 255
            elif f == 2: line[x] = (line[x] + b) & 255
            elif f == 3: line[x] = (line[x] + (a + b) // 2) & 255
            elif f == 4:
                p = a + b - c; pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                pr = a if pa <= pb and pa <= pc else (b if pb <= pc else c)
                line[x] = (line[x] + pr) & 255
        for x in range(w):
            px = line[x * bpp:(x + 1) * bpp]
            if ct == 6: col = tuple(px)
            elif ct == 2: col = tuple(px) + (255,)
            elif ct == 0: col = (px[0],) * 3 + (255,)
            elif ct == 4: col = (px[0],) * 3 + (px[1],)
            else:
                k = px[0]; col = tuple(plte[k * 3:k * 3 + 3]) + ((trns[k] if trns and k < len(trns) else 255),)
            out.p[y * w + x] = list(col)
        prev = line
    return out


def sheet(imgs, k=4, bg=(70, 62, 80, 255), pad=6):
    W = sum(i.w * k + pad for i in imgs) + pad
    H = max(i.h * k for i in imgs) + pad * 2
    s = Img(W, H, bg)
    x = pad
    for i in imgs:
        s.paste(i.scaled(k), x, pad)
        x += i.w * k + pad
    return s
