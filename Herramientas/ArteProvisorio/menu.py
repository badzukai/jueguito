# Rearma la UI de Menu.unity con el estilo nuevo (cafetería de fondo, cartel de madera,
# botones rojos) y un "Cómo jugar" de 3 páginas con imágenes.
# Mantiene la cámara, el EventSystem, el Canvas y el objeto Menu (con su script).
import re, os, json

DIR = os.path.dirname(os.path.abspath(__file__))
ASSETS = r'C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2\Assets'
RUTA = os.path.join(ASSETS, 'Scenes', 'Menu.unity')
guids = json.load(open(os.path.join(DIR, 'guids.json')))

src = open(RUTA, encoding='utf-8', newline='').read()
assert '\r\n' not in src
partes = re.split(r'\n(?=--- !u!)', src)
cab, docs = partes[0], partes[1:]
byid = {re.match(r'--- !u!\d+ &(-?\d+)', d).group(1): d for d in docs}

# Plantilla de texto TMP (UI) sacada del HUD.
hud = open(os.path.join(ASSETS, 'Prefabs', 'HUD.prefab'), encoding='utf-8').read()
TMPL = re.search(r'--- !u!114 &900000130\n(.*?)(?=\n--- )', hud, re.S).group(1)
BOTON_T = re.search(r'--- !u!114 &900000025\n(.*?)(?=\n--- )', src, re.S).group(1) if '&900000025' in src else None

IMAGE = 'fe87c0e1cc204ed48ad3b37840f39efc'
BUTTON = '4e29b1a8efbd4b44bb3f3716e73f07ff'
OUTLINE = '{fileID: 2100000, guid: 79459efec17a4d00a321bdcc27bbc385, type: 2}'
SOMBRA = '{fileID: 2100000, guid: e73a58f6e2794ae7b1b7e50b7fb811b0, type: 2}'
MENU_MB = '900000001'
CANVAS_GO, CANVAS_RT = '900000002', '900000003'


def guid_meta(ruta):
    return open(ruta + '.meta').read().split('guid: ')[1].split()[0]


def spr(clave):
    if clave.startswith('Objetos/'):
        g = guid_meta(os.path.join(ASSETS, 'Sprites', 'Objetos', clave.split('/')[1] + '.png'))
    else:
        g = guids[clave]
    return '{fileID: 21300000, guid: %s, type: 3}' % g


siguiente = [910000000]


def nid():
    siguiente[0] += 1
    return siguiente[0]


nodos = {}      # rt -> dict
orden = []


def v(p):
    return '{x: %s, y: %s}' % (p[0], p[1])


def hexcol(h, a=1):
    h = h.lstrip('#')
    return '{r: %.4f, g: %.4f, b: %.4f, a: %s}' % (int(h[0:2], 16) / 255, int(h[2:4], 16) / 255, int(h[4:6], 16) / 255, a)


def rgba32(h):
    h = h.lstrip('#')
    r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
    return 255 << 24 | b << 16 | g << 8 | r


def nodo(nombre, padre, amin=(0.5, 0.5), amax=None, pos=(0, 0), size=(100, 100), pivot=(0.5, 0.5), activo=1):
    go, rt = nid(), nid()
    nodos[rt] = dict(go=go, nombre=nombre, padre=padre, hijos=[], amin=amin, amax=amax or amin,
                     pos=pos, size=size, pivot=pivot, comps=[], activo=activo)
    orden.append(rt)
    if padre in nodos:
        nodos[padre]['hijos'].append(rt)
    else:
        raiz_hijos.append(rt)
    return rt


def cr(rt):
    f = nid()
    nodos[rt]['comps'].append((f, '--- !u!222 &%d\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n'
                               '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                               '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
                               '  m_CullTransparentMesh: 1\n' % (f, nodos[rt]['go'])))


def imagen(rt, sprite='{fileID: 0}', tipo=0, aspecto=0, color='{r: 1, g: 1, b: 1, a: 1}', raycast=0):
    cr(rt)
    f = nid()
    nodos[rt]['comps'].append((f, '--- !u!114 &%d\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
                               '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                               '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
                               '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
                               '  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n'
                               '  m_Material: {fileID: 0}\n  m_Color: %s\n  m_RaycastTarget: %d\n'
                               '  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n'
                               '  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n'
                               '  m_Sprite: %s\n  m_Type: %d\n  m_PreserveAspect: %d\n  m_FillCenter: 1\n'
                               '  m_FillMethod: 4\n  m_FillAmount: 1\n  m_FillClockwise: 1\n  m_FillOrigin: 0\n'
                               '  m_UseSpriteMesh: 0\n  m_PixelsPerUnitMultiplier: 1\n'
                               % (f, nodos[rt]['go'], IMAGE, color, raycast, sprite, tipo, aspecto)))
    return f


def texto(rt, t, tam, color='#2B1D16', estilo=0, h=2, vert=512, minimo=None, material=None, auto=1, wrap=1):
    cr(rt)
    f = nid()
    b = TMPL.replace('m_GameObject: {fileID: 900000127}', 'm_GameObject: {fileID: %d}' % nodos[rt]['go'])
    t_yaml = t.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n')
    b = re.sub(r'm_text: ".*?"', lambda m: 'm_text: "%s"' % t_yaml, b, count=1)
    reps = [(r'm_fontSize: \d+', 'm_fontSize: %d' % tam), (r'm_fontSizeBase: \d+', 'm_fontSizeBase: %d' % tam),
            (r'm_fontSizeMin: \d+', 'm_fontSizeMin: %d' % (minimo or max(12, tam // 2))),
            (r'm_fontSizeMax: \d+', 'm_fontSizeMax: %d' % tam), (r'm_enableAutoSizing: \d+', 'm_enableAutoSizing: %d' % auto),
            (r'm_fontStyle: \d+', 'm_fontStyle: %d' % estilo), (r'm_HorizontalAlignment: \d+', 'm_HorizontalAlignment: %d' % h),
            (r'm_VerticalAlignment: \d+', 'm_VerticalAlignment: %d' % vert), (r'm_TextWrappingMode: \d+', 'm_TextWrappingMode: %d' % wrap)]
    for p, r in reps:
        b, k = re.subn(p, lambda m: r, b, count=1)
        assert k == 1, p
    b, k = re.subn(r'  m_fontColor32:\n    serializedVersion: 2\n    rgba: \d+\n  m_fontColor: \{.*?\}',
                   lambda m: '  m_fontColor32:\n    serializedVersion: 2\n    rgba: %d\n  m_fontColor: %s' % (rgba32(color), hexcol(color)), b, count=1)
    assert k == 1
    if material:
        b, k = re.subn(r'm_sharedMaterial: \{.*?\}', lambda m: 'm_sharedMaterial: ' + material, b, count=1)
        assert k == 1
    nodos[rt]['comps'].append((f, '--- !u!114 &%d\n' % f + b + '\n'))
    return f


def boton(rt, grafico, metodo, argumento=None):
    f = nid()
    modo = 5 if argumento is not None else 1
    llamada = ('      - m_Target: {fileID: %s}\n        m_TargetAssemblyTypeName: MainMenu, Assembly-CSharp\n'
               '        m_MethodName: %s\n        m_Mode: %d\n        m_Arguments:\n'
               '          m_ObjectArgument: {fileID: 0}\n'
               '          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine\n'
               '          m_IntArgument: 0\n          m_FloatArgument: 0\n          m_StringArgument: %s\n'
               '          m_BoolArgument: 0\n        m_CallState: 2\n') % (MENU_MB, metodo, modo, argumento or '')
    nodos[rt]['comps'].append((f, '--- !u!114 &%d\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
                               '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                               '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
                               '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
                               '  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button\n'
                               '  m_Navigation:\n    m_Mode: 3\n    m_WrapAround: 0\n'
                               '    m_SelectOnUp: {fileID: 0}\n    m_SelectOnDown: {fileID: 0}\n'
                               '    m_SelectOnLeft: {fileID: 0}\n    m_SelectOnRight: {fileID: 0}\n'
                               '  m_Transition: 1\n  m_Colors:\n'
                               '    m_NormalColor: {r: 1, g: 1, b: 1, a: 1}\n'
                               '    m_HighlightedColor: {r: 1, g: 0.85, b: 0.75, a: 1}\n'
                               '    m_PressedColor: {r: 0.78, g: 0.6, b: 0.55, a: 1}\n'
                               '    m_SelectedColor: {r: 1, g: 0.92, b: 0.85, a: 1}\n'
                               '    m_DisabledColor: {r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}\n'
                               '    m_ColorMultiplier: 1\n    m_FadeDuration: 0.1\n'
                               '  m_SpriteState:\n    m_HighlightedSprite: {fileID: 0}\n    m_PressedSprite: {fileID: 0}\n'
                               '    m_SelectedSprite: {fileID: 0}\n    m_DisabledSprite: {fileID: 0}\n'
                               '  m_AnimationTriggers:\n    m_NormalTrigger: Normal\n    m_HighlightedTrigger: Highlighted\n'
                               '    m_PressedTrigger: Pressed\n    m_SelectedTrigger: Selected\n    m_DisabledTrigger: Disabled\n'
                               '  m_Interactable: 1\n  m_TargetGraphic: {fileID: %d}\n'
                               '  m_OnClick:\n    m_PersistentCalls:\n      m_Calls:\n%s'
                               % (f, nodos[rt]['go'], BUTTON, grafico, llamada)))


CREMA, TINTA, CAFE, AMARILLO = '#FFF4E0', '#2B1D16', '#8A5A3A', '#FFE39A'


def boton_rojo(nombre, padre, pos, size, etiqueta, metodo, argumento=None, tam=44, amin=(0.5, 0.5)):
    rt = nodo(nombre, padre, amin=amin, pos=pos, size=size)
    img = imagen(rt, spr('HUD/Boton'), tipo=1, raycast=1)
    boton(rt, img, metodo, argumento)
    t = nodo('Texto', rt, amin=(0, 0), amax=(1, 1), pos=(0, 3), size=(-20, -12))
    texto(t, etiqueta, tam, CREMA, estilo=1, material=SOMBRA, wrap=0)
    return rt


def icono(padre, clave, x, y, w, h, amin=(0, 1), pivot=(0, 1)):
    rt = nodo(clave.split('/')[-1], padre, amin=amin, pos=(x, y), size=(w, h), pivot=pivot)
    imagen(rt, spr(clave), aspecto=1)
    return rt


def etiqueta_texto(padre, t, x, y, w, h, tam, color=TINTA, estilo=0, h_al=1, amin=(0, 1), pivot=(0, 1), vert=512):
    rt = nodo('Texto', padre, amin=amin, pos=(x, y), size=(w, h), pivot=pivot)
    texto(rt, t, tam, color, estilo=estilo, h=h_al, vert=vert)
    return rt


def tecla(padre, letra, x, y, lado=70):
    rt = nodo('Tecla' + letra.replace(' ', ''), padre, amin=(0, 1), pos=(x, y), size=(lado, lado), pivot=(0, 1))
    imagen(rt, spr('HUD/Tecla'), tipo=1)
    t = nodo('Letra', rt, amin=(0, 0), amax=(1, 1), pos=(0, 4), size=(0, -8))
    texto(t, letra, 36, TINTA, estilo=1, wrap=0)
    return rt


raiz_hijos = []

# ------------------------------------------------------------- fondo
fondo = nodo('Fondo', CANVAS_RT, amin=(0, 0), amax=(1, 1), size=(0, 0))
imagen(fondo, spr('Menu/FondoMenu'))
velo = nodo('Velo', CANVAS_RT, amin=(0, 0), amax=(1, 1), size=(0, 0))
imagen(velo, color='{r: 0.17, g: 0.08, b: 0.05, a: 0.45}')

# ---------------------------------------------------- panel principal
pp = nodo('PanelPrincipal', CANVAS_RT, amin=(0, 0), amax=(1, 1), size=(0, 0))
cartel = nodo('CartelTitulo', pp, pos=(0, 255), size=(1180, 250))
imagen(cartel, spr('HUD/BarraMadera'), tipo=1)
t = nodo('Titulo', cartel, amin=(0, 0), amax=(1, 1), pos=(0, 22), size=(-60, -90))
texto(t, 'Barista Caótico', 130, AMARILLO, estilo=1, material=OUTLINE, wrap=0, minimo=60)
t = nodo('Subtitulo', cartel, amin=(0, 0), amax=(1, 0), pos=(0, 52), size=(-80, 60), pivot=(0.5, 0.5))
texto(t, '¡Que no se enfríe el café!', 40, CREMA, estilo=2, material=SOMBRA, wrap=0)
for k, (n, etq, met, arg) in enumerate([('BotonJugar', 'Jugar', 'LoadSceneByName', 'Cafeteria'),
                                        ('BotonComoJugar', 'Cómo jugar', 'ShowHowToPlay', None),
                                        ('BotonCreditos', 'Créditos', 'ShowCredits', None),
                                        ('BotonSalir', 'Salir', 'Exit', None)]):
    boton_rojo(n, pp, (0, -10 - k * 118), (540, 100), etq, met, arg)

# ------------------------------------------------------ cómo jugar
pc = nodo('PanelComoJugar', CANVAS_RT, amin=(0, 0), amax=(1, 1), size=(0, 0), activo=0)
hoja = nodo('Hoja', pc, size=(1600, 960))
imagen(hoja, spr('HUD/CartelPapel'), tipo=1, raycast=1)
t = nodo('Titulo', hoja, amin=(0.5, 1), pos=(0, -28), size=(900, 80), pivot=(0.5, 1))
texto(t, 'Cómo jugar', 64, '#5A3018', estilo=1, wrap=0)

paginas = []


def pagina(nombre, titulo, activa):
    p = nodo(nombre, hoja, amin=(0, 0), amax=(1, 1), pos=(0, -10), size=(-140, -280), activo=activa)
    etiqueta_texto(p, titulo, 0, 0, 1460, 56, 44, CAFE, estilo=1)
    paginas.append(p)
    return p


# Página 1: controles
p1 = pagina('PaginaControles', 'Controles', 1)
filas = [(['W', 'A', 'S', 'D'], 'o las <b>flechas</b>: moverte por la cafetería.'),
         (['E'], 'Usar lo que tenés más cerca: máquinas, repisas, bandejas y el tacho.'),
         (['R'], 'Sacar algo de una bandeja (por si te equivocaste).'),
         (['Z', 'X', 'C'], 'En <b>Ingredientes</b>: granos de café, leche y chocolate.'),
         (['Z', 'X', 'C'], 'En <b>Galletitas</b>: red velvet, vainilla con chips y chocolate.')]
for k, (teclas, desc) in enumerate(filas):
    y = -80 - k * 108
    for i, l in enumerate(teclas):
        tecla(p1, l, i * 80, y)
    etiqueta_texto(p1, desc, 340, y, 660, 72, 32)
icono(p1, 'Escenario/Ingredientes', 1050, -90, 180, 190)
etiqueta_texto(p1, 'Ingredientes', 1020, -290, 240, 40, 28, CAFE, estilo=1, h_al=2)
icono(p1, 'Escenario/Galletitas', 1270, -90, 180, 190)
etiqueta_texto(p1, 'Galletitas', 1240, -290, 240, 40, 28, CAFE, estilo=1, h_al=2)
etiqueta_texto(p1, 'Las letras están pintadas en cada estante, ¡así no te las olvidás!', 1020, -340, 440, 100, 26, CAFE, estilo=2, h_al=2, vert=256)

# Página 2: máquinas y recetas
p2 = pagina('PaginaMaquinas', 'Máquinas y recetas', 0)
maquinas = [('Escenario/Moledora', 'Moledora', 'Muele los granos de café.', [['Objetos/Cafe', '>', 'Objetos/CafeMolido']]),
            ('Escenario/Cafetera', 'Cafetera', 'Hace el café y espuma la leche.',
             [['Objetos/CafeMolido', '>', 'Objetos/CafePreparado'], ['Objetos/Leche', '>', 'Objetos/LecheEspumada']]),
            ('Escenario/Chocolatera', 'Chocolatera', 'Prepara el chocolate caliente.',
             [['Objetos/Chocolate', '>', 'Objetos/ChocolatePreparado']]),
            ('Escenario/Repisa', 'Repisa', 'Juntá dos cosas y se mezclan solas. También sirve para apoyar.',
             [['Objetos/CafePreparado', '+', 'Objetos/LecheEspumada', '>', 'Objetos/CafeConLeche'],
              ['Objetos/ChocolatePreparado', '+', 'Objetos/LecheEspumada', '>', 'Objetos/ChocolateConLeche']]),
            ('Objetos/Tacho', 'Tacho', 'Tira lo que tenés en la mano.', [])]
for k, (img_maq, nombre, desc, cadenas) in enumerate(maquinas):
    y = -70 - k * 112
    icono(p2, img_maq, 0, y + 4, 140, 108)
    etiqueta_texto(p2, '<b>' + nombre + '</b>\n<size=85%>' + desc + '</size>', 160, y, 490, 104, 34)
    x = 660
    for cadena in cadenas:
        for paso in cadena:
            if paso == '>':
                icono(p2, 'HUD/FlechaDerecha', x, y - 30, 48, 36)
                x += 58
            elif paso == '+':
                icono(p2, 'HUD/Mas', x, y - 30, 34, 34)
                x += 44
            else:
                icono(p2, paso, x, y - 8, 80, 80)
                x += 86
        x += 40

# Página 3: pedidos y reglas
p3 = pagina('PaginaPedidos', 'Pedidos y reglas', 0)
reglas = [(['Personajes/Cliente1'], 'Cada cliente pide una <b>bebida</b> y, a veces, una <b>galletita</b>. Lo ves en el globo arriba de su cabeza.'),
          (['Escenario/Bandeja'], 'Poné el pedido en <b>su bandeja</b> y apretá <b>E con las manos vacías</b> para entregarlo.'),
          (['HUD/Corazon', 'HUD/Corazon', 'HUD/CorazonVacio'], 'Si un cliente espera demasiado, se va enojado y perdés un <b>corazón</b>. Sin corazones, perdés.'),
          (['HUD/Moneda'], 'Cuanto más rápido entregás, más <b>propina</b> te dejan. Completá todos los pedidos (arriba a la izquierda) para ganar.'),
          (['HUD/Flecha'], 'Los <b>3 primeros pedidos</b> son un tutorial: seguí la flecha. Con <b>Esc</b> pausás el juego.')]
for k, (imgs, desc) in enumerate(reglas):
    y = -70 - k * 112
    ancho = 260 // len(imgs)
    for i, clave in enumerate(imgs):
        icono(p3, clave, i * ancho, y, min(ancho, 110) - 6, 96)
    etiqueta_texto(p3, desc, 290, y, 1170, 96, 32)

# barra de abajo: anterior / página / siguiente / volver
boton_rojo('BotonAnterior', hoja, (-300, 60), (260, 84), 'Anterior', 'PreviousPage', tam=36, amin=(0.5, 0))
tp = nodo('TextoPagina', hoja, amin=(0.5, 0), pos=(0, 60), size=(200, 60))
texto_pagina = texto(tp, '1 / 3', 38, '#5A3018', estilo=1, wrap=0)
boton_rojo('BotonSiguiente', hoja, (300, 60), (260, 84), 'Siguiente', 'NextPage', tam=36, amin=(0.5, 0))
boton_rojo('BotonVolver', hoja, (-170, 60), (240, 84), 'Volver', 'BackToMenu', tam=36, amin=(1, 0))

# ---------------------------------------------------------- créditos
pk = nodo('PanelCreditos', CANVAS_RT, amin=(0, 0), amax=(1, 1), size=(0, 0), activo=0)
hoja2 = nodo('Hoja', pk, size=(1000, 640))
imagen(hoja2, spr('HUD/CartelPapel'), tipo=1, raycast=1)
t = nodo('Titulo', hoja2, amin=(0.5, 1), pos=(0, -30), size=(800, 80), pivot=(0.5, 1))
texto(t, 'Créditos', 64, '#5A3018', estilo=1, wrap=0)
t = nodo('Texto', hoja2, amin=(0, 0), amax=(1, 1), pos=(0, -10), size=(-120, -280))
texto(t, 'Desarrollo y arte\n<b>Helena Trindade</b>\n\nProgramación de Videojuegos 1\nT.U. en Videojuegos - UTN\n2026', 38, TINTA)
boton_rojo('BotonVolver', hoja2, (0, 60), (260, 84), 'Volver', 'BackToMenu', tam=36, amin=(0.5, 0))

# --------------------------------------------------- armar el archivo
salida_docs = []
for rt in orden:
    n = nodos[rt]
    comps = [rt] + [f for f, _ in n['comps']]
    salida_docs.append('--- !u!1 &%d\nGameObject:\n  m_ObjectHideFlags: 0\n'
                       '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                       '  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n%s'
                       '  m_Layer: 5\n  m_Name: %s\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n'
                       '  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: %d'
                       % (n['go'], ''.join('  - component: {fileID: %d}\n' % c for c in comps), n['nombre'], n['activo']))
    ch = ('  m_Children:\n' + ''.join('  - {fileID: %d}\n' % c for c in n['hijos'])) if n['hijos'] else '  m_Children: []\n'
    salida_docs.append('--- !u!224 &%d\nRectTransform:\n  m_ObjectHideFlags: 0\n'
                       '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
                       '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
                       '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n'
                       '  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n%s'
                       '  m_Father: {fileID: %s}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n'
                       '  m_AnchorMin: %s\n  m_AnchorMax: %s\n  m_AnchoredPosition: %s\n  m_SizeDelta: %s\n  m_Pivot: %s'
                       % (rt, n['go'], ch, n['padre'], v(n['amin']), v(n['amax']), v(n['pos']), v(n['size']), v(n['pivot'])))
    for _, d in n['comps']:
        salida_docs.append(d.rstrip('\n'))

# Documentos que se conservan: todo lo que no es UI vieja (cámara, EventSystem, Canvas, Menu...).
conservar_ids = ['1', '2', '3', '4']
viejos_ui = set()
for d in docs:
    m = re.match(r'--- !u!(\d+) &(-?\d+)', d)
    cls, fid = m.groups()
    es_vieja = fid.startswith('9000000') and int(fid) < 900000095
    es_generada = fid.startswith('91') and len(fid) == 9   # UI de una corrida anterior de este script
    if (es_vieja or es_generada) and fid not in ('900000001', '900000002', '900000003', '900000004', '900000005', '900000006'):
        viejos_ui.add(fid)
nuevos = []
for d in docs:
    fid = re.match(r'--- !u!\d+ &(-?\d+)', d).group(1)
    if fid in viejos_ui:
        continue
    if fid == CANVAS_RT:
        d = re.sub(r'  m_Children:\n(  - \{fileID: \d+\}\n)+', '  m_Children:\n' + ''.join('  - {fileID: %d}\n' % c for c in raiz_hijos), d)
    if fid == MENU_MB:
        d = re.sub(r'  mainPanel: .*', '  mainPanel: {fileID: %d}' % nodos[pp]['go'], d)
        d = re.sub(r'  howToPlayPanel: .*', '  howToPlayPanel: {fileID: %d}' % nodos[pc]['go'], d)
        d = re.sub(r'  creditsPanel: .*', '  creditsPanel: {fileID: %d}' % nodos[pk]['go'], d)
        d = re.sub(r'\n  howToPlayPages:(\n  - .*)*', '', d)
        d = re.sub(r'\n  pageText: .*', '', d)
        d = d.rstrip('\n') + '\n  howToPlayPages:\n' + ''.join('  - {fileID: %d}\n' % nodos[p]['go'] for p in paginas)
        d += '  pageText: {fileID: %d}' % texto_pagina
    nuevos.append(d.rstrip('\n'))
# La UI nueva va antes de la cámara (y SceneRoots queda al final).
i_cam = next(i for i, d in enumerate(nuevos) if '&900000095' in d.split('\n')[0])
final = nuevos[:i_cam] + salida_docs + nuevos[i_cam:]
salida = cab + '\n' + '\n'.join(final) + '\n'
open(RUTA, 'w', encoding='utf-8', newline='\n').write(salida)

defs = re.findall(r'^--- !u!\d+ &(-?\d+)', salida, re.M)
refs = set(re.findall(r'\{fileID: (\d{6,})\}', salida))
print('docs', len(defs), 'dup', len(defs) - len(set(defs)), 'sin def', sorted(refs - set(defs))[:5])
