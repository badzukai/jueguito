# Rehace los paneles de Victoria y Derrota de Cafeteria.unity (cartel, título, resumen y dos botones)
# y le da estilo pixel a las barras de paciencia. Se puede correr más de una vez.
import re, os, json, sys

DIR = os.path.dirname(os.path.abspath(__file__))
RAIZ = os.path.dirname(os.path.dirname(DIR))
ASSETS = os.path.join(RAIZ, 'Assets')
RUTA = os.path.join(ASSETS, 'Scenes', 'Cafeteria.unity')
guids = json.load(open(os.path.join(DIR, 'guids.json')))

texto = open(RUTA, encoding='utf-8', newline='').read()
partes = re.split(r'\n(?=--- !u!)', texto)
cab, docs = partes[0], partes[1:]
HUDP = open(os.path.join(ASSETS, 'Prefabs', 'HUD.prefab'), encoding='utf-8').read()
TMPL = re.search(r'--- !u!114 &900000130\n(.*?)(?=\n--- )', HUDP, re.S).group(1)
SOMBRA = '{fileID: 2100000, guid: e73a58f6e2794ae7b1b7e50b7fb811b0, type: 2}'


def info(d):
    return re.match(r'--- !u!(\d+) &(-?\d+)', d).groups()


def indexar():
    global by, names
    by = {info(d)[1]: d for d in docs}
    names = {}
    for f, d in by.items():
        if d.startswith('--- !u!1 '):
            names.setdefault(re.search(r'm_Name: (.*)', d).group(1), []).append(f)


indexar()


def comps(go):
    return re.findall(r'component: \{fileID: (\d+)\}', by[go])


def rt_de(go):
    return [c for c in comps(go) if by[c].startswith('--- !u!224 ')][0]


def go_de(c):
    return re.search(r'm_GameObject: \{fileID: (\d+)\}', by[c]).group(1)


def hijos(rt):
    return re.findall(r'  - \{fileID: (\d+)\}', by[rt].split('m_Father')[0])


def spr(clave):
    return '{fileID: 21300000, guid: %s, type: 3}' % guids[clave]


def reemplazar(fid, clave, valor):
    d, k = re.subn(r'(\n  %s: ).*' % re.escape(clave), lambda m: m.group(1) + valor, by[fid], count=1)
    assert k == 1, (fid, clave)
    by[fid] = d


# ------------------------------------------------------------ armado de UI
siguiente = [1800000000]
nuevos = []


def nid():
    siguiente[0] += 1
    return siguiente[0]


def hex2(h):
    h = h.lstrip('#')
    r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
    return 255 << 24 | b << 16 | g << 8 | r, '{r: %.4f, g: %.4f, b: %.4f, a: 1}' % (r / 255, g / 255, b / 255)


def nodo(nombre, padre_rt, amin, amax, pos, size, pivot, componentes):
    # componentes: función que recibe el id del GameObject y devuelve [(id, yaml)]
    g, r = nid(), nid()
    cs = componentes(g)
    nuevos.append('--- !u!1 &%d\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n'
                  '  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n'
                  '  m_Component:\n  - component: {fileID: %d}\n%s  m_Layer: 5\n  m_Name: %s\n  m_TagString: Untagged\n'
                  '  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1'
                  % (g, r, ''.join('  - component: {fileID: %d}\n' % c for c, _ in cs), nombre))
    nodos_rt[r] = dict(go=g, padre=padre_rt, hijos=[], amin=amin, amax=amax, pos=pos, size=size, pivot=pivot)
    if padre_rt in nodos_rt:
        nodos_rt[padre_rt]['hijos'].append(r)
    for _, y in cs:
        nuevos.append(y.rstrip('\n'))
    return r, g, [c for c, _ in cs]


nodos_rt = {}


def c_render(g):
    c = nid()
    return (c, '--- !u!222 &%d\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n'
               '  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
               '  m_CullTransparentMesh: 1\n' % (c, g))


def c_imagen(g, sprite, tipo=0, raycast=0, color='{r: 1, g: 1, b: 1, a: 1}'):
    c = nid()
    return (c, '--- !u!114 &%d\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n'
               '  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
               '  m_Enabled: 1\n  m_EditorHideFlags: 0\n'
               '  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n'
               '  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n'
               '  m_Material: {fileID: 0}\n  m_Color: %s\n  m_RaycastTarget: %d\n'
               '  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n'
               '  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n'
               '  m_Sprite: %s\n  m_Type: %d\n  m_PreserveAspect: 0\n  m_FillCenter: 1\n'
               '  m_FillMethod: 4\n  m_FillAmount: 1\n  m_FillClockwise: 1\n  m_FillOrigin: 0\n'
               '  m_UseSpriteMesh: 0\n  m_PixelsPerUnitMultiplier: 1\n' % (c, g, color, raycast, sprite, tipo))


def c_texto(g, t, tam, color, estilo=0, h=2, v=512, material=None, auto=1, wrap=1, minimo=None):
    c = nid()
    b = TMPL.replace('m_GameObject: {fileID: 900000127}', 'm_GameObject: {fileID: %d}' % g)
    b = re.sub(r'm_text: ".*?"', lambda m: 'm_text: "%s"' % t, b, count=1)
    c32, cf = hex2(color)
    for p, r in [(r'm_fontSize: \d+', 'm_fontSize: %d' % tam), (r'm_fontSizeBase: \d+', 'm_fontSizeBase: %d' % tam),
                 (r'm_fontSizeMin: \d+', 'm_fontSizeMin: %d' % (minimo or tam // 2)), (r'm_fontSizeMax: \d+', 'm_fontSizeMax: %d' % tam),
                 (r'm_enableAutoSizing: \d+', 'm_enableAutoSizing: %d' % auto), (r'm_fontStyle: \d+', 'm_fontStyle: %d' % estilo),
                 (r'm_HorizontalAlignment: \d+', 'm_HorizontalAlignment: %d' % h), (r'm_VerticalAlignment: \d+', 'm_VerticalAlignment: %d' % v),
                 (r'm_TextWrappingMode: \d+', 'm_TextWrappingMode: %d' % wrap),
                 (r'  m_fontColor32:\n    serializedVersion: 2\n    rgba: \d+\n  m_fontColor: \{.*?\}',
                  '  m_fontColor32:\n    serializedVersion: 2\n    rgba: %d\n  m_fontColor: %s' % (c32, cf))]:
        b, k = re.subn(p, lambda m: r, b, count=1)
        assert k == 1, p
    if material:
        b = re.sub(r'm_sharedMaterial: \{.*?\}', lambda m: 'm_sharedMaterial: ' + material, b, count=1)
    return (c, '--- !u!114 &%d\n' % c + b + '\n')


def c_boton(g, grafico, target, metodo):
    c = nid()
    return (c, '--- !u!114 &%d\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n'
               '  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
               '  m_Enabled: 1\n  m_EditorHideFlags: 0\n'
               '  m_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}\n'
               '  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button\n'
               '  m_Navigation:\n    m_Mode: 3\n    m_WrapAround: 0\n    m_SelectOnUp: {fileID: 0}\n'
               '    m_SelectOnDown: {fileID: 0}\n    m_SelectOnLeft: {fileID: 0}\n    m_SelectOnRight: {fileID: 0}\n'
               '  m_Transition: 1\n  m_Colors:\n    m_NormalColor: {r: 1, g: 1, b: 1, a: 1}\n'
               '    m_HighlightedColor: {r: 1, g: 0.85, b: 0.75, a: 1}\n    m_PressedColor: {r: 0.78, g: 0.6, b: 0.55, a: 1}\n'
               '    m_SelectedColor: {r: 1, g: 0.92, b: 0.85, a: 1}\n'
               '    m_DisabledColor: {r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}\n'
               '    m_ColorMultiplier: 1\n    m_FadeDuration: 0.1\n'
               '  m_SpriteState:\n    m_HighlightedSprite: {fileID: 0}\n    m_PressedSprite: {fileID: 0}\n'
               '    m_SelectedSprite: {fileID: 0}\n    m_DisabledSprite: {fileID: 0}\n'
               '  m_AnimationTriggers:\n    m_NormalTrigger: Normal\n    m_HighlightedTrigger: Highlighted\n'
               '    m_PressedTrigger: Pressed\n    m_SelectedTrigger: Selected\n    m_DisabledTrigger: Disabled\n'
               '  m_Interactable: 1\n  m_TargetGraphic: {fileID: %d}\n'
               '  m_OnClick:\n    m_PersistentCalls:\n      m_Calls:\n'
               '      - m_Target: {fileID: %s}\n        m_TargetAssemblyTypeName: GameManager, Assembly-CSharp\n'
               '        m_MethodName: %s\n        m_Mode: 1\n        m_Arguments:\n'
               '          m_ObjectArgument: {fileID: 0}\n'
               '          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine\n'
               '          m_IntArgument: 0\n          m_FloatArgument: 0\n          m_StringArgument: \n'
               '          m_BoolArgument: 0\n        m_CallState: 2\n' % (c, g, grafico, target, metodo))


# -------------------------------------------------------- GameManager
gm_go = names['GameManager'][0]
gm_guid = open(os.path.join(ASSETS, 'Scripts', 'GameManager.cs.meta')).read().split('guid: ')[1].split()[0]
gm = [c for c in comps(gm_go) if ('guid: ' + gm_guid) in by[c]][0]


def borrar_subarbol(rt):
    for h in hijos(rt):
        borrar_subarbol(h)
    g = go_de(rt)
    for c in comps(g):
        del by[c]
    del by[g]


resumenes = {}
for panel, titulo, color_titulo in (('PanelVictoria', '\\xA1Ganaste!', '#3F7C27'), ('PanelDerrota', '\\xA1Perdiste!', '#A92E2E')):
    pg = names[panel][0]
    prt = rt_de(pg)
    for h in hijos(prt):
        borrar_subarbol(h)
    # el panel: fondo oscuro que tapa todo
    img = [c for c in comps(pg) if 'UnityEngine.UI.Image' in by[c]][0]
    reemplazar(img, 'm_Color', '{r: 0.1, g: 0.05, b: 0.03, a: 0.65}')
    nodos_rt[prt] = dict(go=pg, padre=None, hijos=[], existente=True)
    hoja, _, _ = nodo('Hoja', prt, (0.5, 0.5), (0.5, 0.5), (0, 0), (900, 640), (0.5, 0.5),
                      lambda g: [c_render(g), c_imagen(g, spr('HUD/CartelPapel'), tipo=1, raycast=1)])
    nodo('Titulo', hoja, (0.5, 1), (0.5, 1), (0, -34), (800, 100), (0.5, 1),
         lambda g: [c_render(g), c_texto(g, titulo, 80, color_titulo, estilo=1, wrap=0)])
    _, _, cs = nodo('TextoResumen', hoja, (0.5, 1), (0.5, 1), (0, -150), (780, 270), (0.5, 1),
                    lambda g: [c_render(g), c_texto(g, 'Resumen de la partida', 34, '#2B1D16', v=256)])
    resumenes[panel] = cs[1]
    for nombre, etiqueta, metodo, x in (('BotonReintentar', 'Volver a jugar', 'ReiniciarPartida', -205),
                                        ('BotonMenu', 'Men\\xFA', 'IrAlMenu', 205)):
        def comp_boton(g, metodo=metodo):
            r = c_render(g)
            i = c_imagen(g, spr('HUD/Boton'), tipo=1, raycast=1)
            return [r, i, c_boton(g, i[0], gm, metodo)]
        b, _, _ = nodo(nombre, hoja, (0.5, 0), (0.5, 0), (x, 60), (380, 92), (0.5, 0), comp_boton)
        nodo('Texto', b, (0, 0), (1, 1), (0, 3), (-20, -12), (0.5, 0.5),
             lambda g, etiqueta=etiqueta: [c_render(g), c_texto(g, etiqueta, 42, '#FFF4E0', estilo=1, material=SOMBRA, wrap=0)])

# referencias del GameManager a los textos del resumen
d = by[gm]
d = re.sub(r'\n  textoResumenVictoria: .*', '', d)
d = re.sub(r'\n  textoResumenDerrota: .*', '', d)
d = d.replace('\n  panelVictoria: ', '\n  textoResumenVictoria: {fileID: %d}\n  textoResumenDerrota: {fileID: %d}\n  panelVictoria: '
              % (resumenes['PanelVictoria'], resumenes['PanelDerrota']), 1)
by[gm] = d

# ------------------------------------------- RectTransforms de los nodos nuevos
def v(p):
    return '{x: %s, y: %s}' % (p[0], p[1])


for r, n in nodos_rt.items():
    if n.get('existente'):
        d = by[r]
        lista = ''.join('\n  - {fileID: %d}' % h for h in n['hijos'])
        d = re.sub(r'  m_Children:.*?\n  m_Father', lambda m: '  m_Children:' + (lista if lista else ' []') + '\n  m_Father', d, count=1, flags=re.S)
        by[r] = d
        continue
    ch = ('  m_Children:\n' + ''.join('  - {fileID: %d}\n' % h for h in n['hijos'])) if n['hijos'] else '  m_Children: []\n'
    nuevos.append('--- !u!224 &%d\nRectTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n'
                  '  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
                  '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n'
                  '  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n%s'
                  '  m_Father: {fileID: %s}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n'
                  '  m_AnchorMin: %s\n  m_AnchorMax: %s\n  m_AnchoredPosition: %s\n  m_SizeDelta: %s\n  m_Pivot: %s'
                  % (r, n['go'], ch, n['padre'], v(n['amin']), v(n['amax']), v(n['pos']), v(n['size']), v(n['pivot'])))

# ------------------------------------------------- barras de paciencia pixel
for k in (1, 2, 3):
    bg_ = names['BarraPaciencia%d' % k][0]
    brt = rt_de(bg_)
    reemplazar(brt, 'm_SizeDelta', '{x: 150, y: 22}')
    for h in hijos(brt):
        hg = go_de(h)
        nombre = re.search(r'm_Name: (.*)', by[hg]).group(1)
        if nombre == 'Background':
            img = [c for c in comps(hg) if 'UnityEngine.UI.Image' in by[c]][0]
            reemplazar(img, 'm_Sprite', spr('HUD/ProgresoFondo'))
            reemplazar(img, 'm_Type', '1')
            reemplazar(img, 'm_Color', '{r: 1, g: 1, b: 1, a: 1}')
        elif nombre == 'Fill Area':
            for hh in hijos(h):
                fg = go_de(hh)
                img = [c for c in comps(fg) if 'UnityEngine.UI.Image' in by[c]][0]
                reemplazar(img, 'm_Sprite', spr('HUD/ProgresoRelleno'))
                reemplazar(img, 'm_Type', '0')
                reemplazar(img, 'm_Color', '{r: 0.45, g: 0.8, b: 0.35, a: 1}')
        elif nombre == 'Handle Slide Area':
            by[hg] = re.sub(r'm_IsActive: \d', 'm_IsActive: 0', by[hg], count=1)

# --------------------------------------------------------------- guardar
orden = [info(d)[1] for d in docs]
docs = [by[f] for f in orden if f in by]
assert docs[-1].startswith('--- !u!1660057539')
docs = docs[:-1] + nuevos + [docs[-1]]
salida = cab + '\n' + '\n'.join(docs) + '\n'
open(sys.argv[1] if len(sys.argv) > 1 else RUTA, 'w', encoding='utf-8', newline='\n').write(salida)
defs = re.findall(r'^--- !u!\d+ &(-?\d+)', salida, re.M)
refs = set(re.findall(r'\{fileID: (\d{3,})\}', salida))
print('ok | nuevos', len(nuevos), '| dup', len(defs) - len(set(defs)), '| refs sin def', sorted(refs - set(defs))[:5])
