# Dibuja una vista previa aproximada de la escena (lo que ve la cámara) + la barra del HUD.
import re, os, sys, glob
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import px

ASSETS = r'C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2\Assets'
escena = open(sys.argv[1], encoding='utf-8').read()
docs = re.split(r'\n(?=--- !u!)', escena)[1:]
by = {}
for d in docs:
    m = re.match(r'--- !u!(\d+) &(-?\d+)', d)
    by[m.group(2)] = (m.group(1), d)

# sprites por guid
sprites = {}
for meta in glob.glob(os.path.join(ASSETS, 'Sprites', '*', '*.png.meta')):
    g = open(meta).read().split('guid: ')[1].split()[0]
    ppu = int(re.search(r'spritePixelsToUnits: (\d+)', open(meta).read()).group(1))
    if os.path.exists(meta[:-5]):
        sprites[g] = (px.load(meta[:-5]), ppu)

trans = {}
for fid, (c, d) in by.items():
    if c == '4':
        g = re.search(r'm_GameObject: \{fileID: (\d+)', d).group(1)
        p = re.search(r'm_LocalPosition: \{x: (.*?), y: (.*?),', d).groups()
        f = re.search(r'm_Father: \{fileID: (\d+)', d).group(1)
        trans[g] = (fid, float(p[0]), float(p[1]), f)
tid = {v[0]: (k, v) for k, v in trans.items()}


def mundo(g):
    x = y = 0
    fid, lx, ly, f = trans[g]
    x, y = lx, ly
    while f != '0' and f in tid:
        _, (fid, lx, ly, f) = tid[f]
        x += lx; y += ly
    return x, y


def activo(g):
    d = by[g][1]
    if 'm_IsActive: 0' in d:
        return False
    fid, _, _, f = trans[g]
    while f != '0' and f in tid:
        gg, (fid, _, _, f) = tid[f]
        if 'm_IsActive: 0' in by[gg][1]:
            return False
    return True


K = 32
camx, camy, size = 0, 0.75, 5.25
W, H = int(size * 2 * 16 / 9 * K), int(size * 2 * K)
img = px.Img(W, H, (90, 48, 24, 255))
items = []
for fid, (c, d) in by.items():
    if c != '212':
        continue
    g = re.search(r'm_GameObject: \{fileID: (\d+)', d).group(1)
    nombre = re.search(r'm_Name: (.*)', by[g][1]).group(1)
    if g not in trans or not activo(g):
        continue
    sg = re.search(r'm_Sprite: \{fileID: -?\d+, guid: (\w+)', d)
    if not sg or sg.group(1) not in sprites:
        continue
    orden = int(re.search(r'm_SortingOrder: (-?\d+)', d).group(1))
    tiled = 'm_DrawMode: 2' in d
    sz = re.search(r'm_Size: \{x: (.*?), y: (.*?)\}', d).groups()
    items.append((orden, g, sprites[sg.group(1)], tiled, float(sz[0]), float(sz[1])))
items.sort(key=lambda t: t[0])
for orden, g, (s, ppu), tiled, sw, sh in items:
    wx, wy = mundo(g)
    k = K / ppu
    if tiled:
        tw, th = int(sw * K), int(sh * K)
        x0 = int((wx - camx) * K + W / 2 - tw / 2)
        y0 = int(H / 2 - (wy - camy) * K - th / 2)
        for j in range(th):
            for i in range(tw):
                c = s.get(int((i - tw / 2) % s.w), int((j - th / 2) % s.h))
                if c[3]:
                    img.set(x0 + i, y0 + j, c)
    else:
        ss = s if k == 1 else None
        if ss is None:
            continue
        x0 = int((wx - camx) * K + W / 2 - s.w / 2)
        y0 = int(H / 2 - (wy - camy) * K - s.h / 2)
        img.paste(s, x0, y0)
if len(sys.argv) > 3 and sys.argv[3] == 'sinhud':
    img.save(sys.argv[2]); print(W, H); sys.exit(0)
# Cliente1 de ejemplo ya está; barra del HUD (108/1080 de la altura)
bh = int(H * 108 / 1080)
madera = px.load(os.path.join(ASSETS, 'Sprites', 'HUD', 'BarraMadera.png'))
for j in range(bh):
    for i in range(W):
        jj = 0 if j < 6 else (31 - (bh - 1 - j) if bh - 1 - j < 6 else 6 + (j % 20))
        ii = i if i < 6 else (31 - (W - 1 - i) if W - 1 - i < 6 else 6 + (i % 20))
        img.set(i, j, madera.get(ii, jj))
img.scaled(2).save(sys.argv[2])
print(W, H)
