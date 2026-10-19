# Genera los efectos de sonido (estilo chiptune) en Assets/Audio, con su .meta (el .meta primero).
#   python sonidos.py
import math, os, random, struct, uuid, wave, re

RAIZ = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SALIDA = os.path.join(RAIZ, 'Assets', 'Audio')
SR = 22050
random.seed(7)


def onda(tipo, fase):
    f = fase % 1.0
    if tipo == 'seno':
        return math.sin(2 * math.pi * f)
    if tipo == 'cuadrada':
        return 1.0 if f < 0.5 else -1.0
    if tipo == 'triangulo':
        return 4 * f - 1 if f < 0.5 else 3 - 4 * f
    if tipo == 'sierra':
        return 2 * f - 1
    raise ValueError(tipo)


def nota(frec, dur, tipo='triangulo', vol=0.5, ataque=0.005, caida=None, frec_fin=None, vibrato=0.0):
    n = int(SR * dur)
    caida = dur if caida is None else caida
    out, fase = [], 0.0
    for i in range(n):
        t = i / SR
        f = frec if frec_fin is None else frec + (frec_fin - frec) * (i / n)
        if vibrato:
            f *= 1 + vibrato * math.sin(2 * math.pi * 7 * t)
        fase += f / SR
        env = min(1.0, t / ataque) * max(0.0, 1 - t / caida)
        out.append(vol * env * onda(tipo, fase))
    return out


def ruido(dur, vol=0.3, caida=None):
    n = int(SR * dur)
    caida = dur if caida is None else caida
    out, prev = [], 0.0
    for i in range(n):
        t = i / SR
        prev = prev * 0.6 + random.uniform(-1, 1) * 0.4      # ruido un poco filtrado
        out.append(vol * prev * max(0.0, 1 - t / caida))
    return out


def mezclar(*pistas):
    # pistas: (inicio_en_segundos, muestras)
    largo = max(int(SR * ini) + len(m) for ini, m in pistas)
    out = [0.0] * largo
    for ini, m in pistas:
        k = int(SR * ini)
        for i, v in enumerate(m):
            out[k + i] += v
    return out


def seq(*pistas):
    return mezclar(*pistas)


N = {'C4': 261.6, 'D4': 293.7, 'Eb4': 311.1, 'E4': 329.6, 'G4': 392.0, 'A4': 440.0, 'B4': 493.9,
     'C5': 523.3, 'D5': 587.3, 'E5': 659.3, 'G5': 784.0, 'A5': 880.0, 'B5': 987.8, 'C6': 1046.5, 'E6': 1318.5, 'G6': 1568.0}

SONIDOS = {
    'Agarrar': lambda: nota(520, 0.09, 'cuadrada', 0.25, frec_fin=880),
    'Apoyar': lambda: mezclar((0, nota(180, 0.07, 'triangulo', 0.6, frec_fin=120)), (0, ruido(0.03, 0.25))),
    'MaquinaLista': lambda: seq((0, nota(N['C6'], 0.35, 'triangulo', 0.45)), (0.1, nota(N['G6'], 0.45, 'triangulo', 0.4)),
                                (0.1, nota(N['G6'] * 2, 0.25, 'seno', 0.12))),
    'Entregar': lambda: seq((0, ruido(0.08, 0.35)), (0.05, nota(N['B5'], 0.12, 'cuadrada', 0.2)),
                            (0.13, nota(N['E6'], 0.4, 'cuadrada', 0.2)), (0.13, nota(N['E6'] * 1.5, 0.3, 'triangulo', 0.15))),
    'Error': lambda: nota(200, 0.18, 'cuadrada', 0.22, frec_fin=130),
    'Tirar': lambda: mezclar((0, ruido(0.32, 0.45)), (0, nota(500, 0.3, 'seno', 0.15, frec_fin=150))),
    'ClienteEnojado': lambda: nota(320, 0.5, 'sierra', 0.22, frec_fin=150, vibrato=0.04),
    'Victoria': lambda: seq((0.00, nota(N['C5'], 0.16, 'cuadrada', 0.18)), (0.15, nota(N['E5'], 0.16, 'cuadrada', 0.18)),
                            (0.30, nota(N['G5'], 0.16, 'cuadrada', 0.18)), (0.45, nota(N['C6'], 0.7, 'cuadrada', 0.18)),
                            (0.45, nota(N['E5'], 0.7, 'triangulo', 0.25)), (0.45, nota(N['G5'], 0.7, 'triangulo', 0.2))),
    'Derrota': lambda: seq((0.00, nota(N['G4'], 0.3, 'triangulo', 0.4)), (0.28, nota(N['Eb4'], 0.3, 'triangulo', 0.4)),
                           (0.56, nota(N['C4'], 0.8, 'triangulo', 0.4, vibrato=0.01))),
}

META = '''fileFormatVersion: 2
guid: %s
AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 0
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
'''


def guardar(nombre, muestras):
    os.makedirs(SALIDA, exist_ok=True)
    ruta = os.path.join(SALIDA, nombre + '.wav')
    meta = ruta + '.meta'
    if os.path.exists(meta):
        g = re.search(r'(?m)^guid: (\w+)', open(meta).read()).group(1)
    else:
        g = uuid.uuid4().hex
    open(meta, 'w', newline='\n').write(META % g)
    pico = max(0.001, max(abs(v) for v in muestras))
    escala = 0.8 / pico
    # un poquito de silencio al final para que no "corte"
    muestras = muestras + [0.0] * int(SR * 0.02)
    with wave.open(ruta, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b''.join(struct.pack('<h', int(max(-1, min(1, v * escala)) * 32767)) for v in muestras))
    return g


if __name__ == '__main__':
    if not os.path.exists(SALIDA + '.meta'):
        open(SALIDA + '.meta', 'w', newline='\n').write(
            'fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n'
            '  assetBundleName: \n  assetBundleVariant: \n' % uuid.uuid4().hex)
    for nombre, fn in SONIDOS.items():
        g = guardar(nombre, fn())
        print(nombre, g)
