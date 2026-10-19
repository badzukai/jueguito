# Pixel art del layout 60/40 (cocina 60 %, café 40 %), vista cenital a 3/4.
# Reusa las piezas de arte.py (personajes, despensas, bandeja, HUD) y agrega:
#   - Fondo: el cuarto entero en una sola imagen (pared, café con sillones y mesitas, piso de la cocina).
#   - Mostrador: la barra larga, con aire arriba para lo que está apoyado.
#   - Máquinas para poner ARRIBA de la barra, mirando al barista (sin mueble).
#   - Repisa: mesa de trabajo (isla) en el medio de la cocina.
# 32 px por unidad. La cámara ve 17.78 x 10 unidades (x ±8.89, y −5…5).
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import arte as A
from px import Img

O = A.O
W, H = 569, 320


def X(x):
    return int(round((x + 8.89) * 32))


def Y(y):
    return int(round((5 - y) * 32))


# ----------------------------------------------------------- piezas chicas
def baldosa():
    # Baldosas de la cocina: damero crema y terracota suave (32x32).
    im = Img(32, 32)
    for ty in range(2):
        for tx in range(2):
            c = '#f1dfc2' if (tx + ty) % 2 == 0 else '#dcae8a'
            im.rect(tx * 16, ty * 16, 16, 16, c)
            im.hline(tx * 16, ty * 16, 16, '#f8ead2' if (tx + ty) % 2 == 0 else '#e6bd9b')
            im.vline(tx * 16 + 15, ty * 16, 16, '#c99a78')
            im.hline(tx * 16, ty * 16 + 15, 16, '#c99a78')
    return im


def puff(color=A.R1, osc=A.R3):
    im = Img(18, 13)
    im.ellipse(9, 6.5, 8.5, 6, color)
    im.ellipse(9, 8.5, 8, 3.5, A.R2)
    im.ellipse(7, 4, 3.5, 2, A.R0)
    im.set(9, 6, osc)
    im.outline(O)
    return im


def mesita():
    im = Img(30, 20)
    im.ellipse(15, 6, 14, 5.5, '#5c443b')
    im.ellipse(13, 5, 8, 2.5, '#76594d')
    im.rect(13, 10, 4, 7, '#3b2a25')
    im.rect(8, 16, 14, 2, '#3b2a25')
    im.outline(O)
    # tazas
    for x, c in ((8, A.C1), (18, A.B2)):
        im.rect(x, 1, 4, 3, c)
        im.frame(x - 1, 0, 6, 5, O)
    return im


def alfombra_sofa():
    im = Img(70, 14)
    im.ellipse(35, 7, 34, 6.5, A.R2)
    im.ellipse(35, 6, 30, 4.5, A.R1)
    im.outline(O)
    return im


def frasco():
    im = Img(22, 18)
    im.rrect(1, 4, 20, 14, A.V3, 2)
    im.rect(3, 6, 16, 3, A.V2)
    for x in (4, 9, 14):
        im.rect(x, 1, 5, 4, A.Y2)
        im.hline(x, 1, 5, A.Y1)
    im.outline(O)
    return im


# ================================================================== FONDO
def fondo():
    im = Img(W, H, (90, 48, 24, 255))
    # piso de la cocina (baldosas) y del café (tablones)
    b = baldosa()
    for y in range(Y(0.5), H, 32):
        for x in range(-6, W, 32):
            im.paste(b, x, y)
    madera = A.piso_cocina()
    for y in range(Y(3.0), Y(0.3), 32):
        for x in range(0, W, 32):
            im.paste(madera, x, y)
    # pared del fondo (la base apoya en y = 2.9)
    pared = A.pared()
    base = Y(2.9)
    for x in range(0, W, 32):
        im.paste(pared, x, base - pared.h + 1)
    # puerta en el centro (por acá entran los clientes)
    puerta = A.puerta()
    im.paste(puerta, X(0) - puerta.w // 2, base - puerta.h + 2)
    # ventana, pizarra y cuadros
    v = A.ventana()
    im.paste(v, X(-4.1) - v.w // 2, base - v.h - 4)
    pz = A.pizarra()
    im.paste(pz, X(4.1) - pz.w // 2, base - pz.h - 4)
    for x, c in ((-1.7, A.cuadro('#e8964f', '#c46a2c')), (1.7, A.cuadro('#7fae6a', '#4f7c3d')),
                 (-7.0, A.cuadro('#8fb6e6', '#4f7cc6')), (7.0, A.cuadro('#e8c25a', '#c9962e'))):
        im.paste(c, X(x) - c.w // 2, base - c.h - 8)
    for x in (-5.0, 5.0):
        lam = A.lampara()
        im.paste(lam, X(x) - lam.w // 2, base - lam.h - 20)
    # sillones contra la pared, con su alfombra
    for x in (-2.6, 2.6):
        alf = alfombra_sofa()
        im.paste(alf, X(x) - alf.w // 2, Y(2.45) - 6)
        s = A.sofa()
        im.paste(s, X(x) - s.w // 2, Y(2.45) - s.h + 2)
    # plantas en las esquinas
    for x in (-8.4, 8.4):
        p = A.planta()
        im.paste(p, X(x) - p.w // 2, Y(2.7) - p.h)
    # mesitas con puffs
    for x, y in ((-7.3, 1.45), (7.3, 2.15)):
        for dx, dy in ((-0.55, 0.15), (0.55, 0.15), (0, -0.3)):
            pf = puff()
            im.paste(pf, X(x + dx) - pf.w // 2, Y(y + dy) - pf.h // 2)
        m = mesita()
        im.paste(m, X(x) - m.w // 2, Y(y) - m.h // 2)
    # sombra suave debajo de la barra
    for x in range(W):
        for k in range(3):
            im.set(x, Y(-0.6) + k, (0, 0, 0, 40 - k * 12))
    # mesada corrida detrás de la fila de abajo (rellena los huecos entre máquinas)
    tapa = Y(-2.5 - 52 / 32)
    im.rect(X(-6.2), tapa, X(6.2) - X(-6.2), 5, A.W1)
    im.hline(X(-6.2), tapa, X(6.2) - X(-6.2), A.W0)
    im.rect(X(-6.2), tapa + 5, X(6.2) - X(-6.2), H - tapa - 5, A.W3)
    im.frame(X(-6.2), tapa - 1, X(6.2) - X(-6.2), H - tapa + 2, O)
    # plantas en las esquinas de abajo (decoración)
    for x in (-7.75, 7.75):
        p = A.planta()
        im.paste(p, X(x) - p.w // 2, Y(-4.75) - p.h)
    return im


# ============================================================== MOSTRADOR
def mostrador():
    # 1.6 de alto: 0.6 de aire arriba, tapa (0.4) y frente (0.6).
    im = Img(W, 51)
    aire = 19
    top = 32
    im.hline(0, aire, W, O)
    im.rect(0, aire + 1, W, top - aire - 1, A.M2)
    im.hline(0, aire + 1, W, A.M1)
    im.hline(0, top - 2, W, A.M3)
    for x in range(6, W, 37):
        im.hline(x, aire + 5, 7, '#c9a2b4')
    im.hline(0, top - 1, W, A.M4)
    im.hline(0, top, W, O)
    im.rect(0, top + 1, W, 51 - top - 1, A.M4)
    for x0 in range(2, W, 16):
        im.rect(x0, top + 3, 12, 11, A.M5)
        im.hline(x0, top + 3, 12, A.M3)
        im.vline(x0, top + 3, 11, A.M3)
    im.hline(0, 48, W, '#3a2230')
    im.hline(0, 49, W, A.G3)
    im.hline(0, 50, W, O)
    # caja registradora y frasco de medialunas (lejos de las bandejas en x −5, 0, 5)
    caja = A.caja_registradora()
    im.paste(caja, X(-7.75) - caja.w // 2, top - caja.h + 4)
    fr = frasco()
    im.paste(fr, X(-6.55) - fr.w // 2, top - fr.h + 3)
    return im


# ========================================================== MÁQUINAS
MW, MH = 70, 52   # 2.19 x 1.63 unidades, pivote abajo al centro


def cafetera():
    im = Img(MW, MH)
    d = Img(MW, MH)
    # tazas arriba (calientatazas)
    for i, tx in enumerate((14, 24, 34, 44)):
        c = [A.C1, A.P2, A.C1, A.B2][i]
        d.rect(tx, 2, 7, 6, c)
        d.hline(tx, 2, 7, A.C3)
    # cuerpo
    d.rrect(6, 8, 58, 32, A.Q2, 2)
    d.rect(6, 8, 58, 5, A.Q1)
    d.hline(8, 9, 54, '#c6f0e8')
    d.rect(6, 34, 58, 6, A.Q3)
    d.vline(7, 13, 20, A.Q1)
    d.vline(62, 13, 20, A.Q3)
    # panel cromado con manómetro
    d.rrect(24, 14, 22, 11, A.G1, 1)
    d.ellipse(35, 19.5, 4.5, 4.5, A.C1)
    d.set(35, 18, A.R2); d.set(36, 17, A.R2)
    d.rect(14, 18, 4, 4, A.R1); d.rect(52, 18, 4, 4, A.Q4)
    # grupos y tazas
    for gx in (13, 47):
        d.rect(gx, 26, 10, 4, A.G3)
        d.rect(gx + 3, 30, 4, 3, A.G4)
    # bandeja de goteo con tazas
    d.rect(4, 44, 62, 6, A.G3)
    d.hline(4, 44, 62, A.G2)
    for gx in (13, 47):
        d.rect(gx + 1, 37, 8, 7, A.C1)
        d.hline(gx + 1, 37, 8, A.K2)
        d.rect(gx + 9, 39, 2, 3, A.C1)
    # vaporizador
    d.vline(66, 16, 22, A.G3); d.rect(65, 37, 3, 3, A.G4)
    d.outline(O)
    im.paste(d, 0, 0)
    return im


def moledora():
    im = Img(MW, MH)
    d = Img(MW, MH)
    # tolva de vidrio con granos
    d.rect(23, 2, 24, 18, '#d9e8ef')
    d.rect(25, 6, 20, 14, A.K1)
    for x, y in [(26, 7), (31, 9), (36, 7), (41, 10), (28, 13), (34, 15), (40, 15), (37, 12), (30, 17)]:
        d.rect(x, y, 3, 2, A.K2)
        d.set(x, y, '#a06a40')
    d.vline(24, 3, 16, '#ffffff')
    d.rect(21, 0, 28, 3, A.R2)
    # cuerpo rojo
    d.rrect(20, 20, 30, 22, A.R1, 2)
    d.vline(22, 22, 18, A.R0)
    d.rect(20, 37, 30, 5, A.R2)
    d.rrect(27, 24, 16, 8, A.C2, 1)
    d.rect(31, 27, 8, 2, A.K2)
    # manija
    d.rect(50, 26, 8, 2, A.G3); d.rect(57, 22, 3, 6, A.K2)
    # boca y vasito
    d.rect(31, 42, 8, 3, A.G4)
    d.rect(29, 45, 12, 6, A.C1)
    d.rect(29, 45, 12, 2, A.K1)
    # bolsa de granos y tazón
    d.rrect(3, 30, 15, 21, A.C4, 2)
    d.rect(5, 27, 11, 4, '#d8b070')
    d.ellipse(10.5, 41, 4, 3, A.K1)
    d.vline(10, 39, 4, A.K2)
    d.rect(55, 42, 13, 9, A.B2)
    d.hline(55, 42, 13, A.K1)
    d.outline(O)
    im.paste(d, 0, 0)
    return im


def chocolatera():
    im = Img(MW, MH)
    d = Img(MW, MH)
    # hornalla
    d.rect(8, 42, 54, 9, A.G4)
    d.hline(9, 42, 52, A.G3)
    d.rect(12, 46, 4, 3, A.R1); d.rect(54, 46, 4, 3, A.R1)
    # olla de cobre
    d.rrect(12, 14, 46, 29, A.CU2, 2)
    d.rect(12, 14, 46, 7, A.CU3)
    d.vline(15, 21, 19, A.CU1)
    d.vline(16, 22, 15, A.CU1)
    d.hline(14, 41, 42, A.CU3)
    d.ellipse(35, 15, 22, 4.5, A.K1)
    d.ellipse(33, 14.5, 14, 2.5, '#8f5a36')
    d.set(40, 15, '#a06a40'); d.set(28, 14, '#a06a40')
    # manijas
    d.rect(5, 20, 7, 4, A.K2); d.rect(58, 20, 7, 4, A.K2)
    # cucharón
    d.vline(46, 2, 13, A.W3); d.rect(45, 1, 3, 3, A.W1)
    d.outline(O)
    im.paste(d, 0, 0)
    # vapor
    for x, y in [(26, 6), (27, 5), (26, 4), (36, 7), (37, 6), (36, 5), (42, 5), (43, 4)]:
        im.set(x, y, A.G1)
    return im


# ======================================================= REPISA (isla)
def repisa():
    # Mesa de trabajo vista desde arriba a 3/4: tapa, frente y patas (64x40).
    im = Img(64, 40)
    im.rect(1, 1, 62, 25, A.W1)
    im.hline(1, 1, 62, A.W0)
    im.hline(1, 25, 62, A.W2)
    for x, y in [(8, 6), (40, 18), (24, 12), (50, 8)]:
        im.hline(x, y, 7, '#c98e56')
    im.rect(1, 26, 62, 7, A.W3)
    im.hline(1, 26, 62, A.W4)
    im.rect(3, 33, 5, 7, A.W4); im.rect(56, 33, 5, 7, A.W4)
    # posavasos donde se apoyan las cosas (a ±0.5 del centro)
    for cx in (16, 48):
        im.ellipse(cx, 13, 9, 5.5, A.C3)
        im.ellipse(cx, 13, 6, 3, A.C1)
    im.frame(0, 0, 64, 34, O)
    im.frame(2, 33, 7, 7, O); im.frame(55, 33, 7, 7, O)
    return im


# ===================================================== FILA DE ABAJO
# Mesada con cajones abajo y el aparato encima, como antes. Pivote ARRIBA.
FW, FH = 84, 80        # 2.625 x 2.5 unidades
MUEBLE_Y = 52          # donde empieza la tapa del mueble


def mueble_fila(im, ancho, y0=MUEBLE_Y):
    h = im.h
    im.rect(1, y0, ancho - 2, 5, A.W1)
    im.hline(1, y0, ancho - 2, A.W0)
    im.hline(1, y0 + 4, ancho - 2, A.W2)
    im.rect(1, y0 + 5, ancho - 2, h - y0 - 6, A.W3)
    n = 2 if ancho > 60 else 1
    w = (ancho - 8 - (n - 1) * 4) // n
    for i in range(n):
        x0 = 4 + i * (w + 4)
        for y in (y0 + 8, y0 + 18):
            im.rect(x0, y, w, 7, A.W2)
            im.hline(x0, y, w, A.W1)
            im.frame(x0, y, w, 7, A.W4)
            im.rect(x0 + w // 2 - 3, y + 3, 6, 1, O)
    im.frame(0, y0 - 1, ancho, h - y0 + 1, O)


def en_mueble(aparato):
    im = Img(FW, FH)
    mueble_fila(im, FW)
    ap = aparato()
    im.paste(ap, (FW - ap.w) // 2, MUEBLE_Y + 3 - ap.h)
    return im


def moledora_fila():
    return en_mueble(moledora)


def cafetera_fila():
    return en_mueble(cafetera)


def chocolatera_fila():
    return en_mueble(chocolatera)


def repisa_fila():
    # Mesada angosta con un estante de dos lugares encima (58 x 80, 1.81 x 2.5 u).
    im = Img(58, FH)
    mueble_fila(im, 58)
    est = Img(58, FH)
    est.rect(3, 6, 52, 48, A.W2)
    est.rect(1, 3, 56, 5, A.W1)
    est.hline(1, 3, 56, A.W0)
    for x0 in (6, 31):
        est.rect(x0, 12, 21, 36, A.W4)
        est.hline(x0, 12, 21, A.W5)
        est.ellipse(x0 + 10.5, 45, 8, 2.6, A.C3)
        est.ellipse(x0 + 10.5, 45, 5, 1.4, A.C1)
    est.outline(O)
    im.paste(est, 0, 0)
    return im


# ================================================================ HUD
def moneda():
    # Moneda dorada para el contador de propinas (16x16).
    im = Img(16, 16)
    im.ellipse(8, 8, 7.5, 7.5, A.Y2)
    im.ellipse(8, 8, 5.5, 5.5, A.Y1)
    im.ellipse(8.5, 8.5, 4.5, 4.5, A.Y2)
    im.rect(7, 5, 2, 7, A.Y3)
    im.rect(6, 6, 4, 1, A.Y3)
    im.rect(6, 10, 4, 1, A.Y3)
    im.set(5, 4, '#ffffff')
    im.outline(A.O)
    return im
