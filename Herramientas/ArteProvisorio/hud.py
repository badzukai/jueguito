# Rearma el prefab HUD (barra de madera arriba + guía del tutorial + cartel de mensajes)
# y crea el prefab Flecha. Mantiene los fileID del root, TextoPedidos, CartelMensaje y TextoMensaje.
import re, os, json, uuid

DIR = os.path.dirname(os.path.abspath(__file__))
ASSETS = r'C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2\Assets'
HUDP = os.path.join(ASSETS, 'Prefabs', 'HUD.prefab')
FLECHAP = os.path.join(ASSETS, 'Prefabs', 'Flecha.prefab')
guids = json.load(open(os.path.join(DIR, 'guids.json')))

src = open(os.path.join(DIR, 'plantilla_HUD.prefab'), encoding='utf-8', newline='').read()
TMPL = re.search(r'--- !u!114 &900000130\n(.*?)(?=\n--- )', src, re.S).group(1)
CANVAS = re.search(r'(--- !u!223 &900000108\n.*?)(?=\n--- )', src, re.S).group(1) + '\n'
SCALER = re.search(r'(--- !u!114 &900000109\n.*?)(?=\n--- )', src, re.S).group(1) + '\n'
GUIOBJ = 'e6d6298195ed4ff79baccf9880b42393'  # no se usa (PanelTutorial borrado)

GUIA_GUID = open(os.path.join(ASSETS, 'Scripts', 'GuiaTutorial.cs.meta')).read().split('guid: ')[1].split()[0]
HUD_GUID = '7eac6faea5ea4976aebfaee21d96873c'
IMAGE = 'fe87c0e1cc204ed48ad3b37840f39efc'
CAFE_ICON = open(os.path.join(ASSETS, 'Sprites', 'Objetos', 'CafePreparado.png.meta')).read().split('guid: ')[1].split()[0]


def spr(clave):
    return '{fileID: 21300000, guid: %s, type: 3}' % guids[clave]


def go(fid, name, comps, active=1):
    c = ''.join('  - component: {fileID: %d}\n' % x for x in comps)
    return ('--- !u!1 &%d\nGameObject:\n  m_ObjectHideFlags: 0\n'
            '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
            '  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n%s'
            '  m_Layer: 5\n  m_Name: %s\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n'
            '  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: %d\n') % (fid, c, name, active)


def v(p):
    return '{x: %s, y: %s}' % (p[0], p[1])


def rt(fid, g, father, children, amin, amax, pos, size, pivot, scale='{x: 1, y: 1, z: 1}'):
    ch = ('  m_Children:\n' + ''.join('  - {fileID: %d}\n' % x for x in children)) if children else '  m_Children: []\n'
    return ('--- !u!224 &%d\nRectTransform:\n  m_ObjectHideFlags: 0\n'
            '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
            '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
            '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n'
            '  m_LocalScale: %s\n  m_ConstrainProportionsScale: 0\n%s'
            '  m_Father: {fileID: %d}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n'
            '  m_AnchorMin: %s\n  m_AnchorMax: %s\n  m_AnchoredPosition: %s\n'
            '  m_SizeDelta: %s\n  m_Pivot: %s\n') % (fid, g, scale, ch, father, v(amin), v(amax), v(pos), v(size), v(pivot))


def cr(fid, g):
    return ('--- !u!222 &%d\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n'
            '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
            '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n'
            '  m_CullTransparentMesh: 1\n') % (fid, g)


def img(fid, g, sprite, typ=0, preserve=0, color='{r: 1, g: 1, b: 1, a: 1}', fill=False):
    # typ: 0 simple, 1 sliced, 3 filled
    return ('--- !u!114 &%d\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
            '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
            '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
            '  m_EditorHideFlags: 0\n'
            '  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
            '  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n'
            '  m_Material: {fileID: 0}\n  m_Color: %s\n  m_RaycastTarget: 0\n'
            '  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n'
            '  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n'
            '  m_Sprite: %s\n  m_Type: %d\n  m_PreserveAspect: %d\n  m_FillCenter: 1\n'
            '  m_FillMethod: %d\n  m_FillAmount: %s\n  m_FillClockwise: 1\n  m_FillOrigin: 0\n'
            '  m_UseSpriteMesh: 0\n  m_PixelsPerUnitMultiplier: 1\n') % (
        fid, g, IMAGE, color, sprite, typ, preserve, 0 if fill else 4, '0.25' if fill else '1')


def sub1(pat, rep, b):
    nb, n = re.subn(pat, rep, b)
    assert n == 1, pat
    return nb


def rgba32(h):
    h = h.lstrip('#')
    r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
    return 255 << 24 | b << 16 | g << 8 | r, '{r: %.4f, g: %.4f, b: %.4f, a: 1}' % (r / 255, g / 255, b / 255)


def tmp(fid, g, text, size, smin, style, halign, valign, color, auto=1, wrap=1):
    b = TMPL.replace('m_GameObject: {fileID: 900000127}', 'm_GameObject: {fileID: %d}' % g)
    b = re.sub(r'm_text: ".*?"', lambda m: 'm_text: "%s"' % text, b, count=1)
    b = sub1(r'm_fontSize: \d+', 'm_fontSize: %d' % size, b)
    b = sub1(r'm_fontSizeBase: \d+', 'm_fontSizeBase: %d' % size, b)
    b = sub1(r'm_fontSizeMin: \d+', 'm_fontSizeMin: %d' % smin, b)
    b = sub1(r'm_fontSizeMax: \d+', 'm_fontSizeMax: %d' % size, b)
    b = sub1(r'm_enableAutoSizing: \d+', 'm_enableAutoSizing: %d' % auto, b)
    b = sub1(r'm_fontStyle: \d+', 'm_fontStyle: %d' % style, b)
    b = sub1(r'm_HorizontalAlignment: \d+', 'm_HorizontalAlignment: %d' % halign, b)
    b = sub1(r'm_VerticalAlignment: \d+', 'm_VerticalAlignment: %d' % valign, b)
    b = sub1(r'm_TextWrappingMode: \d+', 'm_TextWrappingMode: %d' % wrap, b)
    c32, cf = rgba32(color)
    old = ('  m_fontColor32:\n    serializedVersion: 2\n    rgba: 4294967295\n'
           '  m_fontColor: {r: 1, g: 1, b: 1, a: 1}')
    assert b.count(old) == 1
    b = b.replace(old, '  m_fontColor32:\n    serializedVersion: 2\n    rgba: %d\n  m_fontColor: %s' % (c32, cf))
    return '--- !u!114 &%d\n' % fid + b + '\n'


CREMA, CAFE_OSC, TINTA = '#FFF4E0', '#5A3018', '#2B1D16'
ROOT_GO, ROOT_RT = 900000106, 900000107

out = '%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'

# ------------------------------------------------------------- raíz HUD
BARRA, CARTEL = 900000202, 900000124
PAUSA_GO, PAUSA_RT = 900000310, 900000311
out += go(ROOT_GO, 'HUD', [ROOT_RT, 900000108, 900000109, 900000110, 900000201, 900000301, 900000302, 900000303])
out += rt(ROOT_RT, ROOT_GO, 0, [BARRA, CARTEL, PAUSA_RT], (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), '{x: 0, y: 0, z: 0}')
out += CANVAS + SCALER

# ------------------------------------------------- barra de madera (arriba)
out += go(900000203, 'BarraSuperior', [BARRA, 900000204, 900000205])
hijos_barra = [900000208, 900000212, 900000116, 900000220, 900000230, 900000250, 900000271, 900000275, 900000279,
               900000291, 900000295]
out += rt(BARRA, 900000203, ROOT_RT, hijos_barra, (0, 1), (1, 1), (0, 0), (0, 108), (0.5, 1))
out += cr(900000204, 900000203)
out += img(900000205, 900000203, spr('HUD/BarraMadera'), typ=1)

# taza
out += go(900000207, 'IconoPedidos', [900000208, 900000209, 900000210])
out += rt(900000208, 900000207, BARRA, [], (0, 0.5), (0, 0.5), (28, 2), (72, 72), (0, 0.5))
out += cr(900000209, 900000207)
out += img(900000210, 900000207, '{fileID: 21300000, guid: %s, type: 3}' % CAFE_ICON, preserve=1)

# título chiquito "PEDIDOS" arriba de la barra de progreso
out += go(900000211, 'TextoEtiquetaPedidos', [900000212, 900000213, 900000214])
out += rt(900000212, 900000211, BARRA, [], (0, 0.5), (0, 0.5), (108, 22), (150, 30), (0, 0.5))
out += cr(900000213, 900000211)
out += tmp(900000214, 900000211, 'PEDIDOS', 22, 16, 1, 1, 512, '#FFE39A', auto=0)

# TextoPedidos (se mantiene el fileID): "0 / 12"
out += go(900000115, 'TextoPedidos', [900000116, 900000117, 900000118])
out += rt(900000116, 900000115, BARRA, [], (0, 0.5), (0, 0.5), (108, -14), (150, 44), (0, 0.5))
out += cr(900000117, 900000115)
out += tmp(900000118, 900000115, '0 / 12', 38, 24, 1, 1, 512, CREMA, auto=1, wrap=0)

# barra de progreso
out += go(900000219, 'BarraProgreso', [900000220, 900000221, 900000222])
out += rt(900000220, 900000219, BARRA, [900000224], (0, 0.5), (0, 0.5), (250, 0), (190, 30), (0, 0.5))
out += cr(900000221, 900000219)
out += img(900000222, 900000219, spr('HUD/ProgresoFondo'), typ=1)
out += go(900000223, 'RellenoProgreso', [900000224, 900000225, 900000226])
out += rt(900000224, 900000223, 900000220, [], (0, 0), (1, 1), (0, 0), (-12, -12), (0.5, 0.5))
out += cr(900000225, 900000223)
out += img(900000226, 900000223, spr('HUD/ProgresoRelleno'), typ=3, fill=True)

# logo en el centro (se ve cuando no hay tutorial)
out += go(900000229, 'TextoLogo', [900000230, 900000231, 900000232])
out += rt(900000230, 900000229, BARRA, [], (0.5, 0.5), (0.5, 0.5), (0, 0), (900, 80), (0.5, 0.5))
out += cr(900000231, 900000229)
out += tmp(900000232, 900000229, 'Barista Ca\\xF3tico', 46, 30, 1, 2, 512, CREMA, auto=1, wrap=0)

# cartel del tutorial (centro)
CT = 900000250
out += go(900000249, 'CartelTutorial', [CT, 900000251, 900000252])
out += rt(CT, 900000249, BARRA, [900000254, 900000258, 900000262], (0.5, 0.5), (0.5, 0.5), (0, 0), (960, 96), (0.5, 0.5))
out += cr(900000251, 900000249)
out += img(900000252, 900000249, spr('HUD/CartelPapel'), typ=1)
out += go(900000253, 'ImagenPedido', [900000254, 900000255, 900000256])
out += rt(900000254, 900000253, CT, [], (0, 0.5), (0, 0.5), (14, 0), (76, 76), (0, 0.5))
out += cr(900000255, 900000253)
out += img(900000256, 900000253, '{fileID: 0}', preserve=1)
out += go(900000257, 'TextoTitulo', [900000258, 900000259, 900000260])
out += rt(900000258, 900000257, CT, [], (0, 1), (1, 1), (52, -8), (-136, 28), (0.5, 1))
out += cr(900000259, 900000257)
out += tmp(900000260, 900000257, 'Tutorial', 22, 16, 0, 1, 512, '#8A5A3A', auto=1, wrap=0)
out += go(900000261, 'TextoInstruccion', [900000262, 900000263, 900000264])
out += rt(900000262, 900000261, CT, [], (0, 0), (1, 1), (20, -12), (-200, -36), (0.5, 0.5))
out += cr(900000263, 900000261)
out += tmp(900000264, 900000261, 'Instrucci\\xF3n del paso actual', 34, 22, 1, 1, 512, TINTA, auto=1, wrap=0)

# corazones (a la derecha)
corazones_rt = []
for k in range(3):
    gid = 900000270 + k * 4
    corazones_rt.append(gid + 1)
    out += go(gid, 'Corazon%d' % (k + 1), [gid + 1, gid + 2, gid + 3])
    out += rt(gid + 1, gid, BARRA, [], (1, 0.5), (1, 0.5), (-28 - (2 - k) * 66, 0), (58, 58), (1, 0.5))
    out += cr(gid + 2, gid)
    out += img(gid + 3, gid, spr('HUD/Corazon'), preserve=1)
assert corazones_rt == hijos_barra[6:9]

# propinas (a la izquierda de los corazones): moneda + "$ 0"
out += go(900000290, 'IconoMoneda', [900000291, 900000292, 900000293])
out += rt(900000291, 900000290, BARRA, [], (1, 0.5), (1, 0.5), (-392, 0), (50, 50), (1, 0.5))
out += cr(900000292, 900000290)
out += img(900000293, 900000290, spr('HUD/Moneda'), preserve=1)
out += go(900000294, 'TextoPropinas', [900000295, 900000296, 900000297])
out += rt(900000295, 900000294, BARRA, [], (1, 0.5), (1, 0.5), (-236, -2), (150, 50), (1, 0.5))
out += cr(900000296, 900000294)
out += tmp(900000297, 900000294, '$ 0', 38, 24, 1, 1, 512, CREMA, auto=1, wrap=0)

# ---------------------------------------------- cartel de mensajes (abajo)
out += go(900000123, 'CartelMensaje', [CARTEL, 900000125, 900000126])
out += rt(CARTEL, 900000123, ROOT_RT, [900000128], (0.5, 0), (0.5, 0), (0, 18), (1100, 84), (0.5, 0))
out += cr(900000125, 900000123)
out += img(900000126, 900000123, spr('HUD/CartelPapel'), typ=1)
out += go(900000127, 'TextoMensaje', [900000128, 900000129, 900000130])
out += rt(900000128, 900000127, CARTEL, [], (0, 0), (1, 1), (0, -2), (-48, -20), (0.5, 0.5))
out += cr(900000129, 900000127)
out += tmp(900000130, 900000127, 'Mensaje para el jugador', 32, 20, 0, 2, 512, TINTA, auto=1)

# ---------------------------------------------------- scripts del root
cor_imgs = ''.join('  - {fileID: %d}\n' % (900000270 + k * 4 + 3) for k in range(3))
out += ('--- !u!114 &900000110\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
        '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
        '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
        '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
        '  m_Name:\n  m_EditorClassIdentifier: Assembly-CSharp::HUD\n'
        '  textoPedidos: {fileID: 900000118}\n  textoPerdidos: {fileID: 0}\n'
        '  cartelMensaje: {fileID: 900000123}\n  textoMensaje: {fileID: 900000130}\n'
        '  duracionMensaje: 2.5\n  rellenoProgreso: {fileID: 900000226}\n'
        '  corazones:\n%s  corazonLleno: %s\n  corazonVacio: %s\n  textoPropinas: {fileID: 900000297}\n') % (
    ROOT_GO, HUD_GUID, cor_imgs, spr('HUD/Corazon'), spr('HUD/CorazonVacio'))

# ---------------------------------------------- panel de pausa (Esc)
# Tiene su propio Canvas con "Override Sorting" para dibujarse arriba de todo,
# y un GraphicRaycaster para que sus botones reciban los clics.
MENU_PAUSA = 900000303
siguiente_id = [900000320]


def nid():
    siguiente_id[0] += 1
    return siguiente_id[0]


def boton_pausa(padre, nombre, etiqueta, metodo, y):
    g, r, c, i, b = nid(), nid(), nid(), nid(), nid()
    tg, tr, tc, tt = nid(), nid(), nid(), nid()
    s = go(g, nombre, [r, c, i, b])
    s += rt(r, g, padre, [tr], (0.5, 0.5), (0.5, 0.5), (0, y), (440, 88), (0.5, 0.5))
    s += cr(c, g)
    s += img(i, g, spr('HUD/Boton'), typ=1).replace('m_RaycastTarget: 0', 'm_RaycastTarget: 1')
    s += ('--- !u!114 &%d\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
          '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
          '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
          '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}\n'
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
          '      - m_Target: {fileID: %d}\n        m_TargetAssemblyTypeName: MenuPausa, Assembly-CSharp\n'
          '        m_MethodName: %s\n        m_Mode: 1\n        m_Arguments:\n'
          '          m_ObjectArgument: {fileID: 0}\n'
          '          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine\n'
          '          m_IntArgument: 0\n          m_FloatArgument: 0\n          m_StringArgument: \n'
          '          m_BoolArgument: 0\n        m_CallState: 2\n') % (b, g, i, MENU_PAUSA, metodo)
    s += go(tg, 'Texto', [tr, tc, tt])
    s += rt(tr, tg, r, [], (0, 0), (1, 1), (0, 3), (-20, -12), (0.5, 0.5))
    s += cr(tc, tg)
    s += tmp(tt, tg, etiqueta, 44, 24, 1, 2, 512, CREMA, auto=1, wrap=0)
    return r, s


HOJA_GO, HOJA_RT = nid(), nid()
TIT_GO, TIT_RT, TIT_CR, TIT_TMP = nid(), nid(), nid(), nid()
CTL_GO, CTL_RT, CTL_CR, CTL_TMP = nid(), nid(), nid(), nid()
botones = []
texto_botones = ''
for nombre, etiqueta, metodo, y in (('BotonContinuar', 'Continuar', 'Continuar', 30),
                                     ('BotonReiniciar', 'Reiniciar', 'Reiniciar', -80),
                                     ('BotonMenu', 'Men\\xFA', 'IrAlMenu', -190)):
    r, s = boton_pausa(HOJA_RT, nombre, etiqueta, metodo, y)
    botones.append(r)
    texto_botones += s
HOJA_CR, HOJA_IMG = nid(), nid()
CANVAS_PAUSA = CANVAS.replace('--- !u!223 &900000108', '--- !u!223 &900000312').replace(
    'm_GameObject: {fileID: 900000106}', 'm_GameObject: {fileID: %d}' % PAUSA_GO)
CANVAS_PAUSA = re.sub(r'm_OverrideSorting: \d', 'm_OverrideSorting: 1', CANVAS_PAUSA)
CANVAS_PAUSA = re.sub(r'm_SortingOrder: -?\d+', 'm_SortingOrder: 20', CANVAS_PAUSA)
out += go(PAUSA_GO, 'PanelPausa', [PAUSA_RT, 900000312, 900000313, 900000314, 900000315], active=0)
out += rt(PAUSA_RT, PAUSA_GO, ROOT_RT, [HOJA_RT], (0, 0), (1, 1), (0, 0), (0, 0), (0.5, 0.5))
out += CANVAS_PAUSA
out += ('--- !u!114 &900000313\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
        '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
        '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
        '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: dc42784cf147c0c48a680349fa168899, type: 3}\n'
        '  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.GraphicRaycaster\n'
        '  m_IgnoreReversedGraphics: 1\n  m_BlockingObjects: 0\n  m_BlockingMask:\n'
        '    serializedVersion: 2\n    m_Bits: 4294967295\n') % PAUSA_GO
out += cr(900000314, PAUSA_GO)
out += img(900000315, PAUSA_GO, '{fileID: 0}', color='{r: 0.1, g: 0.05, b: 0.03, a: 0.6}').replace('m_RaycastTarget: 0', 'm_RaycastTarget: 1')
out += go(HOJA_GO, 'Hoja', [HOJA_RT, HOJA_CR, HOJA_IMG])
out += rt(HOJA_RT, HOJA_GO, PAUSA_RT, [TIT_RT, CTL_RT] + botones, (0.5, 0.5), (0.5, 0.5), (0, 0), (900, 640), (0.5, 0.5))
out += cr(HOJA_CR, HOJA_GO)
out += img(HOJA_IMG, HOJA_GO, spr('HUD/CartelPapel'), typ=1)
out += go(TIT_GO, 'Titulo', [TIT_RT, TIT_CR, TIT_TMP])
out += rt(TIT_RT, TIT_GO, HOJA_RT, [], (0.5, 1), (0.5, 1), (0, -30), (600, 80), (0.5, 1))
out += cr(TIT_CR, TIT_GO)
out += tmp(TIT_TMP, TIT_GO, 'Pausa', 64, 30, 1, 2, 512, '#5A3018', auto=0, wrap=0)
out += go(CTL_GO, 'TextoControles', [CTL_RT, CTL_CR, CTL_TMP])
out += rt(CTL_RT, CTL_GO, HOJA_RT, [], (0.5, 1), (0.5, 1), (0, -118), (820, 110), (0.5, 1))
out += cr(CTL_CR, CTL_GO)
out += tmp(CTL_TMP, CTL_GO, 'WASD o flechas: moverte  \\xB7  E: usar  \\xB7  R: sacar de la bandeja\\nZ X C: ingredientes y galletitas  \\xB7  Esc: seguir jugando',
           24, 16, 0, 2, 256, '#8A5A3A', auto=1)
out += texto_botones

# ---------------------------------------------- sonidos y pausa (en el root)
AUDIO = os.path.join(ASSETS, 'Audio')


def clip(nombre):
    g = open(os.path.join(AUDIO, nombre + '.wav.meta')).read().split('guid: ')[1].split()[0]
    return '{fileID: 8300000, guid: %s, type: 3}' % g


out += ('--- !u!82 &900000301\nAudioSource:\n  m_ObjectHideFlags: 0\n'
        '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
        '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
        '  serializedVersion: 4\n  OutputAudioMixerGroup: {fileID: 0}\n  m_audioClip: {fileID: 0}\n'
        '  m_PlayOnAwake: 0\n  m_Volume: 1\n  m_Pitch: 1\n  Loop: 0\n  Mute: 0\n  Spatialize: 0\n'
        '  SpatializePostEffects: 0\n  Priority: 128\n  DopplerLevel: 1\n  MinDistance: 1\n  MaxDistance: 500\n'
        '  Pan2D: 0\n  rolloffMode: 0\n  BypassEffects: 0\n  BypassListenerEffects: 0\n  BypassReverbZones: 0\n') % ROOT_GO
SONIDOS_GUID = open(os.path.join(ASSETS, 'Scripts', 'Sonidos.cs.meta')).read().split('guid: ')[1].split()[0]
PAUSA_GUID = open(os.path.join(ASSETS, 'Scripts', 'MenuPausa.cs.meta')).read().split('guid: ')[1].split()[0]
out += ('--- !u!114 &900000302\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
        '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
        '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
        '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
        '  m_Name:\n  m_EditorClassIdentifier: Assembly-CSharp::Sonidos\n'
        '  sonidoAgarrar: %s\n  sonidoApoyar: %s\n  sonidoMaquinaLista: %s\n  sonidoEntregar: %s\n'
        '  sonidoError: %s\n  sonidoTirar: %s\n  sonidoClienteEnojado: %s\n  sonidoVictoria: %s\n'
        '  sonidoDerrota: %s\n  volumen: 0.7\n') % (
    ROOT_GO, SONIDOS_GUID, clip('Agarrar'), clip('Apoyar'), clip('MaquinaLista'), clip('Entregar'),
    clip('Error'), clip('Tirar'), clip('ClienteEnojado'), clip('Victoria'), clip('Derrota'))
out += ('--- !u!114 &900000303\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
        '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
        '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
        '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
        '  m_Name:\n  m_EditorClassIdentifier: Assembly-CSharp::MenuPausa\n'
        '  panelPausa: {fileID: %d}\n') % (ROOT_GO, PAUSA_GUID, PAUSA_GO)

FLECHA_GUID = guids.setdefault('prefab/Flecha', uuid.uuid4().hex)
out += ('--- !u!114 &900000201\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n'
        '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
        '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %d}\n  m_Enabled: 1\n'
        '  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n'
        '  m_Name:\n  m_EditorClassIdentifier: Assembly-CSharp::GuiaTutorial\n'
        '  cartelTutorial: {fileID: 900000249}\n  textoTitulo: {fileID: 900000260}\n'
        '  textoInstruccion: {fileID: 900000264}\n  imagenPedido: {fileID: 900000256}\n'
        '  prefabFlecha: {fileID: 960100, guid: %s, type: 3}\n') % (
    ROOT_GO, GUIA_GUID, FLECHA_GUID)

open(HUDP, 'w', encoding='utf-8', newline='\n').write(out)

# ------------------------------------------------------------ Flecha.prefab
tacho = open(os.path.join(ASSETS, 'Prefabs', 'Tacho.prefab'), encoding='utf-8').read()
sr = re.search(r'--- !u!212 &950300\n(.*?)(?=\n--- )', tacho, re.S).group(1)
sr = sr.replace('m_GameObject: {fileID: 950100}', 'm_GameObject: {fileID: 960100}')
sr = sub1(r'm_SortingOrder: -?\d+', 'm_SortingOrder: 30', sr)
sr = sub1(r'm_Sprite: \{.*?\}', 'm_Sprite: ' + spr('HUD/Flecha'), sr)
flecha = ('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
          '--- !u!1 &960100\nGameObject:\n  m_ObjectHideFlags: 0\n'
          '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
          '  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n'
          '  - component: {fileID: 960200}\n  - component: {fileID: 960300}\n'
          '  m_Layer: 0\n  m_Name: Flecha\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n'
          '  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n'
          '--- !u!4 &960200\nTransform:\n  m_ObjectHideFlags: 0\n'
          '  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
          '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 960100}\n  serializedVersion: 2\n'
          '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n'
          '  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children: []\n'
          '  m_Father: {fileID: 0}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n'
          '--- !u!212 &960300\n' + sr + '\n')
meta = FLECHAP + '.meta'
if not os.path.exists(meta):
    open(meta, 'w', encoding='utf-8', newline='\n').write(
        'fileFormatVersion: 2\nguid: %s\nPrefabImporter:\n  externalObjects: {}\n  userData: \n'
        '  assetBundleName: \n  assetBundleVariant: \n' % FLECHA_GUID)
open(FLECHAP, 'w', encoding='utf-8', newline='\n').write(flecha)
json.dump(guids, open(os.path.join(DIR, 'guids.json'), 'w'), indent=1)

# chequeo de referencias internas
for path in (HUDP, FLECHAP):
    t = open(path, encoding='utf-8').read()
    defs = re.findall(r'^--- !u!\d+ &(\d+)', t, re.M)
    refs = set(re.findall(r'fileID: (9\d{5,8})\}', t))
    print(os.path.basename(path), 'bloques', len(defs), 'dup', len(defs) - len(set(defs)), 'sin def', refs - set(defs))
