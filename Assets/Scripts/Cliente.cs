using UnityEngine;
using UnityEngine.UI;  // Permite trabajar con componentes de interfaz, como Slider.

// Representa a un cliente: guarda su pedido, lo muestra en un globo,
// controla su paciencia y avisa al GameManager cuando lo atendemos.
//
// Recorrido de un cliente:
//   1. El GameManager le asigna un pedido y llama a Entrar().
//   2. Camina hasta el mostrador (MovimientoCliente) y muestra su pedido.
//   3. Recibe el pedido correcto y se va caminando hacia la salida.
//   4. Cuando sale, espera unos segundos con el puesto libre.
//   5. Le avisa al GameManager, que decide quién entra después.
public class Cliente : MonoBehaviour
{
    // ===================== REFERENCIAS (Inspector) =====================

    // Barra visual de paciencia de este cliente.
    [SerializeField] private Slider barraPaciencia;

    // Bebida solicitada.
    [SerializeField]
    private PlayerInventory.TipoObjeto bebidaPedida =
        PlayerInventory.TipoObjeto.CafePreparado;

    // Galletita solicitada. Puede ser Ninguno.
    [SerializeField]
    private PlayerInventory.TipoObjeto galletitaPedida =
        PlayerInventory.TipoObjeto.Ninguno;

    // Referencia al controlador de la partida.
    [SerializeField] private GameManager gameManager;

    // Dónde aparece el globo del pedido respecto del cliente.
    [SerializeField] private Vector3 posicionGlobo = new Vector3(0f, 0.7f, 0f);

    // Bandeja que pertenece a este cliente.
    // La mostramos solamente cuando llega a su puesto.
    [SerializeField] private Bandeja bandejaDelCliente;

    // Tiempo base (en segundos) que el cliente está dispuesto a esperar.
    [SerializeField] private float pacienciaMaxima = 90f;

    // Segundos extra de paciencia según lo difícil que es el pedido:
    // las bebidas "con leche" llevan más pasos, y la galletita es un paso más.
    [SerializeField] private float extraConLeche = 10f;
    [SerializeField] private float extraGalletita = 6f;

    // Segundos que el puesto queda libre entre un cliente y el siguiente.
    [SerializeField] private float pausaEntreClientes = 2f;

    // ========================= ESTADO INTERNO =========================

    // Tiempo que le queda al cliente antes de perder la paciencia.
    private float pacienciaActual;

    // Paciencia total para el pedido actual (la base más los extras).
    private float pacienciaDelPedido = 1f;

    // Relleno de la barra de paciencia: le cambiamos el color según cuánto queda.
    private Image rellenoBarra;

    // Indica si el tiempo de espera está corriendo.
    private bool pacienciaActiva = false;

    // Indica que el cliente ya fue atendido y se está yendo.
    // Evita que se entregue el mismo pedido dos veces.
    private bool cambiandoCliente = false;

    // Indica si el cliente ya salió y estamos esperando la pausa.
    private bool enPausa = false;

    // Segundos que faltan para terminar la pausa.
    private float pausaRestante = 0f;

    // Referencia al script que mueve al cliente.
    private MovimientoCliente movimiento;

    // Globo que muestra el pedido arriba del cliente.
    private GloboPedido globoPedido;

    // ============================ INICIO ============================

    private void Awake()
    {
        // Buscamos el componente que controla el movimiento.
        movimiento = GetComponent<MovimientoCliente>();

        if (movimiento == null)
        {
            Debug.LogError(gameObject.name + " necesita el script MovimientoCliente.");
        }

        // Avisamos si olvidamos asignar la bandeja en el Inspector.
        if (bandejaDelCliente == null)
        {
            Debug.LogWarning("Falta asignar la bandeja de " + gameObject.name);
        }

        // Al comenzar, el cliente todavía no llegó:
        // ocultamos su pedido, su bandeja y su barra de paciencia.
        OcultarPedido();
    }

    private void Start()
    {
        // Buscamos el catálogo de dibujos una sola vez y creamos el globo.
        CatalogoObjetos catalogo = FindFirstObjectByType<CatalogoObjetos>();

        if (catalogo != null)
        {
            globoPedido = catalogo.CrearGloboPedido(transform, posicionGlobo);
        }
        else
        {
            Debug.LogWarning("Falta el CatalogoObjetos en la escena: no se verán los pedidos.");
        }

        // El Slider tiene un "relleno" (fillRect): guardamos su Image para pintarla.
        if (barraPaciencia != null && barraPaciencia.fillRect != null)
        {
            rellenoBarra = barraPaciencia.fillRect.GetComponent<Image>();
        }
    }

    // ============================ PEDIDO ============================

    // Cambia el pedido del cliente.
    public void AsignarPedido(
        PlayerInventory.TipoObjeto nuevaBebida,
        PlayerInventory.TipoObjeto nuevaGalletita)
    {
        // Guardamos los nuevos productos.
        bebidaPedida = nuevaBebida;
        galletitaPedida = nuevaGalletita;

        // Calculamos cuánta paciencia tiene para este pedido (acumulador).
        pacienciaDelPedido = pacienciaMaxima;

        if (bebidaPedida == PlayerInventory.TipoObjeto.CafeConLeche ||
            bebidaPedida == PlayerInventory.TipoObjeto.ChocolateConLeche)
        {
            pacienciaDelPedido += extraConLeche;
        }

        if (galletitaPedida != PlayerInventory.TipoObjeto.Ninguno)
        {
            pacienciaDelPedido += extraGalletita;
        }

        // Mostramos también el pedido en la consola.
        Debug.Log("Nuevo pedido: " + bebidaPedida + " + " + galletitaPedida);
    }

    // Devuelve true si la bebida y la galletita recibidas
    // son exactamente las que pidió el cliente.
    // && significa Y: ambas condiciones deben ser verdaderas.
    // Si el cliente no pidió galletita, ambos valores deben ser Ninguno.
    public bool EsPedidoCorrecto(
        PlayerInventory.TipoObjeto bebida,
        PlayerInventory.TipoObjeto galletita)
    {
        return bebida == bebidaPedida && galletita == galletitaPedida;
    }

    // ===================== LLEGADA Y SALIDA =====================

    // El cliente aparece en la entrada y camina hasta el mostrador.
    // La llama el GameManager.
    public void Entrar()
    {
        // Lo activamos por si estaba oculto.
        gameObject.SetActive(true);

        // Es un cliente nuevo: todavía no fue atendido.
        cambiandoCliente = false;
        enPausa = false;

        movimiento.Entrar();
    }

    // Muestra el pedido y la bandeja cuando el cliente llega al mostrador.
    // La llama MovimientoCliente al llegar.
    public void MostrarPedido()
    {
        // Activamos la bandeja correspondiente a este puesto.
        if (bandejaDelCliente != null)
        {
            bandejaDelCliente.gameObject.SetActive(true);
        }

        // Mostramos el pedido en el globo.
        if (globoPedido != null)
        {
            globoPedido.Mostrar(bebidaPedida, galletitaPedida);
        }

        // Solo comenzamos a descontar paciencia después del tutorial.
        if (gameManager != null && gameManager.EstaTerminadoTutorial())
        {
            IniciarPaciencia();
        }
    }

    // Devuelve la bandeja de este cliente (la usa el tutorial para señalarla).
    public Bandeja ObtenerBandeja()
    {
        return bandejaDelCliente;
    }

    // Devuelve true si el cliente está en el mostrador esperando.
    // La usa la bandeja antes de entregar el pedido.
    public bool PuedeRecibirPedido()
    {
        return movimiento.EstaEnPuesto() && !cambiandoCliente;
    }

    // Se ejecuta cuando entregamos correctamente el pedido.
    public void RecibirPedido()
    {
        // Evitamos que una misma entrega se cuente dos veces.
        if (!PuedeRecibirPedido())
        {
            return;
        }

        Debug.Log("¡Pedido entregado correctamente!");

        // Avisamos al GameManager para que sume el pedido.
        if (gameManager != null)
        {
            gameManager.PedidoCompletado(this);
        }

        // El cliente se va contento.
        Retirarse();
    }

    // Se ejecuta cuando el cliente perdió la paciencia.
    // La llama el GameManager.
    public void IrseEnojado()
    {
        Debug.Log(gameObject.name + " se fue enojado.");

        // Vaciamos su bandeja: lo que había preparado ya no sirve.
        if (bandejaDelCliente != null)
        {
            bandejaDelCliente.VaciarBandeja();
        }

        // Se va igual que cuando lo atendemos, pero sin sumar pedido.
        Retirarse();
    }

    // Lo que pasa siempre que un cliente se va,
    // ya sea contento (lo atendimos) o enojado (perdió la paciencia).
    // Así no repetimos el mismo código en dos lugares.
    private void Retirarse()
    {
        // Bloqueamos nuevas entregas mientras el cliente se va.
        cambiandoCliente = true;

        // El cliente deja de esperar: ocultamos su interfaz y su bandeja.
        DetenerPaciencia();
        OcultarPedido();

        // IMPORTANTE: damos la orden de caminar hacia la salida.
        // Cuando llegue, MovimientoCliente llamará a TerminoDeSalir().
        movimiento.Salir();
    }

    // Se ejecuta cuando el cliente terminó de salir.
    // La llama MovimientoCliente al llegar a la salida.
    public void TerminoDeSalir()
    {
        // Empezamos la pausa con el puesto libre.
        enPausa = true;
        pausaRestante = pausaEntreClientes;
    }

    // Oculta el globo del pedido, la bandeja y la barra de paciencia.
    private void OcultarPedido()
    {
        // En Awake el globo todavía no existe (se crea en Start),
        // por eso comprobamos que no sea null.
        if (globoPedido != null)
        {
            globoPedido.Ocultar();
        }

        if (bandejaDelCliente != null)
        {
            bandejaDelCliente.gameObject.SetActive(false);
        }

        if (barraPaciencia != null)
        {
            barraPaciencia.gameObject.SetActive(false);
        }
    }

    // =========================== PACIENCIA ===========================

    // Inicia o reinicia el tiempo de espera del cliente.
    public void IniciarPaciencia()
    {
        // Recupera toda su paciencia y activa el temporizador.
        pacienciaActual = pacienciaDelPedido;
        pacienciaActiva = true;

        // Hacemos visible la barra y la llenamos.
        // 1 representa el 100% de paciencia.
        if (barraPaciencia != null)
        {
            barraPaciencia.gameObject.SetActive(true);
            barraPaciencia.value = 1f;
        }

        PintarBarra(1f);

        Debug.Log(gameObject.name + " empieza con " + pacienciaActual + " segundos.");
    }

    // Detiene el temporizador del cliente.
    public void DetenerPaciencia()
    {
        pacienciaActiva = false;
    }

    // Devuelve cuánta paciencia le queda, de 0 (nada) a 1 (toda).
    // La usa el GameManager para calcular la propina.
    public float ObtenerPorcentajePaciencia()
    {
        return pacienciaActual / pacienciaDelPedido;
    }

    // Pinta la barra según cuánta paciencia queda:
    // verde (más de la mitad), amarillo (más de un cuarto) o rojo.
    private void PintarBarra(float porcentaje)
    {
        if (rellenoBarra == null)
        {
            return;
        }

        if (porcentaje > 0.5f)
        {
            rellenoBarra.color = new Color(0.45f, 0.8f, 0.35f);
        }
        else if (porcentaje > 0.25f)
        {
            rellenoBarra.color = new Color(0.95f, 0.76f, 0.3f);
        }
        else
        {
            rellenoBarra.color = new Color(0.85f, 0.3f, 0.25f);
        }
    }

    // ============================ UPDATE ============================

    // Se ejecuta una vez por frame. Lleva dos temporizadores:
    // la pausa después de salir y la paciencia en el mostrador.
    private void Update()
    {
        ActualizarPausa();
        ActualizarPaciencia();
    }

    // Cuenta la pausa entre que el cliente sale y llega el siguiente.
    private void ActualizarPausa()
    {
        if (!enPausa)
        {
            return;
        }

        pausaRestante -= Time.deltaTime;

        if (pausaRestante <= 0f)
        {
            enPausa = false;

            // Avisamos que el puesto quedó libre.
            // El GameManager decide qué cliente entra después.
            if (gameManager != null)
            {
                gameManager.ClienteSeFue(this);
            }
        }
    }

    // Descuenta la paciencia del cliente mientras espera su pedido.
    private void ActualizarPaciencia()
    {
        // Si la paciencia no está activa, no hacemos nada.
        if (!pacienciaActiva)
        {
            return;
        }

        // Restamos el tiempo transcurrido desde el último frame.
        // Mathf.Max evita que la paciencia quede negativa.
        pacienciaActual -= Time.deltaTime;
        pacienciaActual = Mathf.Max(pacienciaActual, 0f);

        // Actualizamos visualmente la barra de paciencia.
        // Por ejemplo: 45 / 90 = 0.5 = 50%.
        float porcentaje = pacienciaActual / pacienciaDelPedido;

        if (barraPaciencia != null)
        {
            barraPaciencia.value = porcentaje;
        }

        PintarBarra(porcentaje);

        // Si la paciencia llegó a cero...
        if (pacienciaActual <= 0f)
        {
            // Detenemos el temporizador para no avisar más de una vez.
            pacienciaActiva = false;

            Debug.Log(gameObject.name + " perdió la paciencia.");

            // Avisamos al GameManager que este cliente se cansó.
            if (gameManager != null)
            {
                gameManager.ClienteSinPaciencia(this);
            }
        }
    }
}
