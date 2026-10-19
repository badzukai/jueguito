# Exporta el pixel art (layout 60/40) a Assets/Sprites, con su .meta (filtro Point, pivotes) y los mismos guid.
#   python exportar_pixel.py          -> exporta todo
#   python exportar_pixel.py vista    -> arma vista_60.png con la escena compuesta (sin Unity)
import os, sys, json, re, uuid, shutil
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import px
import arte as A
import arte60 as B

DIR = os.path.dirname(os.path.abspath(__file__))
RAIZ = os.path.dirname(os.path.dirname(DIR))
ASSETS = os.path.join(RAIZ, 'Assets')
GUIDS = os.path.join(DIR, 'guids.json')
ICONOS = os.path.join(DIR, 'iconos_originales')   # los íconos pixel del primer día (32x32 a 64 ppu)
PLANTILLA = open(os.path.join(ASSETS, 'Sprites', 'Objetos', 'Cafe.png.meta'), encoding='utf-8').read()

C = (0.5, 0.5)
ABAJO = (0.5, 0.0)
ARRIBA = (0.5, 1.0)
SPRITES = {
    # clave: (función, ppu, borde 9-slice (izq, abajo, der, arriba), pivote)
    'Escenario/Fondo': (B.fondo, 32, None, C),
    'Escenario/Mostrador': (B.mostrador, 32, None, C),
    'Escenario/Moledora': (B.moledora_fila, 32, None, ARRIBA),
    'Escenario/Cafetera': (B.cafetera_fila, 32, None, ARRIBA),
    'Escenario/Chocolatera': (B.chocolatera_fila, 32, None, ARRIBA),
    'Escenario/Repisa': (B.repisa_fila, 32, None, ARRIBA),
    'Escenario/Ingredientes': (A.ingredientes, 32, None, C),
    'Escenario/Galletitas': (A.galletitas, 32, None, C),
    'Escenario/Bandeja': (A.bandeja, 32, None, C),
    'Personajes/Barista': (A.barista, 32, None, C),
    'Personajes/Cliente1': (A.cliente1, 32, None, C),
    'Personajes/Cliente2': (A.cliente2, 32, None, C),
    'Personajes/Cliente3': (A.cliente3, 32, None, C),
    'HUD/Flecha': (A.flecha, 32, None, C),
    'HUD/FlechaDerecha': (A.flecha_derecha, 32, None, C),
    'HUD/Mas': (A.mas, 32, None, C),
    'HUD/BarraMadera': (A.barra_madera, 32, (6, 6, 6, 6), C),
    'HUD/CartelPapel': (A.cartel_papel, 32, (7, 7, 7, 7), C),
    'HUD/Boton': (A.boton, 32, (7, 7, 7, 7), C),
    'HUD/Tecla': (A.tecla, 32, (5, 5, 5, 5), C),
    'HUD/Corazon': (lambda: A.corazon(True), 32, None, C),
    'HUD/CorazonVacio': (lambda: A.corazon(False), 32, None, C),
    'HUD/ProgresoFondo': (A.progreso_fondo, 32, (3, 3, 3, 3), C),
    'HUD/ProgresoRelleno': (A.progreso_relleno, 32, None, C),
    'HUD/Moneda': (B.moneda, 32, None, C),
}
for n in ('Cafe', 'CafeConLeche', 'CafeMolido', 'CafePreparado', 'Chocolate', 'ChocolateConLeche', 'ChocolatePreparado',
          'GalletitaChocolate', 'GalletitaRedVelvet', 'GalletitaVainillaChocolate', 'Globo', 'GloboDoble', 'Leche', 'LecheEspumada'):
    SPRITES['Objetos/' + n] = ('archivo', 64, None, C)
SPRITES['Objetos/Tacho'] = ('archivo', 32, None, C)


def meta_texto(guid, ppu, borde, pivote):
    def r(pat, val, m):
        nuevo, k = re.subn(pat, val, m)
        assert k >= 1, pat
        return nuevo
    m = r(r'(?m)^guid: \w+', 'guid: ' + guid, PLANTILLA)
    m = r(r'spritePixelsToUnits: \d+', 'spritePixelsToUnits: %d' % ppu, m)
    m = r(r'(?m)^    filterMode: \d+', '    filterMode: 0', m)          # Point: pixel art nítido
    m = r(r'spriteBorder: \{.*?\}', 'spriteBorder: {x: %d, y: %d, z: %d, w: %d}' % (borde or (0, 0, 0, 0)), m)
    m = r(r'(?m)^  alignment: \d+', '  alignment: %d' % (0 if pivote == C else 9), m)
    m = r(r'spritePivot: \{.*?\}', 'spritePivot: {x: %s, y: %s}' % pivote, m)
    m = r(r'maxTextureSize: \d+', 'maxTextureSize: 2048', m)
    m = r(r'spriteID: \w+', 'spriteID: ' + uuid.uuid4().hex[:16] + '0800000000000000', m)
    return m


def imagen(clave):
    fn = SPRITES[clave][0]
    if fn == 'archivo':
        return px.load(os.path.join(ICONOS, clave.split('/')[1] + '.png'))
    return fn()


def exportar():
    guids = json.load(open(GUIDS))
    for clave, (fn, ppu, borde, piv) in SPRITES.items():
        carpeta, nombre = clave.split('/')
        png = os.path.join(ASSETS, 'Sprites', carpeta, nombre + '.png')
        g = None
        if os.path.exists(png + '.meta'):
            g = re.search(r'(?m)^guid: (\w+)', open(png + '.meta', encoding='utf-8').read()).group(1)
        g = g or guids.get(clave) or uuid.uuid4().hex
        guids[clave] = g
        with open(png + '.meta', 'w', encoding='utf-8', newline='\n') as f:   # el .meta va primero
            f.write(meta_texto(g, ppu, borde, piv))
        imagen(clave).save(png)
        print('ok', clave)
    json.dump(guids, open(GUIDS, 'w'), indent=1)


def vista():
    # Compone la escena como la va a ver la cámara (32 px por unidad) y la agranda x2.
    im = B.fondo()

    def poner(dib, x, y, piv=C):
        px_ = int(round(B.X(x) - dib.w * piv[0]))
        py_ = int(round(B.Y(y) - dib.h * (1 - piv[1])))
        im.paste(dib, px_, py_)

    for k, x in enumerate((-5, 0, 5)):
        poner([A.cliente1, A.cliente2, A.cliente3][k](), x, 0.75)
    poner(B.mostrador(), 0, 0.2)
    for x in (-5, 0, 5):
        poner(A.bandeja(), x, 0.2)
    for fn, x in ((B.moledora_fila, -4.75), (B.repisa_fila, -2.35), (B.cafetera_fila, 0),
                  (B.repisa_fila, 2.35), (B.chocolatera_fila, 4.75)):
        poner(fn(), x, -2.5, ARRIBA)
    poner(A.ingredientes(), -7.75, -1.9)
    poner(A.galletitas(), 7.75, -1.9)
    poner(px.load(os.path.join(ICONOS, 'Tacho.png')), 6.1, -1.3)
    poner(A.barista(), 0, -1.3)
    # barra del HUD (aprox.)
    for x in range(B.W):
        for y in range(32):
            im.set(x, y, '#a06c45' if 2 < y < 30 else A.O)
    im.scaled(2).save(os.path.join(DIR, 'vista_60.png'))
    print('vista_60.png')


if __name__ == '__main__':
    if sys.argv[1:] == ['vista']:
        vista()
    else:
        exportar()
