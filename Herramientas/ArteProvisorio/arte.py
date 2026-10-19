# Pixel art provisorio de Barista Caótico, inspirado en la cafetería de Club Penguin
# (paredes terracota con ladrillos, piso de tablones, sillones rojos, máquina de espresso
# verde azulada, mostrador bordó, alfombra verde). 32 px por unidad.
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from px import Img

O = '#2b1d16'          # contorno (igual que los íconos)
# madera
W0, W1, W2, W3, W4, W5 = '#f0c48a', '#d9a066', '#b97a48', '#93592f', '#6e3f22', '#4d2a16'
# crema
C1, C2, C3, C4 = '#fff7ec', '#f4efe6', '#f1e3cc', '#e8c98f'
# pared terracota
T1, T2, T3, T4, T5 = '#f0b49c', '#de9479', '#c97a63', '#a95c4a', '#7a2e2e'
# rojo sillón
R0, R1, R2, R3 = '#ef6a5e', '#d4433b', '#a92e2e', '#7a1d22'
# verde azulado (espresso)
Q1, Q2, Q3, Q4 = '#9fe0d4', '#5cbfb1', '#3a978c', '#246b64'
# verde (alfombra y plantas)
V1, V2, V3, V4 = '#b9d96a', '#8fbf4a', '#6b9a37', '#4a7027'
# mauve (mostrador)
M1, M2, M3, M4, M5 = '#d8b3c2', '#bb8ea2', '#9a6c82', '#734b60', '#4f3142'
G1, G2, G3, G4 = '#eef2f6', '#c4ccd5', '#8995a0', '#5c6870'
K1, K2, K3 = '#7a4a2a', '#5a3018', '#3a1d0e'
Y1, Y2, Y3 = '#ffe39a', '#f2c14e', '#c9962e'
B1, B2, B3 = '#bfe3f7', '#7fb2e8', '#4f8fd6'
P1, P2, P3 = '#fbd5d5', '#f2a7b0', '#d97a8c'
S1, S2, S3 = '#ffe0c4', '#f6c39d', '#e0a07a'
CU1, CU2, CU3 = '#f09a5c', '#d9773b', '#a8542a'   # cobre

LETRAS = {
    'Z': ['#####', '....#', '...#.', '..#..', '.#...', '#....', '#####'],
    'X': ['#...#', '#...#', '.#.#.', '..#..', '.#.#.', '#...#', '#...#'],
    'C': ['.####', '#....', '#....', '#....', '#....', '#....', '.####'],
}


def letra(img, x, y, ch, color=O):
    for j, fila in enumerate(LETRAS[ch]):
        for i, c in enumerate(fila):
            if c == '#':
                img.set(x + i, y + j, color)


def etiqueta(img, x, y, ch):
    # Cartelito con la tecla (9x11): papel crema con borde y sombra.
    img.rect(x, y, 9, 11, C1)
    img.frame(x, y, 9, 11, O)
    img.hline(x + 1, y + 9, 7, C4)
    letra(img, x + 2, y + 2, ch)


def contornear(dibujo):
    dibujo.outline(O)
    return dibujo


# ================================================================ PISOS
def piso_cocina():
    # Tablones de madera horizontales, con juntas y clavitos (32x32).
    im = Img(32, 32)
    tonos = ['#a56a3f', '#9a6139', '#a9703f', '#965c36']
    cortes = [8, 24, 2, 17]
    for k in range(4):
        y = k * 8
        im.rect(0, y, 32, 8, tonos[k])
        im.hline(0, y, 32, '#b97e4c')
        im.hline(0, y + 7, 32, '#5e3820')
        c = cortes[k]
        im.vline(c, y, 8, '#5e3820')
        im.set(c + 1, y + 1, '#c08850')
        im.set((c + 3) % 32, y + 3, '#4d2a16'); im.set((c - 3) % 32, y + 3, '#4d2a16')
        im.hline((c + 7) % 32, y + 4, 5, '#8c5532')
        im.hline((c + 18) % 32, y + 2, 4, '#b07546')
    return im


def piso_salon():
    # Alfombra roja con estrellitas (franja donde esperan los clientes), 32x32.
    im = Img(32, 32, R3)
    im.rect(0, 0, 32, 32, '#8e1f2a')
    estrella = ['..#..', '.###.', '#####', '.#.#.']
    for x, y in [(3, 4), (19, 12), (9, 21), (25, 27)]:
        im.pattern(x, y, estrella, {'#': '#a8323c'})
    im.set(14, 3, '#a8323c'); im.set(28, 18, '#a8323c'); im.set(4, 14, '#a8323c')
    return im


def alfombra():
    # Alfombra verde de bordes redondeados, con manchas más claras (5 x 3 unidades).
    im = Img(160, 96)
    im.rrect(0, 0, 160, 96, O, 2)
    im.rrect(2, 2, 156, 92, V3, 2)
    im.rrect(5, 5, 150, 86, V2, 2)
    for cx, cy, rx, ry in [(40, 30, 26, 14), (112, 62, 30, 16), (128, 22, 14, 8), (30, 72, 16, 9)]:
        im.ellipse(cx, cy, rx, ry, V1)
        im.ellipse(cx - 2, cy - 1, rx * 0.7, ry * 0.6, '#c8e27e')
    for x, y in [(70, 20), (84, 44), (56, 58), (140, 80), (20, 50), (98, 82)]:
        im.set(x, y, V3); im.set(x + 1, y, V3)
    im.hline(6, 90, 148, V3)
    return im


# =============================================================== PARED
def pared():
    # Pared terracota con ladrillos sueltos, moldura arriba y zócalo abajo (32x88).
    im = Img(32, 88, T3)
    im.rect(0, 0, 32, 88, T2)
    # moldura de arriba (queda tapada por el HUD casi siempre)
    im.rect(0, 0, 32, 6, T5)
    im.hline(0, 6, 32, T4)
    # ladrillos sueltos
    for x, y in [(3, 18), (19, 30), (8, 46), (24, 58), (14, 68), (1, 36)]:
        im.rect(x, y, 8, 4, T3)
        im.hline(x, y, 8, T1)
        im.frame(x, y, 8, 4, T4)
    # sombra hacia abajo y zócalo de madera
    im.rect(0, 74, 32, 4, T3)
    im.hline(0, 78, 32, O)
    im.rect(0, 79, 32, 8, W4)
    im.hline(0, 79, 32, W3)
    im.hline(0, 87, 32, O)
    return im


def ventana():
    # Ventanal con una taza pintada (110x44).
    im = Img(112, 44)
    im.rect(0, 0, 112, 44, W4)
    im.rect(4, 4, 104, 34, '#8fb7d6')
    im.rect(4, 4, 104, 12, '#a9cbe3')
    # nubes
    for cx, cy, r in [(18, 34, 6), (28, 33, 7), (40, 35, 5), (78, 34, 6), (92, 33, 7), (102, 35, 5)]:
        im.ellipse(cx, cy, r, r * 0.7, '#dfeaf2')
    # taza pintada
    im.ellipse(56, 18, 20, 4, '#5f6b85')
    im.rect(38, 18, 36, 10, '#7c87a3')
    im.ellipse(56, 28, 18, 5, '#7c87a3')
    im.ellipse(56, 18, 17, 2.6, '#4a536b')
    im.rect(74, 20, 6, 2, '#7c87a3'); im.rect(78, 20, 2, 7, '#7c87a3'); im.rect(74, 25, 6, 2, '#7c87a3')
    im.ellipse(56, 33, 26, 3, '#6a7590')
    # vapor
    for x in (48, 56, 64):
        im.set(x, 8, '#eef4f8'); im.set(x + 1, 9, '#eef4f8'); im.set(x, 10, '#eef4f8')
    # marco y travesaño
    im.frame(3, 3, 106, 36, O)
    im.rect(0, 38, 112, 6, W3)
    im.hline(0, 38, 112, W1)
    im.frame(0, 0, 112, 44, O)
    im.vline(1, 1, 37, W2)
    return im


def puerta():
    # Puerta con ventanita y persiana (40x56). Por acá entran los clientes.
    im = Img(40, 56)
    im.rect(0, 0, 40, 56, W3)
    im.rect(3, 3, 34, 53, '#c9c2b8')
    im.rect(5, 5, 30, 51, '#e6e0d6')
    # ventanita con persiana
    im.rect(9, 9, 22, 26, '#9fbfd8')
    for y in range(10, 20, 2):
        im.hline(9, y, 22, W1)
        im.hline(9, y + 1, 22, W3)
    im.frame(8, 8, 24, 28, O)
    # manija
    im.rect(29, 36, 3, 8, Y2); im.vline(29, 36, 8, Y1)
    im.frame(28, 35, 5, 10, O)
    im.frame(2, 2, 36, 54, O)
    im.frame(0, 0, 40, 56, O)
    # felpudo
    im.rect(4, 53, 32, 3, '#6a5a7a')
    return im


def pizarra():
    # Pizarra con el menú, marco verde (52x44).
    im = Img(52, 44)
    im.rect(0, 0, 52, 44, '#5d6b45')
    im.rect(3, 3, 46, 38, '#2f3a2f')
    im.hline(3, 3, 46, '#3c4a3c')
    # dibujos de tiza
    tiza = '#e9eadf'
    im.ellipse(14, 14, 6, 2, tiza)
    im.rect(9, 14, 10, 6, tiza); im.rect(10, 15, 8, 4, '#2f3a2f')
    im.rect(19, 15, 2, 3, tiza)
    for y in (12, 18, 24, 30):
        im.hline(26, y, 18 if y != 24 else 12, tiza)
    for y in (27, 33):
        im.hline(8, y, 12, tiza)
    im.set(9, 9, tiza); im.set(12, 8, tiza); im.set(15, 9, tiza)
    im.frame(0, 0, 52, 44, O)
    im.frame(2, 2, 48, 40, O)
    im.rect(20, 41, 12, 2, W1)
    return im


def cuadro(fondo, dibujo):
    im = Img(20, 26)
    im.rect(0, 0, 20, 26, W1)
    im.rect(2, 2, 16, 22, fondo)
    im.ellipse(10, 12, 5, 6, dibujo)
    im.ellipse(10, 10, 3, 3, fondo)
    im.set(8, 9, dibujo); im.set(12, 9, dibujo)
    im.frame(0, 0, 20, 26, O)
    im.frame(2, 2, 16, 22, W3)
    return im


def lampara():
    # Lámpara colgante: cable largo y pantalla verde azulada (14x40).
    im = Img(14, 40)
    im.vline(7, 0, 28, '#3b3b3b')
    im.rect(5, 26, 4, 3, G4)
    im.rect(2, 29, 10, 5, Q3)
    im.rect(0, 33, 14, 3, Q3)
    im.hline(1, 29, 10, Q2)
    im.hline(0, 33, 14, Q2)
    im.rect(4, 36, 6, 3, Y1)
    im.hline(5, 38, 4, Y2)
    contornear(im)
    return im


def sofa():
    # Sillón rojo capitoné, de frente (56x34).
    im = Img(56, 34)
    im.rrect(4, 2, 48, 18, R1, 2)
    im.hline(6, 3, 44, R0)
    for x in (14, 28, 42):
        for y in (8, 14):
            im.set(x, y, R3); im.set(x - 7, y + 3, R3)
    im.vline(20, 4, 15, R2); im.vline(36, 4, 15, R2)
    im.rect(4, 18, 48, 9, R1)
    im.hline(4, 18, 48, R0)
    im.vline(20, 19, 8, R2); im.vline(36, 19, 8, R2)
    im.rect(0, 12, 7, 16, R2); im.rect(49, 12, 7, 16, R2)
    im.hline(0, 12, 7, R1); im.hline(49, 12, 7, R1)
    im.rect(1, 12, 5, 3, G3); im.rect(50, 12, 5, 3, G3)
    im.rect(4, 27, 48, 3, R3)
    im.rect(6, 30, 3, 4, K2); im.rect(47, 30, 3, 4, K2)
    contornear(im)
    return im


def planta():
    # Arbolito en maceta (estilo topiario), 28x48.
    im = Img(28, 48)
    for cx, cy, rx, ry in [(14, 9, 8, 7), (9, 16, 6, 6), (19, 16, 6, 6), (14, 22, 8, 6), (10, 27, 5, 4), (18, 27, 5, 4)]:
        im.ellipse(cx, cy, rx, ry, V3)
    for cx, cy, rx, ry in [(12, 7, 4, 3), (7, 14, 3, 3), (17, 14, 3, 2), (12, 20, 4, 3)]:
        im.ellipse(cx, cy, rx, ry, V2)
    for x, y in [(10, 5), (6, 12), (16, 12), (11, 19), (20, 22)]:
        im.set(x, y, V1)
    im.vline(14, 28, 7, K2); im.vline(13, 30, 4, K1)
    im.rect(6, 34, 16, 4, '#9aa3a8')
    im.rect(7, 38, 14, 9, '#c2c8cc')
    im.hline(6, 34, 16, '#dfe3e6')
    im.vline(8, 38, 8, '#e3e7ea')
    contornear(im)
    return im


def caja_registradora():
    im = Img(30, 24)
    im.rect(2, 8, 26, 12, Q2)
    im.hline(2, 8, 26, Q1)
    im.rect(4, 2, 14, 8, Q3)
    im.rect(5, 3, 12, 4, '#2d4a46')
    im.hline(6, 4, 6, Q1)
    for x in range(5, 25, 4):
        im.rect(x, 11, 2, 2, C1)
        im.rect(x, 15, 2, 2, C1)
    im.rect(0, 19, 30, 4, Q4)
    contornear(im)
    return im


# ============================================================ MOSTRADOR
def mostrador():
    # Mostrador bordó/mauve, tileable a lo ancho (32x26).
    im = Img(32, 26)
    im.hline(0, 0, 32, O)
    im.rect(0, 1, 32, 7, M2)
    im.hline(0, 1, 32, M1)
    im.hline(0, 7, 32, M3)
    im.set(6, 3, M1); im.hline(18, 4, 6, '#c9a2b4')
    im.hline(0, 8, 32, M4)
    im.hline(0, 9, 32, O)
    im.rect(0, 10, 32, 14, M4)
    for x0 in (2, 18):
        im.rect(x0, 12, 12, 10, M5)
        im.hline(x0, 12, 12, M3)
        im.vline(x0, 12, 10, M3)
    im.hline(0, 23, 32, '#3a2230')
    im.rect(0, 24, 32, 1, G3)
    im.hline(0, 25, 32, O)
    return im


def bandeja():
    im = Img(44, 18)
    im.rrect(1, 2, 42, 13, R2, 2)
    im.rrect(3, 3, 38, 10, R1, 2)
    im.hline(6, 4, 30, R0)
    im.hline(4, 12, 36, R2)
    im.rect(0, 6, 2, 5, R3); im.rect(42, 6, 2, 5, R3)
    contornear(im)
    return im


# ============================================================= MÁQUINAS
MW, MH = 84, 56   # 2.625 x 1.75 unidades


def mueble(im, y0=30):
    # Mueble de madera bajo cada máquina: tapa clara y cajones.
    im.rect(1, y0, MW - 2, 5, W1)
    im.hline(1, y0, MW - 2, W0)
    im.hline(1, y0 + 4, MW - 2, W2)
    im.rect(1, y0 + 5, MW - 2, MH - y0 - 6, W3)
    for x0 in (4, 44):
        im.rect(x0, y0 + 8, 36, 7, W2)
        im.hline(x0, y0 + 8, 36, W1)
        im.frame(x0, y0 + 8, 36, 7, W4)
        im.rect(x0 + 15, y0 + 11, 6, 1, O)
    im.rect(4, y0 + 17, 76, 2, W4)
    im.frame(0, y0 - 1, MW, MH - y0 + 1, O)


def pegar_contorneado(im, dib):
    contornear(dib)
    im.paste(dib, 0, 0)


def cafetera():
    im = Img(MW, MH)
    mueble(im)
    d = Img(MW, MH)
    # cuerpo verde azulado de la espresso
    d.rrect(16, 4, 52, 28, Q2, 2)
    d.rect(16, 4, 52, 6, Q1)
    d.hline(18, 4, 48, '#c6f0e8')
    d.rect(16, 24, 52, 6, Q3)
    d.vline(17, 10, 14, Q1)
    d.vline(66, 10, 14, Q3)
    # panel cromado con manómetro
    d.rrect(30, 11, 24, 9, G1, 1)
    d.ellipse(42, 15.5, 3.5, 3.5, C1)
    d.frame(38, 12, 9, 8, G3)
    d.set(42, 14, R2); d.set(43, 13, R2)
    d.set(34, 15, R1); d.set(50, 15, Q3)
    # grupos, tazas y vaporizador
    for gx in (22, 54):
        d.rect(gx, 20, 8, 3, G3)
        d.rect(gx + 2, 23, 4, 2, G4)
        d.rect(gx, 26, 8, 5, C1)
        d.hline(gx, 26, 8, K2)
        d.rect(gx + 8, 27, 2, 2, C1)
    d.vline(70, 12, 14, G3); d.rect(69, 25, 3, 2, G4)
    # tazas apiladas arriba
    for tx in (24, 34, 44, 54):
        d.rect(tx, 0, 6, 4, C1)
        d.hline(tx, 0, 6, C3)
    pegar_contorneado(im, d)
    im.set(30, 33, '#9a6c4a'); im.set(60, 32, '#9a6c4a')
    return im


def moledora():
    im = Img(MW, MH)
    mueble(im)
    d = Img(MW, MH)
    # tolva de vidrio llena de granos
    d.rect(30, 0, 24, 14, '#d9e8ef')
    d.rect(32, 3, 20, 11, K1)
    for x, y in [(33, 4), (37, 6), (41, 4), (45, 7), (49, 5), (35, 10), (43, 11), (47, 9), (39, 9)]:
        d.rect(x, y, 2, 2, K2)
        d.set(x, y, '#a06a40')
    d.vline(31, 1, 12, '#ffffff')
    d.rect(28, 0, 28, 2, R2)
    # cuerpo rojo
    d.rrect(26, 14, 32, 16, R1, 2)
    d.vline(28, 15, 13, R0)
    d.rect(26, 26, 32, 4, R2)
    d.rrect(36, 17, 12, 6, C2, 1)
    d.rect(39, 19, 6, 2, K2)
    # manija lateral
    d.rect(58, 18, 6, 2, G3); d.rect(63, 14, 3, 5, K2)
    # vasito de café molido
    d.rect(10, 22, 10, 9, C1)
    d.rect(10, 22, 10, 3, K1)
    d.hline(10, 25, 10, C3)
    # bolsa de granos al costado
    d.rrect(64, 20, 14, 11, C4, 1)
    d.rect(66, 18, 10, 3, '#d8b070')
    d.ellipse(71, 26, 3, 2, K1)
    pegar_contorneado(im, d)
    return im


def chocolatera():
    im = Img(MW, MH)
    mueble(im)
    d = Img(MW, MH)
    # hornalla
    d.rect(18, 26, 48, 5, G4)
    d.hline(19, 26, 46, G3)
    d.rect(22, 29, 4, 2, R1); d.rect(58, 29, 4, 2, R1)
    # olla de cobre grande
    d.rrect(22, 6, 40, 21, CU2, 2)
    d.rect(22, 6, 40, 6, CU3)
    d.vline(25, 12, 13, CU1)
    d.vline(26, 13, 10, CU1)
    d.hline(24, 25, 36, CU3)
    d.ellipse(42, 7, 18, 3.6, K1)
    d.ellipse(40, 6.5, 12, 2, '#8f5a36')
    d.set(46, 7, '#a06a40'); d.set(36, 6, '#a06a40')
    # manijas
    d.rect(16, 11, 6, 3, K2); d.rect(62, 11, 6, 3, K2)
    # cucharón
    d.vline(52, 0, 8, W3); d.rect(51, 0, 3, 2, W1)
    # tazones al costado
    d.rect(68, 22, 10, 6, P2); d.hline(68, 22, 10, K1)
    d.rect(4, 22, 10, 6, B2); d.hline(4, 22, 10, K1)
    pegar_contorneado(im, d)
    # vapor
    for x, y in [(34, 1), (35, 0), (44, 2), (45, 1)]:
        im.set(x, y, G1)
    return im


# ============================================================ ESTACIONES
EW, EH = 72, 76   # 2.25 x 2.375 unidades


def ingredientes():
    im = Img(EW, EH)
    im.rect(0, 0, EW, EH, W3)
    im.rect(0, 0, EW, 5, W2)
    im.hline(0, 0, EW, W1)
    filas = [(6, 'Z'), (29, 'X'), (52, 'C')]
    for y0, ch in filas:
        im.rect(4, y0, EW - 8, 20, W5)
        im.hline(4, y0, EW - 8, '#3a1f10')
        im.rect(4, y0 + 20, EW - 8, 2, W1)
        etiqueta(im, 6, y0 + 4, ch)
    # sacos de granos (con dibujo de grano)
    for k, sx in enumerate((19, 36, 52)):
        s = Img(18, 19)
        s.rrect(0, 4, 17, 15, C4, 2)
        s.rect(3, 0, 11, 6, '#d8b070')
        s.hline(3, 5, 11, '#b8904f')
        s.ellipse(8.5, 12, 3.5, 2.6, K1)
        s.vline(8, 10, 4, K2)
        for x, y in [(4, 1), (7, 0), (10, 1)]:
            s.rect(x, y, 2, 2, K1)
        contornear(s)
        im.paste(s, sx - 1, 7)
    # cajón de botellas de leche
    for bx in (20, 30, 40, 50, 60):
        if bx > 62:
            continue
        im.rect(bx + 2, 30, 4, 3, G2)
        im.rect(bx + 1, 33, 6, 12, C1)
        im.rect(bx + 1, 37, 6, 4, B3)
        im.vline(bx + 2, 34, 9, '#ffffff')
        im.frame(bx + 1, 33, 6, 12, O)
        im.frame(bx + 2, 29, 4, 4, O)
    im.rect(18, 44, 50, 6, W2)
    im.frame(18, 44, 50, 6, O)
    # cajas de chocolate
    for k, bx in enumerate((19, 34, 49)):
        im.rect(bx, 55 - (k % 2), 14, 15, K1)
        for i in range(3):
            for j in range(2):
                im.rect(bx + 1 + i * 4, 56 - (k % 2) + j * 4, 3, 3, '#8f5a36')
        im.rect(bx, 64 - (k % 2), 14, 6, R1)
        im.hline(bx, 65 - (k % 2), 14, Y2)
        im.frame(bx, 55 - (k % 2), 14, 15, O)
    im.frame(0, 0, EW, EH, O)
    return im


def galletitas():
    im = Img(EW, EH)
    im.rect(0, 0, EW, EH, R2)
    im.rect(0, 0, EW, 7, R1)
    for x in range(0, EW, 8):
        im.rect(x, 0, 4, 7, C1)
    im.hline(0, 7, EW, O)
    filas = [(10, 'Z', R2, C1), (32, 'X', C4, K2), (54, 'C', K1, K3)]
    for y0, ch, masa, chips in filas:
        im.rect(4, y0, EW - 8, 19, '#fdeef0')
        im.rect(4, y0 + 15, EW - 8, 4, '#e9c9ce')
        im.hline(4, y0 + 15, EW - 8, P3)
        etiqueta(im, 6, y0 + 3, ch)
        for cx in (26, 39, 52, 63):
            g = Img(12, 10)
            g.ellipse(6, 5, 5.5, 4.4, masa)
            g.set(4, 3, chips); g.set(7, 2, chips); g.set(6, 6, chips); g.set(9, 4, chips); g.set(3, 6, chips)
            g.set(5, 2, '#ffffff' if masa != C4 else '#fff2d0')
            contornear(g)
            im.paste(g, cx - 6, y0 + 4)
        # vidrio
        im.vline(18, y0 + 1, 4, '#ffffff'); im.vline(19, y0 + 1, 2, '#ffffff')
    im.rect(0, EH - 4, EW, 4, R3)
    im.frame(0, 0, EW, EH, O)
    return im


def repisa():
    im = Img(68, 30)
    im.rect(1, 1, 66, 17, W1)
    im.hline(1, 1, 66, W0)
    im.hline(1, 17, 66, W2)
    for x, y in [(12, 5), (44, 12), (28, 9)]:
        im.hline(x, y, 6, '#c98e56')
    im.rect(1, 18, 66, 6, W3)
    im.hline(1, 18, 66, W4)
    im.rect(4, 24, 5, 6, W4); im.rect(59, 24, 5, 6, W4)
    for cx in (18, 50):
        im.ellipse(cx, 9, 8, 4.5, C3)
        im.ellipse(cx, 9, 5.5, 2.6, C1)
    im.frame(0, 0, 68, 25, O)
    im.frame(3, 24, 7, 6, O); im.frame(58, 24, 7, 6, O)
    return im


# ============================================================ PERSONAS
def persona(pelo, pelo2, ropa, ropa2, delantal=None, peinado='corto', anteojos=False, gorro=None):
    im = Img(24, 34)
    # cabeza grande (chibi)
    im.rrect(3, 4, 18, 15, S1, 2)
    im.vline(20, 6, 11, S2)
    im.hline(6, 18, 12, S2)
    # pelo
    if peinado == 'rodete':
        im.ellipse(12, 2.5, 4, 2.6, pelo)
        im.set(11, 1, pelo2); im.set(12, 1, pelo2)
    im.rrect(2, 3, 20, 6, pelo, 1)
    im.rect(2, 8, 2, 5, pelo)
    im.rect(20, 8, 2, 5, pelo)
    if peinado == 'largo':
        im.rect(1, 8, 3, 14, pelo); im.rect(20, 8, 3, 14, pelo)
        im.vline(2, 10, 10, pelo2)
    if peinado == 'rulos':
        for x in range(2, 22, 4):
            im.ellipse(x + 2, 3.5, 2.6, 2.4, pelo)
            im.set(x + 1, 2, pelo2)
    im.hline(6, 4, 8, pelo2)
    im.set(9, 9, pelo); im.set(10, 9, pelo); im.set(14, 9, pelo); im.set(15, 9, pelo)
    if gorro:
        im.rect(4, 1, 16, 4, gorro)
        im.hline(4, 1, 16, C1)
        im.rect(2, 4, 20, 2, gorro)
    # cara
    im.rect(7, 11, 2, 3, O); im.rect(15, 11, 2, 3, O)
    im.set(7, 11, '#ffffff'); im.set(15, 11, '#ffffff')
    im.rect(5, 14, 2, 1, P2); im.rect(17, 14, 2, 1, P2)
    im.hline(11, 15, 2, S3)
    if anteojos:
        im.frame(5, 10, 6, 5, O); im.frame(13, 10, 6, 5, O); im.hline(11, 11, 2, O)
    # cuerpo
    im.rrect(5, 19, 14, 10, ropa, 1)
    im.vline(18, 20, 8, ropa2)
    im.rect(2, 20, 3, 7, ropa); im.rect(19, 20, 3, 7, ropa2)
    im.rect(2, 27, 3, 2, S2); im.rect(19, 27, 3, 2, S2)
    if delantal:
        im.rect(7, 21, 10, 8, delantal)
        im.hline(7, 21, 10, C1)
        im.set(8, 20, delantal); im.set(15, 20, delantal)
        im.rect(10, 24, 4, 2, C1)
    # piernas y zapatos
    im.rect(7, 29, 4, 2, K2); im.rect(13, 29, 4, 2, K2)
    im.rect(6, 31, 5, 2, O); im.rect(13, 31, 5, 2, O)
    contornear(im)
    return im


def barista():
    return persona('#6b3e26', '#8a5233', C2, '#e3d9c9', delantal=Q3, peinado='rodete')


def cliente1():
    return persona(Y2, Y1, B3, '#3f74b3', peinado='corto')


def cliente2():
    return persona(CU2, CU1, V3, V4, peinado='largo')


def cliente3():
    return persona('#3b2f45', '#5a4a66', '#9b7bd4', '#7a5cb3', peinado='rulos', anteojos=True)


# ================================================================== UI
def flecha():
    im = Img(16, 16)
    im.pattern(1, 1, [
        '....######....',
        '....#yyyy#....',
        '....#yyyy#....',
        '....#yyyy#....',
        '....#yyyy#....',
        '#####yyyy#####',
        '#yyyyyyyyyyyy#',
        '.#yyyyyyyyyy#.',
        '..#yyyyyyyy#..',
        '...#yyyyyy#...',
        '....#yyyy#....',
        '.....#yy#.....',
        '......##......',
    ], {'#': O, 'y': Y2})
    im.vline(6, 2, 5, Y1)
    im.set(6, 7, Y1); im.set(7, 7, Y1)
    return im


def flecha_derecha():
    # Flecha "se transforma en" para las reglas (16x12).
    im = Img(16, 12)
    im.pattern(0, 0, [
        '..........##....',
        '..........#y#...',
        '###########yy#..',
        '#yyyyyyyyyyyyy#.',
        '#yyyyyyyyyyyyyy#',
        '#yyyyyyyyyyyyy#.',
        '###########yy#..',
        '..........#y#...',
        '..........##....',
    ], {'#': O, 'y': Y2})
    im.hline(1, 3, 10, Y1)
    return im


def mas():
    im = Img(12, 12)
    im.pattern(0, 0, [
        '....####....',
        '....#yy#....',
        '....#yy#....',
        '#####yy#####',
        '#yyyyyyyyyy#',
        '#yyyyyyyyyy#',
        '#####yy#####',
        '....#yy#....',
        '....#yy#....',
        '....####....',
    ], {'#': O, 'y': C1})
    return im


def barra_madera():
    im = Img(32, 32)
    im.rect(0, 0, 32, 32, W2)
    for y in (10, 21):
        im.hline(0, y, 32, W3)
        im.hline(0, y + 1, 32, W1)
    im.vline(13, 2, 8, W3); im.vline(24, 12, 9, W3); im.vline(9, 23, 7, W3)
    im.hline(0, 0, 32, O)
    im.hline(0, 1, 32, W1)
    im.hline(0, 2, 32, W0)
    im.hline(0, 29, 32, W4)
    im.hline(0, 30, 32, K2)
    im.hline(0, 31, 32, O)
    im.vline(0, 0, 32, O); im.vline(31, 0, 32, O)
    im.vline(1, 1, 30, W3); im.vline(30, 1, 30, W3)
    for x, y in [(3, 4), (28, 4), (3, 27), (28, 27)]:
        im.set(x, y, K3)
    return im


def cartel_papel():
    im = Img(24, 24)
    im.rrect(0, 0, 24, 24, O, 1)
    im.rrect(1, 1, 22, 21, C1, 1)
    im.hline(2, 1, 20, '#ffffff')
    im.hline(1, 21, 22, C4)
    im.hline(1, 22, 22, '#c9a46a')
    return im


def boton():
    # Botón rojo como los sillones (24x24, 9-slice con borde 8).
    im = Img(24, 24)
    im.rrect(0, 0, 24, 24, O, 2)
    im.rrect(1, 1, 22, 21, R1, 2)
    im.hline(3, 2, 18, R0)
    im.hline(2, 3, 20, R0)
    im.hline(2, 19, 20, R2)
    im.hline(2, 20, 20, R3)
    im.hline(2, 21, 20, R3)
    return im


def tecla():
    # Tecla de teclado para las reglas (16x16, 9-slice con borde 5).
    im = Img(16, 16)
    im.rrect(0, 0, 16, 16, O, 1)
    im.rrect(1, 1, 14, 14, G2, 1)
    im.rect(2, 1, 12, 10, C1)
    im.hline(2, 1, 12, '#ffffff')
    im.hline(1, 12, 14, G3)
    im.hline(1, 13, 14, G3)
    return im


def corazon(lleno=True):
    im = Img(16, 16)
    filas = [
        '..###...###...',
        '.#rrr#.#rrr#..',
        '#rwrrr#rrrrr#.',
        '#rwrrrrrrrrr#.',
        '#rrrrrrrrrrr#.',
        '.#rrrrrrrrr#..',
        '..#rrrrrrr#...',
        '...#rrrrr#....',
        '....#rrr#.....',
        '.....#r#......',
        '......#.......',
    ]
    pal = {'#': O, 'r': R1, 'w': '#ffb3ad'} if lleno else {'#': O, 'r': '#6e5a52', 'w': '#8a766d'}
    im.pattern(1, 2, filas, pal)
    return im


def progreso_fondo():
    im = Img(16, 8)
    im.rect(0, 0, 16, 8, O)
    im.rect(1, 1, 14, 6, K3)
    im.hline(1, 6, 14, K2)
    return im


def progreso_relleno():
    im = Img(16, 8)
    im.rect(0, 0, 16, 8, Y2)
    im.hline(0, 0, 16, Y1)
    im.hline(0, 1, 16, Y1)
    im.hline(0, 7, 16, Y3)
    return im


SPRITES = {
    # (carpeta, nombre): (función, ppu, borde 9-slice (izq, abajo, der, arriba))
    ('Escenario', 'PisoCocina'): (piso_cocina, 32, None),
    ('Escenario', 'PisoSalon'): (piso_salon, 32, None),
    ('Escenario', 'Alfombra'): (alfombra, 32, None),
    ('Escenario', 'Pared'): (pared, 32, None),
    ('Escenario', 'Ventana'): (ventana, 32, None),
    ('Escenario', 'Puerta'): (puerta, 32, None),
    ('Escenario', 'Pizarra'): (pizarra, 32, None),
    ('Escenario', 'CuadroNaranja'): (lambda: cuadro('#e8964f', '#c46a2c'), 32, None),
    ('Escenario', 'CuadroVerde'): (lambda: cuadro('#7fae6a', '#4f7c3d'), 32, None),
    ('Escenario', 'Lampara'): (lampara, 32, None),
    ('Escenario', 'Sofa'): (sofa, 32, None),
    ('Escenario', 'Planta'): (planta, 32, None),
    ('Escenario', 'CajaRegistradora'): (caja_registradora, 32, None),
    ('Escenario', 'Mostrador'): (mostrador, 32, None),
    ('Escenario', 'Bandeja'): (bandeja, 32, None),
    ('Escenario', 'Moledora'): (moledora, 32, None),
    ('Escenario', 'Cafetera'): (cafetera, 32, None),
    ('Escenario', 'Chocolatera'): (chocolatera, 32, None),
    ('Escenario', 'Ingredientes'): (ingredientes, 32, None),
    ('Escenario', 'Galletitas'): (galletitas, 32, None),
    ('Escenario', 'Repisa'): (repisa, 32, None),
    ('Personajes', 'Barista'): (barista, 32, None),
    ('Personajes', 'Cliente1'): (cliente1, 32, None),
    ('Personajes', 'Cliente2'): (cliente2, 32, None),
    ('Personajes', 'Cliente3'): (cliente3, 32, None),
    ('HUD', 'Flecha'): (flecha, 32, None),
    ('HUD', 'FlechaDerecha'): (flecha_derecha, 32, None),
    ('HUD', 'Mas'): (mas, 32, None),
    ('HUD', 'BarraMadera'): (barra_madera, 32, (6, 6, 6, 6)),
    ('HUD', 'CartelPapel'): (cartel_papel, 32, (7, 7, 7, 7)),
    ('HUD', 'Boton'): (boton, 32, (7, 7, 7, 7)),
    ('HUD', 'Tecla'): (tecla, 32, (5, 5, 5, 5)),
    ('HUD', 'Corazon'): (lambda: corazon(True), 32, None),
    ('HUD', 'CorazonVacio'): (lambda: corazon(False), 32, None),
    ('HUD', 'ProgresoFondo'): (progreso_fondo, 32, (3, 3, 3, 3)),
    ('HUD', 'ProgresoRelleno'): (progreso_relleno, 32, None),
}
