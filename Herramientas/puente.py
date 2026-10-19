# Cliente del puente de pruebas (Packages/com.claude.puente).
# Uso:  python puente.py "jugar" "velocidad 3" "esperar 2" "captura inicio" "estado"
#       python puente.py -f archivo_con_comandos.txt
# Escribe PuenteClaude/comando.txt, espera a que Unity lo ejecute y muestra resultado.txt.
import os, sys, time, random
sys.stdout.reconfigure(encoding='utf-8')

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CARPETA = os.path.join(RAIZ, 'PuenteClaude')
COMANDO = os.path.join(CARPETA, 'comando.txt')
RESULTADO = os.path.join(CARPETA, 'resultado.txt')
PENDIENTE = os.path.join(CARPETA, 'pendiente.txt')

args = sys.argv[1:]
timeout = 180
if args and args[0].startswith('--timeout='):
    timeout = float(args[0].split('=')[1])
    args = args[1:]
if args and args[0] == '-f':
    comandos = [l.rstrip('\n') for l in open(args[1], encoding='utf-8')]
else:
    comandos = args

os.makedirs(CARPETA, exist_ok=True)
if os.path.exists(PENDIENTE):
    print('Ojo: Unity todavía está ejecutando un pedido anterior (existe pendiente.txt).')
ident = str(random.randint(100000, 999999))
tmp = COMANDO + '.tmp'
with open(tmp, 'w', encoding='utf-8', newline='\n') as f:
    f.write('id ' + ident + '\n' + '\n'.join(comandos) + '\n')
os.replace(tmp, COMANDO)

inicio = time.time()
while True:
    if os.path.exists(RESULTADO):
        try:
            texto = open(RESULTADO, encoding='utf-8').read()
        except OSError:
            texto = ''
        if texto.startswith('id ' + ident) and ('FIN ' + ident) in texto:
            print(texto.strip())
            print('(%.1f s)' % (time.time() - inicio))
            break
    if time.time() - inicio > timeout:
        estado = 'Unity no tomó el comando (¿está cerrado o compilando?)' if os.path.exists(COMANDO) else 'Unity sigue ejecutando'
        print('TIMEOUT después de %d s: %s' % (timeout, estado))
        if os.path.exists(os.path.join(CARPETA, 'parcial.txt')):
            print(open(os.path.join(CARPETA, 'parcial.txt'), encoding='utf-8').read())
        sys.exit(1)
    time.sleep(0.2)
