# Reacomoda Cafeteria.unity (versión 4: pixel art, layout 60/40 cocina/café, vista cenital a 3/4).
# Regla de la vista 3/4: el barista se acerca a las cosas desde adelante (abajo) o desde el costado.
#  - Café arriba (un solo fondo dibujado), clientes detrás de la barra (la barra les tapa las piernas).
#  - Barra con las bandejas Y las máquinas arriba, mirando al barista (pivote de las máquinas abajo).
#  - Cocina: despensas a los costados (más arriba), dos mesas de trabajo (repisas) y el tacho en el medio.
import re, os, json, sys

DIR = os.path.dirname(os.path.abspath(__file__))
RAIZ = os.path.dirname(os.path.dirname(DIR))
RUTA = os.path.join(RAIZ, 'Assets', 'Scenes', 'Cafeteria.unity')
SCRIPTS = os.path.join(RAIZ, 'Assets', 'Scripts')
guids = json.load(open(os.path.join(DIR, 'guids.json')))
PPU = 32.0

texto = open(RUTA, encoding='utf-8', newline='').read()
assert '\r\n' not in texto
partes = re.split(r'\n(?=--- !u!)', texto)
cab, docs = partes[0], partes[1:]


def info(d):
    m = re.match(r'--- !u!(\d+) &(-?\d+)', d)
    return m.group(1), m.group(2)


idx = {info(d)[1]: i for i, d in enumerate(docs)}
nombres = {}
for d in docs:
    c, f = info(d)
    if c == '1':
        nombres.setdefault(re.search(r'm_Name: (.*)', d).group(1), []).append(f)


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


def editar(fid, clave, valor):
    # Reemplaza la línea "clave: ..." (solo la primera) por "clave: valor".
    i = idx[fid]
    nuevo, k = re.subn(r'(\n  %s: ).*' % re.escape(clave), lambda m: m.group(1) + valor, docs[i], count=1)
    assert k == 1, (fid, clave)
    docs[i] = nuevo


def v3(x, y, z=0):
    return '{x: %s, y: %s, z: %s}' % (x, y, z)


def mover(n, x, y):
    t = comp(n, '4')
    z = re.search(r'm_LocalPosition: \{x: .*?, y: .*?, z: (.*?)\}', docs[idx[t]]).group(1)
    editar(t, 'm_LocalPosition', v3(x, y, z))
    editar(t, 'm_LocalScale', v3(1, 1, 1))


def spr(clave):
    return '{fileID: 21300000, guid: %s, type: 3}' % guids[clave]


def sprite(n, clave, w_px, h_px, orden):
    s = comp(n, '212')
    editar(s, 'm_Sprite', spr(clave))
    editar(s, 'm_Color', '{r: 1, g: 1, b: 1, a: 1}')
    editar(s, 'm_DrawMode', '0')
    editar(s, 'm_Size', '{x: %s, y: %s}' % (round(w_px / PPU, 4), round(h_px / PPU, 4)))
    editar(s, 'm_SortingOrder', str(orden))


def caja(n, ancho, alto, ox=0, oy=0):
    c = comp(n, '61')
    editar(c, 'm_Size', '{x: %s, y: %s}' % (ancho, alto))
    editar(c, 'm_Offset', '{x: %s, y: %s}' % (ox, oy))


def gs(n):
    return open(os.path.join(SCRIPTS, n + '.cs.meta')).read().split('guid: ')[1].split()[0]


def campo(n, script, clave, valor):
    editar(comp(n, '114', gs(script)), clave, valor)


# ------------------------------------------------------------ cámara (igual que v3)
editar(comp('Main Camera', '4'), 'm_LocalPosition', v3(2.58364, 0.65993, -10))
editar(comp('Main Camera', '20'), 'orthographic size', '5')

# ------------------------------------------------------------ fondo
mover('PisoBase', 0, 0)
sprite('PisoBase', 'Escenario/Fondo', 569, 320, -10)

# ------------------------------------------------------------ barra
# El sprite mide 51 px: 19 de aire arriba, después la tapa (la tapa empieza en y = 0.4).
mover('Barra', 0, 0.197)
sprite('Barra', 'Escenario/Mostrador', 569, 51, 5)
caja('Barra', 17.78, 1.0, 0, -0.297)

XS = (-5, 0, 5)
for k, x in enumerate(XS, start=1):
    mover('Bandeja%d' % k, x, 0.2)
    sprite('Bandeja%d' % k, 'Escenario/Bandeja', 44, 18, 6)
    mover('Cliente%d' % k, x, 0.75)
    mover('PuestoCliente%d' % k, x, 0.75)
    sprite('Cliente%d' % k, 'Personajes/Cliente%d' % k, 24, 34, 0)
    campo('Cliente%d' % k, 'Cliente', 'posicionGlobo', '{x: 0, y: 1, z: 0}')
mover('EntradaClientes', 0, 3.0)

# Máquinas arriba de la barra (pivote abajo, apoyadas sobre la tapa).
for n, x, script in (('Moledora', -2.5, 'MoledoraCafe'), ('Cafetera', 2.5, 'Cafetera'), ('Chocolatera', 7.6, 'Chocolatera')):
    mover(n, x, 0.08)
    sprite(n, 'Escenario/' + n, 70, 52, 6)
    caja(n, 2.19, 1.63, 0, 0.81)
    campo(n, script, 'posicionGlobo', '{x: 0, y: 2.05, z: 0}')

# ------------------------------------------------------------ barista
mover('Player', 0, -1.3)
sprite('Player', 'Personajes/Barista', 24, 34, 7)
caja('Player', 0.6, 0.4, 0, -0.33)
campo('Player', 'PlayerInventory', 'posicionGlobo', '{x: 0, y: 1, z: 0}')
campo('Player', 'PlayerInteraction', 'distanciaInteraccion', '1.5')

# ------------------------------------------------------------ cocina
for n, x, clave, script in (('EstacionIngredientes', -7.75, 'Ingredientes', 'EstacionIngredientes'),
                            ('EstacionGalletitas', 7.75, 'Galletitas', 'EstacionGalletitas')):
    mover(n, x, -2.5)
    sprite(n, 'Escenario/' + clave, 72, 76, 3)
    caja(n, 2.25, 2.375)
    campo(n, script, 'distanciaInteraccion', '1.8')

for n, x in (('RepisaIzquierda', -2.6), ('RepisaDerecha', 2.6)):
    mover(n, x, -3.4)
    sprite(n, 'Escenario/Repisa', 64, 40, 3)
    caja(n, 2.0, 1.06, 0, 0.094)
    r = comp(n, '114', gs('Repisa'))
    d2, k = re.subn(r'posicionesEspacios:\n  - \{.*?\}\n  - \{.*?\}',
                    'posicionesEspacios:\n  - {x: -0.5, y: 0.22, z: 0}\n  - {x: 0.5, y: 0.22, z: 0}', docs[idx[r]])
    assert k == 1
    docs[idx[r]] = d2

# Tacho (instancia de prefab) entre las dos mesas.
for i, d in enumerate(docs):
    if d.startswith('--- !u!1001 ') and '756fae0b759a4c7299c06a77e5c571f6' in d:
        d = re.sub(r'(propertyPath: m_LocalPosition\.x\n\s+value: ).*', r'\g<1>0', d, count=1)
        d = re.sub(r'(propertyPath: m_LocalPosition\.y\n\s+value: ).*', r'\g<1>-3.4', d, count=1)
        docs[i] = d

# ------------------------------------- barras de paciencia (Canvas 1920x1080, 108 px por unidad)
for k, x in enumerate(XS, start=1):
    editar(comp('TextoPedido%d' % k, '224'), 'm_AnchoredPosition', '{x: %.1f, y: -272}' % (x * 108))

assert docs[-1].startswith('--- !u!1660057539')
salida = cab + '\n' + '\n'.join(docs)
if not salida.endswith('\n'):
    salida += '\n'
destino = sys.argv[1] if len(sys.argv) > 1 else RUTA
open(destino, 'w', encoding='utf-8', newline='\n').write(salida)

defs = re.findall(r'^--- !u!\d+ &(-?\d+)', salida, re.M)
refs = set(re.findall(r'\{fileID: (\d{3,})\}', salida))
dobles = re.findall(r'^\s*([A-Za-z_ ]+): \1:', salida, re.M)
print('ok | dup', len(defs) - len(set(defs)), '| refs sin def', sorted(refs - set(defs))[:5], '| claves dobles', dobles[:3])
