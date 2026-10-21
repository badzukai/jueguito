# Reacomoda Cafeteria.unity (versión 2, estilo cafetería de Club Penguin):
# pared con decoración, puerta de entrada, máquinas más grandes y juntas, repisas en las esquinas.
import re, os, json, sys

DIR = os.path.dirname(os.path.abspath(__file__))
RUTA = r'C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2\Assets\Scenes\Cafeteria.unity'
SCRIPTS = r'C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2\Assets\Scripts'
guids = json.load(open(os.path.join(DIR, 'guids.json')))

texto = open(RUTA, encoding='utf-8', newline='').read()
assert '\r\n' not in texto
partes = re.split(r'\n(?=--- !u!)', texto)
cab, docs = partes[0], partes[1:]


def info(d):
    m = re.match(r'--- !u!(\d+) &(-?\d+)', d)
    return m.group(1), m.group(2)


def reindexar():
    global idx, nombres
    idx = {info(d)[1]: i for i, d in enumerate(docs)}
    nombres = {}
    for d in docs:
        c, f = info(d)
        if c == '1':
            nombres.setdefault(re.search(r'm_Name: (.*)', d).group(1), []).append(f)


reindexar()


def go_id(n):
    l = nombres[n]
    assert len(l) == 1, (n, l)
    return l[0]


def comp(n, cls, script=None):
    g = docs[idx[go_id(n)]]
    res = [c for c in re.findall(r'component: \{fileID: (\d+)\}', g)
           if info(docs[idx[c]])[0] == cls and (script is None or ('guid: ' + script) in docs[idx[c]])]
    assert len(res) == 1, (n, cls, res)
    return res[0]


def editar(fid, pat, rep):
    i = idx[fid]
    clave = pat.split(':')[0]
    nuevo, k = re.subn(pat, lambda m: clave + ': ' + rep, docs[i], count=1)
    assert k == 1, (fid, pat)
    docs[i] = nuevo


def v3(x, y, z=0):
    return '{x: %s, y: %s, z: %s}' % (x, y, z)


def mover(n, x=None, y=None):
    t = comp(n, '4')
    px_, py_, pz_ = re.search(r'm_LocalPosition: \{x: (.*?), y: (.*?), z: (.*?)\}', docs[idx[t]]).groups()
    editar(t, r'm_LocalPosition: \{.*?\}', v3(px_ if x is None else x, py_ if y is None else y, pz_))


def spr(clave):
    return '{fileID: 21300000, guid: %s, type: 3}' % guids[clave]


def sprite(n, clave, ancho, alto, orden=None, tiled=False):
    s = comp(n, '212')
    editar(s, r'm_Sprite: \{.*?\}', spr(clave))
    editar(s, r'm_DrawMode: \d+', '2' if tiled else '0')
    editar(s, r'm_Size: \{.*?\}', '{x: %s, y: %s}' % (ancho, alto))
    if orden is not None:
        editar(s, r'm_SortingOrder: -?\d+', str(orden))


def caja(n, ancho, alto, ox=0, oy=0):
    c = comp(n, '61')
    editar(c, r'm_Size: \{.*?\}', '{x: %s, y: %s}' % (ancho, alto))
    editar(c, r'm_Offset: \{.*?\}', '{x: %s, y: %s}' % (ox, oy))


def gs(n):
    # Los scripts se renombraron al inglés (20/10): traducimos el nombre viejo.
    n = {'Bandeja': 'Tray', 'Cafetera': 'CoffeeMachine', 'CatalogoObjetos': 'ItemCatalog', 'Chocolatera': 'ChocolateMachine', 'Cliente': 'Customer', 'EstacionGalletitas': 'CookieStation', 'EstacionIngredientes': 'IngredientStation', 'EstacionInteractiva': 'InteractiveStation', 'GloboObjeto': 'ItemBubble', 'GloboPedido': 'OrderBubble', 'GuiaTutorial': 'TutorialGuide', 'MenuPausa': 'PauseMenu', 'MenuPrincipal': 'MainMenu', 'MoledoraCafe': 'CoffeeGrinder', 'MovimientoCliente': 'CustomerMovement', 'Repisa': 'Shelf', 'Sonidos': 'SoundEffects', 'Tacho': 'TrashCan'}.get(n, n)
    return open(os.path.join(SCRIPTS, n + '.cs.meta')).read().split('guid: ')[1].split()[0]


def campo(n, script, pat, rep):
    editar(comp(n, '114', gs(script)), pat, rep)


PPU = 32.0

# ------------------------------------------------------------ pisos
mover('PisoBase', 0, -1.1)
sprite('PisoBase', 'Escenario/PisoCocina', 19.2, 7, orden=-10, tiled=True)
mover('PisoSalon', 0, 3.15)
sprite('PisoSalon', 'Escenario/PisoSalon', 19.2, 0.7, orden=-9, tiled=True)
mover('Alfombra', 0, -0.6)
sprite('Alfombra', 'Escenario/Alfombra', 160 / PPU, 96 / PPU, orden=-8)

# ------------------------------------------------- mostrador y bandejas
XS = (-5.6, 0, 5.6)
for k, x in enumerate(XS, start=1):
    mover('Bandeja%d' % k, x)
    sprite('Bandeja%d' % k, 'Escenario/Bandeja', 44 / PPU, 18 / PPU)
    mover('Cliente%d' % k, x)
    mover('PuestoCliente%d' % k, x)
    sprite('Cliente%d' % k, 'Personajes/Cliente%d' % k, 24 / PPU, 34 / PPU)
    campo('Cliente%d' % k, 'Cliente', r'bubblePosition: \{.*?\}', '{x: 0, y: 0.95, z: 0}')
# Los clientes entran por la puerta.
mover('EntradaClientes', -2.8, 3.4)

# --------------------------------------------------------- barista
sprite('Player', 'Personajes/Barista', 24 / PPU, 34 / PPU)
caja('Player', 0.6, 0.4, 0, -0.33)
campo('Player', 'PlayerInventory', r'bubblePosition: \{.*?\}', '{x: 0, y: 1, z: 0}')
campo('Player', 'PlayerInteraction', r'interactionDistance: .*', '1.9')

# --------------------------------------------------------- máquinas
for n, x, script in (('Moledora', -4.4, 'MoledoraCafe'), ('Cafetera', 0, 'Cafetera'), ('Chocolatera', 4.4, 'Chocolatera')):
    mover(n, x, -3.5)
    sprite(n, 'Escenario/' + n, 84 / PPU, 56 / PPU)
    caja(n, 84 / PPU, 56 / PPU)
    campo(n, script, r'bubblePosition: \{.*?\}', '{x: 0, y: 1.2, z: 0}')

# -------------------------------------------- estaciones de los costados
for n, x, clave in (('EstacionIngredientes', -7.95, 'Ingredientes'), ('EstacionGalletitas', 7.95, 'Galletitas')):
    mover(n, x, 0.45)
    sprite(n, 'Escenario/' + clave, 72 / PPU, 76 / PPU)
    caja(n, 72 / PPU, 76 / PPU)
for n, x in (('RepisaIzquierda', -7.95), ('RepisaDerecha', 7.95)):
    mover(n, x, -3.55)
    sprite(n, 'Escenario/Repisa', 68 / PPU, 30 / PPU)
    caja(n, 68 / PPU, 30 / PPU)
    r = comp(n, '114', gs('Repisa'))
    d = docs[idx[r]]
    d2, k = re.subn(r'slotPositions:\n  - \{.*?\}\n  - \{.*?\}',
                    'slotPositions:\n  - {x: -0.5, y: 0.3, z: 0}\n  - {x: 0.5, y: 0.3, z: 0}', d)
    assert k == 1, d[-400:]
    docs[idx[r]] = d2

# ------------------------------------------- Tacho (instancia de prefab)
for i, d in enumerate(docs):
    if d.startswith('--- !u!1001 ') and '756fae0b759a4c7299c06a77e5c571f6' in d:
        d = re.sub(r'(propertyPath: m_LocalPosition\.x\n\s+value: ).*', r'\g<1>7.95', d, count=1)
        d = re.sub(r'(propertyPath: m_LocalPosition\.y\n\s+value: ).*', r'\g<1>-1.75', d, count=1)
        docs[i] = d

# ------------------------------------- barras de paciencia (Canvas)
for k, x in enumerate(XS, start=1):
    editar(comp('TextoPedido%d' % k, '224'), r'm_AnchoredPosition: \{.*?\}', '{x: %.1f, y: -96.4}' % (x * 1080 / 10.5))

# ------------------------------------------------ decoración nueva
# Borramos la decoración de la versión anterior (plantas) y armamos la nueva.
piso_t = comp('Piso', '4')
viejos = []
for n in ('PlantaSalonIzquierda', 'PlantaSalonDerecha', 'PlantaCocinaIzquierda', 'PlantaCocinaDerecha'):
    if n in nombres:
        g = go_id(n)
        viejos += [g] + re.findall(r'component: \{fileID: (\d+)\}', docs[idx[g]])
viejos_t = [v for v in viejos if info(docs[idx[v]])[0] == '4']
docs = [d for d in docs if info(d)[1] not in viejos]
reindexar()
i = idx[piso_t]
for t in viejos_t:
    docs[i] = docs[i].replace('\n  - {fileID: %s}' % t, '')

plantilla_sr = docs[idx[comp('Alfombra', '212')]]
siguiente = [1700000100]
nuevos, hijos = [], []


def nuevo_id():
    siguiente[0] += 1
    return siguiente[0]


def objeto(nombre, x, y, clave, w, h, orden, tiled=False, ancho=None, alto=None):
    if nombre in nombres:
        return
    g, t, s = nuevo_id(), nuevo_id(), nuevo_id()
    nuevos.append('--- !u!1 &%d\nGameObject:\n  m_ObjectHideFlags: 0\n'
                  '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                  '  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n'
                  '  - component: {fileID: %d}\n  - component: {fileID: %d}\n'
                  '  m_Layer: 0\n  m_Name: %s\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n'
                  '  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1' % (g, t, s, nombre))
    nuevos.append('--- !u!4 &%d\nTransform:\n  m_ObjectHideFlags: 0\n'
                  '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                  '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  serializedVersion: 2\n'
                  '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: %s\n'
                  '  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children: []\n'
                  '  m_Father: {fileID: %s}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}' % (t, g, v3(x, y), piso_t))
    sr = re.sub(r'^--- !u!212 &\d+', '--- !u!212 &%d' % s, plantilla_sr)
    sr = re.sub(r'm_GameObject: \{fileID: \d+\}', 'm_GameObject: {fileID: %d}' % g, sr, count=1)
    sr = re.sub(r'm_Sprite: \{.*?\}', lambda m: 'm_Sprite: ' + spr(clave), sr, count=1)
    sr = re.sub(r'm_DrawMode: \d+', 'm_DrawMode: %d' % (2 if tiled else 0), sr, count=1)
    sr = re.sub(r'm_Size: \{.*?\}', 'm_Size: {x: %s, y: %s}' % (ancho or w / PPU, alto or h / PPU), sr, count=1)
    sr = re.sub(r'm_SortingOrder: -?\d+', 'm_SortingOrder: %d' % orden, sr, count=1)
    nuevos.append(sr.rstrip('\n'))
    hijos.append(t)


# Pared del fondo (de y = 3.45 para arriba) y lo que cuelga de ella.
objeto('Pared', 0, 3.45 + 88 / PPU / 2, 'Escenario/Pared', 32, 88, -9, tiled=True, ancho=19.2, alto=88 / PPU)
objeto('Puerta', -2.8, 3.45 + 56 / PPU / 2, 'Escenario/Puerta', 40, 56, -8)
objeto('Ventana', 2.8, 4.4, 'Escenario/Ventana', 112, 44, -8)
objeto('Pizarra', 8.0, 4.3, 'Escenario/Pizarra', 52, 44, -8)
objeto('CuadroNaranja', -8.55, 4.4, 'Escenario/CuadroNaranja', 20, 26, -8)
objeto('CuadroVerde', -7.65, 4.3, 'Escenario/CuadroVerde', 20, 26, -8)
objeto('LamparaIzquierda', -4.2, 4.6, 'Escenario/Lampara', 14, 40, -7)
objeto('LamparaDerecha', 4.4, 4.6, 'Escenario/Lampara', 14, 40, -7)
# Muebles del salón.
objeto('Sofa', -8.0, 3.35, 'Escenario/Sofa', 56, 34, 0)
objeto('PlantaSalon', 8.7, 3.6, 'Escenario/Planta', 28, 48, 1)
objeto('CajaRegistradora', 8.35, 2.78, 'Escenario/CajaRegistradora', 30, 24, 2)
# Cocina.
objeto('PlantaCocina', -8.6, -1.85, 'Escenario/Planta', 28, 48, 0)

i = idx[piso_t]
m = re.search(r'  m_Children:(.*?)\n  m_Father', docs[i], re.S)
actuales = '' if m.group(1).strip() == '[]' else m.group(1)
docs[i] = docs[i][:m.start()] + '  m_Children:' + actuales + ''.join('\n  - {fileID: %d}' % t for t in hijos) + '\n  m_Father' + docs[i][m.end():]

ultimo = docs[-1]
assert ultimo.startswith('--- !u!1660057539')
docs = docs[:-1] + nuevos + [ultimo]
salida = cab + '\n' + '\n'.join(docs)
if not salida.endswith('\n'):
    salida += '\n'
destino = sys.argv[1] if len(sys.argv) > 1 else RUTA
open(destino, 'w', encoding='utf-8', newline='\n').write(salida)

# chequeo
t = salida
defs = re.findall(r'^--- !u!\d+ &(-?\d+)', t, re.M)
refs = set(re.findall(r'\{fileID: (\d{3,})\}', t))
print('ok', len(nuevos), 'docs nuevos | dup', len(defs) - len(set(defs)), '| refs sin def', sorted(refs - set(defs))[:5])
