# Reacomoda Cafeteria.unity (versión 3, estilo Club Penguin y layout 50/50):
#  - un fondo grande dibujado (pared, salón con sillones y mesitas, piso de la cocina),
#  - mostrador grande en el medio, clientes detrás (el mostrador les tapa los pies),
#  - cocina: fila de mesada abajo (moledora, repisa, cafetera, repisa, chocolatera),
#    despensas altas a los costados (Ingredientes y Galletitas) y el tacho.
# Los sprites de la fila de abajo tienen el pivote ARRIBA, así la distancia para usarlos
# se mide desde el borde donde se para el barista.
import re, os, json, sys

DIR = os.path.dirname(os.path.abspath(__file__))
RAIZ = os.path.dirname(os.path.dirname(DIR))
RUTA = os.path.join(RAIZ, 'Assets', 'Scenes', 'Cafeteria.unity')
SCRIPTS = os.path.join(RAIZ, 'Assets', 'Scripts')
guids = json.load(open(os.path.join(RAIZ, 'Herramientas', 'ArteProvisorio', 'guids.json')))

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


def sprite(n, clave, ancho, alto, orden):
    s = comp(n, '212')
    editar(s, 'm_Sprite', spr(clave))
    editar(s, 'm_Color', '{r: 1, g: 1, b: 1, a: 1}')
    editar(s, 'm_DrawMode', '0')
    editar(s, 'm_Size', '{x: %s, y: %s}' % (ancho, alto))
    editar(s, 'm_SortingOrder', str(orden))


def caja(n, ancho, alto, ox=0, oy=0):
    c = comp(n, '61')
    editar(c, 'm_Size', '{x: %s, y: %s}' % (ancho, alto))
    editar(c, 'm_Offset', '{x: %s, y: %s}' % (ox, oy))


def gs(n):
    return open(os.path.join(SCRIPTS, n + '.cs.meta')).read().split('guid: ')[1].split()[0]


def campo(n, script, clave, valor):
    editar(comp(n, '114', gs(script)), clave, valor)


# ------------------------------------------------------------ cámara
editar(comp('Main Camera', '4'), 'm_LocalPosition', v3(2.58364, 0.65993, -10))
editar(comp('Main Camera', '20'), 'orthographic size', '5')

# ------------------------------------------------- borrar la decoración vieja
viejos = []
for n in ('PisoSalon', 'Alfombra', 'Pared', 'Puerta', 'Ventana', 'Pizarra', 'CuadroNaranja', 'CuadroVerde',
          'LamparaIzquierda', 'LamparaDerecha', 'Sofa', 'PlantaSalon', 'CajaRegistradora', 'PlantaCocina'):
    if n in nombres:
        g = go_id(n)
        viejos += [g] + re.findall(r'component: \{fileID: (\d+)\}', docs[idx[g]])
viejos_t = [v for v in viejos if info(docs[idx[v]])[0] == '4']
piso_t = comp('Piso', '4')
docs = [d for d in docs if info(d)[1] not in viejos]
reindexar()
for t in viejos_t:
    docs[idx[piso_t]] = docs[idx[piso_t]].replace('\n  - {fileID: %s}' % t, '')

# ------------------------------------------------------------ fondo
mover('PisoBase', 0, 0)
sprite('PisoBase', 'Escenario/Fondo', 17.78, 10, -10)

# ------------------------------------------------- mostrador y bandejas
# El sprite tiene 0.4 de aire arriba: la tapa empieza en y = 0.
mover('Barra', 0, -0.35)
sprite('Barra', 'Escenario/Mostrador', 17.78, 1.5, 5)
caja('Barra', 17.78, 1.1, 0, -0.2)
XS = (-5.6, 0, 5.6)
for k, x in enumerate(XS, start=1):
    mover('Bandeja%d' % k, x, -0.2)
    sprite('Bandeja%d' % k, 'Escenario/Bandeja', 1.5, 0.6, 6)
    # clientes detrás del mostrador
    mover('Cliente%d' % k, x, 0.5)
    mover('PuestoCliente%d' % k, x, 0.5)
    sprite('Cliente%d' % k, 'Personajes/Cliente%d' % k, 0.95, 1.3, 0)
    campo('Cliente%d' % k, 'Cliente', 'posicionGlobo', '{x: 0, y: 1.1, z: 0}')
mover('EntradaClientes', 0, 3.0)

# --------------------------------------------------------- barista
mover('Player', 0, -1.4)
sprite('Player', 'Personajes/Barista', 0.95, 1.3, 7)
caja('Player', 0.6, 0.35, 0, -0.45)
campo('Player', 'PlayerInventory', 'posicionGlobo', '{x: 0, y: 1.05, z: 0}')
campo('Player', 'PlayerInteraction', 'distanciaInteraccion', '1.6')

# --------------------------------------------- fila de mesada (pivote arriba)
for n, x, clave, script in (('Moledora', -4.7, 'Moledora', 'MoledoraCafe'),
                            ('Cafetera', 0, 'Cafetera', 'Cafetera'),
                            ('Chocolatera', 4.7, 'Chocolatera', 'Chocolatera')):
    mover(n, x, -2.6)
    sprite(n, 'Escenario/' + clave, 2.6, 2.4, 3)
    caja(n, 2.6, 2.4, 0, -1.2)
    campo(n, script, 'posicionGlobo', '{x: 0, y: -0.25, z: 0}')
for n, x in (('RepisaIzquierda', -2.35), ('RepisaDerecha', 2.35)):
    mover(n, x, -2.6)
    sprite(n, 'Escenario/Repisa', 1.8, 2.4, 3)
    caja(n, 1.8, 2.4, 0, -1.2)
    r = comp(n, '114', gs('Repisa'))
    d2, k = re.subn(r'posicionesEspacios:\n  - \{.*?\}\n  - \{.*?\}',
                    'posicionesEspacios:\n  - {x: -0.45, y: -0.73, z: 0}\n  - {x: 0.45, y: -0.73, z: 0}', docs[idx[r]])
    assert k == 1
    docs[idx[r]] = d2

# -------------------------------------------- despensas de los costados
mover('EstacionIngredientes', -6.6, -3.05)
sprite('EstacionIngredientes', 'Escenario/Ingredientes', 2.3, 3.9, 3)
caja('EstacionIngredientes', 2.3, 3.9, -1.15, 0)
campo('EstacionIngredientes', 'EstacionIngredientes', 'distanciaInteraccion', '2.3')
mover('EstacionGalletitas', 6.6, -3.05)
sprite('EstacionGalletitas', 'Escenario/Galletitas', 2.3, 3.9, 3)
caja('EstacionGalletitas', 2.3, 3.9, 1.15, 0)
campo('EstacionGalletitas', 'EstacionGalletitas', 'distanciaInteraccion', '2.3')

# ------------------------------------------- Tacho (instancia de prefab)
for i, d in enumerate(docs):
    if d.startswith('--- !u!1001 ') and '756fae0b759a4c7299c06a77e5c571f6' in d:
        d = re.sub(r'(propertyPath: m_LocalPosition\.x\n\s+value: ).*', r'\g<1>6.15', d, count=1)
        d = re.sub(r'(propertyPath: m_LocalPosition\.y\n\s+value: ).*', r'\g<1>-1.4', d, count=1)
        docs[i] = d

# ------------------------------------- barras de paciencia (Canvas 1920x1080)
for k, x in enumerate(XS, start=1):
    editar(comp('TextoPedido%d' % k, '224'), 'm_AnchoredPosition', '{x: %.1f, y: -288}' % (x * 108))

ultimo = docs[-1]
assert ultimo.startswith('--- !u!1660057539')
salida = cab + '\n' + '\n'.join(docs)
if not salida.endswith('\n'):
    salida += '\n'
destino = sys.argv[1] if len(sys.argv) > 1 else RUTA
open(destino, 'w', encoding='utf-8', newline='\n').write(salida)

defs = re.findall(r'^--- !u!\d+ &(-?\d+)', salida, re.M)
refs = set(re.findall(r'\{fileID: (\d{3,})\}', salida))
dobles = re.findall(r'^\s*([A-Za-z_ ]+): \1:', salida, re.M)
print('ok | borrados', len(viejos), '| dup', len(defs) - len(set(defs)), '| refs sin def', sorted(refs - set(defs))[:5], '| claves dobles', dobles[:3])
