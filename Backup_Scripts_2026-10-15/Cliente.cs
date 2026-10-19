using System.Collections; // Permite utilizar IEnumerator para las corrutinas.
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Permite trabajar con componentes de interfaz, como Slider.

// Representa a un cliente y guarda su pedido.

/// <summary>
/// Defines the <see cref="Cliente" />
/// </summary>
public class Cliente : MonoBehaviour
{
    // Referencia a la barra visual de paciencia de este cliente.
    // Slider es un componente de la interfaz de Unity.

    /// <summary>
    /// Defines the barraPaciencia
    /// </summary>
    [SerializeField] private Slider barraPaciencia;

    // Bebida solicitada.

    /// <summary>
    /// Defines the bebidaPedida
    /// </summary>
    [SerializeField]
    private PlayerInventory.TipoObjeto bebidaPedida =
        PlayerInventory.TipoObjeto.CafePreparado;

    // Galletita solicitada. Puede ser Ninguno.

    /// <summary>
    /// Defines the galletitaPedida
    /// </summary>
    [SerializeField]
    private PlayerInventory.TipoObjeto galletitaPedida =
        PlayerInventory.TipoObjeto.Ninguno;

    // Referencia al controlador de la partida.

    /// <summary>
    /// Defines the gameManager
    /// </summary>
    [SerializeField] private GameManager gameManager;

    // Referencia al texto que muestra el pedido en pantalla.

    /// <summary>
    /// Defines the textoPedido
    /// </summary>
    [SerializeField] private TMP_Text textoPedido;

    // Bandeja que pertenece a este cliente.
    // La mostraremos solamente cuando llegue a su puesto.

    /// <summary>
    /// Defines the bandejaDelCliente
    /// </summary>
    [SerializeField] private Bandeja bandejaDelCliente;

    // Tiempo máximo que el cliente está dispuesto a esperar.
    // Podemos modificarlo desde el Inspector.

    /// <summary>
    /// Defines the pacienciaMaxima
    /// </summary>
    [SerializeField] private float pacienciaMaxima = 90f;

    // Tiempo que le queda al cliente.
    // No usamos SerializeField porque lo inicializamos por código.

    /// <summary>
    /// Defines the pacienciaActual
    /// </summary>
    private float pacienciaActual;

    // Indica si el tiempo de espera está corriendo.

    /// <summary>
    /// Defines the pacienciaActiva
    /// </summary>
    private bool pacienciaActiva = false;

    // Evita procesar el agotamiento de paciencia varias veces.

    /// <summary>
    /// Defines the tiempoAgotado
    /// </summary>
    private bool tiempoAgotado = false;

    // Referencia al script que mueve al cliente.

    /// <summary>
    /// Defines the movimiento
    /// </summary>
    private MovimientoCliente movimiento;

    // Tiempo que el puesto permanece libre entre clientes.

    /// <summary>
    /// Defines the pausaEntreClientes
    /// </summary>
    [SerializeField] private float pausaEntreClientes = 2f;

    // Indica si el cliente está saliendo o siendo reemplazado.
    // Evita entregar el mismo pedido varias veces.

    /// <summary>
    /// Defines the cambiandoCliente
    /// </summary>
    private bool cambiandoCliente = false;

    // Propiedad de solo lectura para consultar la bebida.

    /// <summary>
    /// Gets the BebidaPedida
    /// </summary>
    public PlayerInventory.TipoObjeto BebidaPedida
    {
        get { return bebidaPedida; }
    }

    // Propiedad de solo lectura para consultar la galletita.

    /// <summary>
    /// Gets the GalletitaPedida
    /// </summary>
    public PlayerInventory.TipoObjeto GalletitaPedida
    {
        get { return galletitaPedida; }
    }

    // Cambia el pedido del cliente.

    /// <summary>
    /// The Awake
    /// </summary>
    private void Awake()
    {
        // Buscamos el componente que controla el movimiento.
        movimiento = GetComponent<MovimientoCliente>();

        // Si el cliente tiene movimiento, ocultamos su pedido
        // antes de que empiece a entrar caminando.
        if (movimiento != null && textoPedido != null)
        {
            textoPedido.gameObject.SetActive(false);
        }

        // También ocultamos la barra de paciencia.
        // Solamente debería mostrarse cuando llegue al puesto.
        if (movimiento != null && barraPaciencia != null)
        {
            barraPaciencia.gameObject.SetActive(false);
        }

        // Ocultamos la bandeja mientras el cliente no llegó.
        if (bandejaDelCliente != null && movimiento != null)
        {
            bandejaDelCliente.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// The AsignarPedido
    /// </summary>
    /// <param name="nuevaBebida">The nuevaBebida<see cref="PlayerInventory.TipoObjeto"/></param>
    /// <param name="nuevaGalletita">The nuevaGalletita<see cref="PlayerInventory.TipoObjeto"/></param>
    public void AsignarPedido(
        PlayerInventory.TipoObjeto nuevaBebida,
        PlayerInventory.TipoObjeto nuevaGalletita)
    {
        // Guardamos los nuevos productos.
        bebidaPedida = nuevaBebida;
        galletitaPedida = nuevaGalletita;

        // Actualizamos el texto que aparece en pantalla.
        ActualizarTextoPedido();

        // Mostramos también el pedido en la consola.
        Debug.Log("Nuevo pedido: " + bebidaPedida +
                  " + " + galletitaPedida);
    }

    // Actualiza el cartel visual del pedido.

    /// <summary>
    /// The ActualizarTextoPedido
    /// </summary>
    private void ActualizarTextoPedido()
    {
        // Si no asignamos un texto, no intentamos modificarlo.
        if (textoPedido == null)
        {
            return;
        }

        // Convertimos el enum de la bebida en un nombre legible.
        string nombreBebida = NombreProducto(bebidaPedida);

        // Empezamos el cartel mostrando la bebida.
        string pedido = "Pedido: " + nombreBebida;

        // Si también pidió galletita, la agregamos al texto.
        if (galletitaPedida != PlayerInventory.TipoObjeto.Ninguno)
        {
            pedido += " + " + NombreProducto(galletitaPedida);
        }

        // Cambiamos el texto visible en Unity.
        textoPedido.text = pedido;
    }

    // Convierte los valores del enum en nombres más fáciles de leer.

    /// <summary>
    /// The NombreProducto
    /// </summary>
    /// <param name="producto">The producto<see cref="PlayerInventory.TipoObjeto"/></param>
    /// <returns>The <see cref="string"/></returns>
    private string NombreProducto(PlayerInventory.TipoObjeto producto)
    {
        switch (producto)
        {
            case PlayerInventory.TipoObjeto.CafePreparado:
                return "Café";

            case PlayerInventory.TipoObjeto.CafeConLeche:
                return "Café con leche";

            case PlayerInventory.TipoObjeto.ChocolatePreparado:
                return "Chocolate";

            case PlayerInventory.TipoObjeto.ChocolateConLeche:
                return "Chocolate con leche";

            case PlayerInventory.TipoObjeto.GalletitaRedVelvet:
                return "Galletita red velvet";

            case PlayerInventory.TipoObjeto.GalletitaVainillaChocolate:
                return "Galletita vainilla/chocolate";

            default:
                return "Desconocido";
        }
    }

    // Muestra el pedido y la bandeja cuando el cliente llega al mostrador.
    public void MostrarPedido()
    {
        // Activamos la bandeja correspondiente a este puesto.
        if (bandejaDelCliente != null)
        {
            bandejaDelCliente.gameObject.SetActive(true);
        }

        // Mostramos el pedido actualizado.
        if (textoPedido != null)
        {
            ActualizarTextoPedido();
            textoPedido.gameObject.SetActive(true);
        }

        // Solo comenzamos a descontar paciencia después del tutorial.
        if (gameManager != null && gameManager.TutorialTerminado)
        {
            IniciarPaciencia();
        }
    }

    // Se ejecuta cuando entregamos correctamente el pedido.
    public void RecibirPedido()
    {
        // Evitamos que una entrega active varias corrutinas a la vez.
        if (cambiandoCliente)
        {
            return;
        }

        Debug.Log("¡Pedido entregado correctamente!");

        // Conservamos el funcionamiento anterior si no hay movimiento.
        if (movimiento == null)
        {
            if (gameManager != null)
            {
                gameManager.PedidoCompletado(this);
            }

            return;
        }

        // El cliente se retira y luego se prepara el siguiente pedido.
        StartCoroutine(ReemplazarCliente());
    }

    // Gestiona una sola salida, pausa y nueva entrada por pedido.
    private IEnumerator ReemplazarCliente()
    {
        // Bloqueamos nuevas entregas mientras se cambia de cliente.
        cambiandoCliente = true;

        // El cliente deja de esperar: ocultamos su interfaz y su bandeja.
        DetenerPaciencia();
        if (textoPedido != null)
        {
            textoPedido.gameObject.SetActive(false);
        }

        if (bandejaDelCliente != null)
        {
            bandejaDelCliente.gameObject.SetActive(false);
        }

        if (barraPaciencia != null)
        {
            barraPaciencia.gameObject.SetActive(false);
        }


        // Esperamos a que el cliente termine de salir.
        yield return new WaitUntil(() => movimiento.EstaEnSalida);

        // Dejamos pasar unos segundos antes de continuar.
        yield return new WaitForSeconds(pausaEntreClientes);

        // Registramos el pedido terminado.
        // GameManager decidirá qué cliente debe aparecer después.
        if (gameManager != null)
        {
            gameManager.PedidoCompletado(this);
        }

        // Durante el tutorial, este cliente no vuelve a entrar.
        // El siguiente pedido lo hará otro cliente.
        if (gameManager != null && !gameManager.TutorialTerminado)
        {
            cambiandoCliente = false;
            yield break;
        }

        // Cuando termina el tutorial, mantenemos el sistema
        // de clientes que salen y vuelven a entrar.
        movimiento.Entrar();

        // Esperamos hasta que llegue al mostrador.
        yield return new WaitUntil(() => movimiento.EnPuesto);

        // Permitimos atender nuevamente a este cliente.
        cambiandoCliente = false;


        // Dejamos el puesto desocupado durante unos segundos.
        yield return new WaitForSeconds(pausaEntreClientes);

        // Registramos el pedido para preparar el siguiente antes de entrar.
        if (gameManager != null)
        {
            gameManager.PedidoCompletado(this);
        }

        // El nuevo cliente entra y el movimiento mostrará su pedido al llegar.
        movimiento.Entrar();
        yield return new WaitUntil(() => movimiento.EnPuesto);

        // Permitimos nuevamente la entrega del pedido.
        cambiandoCliente = false;
    }

    // Inicia o reinicia la paciencia de este cliente.

    // Inicia o reinicia el tiempo de espera del cliente.
    public void IniciarPaciencia()
    {
        // Recupera toda su paciencia.
        pacienciaActual = pacienciaMaxima;

        // Activa el temporizador.
        pacienciaActiva = true;

        // Permite detectar nuevamente cuándo se agota.
        tiempoAgotado = false;

        // Comprobamos que haya una barra asignada.
        if (barraPaciencia != null)
        {
            // Hacemos visible la barra.
            barraPaciencia.gameObject.SetActive(true);

            // La llenamos completamente.
            // 1 representa el 100% de paciencia.
            barraPaciencia.value = 1f;
        }

        // Mostramos con cuántos segundos empieza este cliente.
        // Así podemos comprobar que la paciencia se inicializa correctamente.
        Debug.Log(gameObject.name + " empieza con " + pacienciaActual + " segundos.");

    }

    // Detiene el temporizador del cliente.
    public void DetenerPaciencia()
    {
        pacienciaActiva = false;
    }

    // Update se ejecuta una vez por fotograma.

    // Se ejecuta una vez por fotograma.

    private void Update()
    {
        // Si la paciencia no está activa o ya se agotó,
        // no seguimos ejecutando este método.
        if (!pacienciaActiva || tiempoAgotado)
        {
            return;
        }

        // Restamos el tiempo transcurrido desde el último fotograma.
        pacienciaActual -= Time.deltaTime;

        // Evitamos que la paciencia tenga valores negativos.
        pacienciaActual = Mathf.Max(pacienciaActual, 0f);

        // Actualizamos visualmente la barra de paciencia.
        if (barraPaciencia != null)
        {
            // Calculamos qué proporción de paciencia queda.
            // Por ejemplo: 45 / 90 = 0.5 = 50%.
            float porcentaje = pacienciaActual / pacienciaMaxima;

            // Actualizamos el Slider.
            barraPaciencia.value = porcentaje;
        }

        // Solamente si la paciencia llegó a cero...
        if (pacienciaActual <= 0f)
        {
            // Registramos que se agotó la paciencia.
            tiempoAgotado = true;

            // Detenemos el temporizador.
            pacienciaActiva = false;

            // Mostramos un mensaje en la Console.
            Debug.Log(gameObject.name + " perdió la paciencia.");

            // Avisamos al GameManager que este cliente se cansó.
            // IMPORTANTE: esto está DENTRO del if de paciencia <= 0.
            if (gameManager != null)
            {
                gameManager.ClienteSinPaciencia(this);
            }
        }
    }

}
