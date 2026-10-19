# Arte provisorio de Barista Caótico, al estilo de la cafetería clásica de Club Penguin:
# contornos marrón oscuro gruesos, colores planos con degradés suaves y brillos.
# Todo lo del mundo se dibuja a 128 px por unidad de Unity.
import math
from svgkit import Dib, OL, OW

U = 128  # px por unidad

# ------------------------------------------------------------ paleta
PARED_OSC, PARED, PARED_CL = '#5e2328', '#7f3034', '#9a4440'
ZOCALO = '#4a1a1e'
PISO, PISO_CL, PISO_OSC = '#efb488', '#f8cba2', '#d99a6c'
SOFA, SOFA_CL, SOFA_OSC = '#c4313a', '#e45c5f', '#8a1d26'
ALFOMBRA, ALFOMBRA_OSC = '#b2353b', '#8c262d'
MESA, MESA_CL = '#3b2a25', '#5c443b'
LAMPARA, LAMPARA_CL = '#3f8d98', '#78bcc3'
PLANTA, PLANTA_CL, PLANTA_OSC = '#4b6e33', '#6f9a46', '#33501f'
MACETA = '#8d9b86'
MOST_TOP, MOST_TOP_CL, MOST_FR, MOST_FR_OSC = '#a98394', '#c6a5b4', '#6c4a5a', '#4d3240'
METAL, METAL_CL, METAL_OSC = '#aab0b6', '#e2e6ea', '#6b737b'
MADERA, MADERA_CL, MADERA_OSC = '#a06c45', '#c48e62', '#6c4428'
YUTE = '#c9a06a'
NARANJA = '#f39a2b'
CREMA = '#fff6e6'
BLANCO = '#ffffff'


def grano(d, cx, cy, s=1.0, rot=0):
    def f():
        d.ell(cx, cy, 13 * s, 17 * s, d.grad('#8a5232', '#5a3018'))
        d.path('M%.1f %.1f Q%.1f %.1f %.1f %.1f' % (cx - 2 * s, cy - 14 * s, cx + 6 * s, cy, cx - 2 * s, cy + 14 * s), 'none', sw=3)
        d.brillo(cx - 5 * s, cy - 6 * s, 3 * s, 5 * s, 0.3)
    d.grupo('rotate(%s %.1f %.1f)' % (rot, cx, cy), f)


# ================================================================ PINGÜINOS
def pinguino(color, color_osc, accesorio=None):
    d = Dib(0.95 * U, 1.3 * U)   # 122 x 166
    # pies
    for x in (42, 80):
        d.ell(x, 156, 19, 8, d.grad('#ffb347', NARANJA))
    # aletas
    d.ell(18, 104, 11, 30, color_osc, extra='transform="rotate(18 18 104)"')
    d.ell(104, 104, 11, 30, color_osc, extra='transform="rotate(-18 104 104)"')
    # cuerpo (huevo)
    cuerpo = d.radial(color, color, color_osc, cx=0.38, cy=0.3, r=0.8)
    d.path('M61 16 C 99 16 108 70 106 112 C 104 146 86 157 61 157 C 36 157 18 146 16 112 C 14 70 23 16 61 16 Z', cuerpo)
    d.brillo(42, 40, 12, 18, 0.25, rot=-20)
    # panza
    d.path('M61 70 C 84 70 92 96 90 120 C 88 142 76 151 61 151 C 46 151 34 142 32 120 C 30 96 38 70 61 70 Z',
           d.grad(BLANCO, BLANCO, '#e4e9ee'), sw=3)
    # cara blanca
    d.path('M61 40 C 70 29 89 33 89 51 C 89 66 77 73 61 73 C 45 73 33 66 33 51 C 33 33 52 29 61 40 Z', BLANCO, sw=3)
    # ojos
    for x, px in ((49, 52), (73, 70)):
        d.ell(x, 50, 8.5, 11.5, BLANCO, sw=3)
        d.ell(px, 52, 4.5, 6.5, '#1b1b1b', stroke=False)
        d.circ(px + 1.5, 48.5, 1.8, BLANCO, stroke=False)
    # pico
    d.path('M50 63 Q61 56 72 63 Q67 74 61 75 Q55 74 50 63 Z', d.grad('#ffb347', NARANJA), sw=3)
    d.path('M53 66 Q61 70 69 66', 'none', sw=2, op=0.6)
    if accesorio:
        accesorio(d)
    return d


def acc_barista(d):
    # delantal verde con bolsillo y gorrito blanco
    d.path('M38 92 L84 92 L88 140 Q61 150 34 140 Z', d.grad('#4fae84', '#2f8461'), sw=4)
    d.path('M42 92 Q61 74 80 92', 'none', stroke='#2f8461', sw=5)
    d.rect(51, 110, 20, 14, '#2f8461', rx=3, sw=3)
    d.path('M56 112 L66 112', 'none', stroke=CREMA, sw=2)
    d.path('M32 26 Q61 -4 90 26 L86 34 Q61 22 36 34 Z', d.grad(BLANCO, '#dfe4e8'), sw=4)
    d.rect(58, 6, 6, 8, '#2f8461', rx=2, sw=2)


def acc_gorra(d):
    d.path('M30 30 Q61 2 92 30 Z', d.grad('#4d7fd6', '#2f5bb0'), sw=4)
    d.path('M86 28 Q108 30 110 38 Q96 40 84 34 Z', '#2f5bb0', sw=4)
    d.circ(61, 10, 4, '#2f5bb0', sw=3)


def acc_mono(d):
    d.path('M70 16 Q84 2 92 12 Q88 22 74 22 Z', d.grad('#ff8fc4', '#e2559c'), sw=4)
    d.path('M70 16 Q58 2 50 12 Q54 22 68 22 Z', d.grad('#ff8fc4', '#e2559c'), sw=4)
    d.circ(70, 18, 5, '#e2559c', sw=3)


def acc_anteojos_gorro(d):
    d.path('M28 32 Q61 -6 94 32 Q61 22 28 32 Z', d.grad('#ff9d4d', '#e2702c'), sw=4)
    d.rect(26, 26, 70, 9, '#e2702c', rx=4, sw=4)
    d.circ(61, 4, 7, '#ffd36b', sw=3)
    for x in (49, 73):
        d.ell(x, 51, 12, 12, 'none', sw=4)
    d.line(61, 50, 61, 50, sw=4)
    d.path('M59 50 Q61 47 63 50', 'none', sw=3)


def barista():
    return pinguino('#3c62c7', '#24418f', acc_barista)


def cliente1():
    return pinguino('#d8403f', '#a3282c', acc_gorra)


def cliente2():
    return pinguino('#4caa52', '#2f7c36', acc_mono)


def cliente3():
    return pinguino('#8a5ad0', '#5f3a9e', acc_anteojos_gorro)


# ================================================================ MÁQUINAS
MW, MH = 2.6 * U, 2.4 * U    # 333 x 307  (pivote arriba al centro)
TOPE = 0.42 * MH              # base de los aparatos en el dibujo original (y px)
TOPE_NUEVO = 0.5 * MH         # donde empieza la tapa del mueble


def mueble(d, tope=None, x0=0, ancho=None):
    tope = TOPE_NUEVO if tope is None else tope
    ancho = ancho or d.w
    # tapa
    d.rect(x0 + 3, tope, ancho - 6, 30, d.grad(MADERA_CL, MADERA), rx=6)
    # frente con dos filas de cajones
    d.rect(x0 + 6, tope + 26, ancho - 12, d.h - tope - 32, d.grad(MADERA, MADERA_OSC), rx=6)
    n = 2 if ancho > 260 else 1
    w = (ancho - 36 - (n - 1) * 12) / n
    alto = (d.h - tope - 32 - 42) / 2
    for i in range(n):
        x = x0 + 18 + i * (w + 12)
        for k in range(2):
            y = tope + 40 + k * (alto + 10)
            d.rect(x, y, w, alto, d.grad(MADERA_CL, MADERA), rx=6, sw=4)
            d.rect(x + w / 2 - 16, y + alto / 2 - 4, 32, 9, METAL_OSC, rx=4, sw=3)
    d.brillo(x0 + ancho / 2, tope + 8, ancho * 0.4, 5, 0.3)


def maquina(dibujar, escala=1.18):
    # Mueble abajo y el aparato arriba, agrandado: el aparato se dibuja con las medidas
    # viejas (base en TOPE) y lo escalamos para que apoye en la tapa nueva.
    d = Dib(MW, MH)
    mueble(d)
    d.grupo('translate(%.1f %.1f) scale(%.2f) translate(%.1f %.1f)' % (MW / 2, TOPE_NUEVO + 4, escala, -MW / 2, -TOPE - 4),
            lambda: dibujar(d))
    return d


def taza(d, cx, by, color='#f4f1ec', s=1.0, cafe=True):
    w, h = 26 * s, 22 * s
    d.path('M%.1f %.1f L%.1f %.1f Q%.1f %.1f %.1f %.1f L%.1f %.1f Q%.1f %.1f %.1f %.1f Z' % (
        cx - w / 2, by - h, cx + w / 2, by - h, cx + w / 2, by, cx + w / 4, by, cx - w / 4, by, cx - w / 2, by, cx - w / 2, by - h),
        d.grad(color, '#cfc8bf'), sw=3)
    d.path('M%.1f %.1f q%.1f 0 %.1f %.1f q0 %.1f %.1f %.1f' % (cx + w / 2 - 2, by - h + 5, 9 * s, 9 * s, 8 * s, 8 * s, -9 * s, 8 * s), 'none', sw=3)
    if cafe:
        d.ell(cx, by - h + 1, w / 2 - 2, 3.5 * s, '#5a3018', sw=2)


def cafetera():
    return maquina(_cafetera)


def _cafetera(d):
    # cuerpo cromado de la espresso
    d.rect(52, 26, 230, 112, d.grad(METAL_CL, METAL, METAL_OSC), rx=18)
    d.rect(52, 26, 230, 22, d.grad('#f4f6f8', METAL_CL), rx=12)
    d.brillo(110, 60, 40, 10, 0.5)
    # franja roja y manómetro
    d.rect(52, 84, 230, 16, d.grad('#e2585a', '#a92e2e'), rx=4, sw=4)
    d.circ(167, 62, 17, d.radial(BLANCO, '#e8ecef'), sw=4)
    d.line(167, 62, 177, 54, color='#a92e2e', sw=3)
    for x in (105, 229):
        d.circ(x, 62, 9, METAL_OSC, sw=3)
    # grupos y tazas
    for gx in (110, 224):
        d.rect(gx - 22, 100, 44, 16, METAL_OSC, rx=5, sw=4)
        d.rect(gx - 8, 116, 16, 10, '#3b3b3b', rx=3, sw=3)
        taza(d, gx, TOPE + 4, s=1.2)
    # vaporizador
    d.path('M290 70 L300 70 L300 128 L292 132', 'none', sw=6)
    # tazas arriba
    for i, tx in enumerate((96, 136, 176, 216)):
        taza(d, tx, 26, color=['#f4f1ec', '#ef8f8f', '#f4f1ec', '#8fc3e6'][i], s=0.9, cafe=False)
    return d


def moledora():
    return maquina(_moledora)


def _moledora(d):
    # tolva de vidrio con granos
    d.path('M118 12 L214 12 L196 74 L136 74 Z', d.grad('#dceaf1', '#a9c6d6'), sw=5)
    d.path('M126 34 L206 34 L196 72 L136 72 Z', d.grad('#7a4a2a', '#4f2a15'), stroke=False)
    for x, y, r in [(140, 44, 10), (162, 40, 25), (184, 46, -15), (150, 60, 40), (176, 62, 5), (198, 42, 60)]:
        grano(d, x, y, 0.45, r)
    d.brillo(132, 30, 5, 16, 0.6)
    d.rect(110, 2, 112, 16, d.grad('#e2585a', '#a92e2e'), rx=6)
    # cuerpo rojo
    d.path('M120 74 L212 74 Q226 74 226 90 L226 138 L106 138 L106 90 Q106 74 120 74 Z', d.grad('#e2585a', '#c4313a', '#8a1d26'))
    d.brillo(130, 96, 10, 24, 0.3)
    d.rect(142, 92, 48, 24, d.grad(CREMA, '#e9dccb'), rx=6, sw=4)
    d.text(166, 110, 'CAFÉ', 15, '#8a1d26')
    # manija
    d.path('M226 100 L262 92', 'none', sw=7)
    d.circ(266, 90, 9, '#3b3b3b', sw=4)
    # boca y bolsita
    d.rect(152, 138, 28, 12, METAL_OSC, rx=3, sw=4)
    # bolsa de granos al costado
    d.path('M34 TOPE Q30 90 48 80 L92 80 Q108 90 104 TOPE Z'.replace('TOPE', '%.0f' % (TOPE + 2)), d.grad('#dcb47e', YUTE, '#a87f4c'), sw=5)
    grano(d, 69, 108, 0.8)
    # tazón de café molido
    d.path('M258 %.0f Q258 112 268 106 L304 106 Q314 112 314 %.0f Z' % (TOPE + 2, TOPE + 2), d.grad('#8fc3e6', '#5b95c4'), sw=4)
    d.ell(286, 106, 18, 5, '#5a3018', sw=3)
    return d


def chocolatera():
    return maquina(_chocolatera)


def _chocolatera(d):
    # hornalla
    d.rect(70, 112, 194, 22, d.grad('#5c6870', '#3b4248'), rx=6)
    for x in (98, 236):
        d.circ(x, 123, 6, '#e2585a', sw=3)
    # olla de cobre
    olla = d.grad('#f6a66a', '#d9773b', '#9c4c22')
    d.path('M86 40 L248 40 L240 112 Q167 124 94 112 Z', olla)
    d.ell(167, 40, 82, 16, '#c96a33')
    d.ell(167, 40, 70, 11, d.grad('#8f5a36', '#5a3018'), sw=3)
    d.brillo(106, 72, 8, 24, 0.35)
    d.path('M150 38 Q160 32 172 38 Q182 44 190 38', 'none', stroke='#a8774f', sw=3)
    # manijas
    for x, s in ((80, -1), (254, 1)):
        d.path('M%d 56 q%d -2 %d 12 q%d 6 %d 0' % (x, s * 18, s * 14, -s * 4, -s * 14), 'none', sw=7)
    # cucharón
    d.path('M196 30 L226 -4', 'none', stroke=MADERA_OSC, sw=9)
    d.path('M196 30 L226 -4', 'none', stroke=MADERA_CL, sw=4)
    # vapor
    for x in (130, 168, 206):
        d.path('M%d 22 q-8 -8 0 -16 q8 -8 0 -16' % x, 'none', stroke=BLANCO, sw=4, op=0.8)
    # tazones al costado
    for x, c in ((34, '#8fc3e6'), (292, '#ef8f8f')):
        d.path('M%d %.0f Q%d 112 %d 106 L%d 106 Q%d 112 %d %.0f Z' % (x - 22, TOPE + 2, x - 22, x - 12, x + 12, x + 22, x + 22, TOPE + 2), d.grad(c, '#5b95c4' if c[1] == '8' else '#c45c5c'), sw=4)
    return d


def repisa():
    d = Dib(1.8 * U, MH)
    mueble(d)
    T = TOPE_NUEVO
    # estante de madera de dos lugares sobre la mesada
    x0, x1 = 22, d.w - 22
    d.rect(x0, 30, x1 - x0, T - 26, d.grad(MADERA_CL, MADERA), rx=10)
    for cx in (d.w / 2 - 0.45 * U, d.w / 2 + 0.45 * U):
        d.rect(cx - 46, 44, 92, T - 54, d.grad('#7a4e30', '#5a361e'), rx=8, sw=4)
        d.ell(cx, T - 16, 34, 7, d.grad(CREMA, '#e6d6bf'), sw=3)
    d.rect(x0 - 8, 22, x1 - x0 + 16, 18, d.grad(MADERA_CL, MADERA_OSC), rx=6)
    d.brillo(d.w / 2, 28, 70, 4, 0.35)
    return d


# =========================================================== ESTANTES
EW, EH = 2.3 * U, 3.9 * U     # 294 x 499


def cartel_tecla(d, cx, cy, letra):
    d.circ(cx, cy, 24, d.radial('#ffe39a', '#f2c14e', cx=0.4, cy=0.35), sw=5)
    d.text(cx, cy + 10, letra, 30, OL, family='Arial Black, Verdana, sans-serif')


def ingredientes():
    d = Dib(EW, EH)
    d.rect(4, 4, EW - 8, EH - 8, d.grad(MADERA_CL, MADERA, MADERA_OSC, vertical=False), rx=14)
    d.rect(4, 4, EW - 8, 34, d.grad('#d6a374', MADERA), rx=12)
    alto = (EH - 60) / 3
    for i in range(3):
        y = 42 + i * alto
        d.rect(18, y, EW - 36, alto - 16, d.grad('#5a361e', '#3d2414'), rx=8, sw=4)
        d.rect(12, y + alto - 18, EW - 24, 14, d.grad(MADERA_CL, MADERA), rx=5, sw=4)
    # Z: bolsas de granos
    for i, x in enumerate((96, 160, 222)):
        y0 = 42 + alto - 22
        d.path('M%d %.0f Q%d %.0f %d %.0f L%d %.0f Q%d %.0f %d %.0f Z' % (x - 30, y0, x - 34, y0 - 70, x - 18, y0 - 86, x + 18, y0 - 86, x + 34, y0 - 70, x + 30, y0),
               d.grad('#e2bd88', YUTE, '#a87f4c'), sw=4)
        d.path('M%d %.0f L%d %.0f' % (x - 16, y0 - 86, x + 16, y0 - 86), 'none', sw=4)
        grano(d, x, y0 - 40, 0.75, 15)
    # X: botellas de leche en un cajón
    y0 = 42 + 2 * alto - 22
    for x in (90, 130, 170, 210, 250):
        d.path('M%d %.0f L%d %.0f L%d %.0f L%d %.0f L%d %.0f L%d %.0f Z' % (x - 14, y0, x - 14, y0 - 60, x - 7, y0 - 78, x + 7, y0 - 78, x + 14, y0 - 60, x + 14, y0),
               d.grad(BLANCO, '#e4e9ee'), sw=4)
        d.rect(x - 14, y0 - 46, 28, 18, '#4f8fd6', sw=3)
        d.rect(x - 8, y0 - 86, 16, 10, '#d8403f', rx=3, sw=3)
    d.rect(70, y0 - 24, 200, 24, d.grad('#d6a374', MADERA), rx=4, sw=4)
    # C: tabletas de chocolate
    y0 = 42 + 3 * alto - 22
    for i, x in enumerate((100, 164, 228)):
        y = y0 - 70 + (i % 2) * 6
        d.rect(x - 28, y, 56, 70, d.grad('#8a5232', '#5a3018'), rx=4, sw=4)
        for j in range(2):
            for k in range(2):
                d.rect(x - 22 + j * 23, y + 6 + k * 20, 20, 17, '#7a4a2a', rx=2, sw=2)
        d.rect(x - 28, y + 46, 56, 24, d.grad('#e2585a', '#a92e2e'), sw=4)
        d.rect(x - 28, y + 52, 56, 6, '#f2c14e', stroke=False)
    for i, l in enumerate('ZXC'):
        cartel_tecla(d, 44, 42 + i * alto + 34, l)
    d.text(EW / 2, 30, 'INGREDIENTES', 20, CREMA, stroke=OL, sw=4)
    return d


def galletitas():
    d = Dib(EW, EH)
    d.rect(4, 4, EW - 8, EH - 8, d.grad('#f2a7b0', '#d97a8c', '#b3566a', vertical=False), rx=14)
    # toldito
    for i in range(7):
        x = 4 + i * (EW - 8) / 7
        d.rect(x, 4, (EW - 8) / 7, 40, '#e2585a' if i % 2 == 0 else CREMA, stroke=False)
    d.rect(4, 4, EW - 8, 40, 'none', rx=12)
    alto = (EH - 66) / 3
    masas = [('#c4313a', BLANCO), ('#e9c38a', '#5a3018'), ('#6b3e22', '#2e170b')]
    for i in range(3):
        y = 50 + i * alto
        d.rect(18, y, EW - 36, alto - 16, d.grad('#fff3f4', '#f6d3d8'), rx=8, sw=4)
        d.rect(12, y + alto - 18, EW - 24, 14, d.grad(BLANCO, '#e9c9ce'), rx=5, sw=4)
        masa, chip = masas[i]
        for j, x in enumerate((104, 160, 216, 128, 190)):
            cy = y + alto - 46 - (24 if j >= 3 else 0)
            d.ell(x, cy, 28, 18, d.radial(masa, masa, '#000000' if False else masa, cx=0.4, cy=0.35), sw=4)
            for a in range(5):
                ang = a * 1.3 + j
                d.ell(x + 15 * math.cos(ang), cy + 8 * math.sin(ang), 4, 3, chip, stroke=False)
            d.brillo(x - 8, cy - 7, 9, 4, 0.3)
        d.brillo(40, y + 30, 6, 30, 0.6)
    for i, l in enumerate('ZXC'):
        cartel_tecla(d, 44, 50 + i * alto + 34, l)
    d.text(EW / 2, 32, 'GALLETITAS', 20, CREMA, stroke=OL, sw=4)
    return d


# ===================================================== MOSTRADOR Y BANDEJA
def mostrador():
    # 1.5 de alto: los primeros 0.4 son aire (para lo que está apoyado arriba),
    # después la tapa (0.45) y el frente (0.65).
    W, H = 17.78 * U, 1.5 * U
    d = Dib(W, H)
    aire = 0.4 * U
    top = aire + 0.45 * U
    d.rect(-10, aire + 4, W + 20, top - aire, d.grad(MOST_TOP_CL, MOST_TOP), rx=0)
    d.brillo(W / 2, aire + 14, W * 0.48, 5, 0.35)
    d.rect(-10, top, W + 20, H - top - 4, d.grad(MOST_FR, MOST_FR_OSC), rx=0)
    n = 22
    pw = W / n
    for i in range(n):
        d.rect(i * pw + 8, top + 12, pw - 16, H - top - 40, d.grad('#5c3d4c', '#432a37'), rx=6, sw=3)
    d.rect(-10, H - 24, W + 20, 20, d.grad('#9ea2a7', '#6b737b'), rx=0, sw=4)

    def wx(x):
        return (x + 8.89) * U
    apoyo = aire + 0.3 * U   # altura donde "apoyan" las cosas sobre la tapa
    # caja registradora
    x = wx(7.9)
    d.rect(x - 64, apoyo - 50, 128, 54, d.grad('#5ab4a6', '#3a8a7e'), rx=8)
    for k in range(4):
        for j in range(2):
            d.rect(x - 50 + k * 26, apoyo - 40 + j * 18, 18, 12, CREMA, rx=3, sw=2)
    d.rect(x - 46, apoyo - 92, 74, 44, d.grad('#3a8a7e', '#2a6a60'), rx=6)
    d.rect(x - 38, apoyo - 86, 58, 22, '#173a35', rx=3, sw=3)
    d.text(x - 9, apoyo - 70, '$ 0.00', 13, '#8ff0c0', family='Courier New, monospace')
    # tazas apiladas
    for xx in (wx(-2.9), wx(2.9)):
        for k in range(3):
            taza(d, xx - 30 + k * 30, apoyo, color=['#f4f1ec', '#8fc3e6', '#ef8f8f'][k], s=1.1, cafe=False)
    # frasco con medialunas
    x = wx(-7.6)
    d.path('M%.0f %.0f Q%.0f %.0f %.0f %.0f L%.0f %.0f Q%.0f %.0f %.0f %.0f Z' % (x - 50, apoyo, x - 56, apoyo - 44, x - 32, apoyo - 62, x + 32, apoyo - 62, x + 56, apoyo - 44, x + 50, apoyo),
           d.grad('#6fae5a', '#4b8a3a'), sw=5)
    for dx, dy in ((-24, -58), (2, -66), (26, -56)):
        d.path('M%.0f %.0f q16 -18 32 0 q-16 11 -32 0 Z' % (x + dx - 16, apoyo + dy), d.grad('#f2c14e', '#d69a2e'), sw=3)
    return d


def bandeja():
    d = Dib(1.5 * U, 0.6 * U)
    d.rect(6, 12, d.w - 12, d.h - 20, d.grad('#e2585a', '#c4313a', '#8a1d26'), rx=22)
    d.rect(22, 20, d.w - 44, d.h - 38, d.grad('#d4403f', '#b02a30'), rx=14, sw=3)
    d.brillo(d.w / 2, 22, d.w * 0.35, 4, 0.4)
    return d


def tacho():
    d = Dib(0.9 * U, 1.0 * U)
    w, h = d.w, d.h
    d.path('M18 32 L%d 32 L%d %d L26 %d Z' % (w - 18, w - 26, h - 6, h - 6), d.grad(METAL_CL, METAL, METAL_OSC, vertical=False))
    for x in (38, w / 2, w - 38):
        d.line(x, 42, x, h - 16, color=METAL_OSC, sw=4)
    d.rect(10, 20, w - 20, 16, d.grad('#c7ccd1', '#8a939b'), rx=6)
    d.rect(w / 2 - 14, 8, 28, 14, METAL_OSC, rx=6, sw=4)
    d.brillo(30, 60, 5, 20, 0.5)
    return d


# =================================================================== FONDO
def fondo():
    W, H = 17.78 * U, 10 * U
    d = Dib(W, H)

    def X(x):
        return (x + 8.89) * U

    def Y(y):
        return (5 - y) * U

    # --- piso de la cocina (baldosas) y del salón
    d.rect(-10, Y(0.2), W + 20, H, '#e7c6a4', stroke=False)
    t = 0.8 * U
    filas = int((H - Y(0.2)) / t) + 2
    for j in range(filas):
        for i in range(int(W / t) + 2):
            if (i + j) % 2 == 0:
                d.rect(i * t - 30, Y(0.2) + j * t, t, t, '#dcb18c', stroke=False)
    d.rect(-10, Y(2.75), W + 20, Y(0) - Y(2.75) + 10, d.grad(PISO, PISO_CL, PISO), stroke=False)
    for cx, cy, rx, ry in [(-6, 1.6, 2.2, 0.6), (0.5, 2.1, 2.8, 0.45), (5.5, 1.2, 2.4, 0.5), (-2, 0.6, 2.0, 0.35)]:
        d.ell(X(cx), Y(cy), rx * U, ry * U, PISO_CL, stroke=False, op=0.7)
    # --- pared del fondo
    d.rect(-10, -10, W + 20, Y(2.75) + 10, d.grad(PARED_OSC, PARED, PARED_CL), stroke=False)
    d.rect(-10, Y(3.95), W + 20, 10, '#4a1a1e', stroke=False, op=0.5)
    d.rect(-10, Y(2.95), W + 20, 0.2 * U, d.grad('#6a252b', ZOCALO), stroke=OL, sw=4)
    d.line(-10, Y(2.75), W + 10, Y(2.75), sw=5)
    # --- ventana con "CAFÉ" al revés
    x0, x1, ya, yb = X(-5.0), X(-1.6), Y(4.15), Y(3.0)
    d.rect(x0, ya, x1 - x0, yb - ya, d.grad('#d9d6d0', '#a9a49c'), rx=10, sw=6)
    d.rect(x0 + 16, ya + 16, x1 - x0 - 32, yb - ya - 30, d.grad('#bcd7e6', '#8fb6cd'), rx=4, sw=4)
    d.path('M%.0f %.0f L%.0f %.0f L%.0f %.0f L%.0f %.0f Z' % (x0 + 60, yb - 14, x0 + 160, ya + 16, x0 + 220, ya + 16, x0 + 120, yb - 14), BLANCO, stroke=False, op=0.25)
    d.grupo('translate(%.0f 0) scale(-1 1)' % (x0 + x1), lambda: d.text((x0 + x1) / 2, yb - 40, 'CAFÉ', 70, BLANCO, stroke='#6b8ea6', sw=3, extra='opacity="0.85"'))
    d.rect(x0 - 6, yb - 6, x1 - x0 + 12, 18, d.grad('#e2ded8', '#a9a49c'), rx=4, sw=4)
    # --- puerta con cartel ABIERTO
    x0, x1, ya, yb = X(-0.65), X(0.65), Y(4.75), Y(2.78)
    d.rect(x0 - 14, ya - 14, x1 - x0 + 28, yb - ya + 14, d.grad('#b5b1aa', '#87837c'), rx=6, sw=5)
    d.rect(x0, ya, x1 - x0, yb - ya, d.grad('#e6e1d7', '#c9c2b6'), rx=4, sw=5)
    d.rect(x0 + 22, ya + 26, x1 - x0 - 44, 70, d.grad('#bcd7e6', '#8fb6cd'), rx=4, sw=4)
    for k in range(5):
        d.line(x0 + 26, ya + 34 + k * 12, x1 - 26, ya + 34 + k * 12, color='#d8cfc0', sw=4)
    d.rect(x1 - 30, ya + 120, 12, 34, d.grad('#ffe39a', '#c9962e'), rx=4, sw=4)
    d.rect(x0 + 30, ya + 104, x1 - x0 - 72, 34, d.grad('#57b25f', '#3a8a42'), rx=6, sw=4)
    d.text(x0 + 30 + (x1 - x0 - 72) / 2, ya + 127, 'ABIERTO', 17, BLANCO)
    d.rect(x0 + 2, yb - 16, x1 - x0 - 4, 14, '#6a5a7a', rx=3, sw=3)
    # --- pizarra con el menú
    x0, x1, ya, yb = X(2.3), X(4.3), Y(4.1), Y(2.95)
    d.rect(x0, ya, x1 - x0, yb - ya, d.grad('#8a5a36', '#5c3a22'), rx=8, sw=6)
    d.rect(x0 + 14, ya + 14, x1 - x0 - 28, yb - ya - 28, d.grad('#3a3735', '#262423'), rx=4, sw=4)
    tiza = '#efeee6'
    d.text((x0 + x1) / 2, ya + 58, 'Café', 30, tiza, family='Comic Sans MS, cursive')
    d.text((x0 + x1) / 2, ya + 92, '& Medialunas', 22, tiza, family='Comic Sans MS, cursive')
    for k in range(2):
        d.line(x0 + 40, ya + 116 + k * 22, x1 - 40, ya + 116 + k * 22, color=tiza, sw=3, op=0.7)
    d.rect(x0 + 60, yb - 12, 50, 10, '#f3efe3', rx=3, sw=2)
    # --- cuadros
    for x, c1, c2 in ((-7.0, '#e8964f', '#c46a2c'), (7.0, '#7fae6a', '#4f7c3d'), (1.35, '#e8c25a', '#c9962e'), (-1.35, '#8fb6e6', '#4f7cc6')):
        d.rect(X(x) - 34, Y(4.05), 68, 84, d.grad('#d9a066', '#93592f'), rx=4, sw=5)
        d.rect(X(x) - 22, Y(4.05) + 12, 44, 60, d.grad(c1, c2), rx=2, sw=3)
        d.circ(X(x), Y(4.05) + 40, 12, c2, sw=3)
    # --- lámparas colgantes sobre cada puesto
    for x in (-5.6, 5.6):
        d.line(X(x), -10, X(x), Y(3.55), color='#2a2a2a', sw=4)
        d.path('M%.0f %.0f L%.0f %.0f L%.0f %.0f L%.0f %.0f Z' % (X(x) - 16, Y(3.62), X(x) + 16, Y(3.62), X(x) + 44, Y(3.3), X(x) - 44, Y(3.3)),
               d.grad(LAMPARA_CL, LAMPARA))
        d.ell(X(x), Y(3.3) + 6, 20, 9, d.radial('#fff8d0', '#ffe07a'), sw=3)
        d.ell(X(x), Y(2.4), 1.1 * U, 0.6 * U, '#fff3c0', stroke=False, op=0.10)
    # --- alfombras y sillones
    def sofa(cx, by, ancho):
        x0 = X(cx) - ancho * U / 2
        x1 = X(cx) + ancho * U / 2
        y0 = Y(by)
        h = 1.15 * U
        d.ell(X(cx), y0 - 6, ancho * U * 0.62, 0.32 * U, d.grad(ALFOMBRA, ALFOMBRA_OSC), sw=4)
        # respaldo
        d.path('M%.0f %.0f Q%.0f %.0f %.0f %.0f L%.0f %.0f Q%.0f %.0f %.0f %.0f Z' % (
            x0 + 20, y0 - 40, x0 + 10, y0 - h, x0 + 60, y0 - h, x1 - 60, y0 - h, x1 - 10, y0 - h, x1 - 20, y0 - 40),
            d.grad(SOFA_CL, SOFA, SOFA_OSC))
        nb = max(2, int(ancho / 0.8))
        for k in range(1, nb):
            xx = x0 + k * (x1 - x0) / nb
            d.line(xx, y0 - h + 14, xx, y0 - 44, color=SOFA_OSC, sw=3)
        for k in range(nb * 2):
            d.circ(x0 + (k + 0.5) * (x1 - x0) / (nb * 2), y0 - h * 0.62, 3, SOFA_OSC, stroke=False)
        # asiento
        d.rect(x0 + 6, y0 - 58, x1 - x0 - 12, 48, d.grad(SOFA_CL, SOFA), rx=22)
        # apoyabrazos
        for xa in (x0 - 8, x1 - 34):
            d.rect(xa, y0 - h * 0.66, 42, h * 0.66 - 6, d.grad(SOFA_CL, SOFA, SOFA_OSC, vertical=False), rx=20)
        d.brillo(X(cx) - ancho * U * 0.2, y0 - h * 0.8, ancho * U * 0.18, 8, 0.3)
        for xp in (x0 + 18, x1 - 30):
            d.rect(xp, y0 - 14, 12, 16, '#2a2a2a', rx=3, sw=3)

    sofa(-3.3, 1.95, 2.6)
    sofa(3.3, 1.95, 2.4)
    # --- planta alta (ciprés en maceta)
    def planta(cx, by, alto):
        x, y = X(cx), Y(by)
        d.rect(x - 34, y - 70, 68, 70, d.grad('#a9b7a2', MACETA, '#6b7a66', vertical=False), rx=10)
        d.rect(x - 40, y - 78, 80, 18, d.grad('#c3d0bc', MACETA), rx=6, sw=4)
        d.line(x, y - 78, x, y - 110, color='#5a3a22', sw=8)
        hojas = d.grad(PLANTA_CL, PLANTA, PLANTA_OSC, vertical=False)
        d.path('M%.0f %.0f Q%.0f %.0f %.0f %.0f Q%.0f %.0f %.0f %.0f Z' % (x - 46, y - 100, x - 60, y - alto * 0.55, x, y - alto, x + 60, y - alto * 0.55, x + 46, y - 100), hojas)
        for k in range(6):
            yy = y - 120 - k * (alto - 140) / 6
            d.path('M%.0f %.0f q20 -10 40 0' % (x - 24 - (k % 2) * 8, yy), 'none', stroke=PLANTA_OSC, sw=3, op=0.7)
        d.brillo(x - 18, y - alto * 0.6, 10, alto * 0.2, 0.2)

    planta(-8.45, 2.15, 2.0 * U)
    planta(8.45, 2.15, 2.0 * U)
    # --- mesitas con puffs
    def puff(cx, cy, color, color_osc):
        d.ell(X(cx), Y(cy), 0.42 * U, 0.3 * U, d.radial(color, color, color_osc, cx=0.4, cy=0.3))
        d.path('M%.0f %.0f q%.0f -14 %.0f 0' % (X(cx) - 18, Y(cy) - 10, 18, 36), 'none', stroke=color_osc, sw=3)
        d.brillo(X(cx) - 14, Y(cy) - 12, 14, 6, 0.3)

    def mesita(cx, cy, con_planta=False):
        x, y = X(cx), Y(cy)
        d.ell(x, y + 34, 0.55 * U, 0.14 * U, '#000000', stroke=False, op=0.15)
        d.path('M%.0f %.0f L%.0f %.0f M%.0f %.0f L%.0f %.0f' % (x - 30, y, x - 40, y + 40, x + 30, y, x + 40, y + 40), 'none', sw=7)
        d.ell(x, y, 0.55 * U, 0.2 * U, d.grad(MESA_CL, MESA))
        d.brillo(x - 20, y - 6, 26, 5, 0.25)
        if con_planta:
            d.rect(x - 12, y - 30, 24, 24, d.grad('#a9b7a2', MACETA), rx=5, sw=3)
            d.path('M%.0f %.0f q-10 -26 -2 -40 M%.0f %.0f q0 -30 6 -46 M%.0f %.0f q10 -22 18 -30' % (x - 4, y - 30, x, y - 30, x + 4, y - 30), 'none', stroke=PLANTA, sw=6)
        else:
            taza(d, x - 22, y - 6, s=0.9)
            taza(d, x + 20, y - 2, color='#8fc3e6', s=0.9)

    for cx, cy, planta_ in ((-7.6, 0.95, False), (-2.8, 0.85, True), (2.8, 0.85, False), (7.6, 0.95, True)):
        puff(cx - 0.75, cy + 0.15, SOFA, SOFA_OSC)
        puff(cx + 0.75, cy + 0.15, SOFA, SOFA_OSC)
        mesita(cx, cy, planta_)
        puff(cx, cy - 0.45, SOFA, SOFA_OSC) if cx in (-7.6, 7.6) else None
    # --- mesada del fondo de la cocina (detrás de las máquinas)
    d.rect(-10, Y(-3.6), W + 20, 0.3 * U, d.grad(MADERA_CL, MADERA), stroke=OL, sw=4)
    d.rect(-10, Y(-3.9), W + 20, H - Y(-3.9) + 10, d.grad(MADERA, MADERA_OSC), stroke=OL, sw=4)
    # --- sombras suaves de la cocina
    d.rect(-10, Y(-1.1), W + 20, 30, '#000000', stroke=False, op=0.12)
    return d


# ======================================================== ÍCONOS (globos)
def icono(fn):
    d = Dib(128, 128)
    fn(d)
    return d


def ic_cafe(d):
    for x, y, r in ((44, 52, -25), (82, 44, 20), (60, 86, 10), (94, 84, -40)):
        grano(d, x, y, 1.15, r)


def ic_leche(d):
    d.path('M40 120 L40 46 L52 26 L76 26 L88 46 L88 120 Z', d.grad(BLANCO, '#dfe5ea'), sw=5)
    d.rect(40, 62, 48, 30, d.grad('#6fa8e6', '#3f7cc6'), sw=4)
    d.text(64, 84, 'LECHE', 11, BLANCO)
    d.rect(50, 14, 28, 14, '#d8403f', rx=4, sw=4)
    d.brillo(50, 90, 4, 22, 0.6)


def ic_chocolate(d):
    d.rect(22, 30, 84, 72, d.grad('#8a5232', '#5a3018'), rx=6)
    for i in range(3):
        for j in range(2):
            d.rect(29 + i * 25, 36 + j * 22, 21, 18, '#7a4a2a', rx=3, sw=3)
    d.path('M22 80 L106 80 L106 102 L22 102 Z', d.grad('#e2585a', '#a92e2e'), sw=5)
    d.rect(22, 86, 84, 7, '#f2c14e', stroke=False)


def ic_cafe_molido(d):
    d.path('M30 122 Q24 70 40 40 L88 40 Q104 70 98 122 Z', d.grad('#d9b07c', '#b08350', '#8a6236'), sw=5)
    d.path('M40 40 L46 24 L82 24 L88 40', d.grad('#d9b07c', '#b08350'), sw=5)
    d.ell(64, 32, 22, 6, '#4f2a15', sw=3)
    d.circ(64, 82, 20, CREMA, sw=4)
    grano(d, 64, 82, 0.7, 20)


def taza_grande(d, liquido, color_taza=('#ffffff', '#d9d4ce'), espuma=None, vapor=True):
    d.ell(64, 112, 46, 10, d.grad('#ffffff', '#cfc8bf'), sw=4)
    d.path('M24 50 L104 50 Q102 104 64 108 Q26 104 24 50 Z', d.grad(*color_taza), sw=5)
    d.path('M102 60 q22 0 20 20 q-2 18 -24 16', 'none', sw=6)
    d.ell(64, 50, 40, 10, liquido, sw=4)
    if espuma:
        for x, y, r in ((46, 46, 12), (64, 42, 14), (82, 46, 12)):
            d.circ(x, y, r, espuma, sw=3)
    d.brillo(40, 76, 5, 16, 0.5)
    if vapor:
        for x in (48, 64, 80):
            d.path('M%d 30 q-6 -7 0 -14 q6 -7 0 -14' % x, 'none', stroke='#c9d2d8', sw=4)


def ic_cafe_preparado(d):
    taza_grande(d, d.grad('#7a4a2a', '#4f2a15'))


def ic_chocolate_preparado(d):
    taza_grande(d, d.grad('#8f5a36', '#5a3018'), color_taza=('#e2585a', '#a92e2e'))


def ic_cafe_con_leche(d):
    d.path('M34 30 L94 30 L88 118 L40 118 Z', d.grad('#f1ece5', '#d4cbc0'), sw=5, op=None)
    d.path('M37 70 L91 70 L88 118 L40 118 Z', d.grad('#c48e62', '#8a5a36'), stroke=False)
    d.path('M36 52 L92 52 L91 70 L37 70 Z', '#e9d2b5', stroke=False)
    d.path('M34 30 L94 30 L88 118 L40 118 Z', 'none', sw=5)
    for x, y, r in ((46, 30, 12), (64, 24, 15), (82, 30, 12)):
        d.circ(x, y, r, BLANCO, sw=3)
    d.brillo(44, 86, 4, 20, 0.5)


def ic_chocolate_con_leche(d):
    taza_grande(d, d.grad('#a06a40', '#6b3e22'), color_taza=('#e2585a', '#a92e2e'), espuma=BLANCO, vapor=False)
    d.ell(60, 26, 6, 4, '#c4313a', sw=3)


def ic_leche_espumada(d):
    d.path('M30 44 L92 44 L98 116 Q64 124 30 116 Z', d.grad(METAL_CL, METAL, METAL_OSC, vertical=False), sw=5)
    d.path('M92 54 q24 4 18 30 q-4 16 -16 18', 'none', sw=7)
    d.path('M30 44 L18 34 L42 40 Z', METAL, sw=4)
    for x, y, r in ((40, 40, 13), (60, 34, 16), (80, 40, 13)):
        d.circ(x, y, r, BLANCO, sw=3)
    d.brillo(44, 84, 5, 22, 0.6)


def galleta(masa, chip, borde=None):
    def f(d):
        d.circ(64, 66, 50, d.radial(masa, masa, borde or masa, cx=0.4, cy=0.35), sw=5)
        for a in range(8):
            ang = a * 0.8 + 0.4
            r = 18 + (a % 3) * 9
            d.ell(64 + r * math.cos(ang), 66 + r * math.sin(ang), 6, 5, chip, stroke=False)
        d.brillo(46, 46, 14, 7, 0.3, rot=-30)
    return f


def ic_tacho(d):
    pass


def globo(ancho):
    # globo de diálogo: 0.75 de alto (192 px a 256 ppu)
    d = Dib(ancho, 192)
    w = ancho
    d.path('M24 8 L%d 8 Q%d 8 %d 32 L%d 132 Q%d 156 %d 156 L%d 156 L%d 184 L%d 156 L24 156 Q8 156 8 132 L8 32 Q8 8 24 8 Z'
           % (w - 24, w - 8, w - 8, w - 8, w - 8, w - 24, w / 2 + 18, w / 2, w / 2 - 18), BLANCO, sw=8)
    d.brillo(w / 2, 26, w * 0.32, 6, 0.0)
    return d


# =========================================================== UI (100 ppu)
def ui_madera():
    d = Dib(96, 96)
    d.rect(4, 4, 88, 88, d.grad('#c48e62', '#a06c45', '#7a4e30'), rx=18, sw=6)
    for y in (34, 62):
        d.line(10, y, 86, y, color='#7a4e30', sw=3)
    d.brillo(48, 14, 34, 4, 0.35)
    return d


def ui_papel():
    d = Dib(96, 96)
    d.rect(4, 6, 88, 86, '#c9a46a', rx=20, sw=6)
    d.rect(4, 4, 88, 82, d.grad('#fffaf0', '#fff1d8'), rx=20, sw=6)
    return d


def ui_boton():
    d = Dib(96, 96)
    d.rect(4, 8, 88, 84, '#7a1d22', rx=26, sw=6)
    d.rect(4, 4, 88, 80, d.grad('#ef6a62', '#d4433b', '#b02a30'), rx=26, sw=6)
    d.brillo(48, 22, 32, 7, 0.4)
    return d


def ui_tecla():
    d = Dib(64, 64)
    d.rect(4, 6, 56, 54, '#8995a0', rx=12, sw=5)
    d.rect(4, 4, 56, 48, d.grad('#ffffff', '#e4e9ee'), rx=12, sw=5)
    return d


def ui_corazon(lleno=True):
    d = Dib(64, 64)
    c1, c2 = ('#ef6a62', '#b02a30') if lleno else ('#8a7a74', '#5e504b')
    d.path('M32 56 C 8 40 2 28 6 18 C 10 6 26 4 32 16 C 38 4 54 6 58 18 C 62 28 56 40 32 56 Z', d.grad(c1, c2), sw=5)
    d.brillo(18, 20, 6, 4, 0.5 if lleno else 0.2, rot=-30)
    return d


def ui_progreso_fondo():
    d = Dib(48, 24)
    d.rect(3, 3, 42, 18, d.grad('#3a1d0e', '#5a3018'), rx=9, sw=4)
    return d


def ui_progreso_relleno():
    d = Dib(48, 24)
    d.rect(0, 0, 48, 24, d.grad('#ffe39a', '#f2c14e', '#c9962e'), stroke=False)
    return d


def flecha():
    d = Dib(64, 64)
    d.path('M22 4 L42 4 L42 30 L58 30 L32 60 L6 30 L22 30 Z', d.grad('#ffe39a', '#f2c14e', '#e0a020'), sw=5)
    d.brillo(28, 18, 3, 10, 0.6)
    return d


def flecha_derecha():
    d = Dib(64, 48)
    d.path('M4 16 L34 16 L34 4 L60 24 L34 44 L34 32 L4 32 Z', d.grad('#ffe39a', '#f2c14e', '#e0a020'), sw=5)
    return d


def mas():
    d = Dib(48, 48)
    d.path('M18 4 L30 4 L30 18 L44 18 L44 30 L30 30 L30 44 L18 44 L18 30 L4 30 L4 18 L18 18 Z', d.grad(BLANCO, '#e8e0d0'), sw=4)
    return d
