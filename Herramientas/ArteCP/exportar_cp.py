# Exporta el arte estilo Club Penguin a Assets/Sprites (PNG + .meta, manteniendo los guid).
#   python exportar_cp.py            -> exporta todo
#   python exportar_cp.py hoja       -> solo arma una hoja de vista previa (hoja.png)
#   python exportar_cp.py Fondo ...  -> exporta solo esos sprites
import os, sys, json, uuid, re
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import svgkit
import arte_cp as A

DIR = os.path.dirname(os.path.abspath(__file__))
RAIZ = os.path.dirname(os.path.dirname(DIR))
ASSETS = os.path.join(RAIZ, 'Assets')
TMP = os.path.join(DIR, '_tmp')
GUIDS = os.path.join(RAIZ, 'Herramientas', 'ArteProvisorio', 'guids.json')
PLANTILLA = open(os.path.join(ASSETS, 'Sprites', 'Objetos', 'Cafe.png.meta'), encoding='utf-8').read()

# carpeta/nombre: (función, ppu, borde 9-slice (izq, abajo, der, arriba) o None, pivote (x, y))
C = (0.5, 0.5)
SPRITES = {
    'Escenario/Fondo': (A.fondo, 128, None, C),
    'Escenario/Mostrador': (A.mostrador, 128, None, C),
    'Escenario/Bandeja': (A.bandeja, 128, None, C),
    'Escenario/Moledora': (A.moledora, 128, None, (0.5, 1)),
    'Escenario/Cafetera': (A.cafetera, 128, None, (0.5, 1)),
    'Escenario/Chocolatera': (A.chocolatera, 128, None, (0.5, 1)),
    'Escenario/Repisa': (A.repisa, 128, None, (0.5, 1)),
    'Escenario/Ingredientes': (A.ingredientes, 128, None, (1, 0.5)),
    'Escenario/Galletitas': (A.galletitas, 128, None, (0, 0.5)),
    'Personajes/Barista': (A.barista, 128, None, C),
    'Personajes/Cliente1': (A.cliente1, 128, None, C),
    'Personajes/Cliente2': (A.cliente2, 128, None, C),
    'Personajes/Cliente3': (A.cliente3, 128, None, C),
    'Objetos/Tacho': (A.tacho, 128, None, C),
    'Objetos/Cafe': (lambda: A.icono(A.ic_cafe), 256, None, C),
    'Objetos/Leche': (lambda: A.icono(A.ic_leche), 256, None, C),
    'Objetos/Chocolate': (lambda: A.icono(A.ic_chocolate), 256, None, C),
    'Objetos/CafeMolido': (lambda: A.icono(A.ic_cafe_molido), 256, None, C),
    'Objetos/CafePreparado': (lambda: A.icono(A.ic_cafe_preparado), 256, None, C),
    'Objetos/LecheEspumada': (lambda: A.icono(A.ic_leche_espumada), 256, None, C),
    'Objetos/ChocolatePreparado': (lambda: A.icono(A.ic_chocolate_preparado), 256, None, C),
    'Objetos/CafeConLeche': (lambda: A.icono(A.ic_cafe_con_leche), 256, None, C),
    'Objetos/ChocolateConLeche': (lambda: A.icono(A.ic_chocolate_con_leche), 256, None, C),
    'Objetos/GalletitaRedVelvet': (lambda: A.icono(A.galleta('#c4313a', '#ffffff', '#8a1d26')), 256, None, C),
    'Objetos/GalletitaVainillaChocolate': (lambda: A.icono(A.galleta('#efcf98', '#5a3018', '#c99a5a')), 256, None, C),
    'Objetos/GalletitaChocolate': (lambda: A.icono(A.galleta('#7a4a2a', '#2e170b', '#4f2a15')), 256, None, C),
    'Objetos/Globo': (lambda: A.globo(176), 256, None, C),
    'Objetos/GloboDoble': (lambda: A.globo(304), 256, None, C),
    'HUD/Flecha': (A.flecha, 128, None, C),
    'HUD/FlechaDerecha': (A.flecha_derecha, 100, None, C),
    'HUD/Mas': (A.mas, 100, None, C),
    'HUD/BarraMadera': (A.ui_madera, 100, (28, 28, 28, 28), C),
    'HUD/CartelPapel': (A.ui_papel, 100, (30, 30, 30, 30), C),
    'HUD/Boton': (A.ui_boton, 100, (30, 30, 30, 30), C),
    'HUD/Tecla': (A.ui_tecla, 100, (18, 18, 18, 18), C),
    'HUD/Corazon': (lambda: A.ui_corazon(True), 100, None, C),
    'HUD/CorazonVacio': (lambda: A.ui_corazon(False), 100, None, C),
    'HUD/ProgresoFondo': (A.ui_progreso_fondo, 100, (11, 11, 11, 11), C),
    'HUD/ProgresoRelleno': (A.ui_progreso_relleno, 100, None, C),
}


def guid_existente(ruta_meta):
    if os.path.exists(ruta_meta):
        m = re.search(r'^guid: (\w+)', open(ruta_meta, encoding='utf-8').read(), re.M)
        if m:
            return m.group(1)
    return None


def meta_texto(guid, ppu, borde, pivote, grande=False):
    # Reemplazos con expresiones regulares: no dependen de los valores que traiga la plantilla.
    def r(pat, val, m):
        nuevo, k = re.subn(pat, val, m)
        assert k >= 1, pat
        return nuevo
    m = r(r'(?m)^guid: \w+', 'guid: ' + guid, PLANTILLA)
    m = r(r'spritePixelsToUnits: \d+', 'spritePixelsToUnits: %d' % ppu, m)
    m = r(r'(?m)^    filterMode: \d+', '    filterMode: 1', m)      # bilineal: arte suave, no pixel art
    m = r(r'spriteBorder: \{.*?\}', 'spriteBorder: {x: %d, y: %d, z: %d, w: %d}' % (borde or (0, 0, 0, 0)), m)
    m = r(r'(?m)^  alignment: \d+', '  alignment: %d' % (0 if pivote == C else 9), m)
    m = r(r'spritePivot: \{.*?\}', 'spritePivot: {x: %s, y: %s}' % pivote, m)
    m = r(r'maxTextureSize: \d+', 'maxTextureSize: %d' % (4096 if grande else 2048), m)
    m = r(r'spriteID: \w+', 'spriteID: ' + uuid.uuid4().hex[:16] + '0800000000000000', m)
    return m


def hoja():
    partes, x, y, fila_h = [], 10, 10, 0
    ANCHO = 2400
    for clave, (fn, ppu, borde, piv) in SPRITES.items():
        if clave == 'Escenario/Fondo':
            continue
        d = fn()
        esc = 0.5 if d.w > 1000 else 1.0
        w, h = d.w * esc, d.h * esc
        if x + w > ANCHO:
            x, y, fila_h = 10, y + fila_h + 30, 0
        partes.append('<div style="position:absolute;left:%dpx;top:%dpx;width:%dpx;height:%dpx">%s</div>'
                      % (x, y, w, h, d.svg().replace('<svg ', '<svg style="width:%dpx;height:%dpx" ' % (w, h), 1)))
        partes.append('<div style="position:absolute;left:%dpx;top:%dpx;font:12px Arial;color:#ddd">%s</div>' % (x, y + h + 2, clave))
        x += w + 20
        fila_h = max(fila_h, h)
    alto = int(y + fila_h + 40)
    html = svgkit.pagina('<div style="position:absolute;inset:0;background:#4a4458"></div>' + ''.join(partes), ANCHO, alto)
    svgkit.render([(html, ANCHO, alto, os.path.join(DIR, 'hoja.png'))], TMP, hilos=1)
    print('hoja.png', ANCHO, alto)


def exportar(claves):
    guids = json.load(open(GUIDS))
    trabajos, metas = [], []
    for clave in claves:
        fn, ppu, borde, piv = SPRITES[clave]
        carpeta, nombre = clave.split('/')
        d = fn()
        dir_ = os.path.join(ASSETS, 'Sprites', carpeta)
        png = os.path.join(dir_, nombre + '.png')
        g = guid_existente(png + '.meta') or guids.get(clave) or uuid.uuid4().hex
        guids[clave] = g
        # el .meta va antes que el PNG
        with open(png + '.meta', 'w', encoding='utf-8', newline='\n') as f:
            f.write(meta_texto(g, ppu, borde, piv, grande=max(d.w, d.h) > 2048))
        trabajos.append((svgkit.pagina(d.svg(), d.w, d.h), d.w, d.h, png))
    json.dump(guids, open(GUIDS, 'w'), indent=1)
    svgkit.render(trabajos, TMP)
    for clave in claves:
        print('ok', clave)


if __name__ == '__main__':
    args = sys.argv[1:]
    if args == ['hoja']:
        hoja()
    else:
        exportar([k for k in SPRITES if not args or k in args or k.split('/')[1] in args])
