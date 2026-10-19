
using UnityEngine;
using UnityEngine.SceneManagement; // Permite volver a cargar la escena para reiniciar.

// Controla los pedidos y el progreso de la partida.
public class GameManager : MonoBehaviour
{

    // Referencia al panel que aparece cuando perdemos.
    [SerializeField] private GameObject panelDerrota;

    // Indica si la partida ya terminó.
    // Evita seguir contando pedidos o clientes perdidos.
    private bool partidaTerminada = false;

    // Referencia al panel que aparecerá cuando ganemos.
    // Lo asignaremos desde el Inspector de Unity.
    [SerializeField] private GameObject panelVictoria;

    // Cantidad de pedidos correctos necesarios para ganar.
    // Podemos modificarla desde el Inspector.
    [SerializeField] private int pedidosParaGanar = 15;


    // Referencias a los dos primeros clientes.
    [SerializeField] private Cliente cliente1;
    [SerializeField] private Cliente cliente2;

    // Referencia al tercer cliente.
    [SerializeField] private Cliente cliente3;

    // Bandeja que corresponde al tercer cliente.
    [SerializeField] private GameObject bandeja3;

    // Texto que muestra el pedido del tercer cliente.
    [SerializeField] private GameObject textoPedido3;

    // Cantidad de pedidos necesarios para desbloquear
    // el tercer puesto. Podemos modificarla desde el Inspector.
    [SerializeField] private int pedidosParaTercerCliente = 10;

    // Evita activar el tercer puesto más de una vez.
    private bool tercerClienteActivado = false;


    // Objetos que se activarán al terminar el tutorial.
    [SerializeField] private GameObject bandeja2;
    [SerializeField] private GameObject textoPedido2;

    // Cantidad total de pedidos entregados correctamente.
    [SerializeField] private int pedidosCompletados = 0;

    // Referencias a las bandejas de los tres clientes.
    // Nos permiten vaciar la bandeja correcta cuando alguien se va.
    [SerializeField] private Bandeja bandeja1;
    [SerializeField] private Bandeja bandejaCliente2;
    [SerializeField] private Bandeja bandejaCliente3;

    // Lleva la cuenta de cuántos clientes perdieron la paciencia.
    [SerializeField] private int clientesPerdidos = 0;

    // Cantidad máxima de clientes que podemos perder.
    [SerializeField] private int maximoClientesPerdidos = 3;


    // Indica si ya terminamos los tres pedidos iniciales.
    private bool tutorialTerminado = false;


    // Permite consultar si terminó el tutorial.
    // Otros scripts pueden leer este valor, pero no modificarlo.
    public bool TutorialTerminado
    {
        get { return tutorialTerminado; }
    }


    // Lista de bebidas que pueden aparecer en pedidos aleatorios.
    private PlayerInventory.TipoObjeto[] bebidas =
    {
        PlayerInventory.TipoObjeto.CafePreparado,
        PlayerInventory.TipoObjeto.CafeConLeche,
        PlayerInventory.TipoObjeto.ChocolatePreparado,
        PlayerInventory.TipoObjeto.ChocolateConLeche
    };

    // Lista de opciones de galletitas.
    // Ninguno permite generar pedidos sin galletita.
    private PlayerInventory.TipoObjeto[] galletitas =
    {
        PlayerInventory.TipoObjeto.Ninguno,
        PlayerInventory.TipoObjeto.GalletitaRedVelvet,
        PlayerInventory.TipoObjeto.GalletitaVainillaChocolate
    };

    // Start se ejecuta al comenzar la partida.
    private void Start()
    {
        // Comprobamos que asignamos las referencias necesarias.

        // Comprobamos que todos los puestos estén asignados.
        if (cliente1 == null || cliente2 == null || cliente3 == null ||
            bandeja2 == null || bandeja3 == null ||
            textoPedido2 == null || textoPedido3 == null)

        {
            Debug.LogError("Faltan referencias en GameManager.");
            enabled = false;
            return;
        }

        // El segundo puesto comienza desactivado.
        cliente2.gameObject.SetActive(false);
        bandeja2.SetActive(false);
        textoPedido2.SetActive(false);

        // El tercer cliente comienza oculto.
        cliente3.gameObject.SetActive(false);

        // Su bandeja y su cartel también comienzan ocultos.
        bandeja3.SetActive(false);
        textoPedido3.SetActive(false);

        // Empezamos por el primer pedido del tutorial.
        GenerarPedidoTutorial();
    }

    // Se ejecuta cuando un cliente recibe su pedido.
    // Recibimos una referencia para saber cuál fue.
    public void PedidoCompletado(Cliente clienteAtendido)
    {

        // Si la partida terminó, ignoramos nuevas entregas.
        if (partidaTerminada)
        {
            return;
        }

        // Sumamos un pedido al contador general.
        pedidosCompletados++;


        // Comprobamos si ya alcanzamos la cantidad de pedidos
        // necesaria para ganar la partida.
        if (pedidosCompletados >= pedidosParaGanar)
        {
            // Ejecutamos el método que termina la partida con victoria.
            GanarPartida();

            // Salimos del método para evitar generar otro pedido.
            return;
        }


        Debug.Log("Pedidos completados: " + pedidosCompletados);

        // Mientras estamos en el tutorial, seguimos
        // la secuencia de los tres pedidos iniciales.
        if (!tutorialTerminado)
        {
            // Si ya completamos los tres, termina el tutorial.
            if (pedidosCompletados >= 3)
            {
                TerminarTutorial();
            }
            else
            {
                // Si todavía faltan pedidos, generamos el siguiente.
                GenerarPedidoTutorial();
            }

            return;
        }

        // Después del tutorial, cada cliente que termina
        // recibe un nuevo pedido aleatorio.
        GenerarPedidoAleatorio(clienteAtendido);

        // Cuando alcanzamos la cantidad necesaria de pedidos,
        // desbloqueamos el tercer puesto.
        if (!tercerClienteActivado &&
            pedidosCompletados >= pedidosParaTercerCliente)
        {
            ActivarTercerCliente();
        }

    }

    // Define los tres pedidos iniciales en un orden fijo.

    private void GenerarPedidoTutorial()
    {
        // Primer pedido: Cliente1 pide un café.
        if (pedidosCompletados == 0)
        {
            cliente1.AsignarPedido(
                PlayerInventory.TipoObjeto.CafePreparado,
                PlayerInventory.TipoObjeto.Ninguno
            );
        }
        // Segundo pedido: Cliente2 pide café con leche.
        else if (pedidosCompletados == 1)
        {
            cliente2.AsignarPedido(
                PlayerInventory.TipoObjeto.CafeConLeche,
                PlayerInventory.TipoObjeto.Ninguno
            );

            // Activamos al segundo cliente para que entre caminando.
            // Su bandeja y su texto aparecerán cuando llegue.
            cliente2.gameObject.SetActive(true);
        }
        // Tercer pedido: Cliente3 pide chocolate.
        else if (pedidosCompletados == 2)
        {
            cliente3.AsignarPedido(
                PlayerInventory.TipoObjeto.ChocolatePreparado,
                PlayerInventory.TipoObjeto.Ninguno
            );

            // Activamos al tercer cliente para que entre caminando.
            cliente3.gameObject.SetActive(true);
        }
    }


    // Se ejecuta cuando completamos los tres pedidos.
    private void TerminarTutorial()
    {
        // Marcamos que el tutorial ya terminó.
        tutorialTerminado = true;

        Debug.Log("¡Tutorial terminado! Ahora hay dos clientes.");

        // Activamos el segundo puesto de atención.
        cliente2.gameObject.SetActive(true);

        // Ambos clientes reciben pedidos independientes.
        GenerarPedidoAleatorio(cliente1);
        GenerarPedidoAleatorio(cliente2);

    }

    // Genera una bebida y una galletita opcional al azar.
    private void GenerarPedidoAleatorio(Cliente cliente)
    {
        // Elegimos una posición aleatoria dentro del array
        // de bebidas. El límite superior no se incluye.
        int indiceBebida = Random.Range(0, bebidas.Length);

        // Elegimos una posición aleatoria del array
        // de galletitas, que también incluye Ninguno.
        int indiceGalletita = Random.Range(0, galletitas.Length);

        // Obtenemos los productos correspondientes.
        PlayerInventory.TipoObjeto bebidaElegida =
            bebidas[indiceBebida];

        PlayerInventory.TipoObjeto galletitaElegida =
            galletitas[indiceGalletita];

        // Le asignamos el nuevo pedido al cliente indicado.
        cliente.AsignarPedido(bebidaElegida, galletitaElegida);
    }

    // Activa el tercer puesto de atención.
    private void ActivarTercerCliente()
    {
        // Registramos que el puesto ya fue desbloqueado.
        tercerClienteActivado = true;

        // Hacemos visibles al cliente, su bandeja y su pedido.
        cliente3.gameObject.SetActive(true);

        // Generamos su primer pedido aleatorio.
        GenerarPedidoAleatorio(cliente3);

        // Mostramos un mensaje para comprobar la activación.
        Debug.Log("¡Tercer cliente desbloqueado!");
    }

    // Se ejecuta cuando un cliente agota su paciencia.
    public void ClienteSinPaciencia(Cliente cliente)
    {
        // Solo contamos clientes perdidos después del tutorial.
        if (!tutorialTerminado)
        {
            return;
        }

        // Aumentamos la cantidad de clientes perdidos.
        clientesPerdidos++;

        Debug.Log("Clientes perdidos: " + clientesPerdidos);

        // Identificamos qué cliente perdió la paciencia
        // para vaciar únicamente su bandeja.

        // Identificamos al cliente y buscamos su bandeja.
        if (cliente == cliente1)
        {
            // Solo vaciamos la bandeja si está asignada.
            if (bandeja1 != null)
            {
                bandeja1.VaciarBandeja();
            }
        }
        else if (cliente == cliente2)
        {
            // Comprobamos que exista la referencia.
            if (bandejaCliente2 != null)
            {
                bandejaCliente2.VaciarBandeja();
            }
        }
        else if (cliente == cliente3)
        {
            // Comprobamos que exista la referencia.
            if (bandejaCliente3 != null)
            {
                bandejaCliente3.VaciarBandeja();
            }
        }


        // Comprobamos si llegamos al máximo de clientes perdidos.

        if (clientesPerdidos >= maximoClientesPerdidos)
        {
            // Marcamos que la partida terminó.
            partidaTerminada = true;

            // Detenemos los temporizadores de todos los clientes.
            cliente1.DetenerPaciencia();
            cliente2.DetenerPaciencia();
            cliente3.DetenerPaciencia();

            // Mostramos la pantalla de derrota.
            if (panelDerrota != null)
            {
                panelDerrota.SetActive(true);
            }
            else
            {
                // Avisamos si olvidamos asignar el panel.
                Debug.LogWarning("Falta asignar PanelDerrota.");
            }

            Debug.Log("GAME OVER: perdiste tres clientes.");

            // No generamos otro pedido después de perder.
            return;
        }


        // Si todavía no perdimos, generamos un nuevo pedido
        // para reemplazar el que se canceló.
        GenerarPedidoAleatorio(cliente);

        // El nuevo pedido empieza con la paciencia completa.
        cliente.IniciarPaciencia();
    }

    // Este método será llamado por el botón Volver a intentar.
    public void ReiniciarPartida()
    {
        // Cargamos nuevamente la escena actual.
        // Así se reinician los pedidos, los clientes y las máquinas.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    // Termina la partida cuando completamos todos los pedidos.
    private void GanarPartida()
    {
        // Marcamos que la partida terminó.
        // Esto evita seguir sumando pedidos o clientes perdidos.
        partidaTerminada = true;

        // Detenemos la paciencia de todos los clientes.
        // Ya no pueden perder la paciencia después de ganar.
        cliente1.DetenerPaciencia();
        cliente2.DetenerPaciencia();
        cliente3.DetenerPaciencia();

        // Comprobamos que el panel de victoria esté asignado.
        if (panelVictoria != null)
        {
            // Mostramos la pantalla de victoria.
            panelVictoria.SetActive(true);
        }
        else
        {
            // Si olvidamos asignarlo, mostramos una advertencia.
            Debug.LogWarning("Falta asignar PanelVictoria.");
        }

        // Mensaje para comprobar que la victoria funciona.
        Debug.Log("¡VICTORIA! Completaste todos los pedidos.");
    }

}
