# Exporta los sprites a Assets/Sprites/<carpeta>/<Nombre>.png con su .meta (el .meta primero).
import os, sys, uuid, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import arte

ASSETS = r'C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2\Assets'
PLANTILLA = open(os.path.join(ASSETS, 'Sprites', 'Objetos', 'Cafe.png.meta'), encoding='utf-8').read()
GUIDS = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'guids.json')

guids = json.load(open(GUIDS)) if os.path.exists(GUIDS) else {}


def guid_de(clave):
    if clave not in guids:
        guids[clave] = uuid.uuid4().hex
    return guids[clave]


def escribir(ruta, texto):
    with open(ruta, 'w', encoding='utf-8', newline='\n') as f:
        f.write(texto)


for (carpeta, nombre), (fn, ppu, borde) in arte.SPRITES.items():
    d = os.path.join(ASSETS, 'Sprites', carpeta)
    if not os.path.isdir(d):
        meta_carpeta = d + '.meta'
        escribir(meta_carpeta, 'fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n'
                 '  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
                 % guid_de('carpeta/' + carpeta))
        os.makedirs(d)
    png = os.path.join(d, nombre + '.png')
    meta = png + '.meta'
    g = guid_de(carpeta + '/' + nombre)
    if os.path.exists(meta):
        # Mantenemos el guid que ya tenía.
        for linea in open(meta, encoding='utf-8'):
            if linea.startswith('guid: '):
                g = linea.split()[1]
        guids[carpeta + '/' + nombre] = g
    m = PLANTILLA.replace('guid: 58960ac583af41fe896c2da23035ae0e', 'guid: ' + g)
    m = m.replace('spritePixelsToUnits: 64', 'spritePixelsToUnits: %d' % ppu)
    if borde:
        m = m.replace('spriteBorder: {x: 0, y: 0, z: 0, w: 0}',
                      'spriteBorder: {x: %d, y: %d, z: %d, w: %d}' % borde)
    # Para los pisos usamos "wrap" repetido (no hace falta para Tiled, pero no molesta).
    m = m.replace('spriteID: 5e97eb03825dee720800000000000000', 'spriteID: ' + uuid.uuid4().hex[:16] + '0800000000000000')
    escribir(meta, m)
    fn().save(png)
    print(carpeta, nombre, g)

json.dump(guids, open(GUIDS, 'w'), indent=1)
