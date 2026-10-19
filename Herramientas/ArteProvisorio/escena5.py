# Reacomoda Cafeteria.unity (versión 5): parte de la versión 4 y vuelve a poner las máquinas
# en la fila de mesada de abajo (como antes), con las despensas más arriba. Pixel art, 60/40.
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


# ---------------------------------------------- máquinas y repisas: fila de abajo (pivote arriba)
for n, x, script in (('Moledora', -4.75, 'MoledoraCafe'), ('Cafetera', 0, 'Cafetera'), ('Chocolatera', 4.75, 'Chocolatera')):
    mover(n, x, -2.5)
    sprite(n, 'Escenario/' + n, 84, 80, 3)
    caja(n, 2.625, 2.5, 0, -1.25)
    campo(n, script, 'posicionGlobo', '{x: 0, y: -0.3, z: 0}')

for n, x in (('RepisaIzquierda', -2.35), ('RepisaDerecha', 2.35)):
    mover(n, x, -2.5)
    sprite(n, 'Escenario/Repisa', 58, 80, 3)
    caja(n, 1.8125, 2.5, 0, -1.25)
    r = comp(n, '114', gs('Repisa'))
    d2, k = re.subn(r'posicionesEspacios:\n  - \{.*?\}\n  - \{.*?\}',
                    'posicionesEspacios:\n  - {x: -0.41, y: -0.85, z: 0}\n  - {x: 0.41, y: -0.85, z: 0}', docs[idx[r]])
    assert k == 1
    docs[idx[r]] = d2

# ---------------------------------------------- despensas más arriba, al lado de la barra
mover('EstacionIngredientes', -7.75, -1.9)
mover('EstacionGalletitas', 7.75, -1.9)

# ---------------------------------------------- tacho al lado de las galletitas
for i, d in enumerate(docs):
    if d.startswith('--- !u!1001 ') and '756fae0b759a4c7299c06a77e5c571f6' in d:
        d = re.sub(r'(propertyPath: m_LocalPosition\.x\n\s+value: ).*', r'\g<1>6.1', d, count=1)
        d = re.sub(r'(propertyPath: m_LocalPosition\.y\n\s+value: ).*', r'\g<1>-1.3', d, count=1)
        docs[i] = d

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
