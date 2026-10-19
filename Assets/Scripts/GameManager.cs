using TMPro;                       // Permite usar textos de TextMeshPro (el resumen final).
using UnityEngine;
using UnityEngine.SceneManagement; // Permite volver a cargar la escena para reiniciar.

// Controla los pedidos y el progreso de la partida:
//   - El tutorial: tres pedidos fijos, uno por cliente, sin paciencia.
//   - La partida normal: pedidos al azar y clientes con paciencia.
//   - Las propinas: cuanto más rápido entregamos, más propina deja el cliente.
//   - La victoria y la derrota (con un resumen de la partida).
public class GameManager : MonoBehaviour
{
    // ===================== REFERENCIAS (Inspector) =====================

    // Panel que aparece cuando perdemos.
    [SerializeField] private GameObject panelDerrota;

    // Panel que aparece cuando ganamos.
    [SerializeField] private GameObject panelVictoria;

    // Textos de los paneles finales donde escribimos el resumen de la partida.
    [SerializeField] private TMP_Text textoResumenVictoria;
    [SerializeField] private TMP_Text textoResumenDerrota;

    // Los tres clientes de la cafetería.
    [SerializeField] private Cliente cliente1;
    [SerializeField] private Cliente cliente2;
    [SerializeField] private Cliente cliente3;

    // ======================= REGLAS DEL JUEGO =======================

    // Cantidad de pedidos correctos necesarios para ganar.
    [SerializeField] private int pedidosParaGanar = 15;

    // Cantidad de pedidos necesarios para desbloquear el tercer puesto.
    [SerializeField] private int pedidosParaTercerCliente = 10;

    // Cantidad máxima de clientes que podemos perder.
    [SerializeField] private int maximoClientesPerdidos = 3;

    // Propina por cada pedido: una parte fija y una extra según la rapidez.
    // Si el cliente todavía tiene toda la paciencia, deja la extra completa;
    // si le queda la mitad, deja la mitad de la extra, etc.
    [SerializeField] private int propinaBase = 5;
    [SerializeField] private int propinaExtraMaxima = 10;

    // Cantidad de pedidos que tiene el tutorial (uno por cliente).
    // Es una constante porque no cambia durante el juego.
    private const int PedidosDelTutorial = 3;

    // ======================== ESTADO DE LA PARTIDA ========================
    // Los dejamos con [SerializeField] para ver los contadores
    // en el Inspector mientras probamos el juego.

    // Cantidad total de pedidos entregados correctamente (contador).
    [SerializeField] private int pedidosCompletados = 0;

    // Cantidad de clientes que perdieron la paciencia (contador).
    [SerializeField] private int clientesPerdidos = 0;

    // Plata juntada en propinas (acumulador).
    [SerializeField] private int propinas = 0;

    // Para explicar las galletitas una sola vez (el tutorial no las usa).
    private bool yaExplicoGalletitas = false;

    // Indica si ya terminamos los tres pedidos iniciales.
    private bool tutorialTerminado = false;

    // Evita activar el tercer puesto más de una vez.
    private bool tercerClienteActivado = false;

    // Indica si la partida ya terminó (por victoria o derrota).
    private bool partidaTerminada = false;

    // Array con los tres clientes, para recorrerlos con un for.
    private Cliente[] clientes;

    // HUD con los contadores y los mensajes en pantalla.
    private HUD hud;

    // Guía que explica el tutorial de a un paso (en la barra de arriba).
    private GuiaTutorial guiaTutorial;

    // Sonidos del juego.
    private Sonidos sonidos;

    // Bebidas que pueden aparecer en pedidos aleatorios.
    private PlayerInventory.TipoObjeto[] bebidas =
    {
        PlayerInventory.TipoObjeto.CafePreparado,
        PlayerInventory.TipoObjeto.CafeConLeche,
        PlayerInventory.TipoObjeto.ChocolatePreparado,
        PlayerInventory.TipoObjeto.ChocolateConLeche
    };

    // Opciones de galletitas.
    // Ninguno permite generar pedidos sin galletita.
    private PlayerInventory.TipoObjeto[] galletitas =
    {
        PlayerInventory.TipoObjeto.Ninguno,
        PlayerInventory.TipoObjeto.GalletitaRedVelvet,
        PlayerInventory.TipoObjeto.GalletitaVainillaChocolate,
        PlayerInventory.TipoObjeto.GalletitaChocolate
    };

    // ============================ INICIO ============================

    // Start se ejecuta al comenzar la partida.
    private void Start()
    {
        // Comprobamos que todos los clientes estén asignados.
        if (cliente1 == null || cliente2 == null || cliente3 == null)
        {
            Debug.LogError("Faltan clientes en el GameManager.");
            enabled = false;
            return;
        }

        // Guardamos los tres clientes en un array.
        clientes = new Cliente[] { cliente1, cliente2, cliente3 };

        // Nos aseguramos de que el juego corra a velocidad normal
        // (al terminar la partida lo pausamos con timeScale = 0).
        Time.timeScale = 1f;

        // Buscamos el HUD una sola vez y mostramos los contadores en cero.
        hud = FindFirstObjectByType<HUD>();
        ActualizarHUD();

        // También buscamos la guía del tutorial y los sonidos (vienen en el prefab HUD).
        guiaTutorial = FindFirstObjectByType<GuiaTutorial>();
        sonidos = FindFirstObjectByType<Sonidos>();

        // El segundo y el tercer cliente comienzan ocultos.
        // Van a aparecer durante el tutorial.
        cliente2.gameObject.SetActive(false);
        cliente3.gameObject.SetActive(false);

        // Empezamos por el primer pedido del tutorial.
        GenerarPedidoTutorial();
    }

    // ==================== AVISOS DE LOS CLIENTES ====================

    // Se ejecuta cuando un cliente recibe su pedido correcto.
    public void PedidoCompletado(Cliente clienteAtendido)
    {
        // Si la partida terminó, ignoramos nuevas entregas.
        if (partidaTerminada)
        {
            return;
        }

        // Sumamos un pedido al contador y la propina al acumulador.
        pedidosCompletados++;
        int propina = CalcularPropina(clienteAtendido);
        propinas += propina;
        ActualizarHUD();

        Debug.Log("Pedidos completados: " + pedidosCompletados + " | propina: $" + propina);
        Mensaje("¡Pedido entregado! +$" + propina + " de propina", 2f);

        if (sonidos != null)
        {
            sonidos.Entregar();
        }

        // Durante el tutorial, le avisamos a la guía que ya se entregó.
        if (!tutorialTerminado && guiaTutorial != null)
        {
            guiaTutorial.PedidoEntregado();
        }

        // Comprobamos si ya alcanzamos la cantidad necesaria para ganar.
        if (pedidosCompletados >= pedidosParaGanar)
        {
            GanarPartida();
        }
    }

    // Se ejecuta cuando un cliente terminó de salir y pasó la pausa.
    // Acá decidimos quién entra después.
    public void ClienteSeFue(Cliente clienteQueSeFue)
    {
        // Si la partida terminó, no entra nadie más.
        if (partidaTerminada)
        {
            return;
        }

        // ----- Durante el tutorial -----
        if (!tutorialTerminado)
        {
            // El cliente que se fue desaparece hasta nuevo aviso.
            clienteQueSeFue.gameObject.SetActive(false);

            // Si todavía faltan pedidos del tutorial, entra el siguiente.
            // Si no, termina el tutorial.
            if (pedidosCompletados < PedidosDelTutorial)
            {
                GenerarPedidoTutorial();
            }
            else
            {
                TerminarTutorial();
            }

            return;
        }

        // ----- Durante la partida normal -----
        // Llega un cliente nuevo al puesto que quedó libre.
        LlegaClienteNuevo(clienteQueSeFue);

        // Si alcanzamos la cantidad necesaria, desbloqueamos el tercer puesto.
        if (!tercerClienteActivado && pedidosCompletados >= pedidosParaTercerCliente)
        {
            ActivarTercerCliente();
        }
    }

    // Se ejecuta cuando un cliente agota su paciencia.
    public void ClienteSinPaciencia(Cliente cliente)
    {
        // Solo contamos clientes perdidos después del tutorial
        // y mientras la partida siga en juego.
        if (!tutorialTerminado || partidaTerminada)
        {
            return;
        }

        // Aumentamos la cantidad de clientes perdidos.
        clientesPerdidos++;
        ActualizarHUD();

        Debug.Log("Clientes perdidos: " + clientesPerdidos);

        // Comprobamos si llegamos al máximo de clientes perdidos.
        if (clientesPerdidos >= maximoClientesPerdidos)
        {
            PerderPartida();
            return;
        }

        Mensaje("¡Un cliente se cansó de esperar y se fue enojado!");

        if (sonidos != null)
        {
            sonidos.ClienteEnojado();
        }

        // Si todavía no perdimos, el cliente se va enojado.
        // Cuando termine de salir y pase la pausa, va a llamar a
        // ClienteSeFue() y entrará un cliente nuevo a ese puesto.
        cliente.IrseEnojado();
    }

    // Permite que otros scripts consulten si terminó el tutorial,
    // sin poder modificarlo.
    public bool EstaTerminadoTutorial()
    {
        return tutorialTerminado;
    }

    // Permite consultar si la partida ya terminó (la usa la pausa).
    public bool EstaTerminadaPartida()
    {
        return partidaTerminada;
    }

    // Calcula la propina de un pedido.
    // En el tutorial no hay paciencia, así que deja solo la parte fija.
    private int CalcularPropina(Cliente cliente)
    {
        if (!tutorialTerminado)
        {
            return propinaBase;
        }

        // ObtenerPorcentajePaciencia devuelve un número entre 0 y 1.
        // (int) corta los decimales: por ejemplo, (int)7.8 da 7.
        return propinaBase + (int)(propinaExtraMaxima * cliente.ObtenerPorcentajePaciencia());
    }

    // =========================== TUTORIAL ===========================

    // Define los tres pedidos iniciales en un orden fijo.
    // Usamos switch porque comparamos la misma variable
    // (pedidosCompletados) contra varios valores.
    private void GenerarPedidoTutorial()
    {
        // Primero elegimos qué cliente entra y qué bebida pide.
        Cliente cliente = null;
        PlayerInventory.TipoObjeto bebida = PlayerInventory.TipoObjeto.Ninguno;

        switch (pedidosCompletados)
        {
            // Primer pedido: Cliente1 pide un café.
            case 0:
                cliente = cliente1;
                bebida = PlayerInventory.TipoObjeto.CafePreparado;
                break;

            // Segundo pedido: Cliente2 pide café con leche.
            case 1:
                cliente = cliente2;
                bebida = PlayerInventory.TipoObjeto.CafeConLeche;
                break;

            // Tercer pedido: Cliente3 pide chocolate.
            case 2:
                cliente = cliente3;
                bebida = PlayerInventory.TipoObjeto.ChocolatePreparado;
                break;
        }

        if (cliente == null)
        {
            return;
        }

        // En el tutorial los pedidos no llevan galletita.
        cliente.AsignarPedido(bebida, PlayerInventory.TipoObjeto.Ninguno);
        cliente.Entrar();

        // La guía va a explicar la receta de a un paso.
        // Le sumamos 1 porque para el jugador el primer pedido es el "1".
        if (guiaTutorial != null)
        {
            guiaTutorial.EmpezarPedido(cliente, bebida, pedidosCompletados + 1, PedidosDelTutorial);
        }
    }

    // Se ejecuta cuando completamos los tres pedidos del tutorial.
    private void TerminarTutorial()
    {
        tutorialTerminado = true;

        // Ya no hace falta la guía del tutorial.
        if (guiaTutorial != null)
        {
            guiaTutorial.Terminar();
        }

        Debug.Log("¡Tutorial terminado! Ahora hay dos clientes.");
        Mensaje("¡Terminó el tutorial! Ahora los clientes tienen paciencia: " +
                "¡no los hagas esperar!", 5f);

        // Vuelven a entrar el primer y el segundo cliente,
        // cada uno con un pedido al azar.
        // El tercero espera hasta que se desbloquee su puesto.
        LlegaClienteNuevo(cliente1);
        LlegaClienteNuevo(cliente2);
    }

    // ======================== PARTIDA NORMAL ========================

    // Le da un pedido al azar al cliente y lo hace entrar.
    private void LlegaClienteNuevo(Cliente cliente)
    {
        PlayerInventory.TipoObjeto galletita = GenerarPedidoAleatorio(cliente);
        cliente.Entrar();

        // La primera vez que alguien pide galletita, la explicamos
        // (el tutorial no las usa). true = esperar a que termine el mensaje anterior.
        if (galletita != PlayerInventory.TipoObjeto.Ninguno && !yaExplicoGalletitas)
        {
            yaExplicoGalletitas = true;

            if (hud != null)
            {
                hud.MostrarMensaje("¡Un cliente también quiere galletita! Agarrala en Galletitas (Z, X o C) " +
                                   "y ponela en su bandeja junto con la bebida.", 7f, true);
            }
        }
    }

    // Genera una bebida y una galletita opcional al azar.
    // Devuelve la galletita elegida (puede ser Ninguno).
    private PlayerInventory.TipoObjeto GenerarPedidoAleatorio(Cliente cliente)
    {
        // Random.Range elige un número al azar entre el primero (incluido)
        // y el segundo (NO incluido). Así obtenemos una posición válida
        // dentro de cada array.
        int indiceBebida = Random.Range(0, bebidas.Length);
        int indiceGalletita = Random.Range(0, galletitas.Length);

        // Le asignamos el nuevo pedido al cliente indicado.
        cliente.AsignarPedido(bebidas[indiceBebida], galletitas[indiceGalletita]);

        return galletitas[indiceGalletita];
    }

    // Desbloquea el tercer puesto de atención.
    private void ActivarTercerCliente()
    {
        tercerClienteActivado = true;

        LlegaClienteNuevo(cliente3);

        Debug.Log("¡Tercer cliente desbloqueado!");
        Mensaje("¡Se abrió un tercer puesto! Ahora vienen más clientes.", 4f);
    }

    // ============================== HUD ==============================

    // Muestra los contadores actualizados en el HUD (si hay HUD en la escena).
    private void ActualizarHUD()
    {
        if (hud != null)
        {
            hud.ActualizarContadores(
                pedidosCompletados,
                pedidosParaGanar,
                clientesPerdidos,
                maximoClientesPerdidos);
            hud.ActualizarPropinas(propinas);
        }
    }

    // Muestra un mensaje en el cartel del HUD (si hay HUD en la escena).
    // segundos es opcional: si no lo pasamos, el HUD usa su duración normal.
    private void Mensaje(string texto, float segundos = -1f)
    {
        Debug.Log(texto);

        if (hud != null)
        {
            hud.MostrarMensaje(texto, segundos);
        }
    }

    // ======================== FIN DE PARTIDA ========================

    // Termina la partida con victoria.
    private void GanarPartida()
    {
        TerminarPartida();

        // Un comentario distinto según cuántos clientes se fueron enojados.
        string comentario;

        if (clientesPerdidos == 0)
        {
            comentario = "¡Barista perfecto! No se te fue nadie.";
        }
        else if (clientesPerdidos == 1)
        {
            comentario = "¡Muy bien! Se te fue un solo cliente.";
        }
        else
        {
            comentario = "¡Lo lograste, por poquito!";
        }

        if (textoResumenVictoria != null)
        {
            textoResumenVictoria.text = comentario + "\n\n" + ArmarResumen();
        }

        if (sonidos != null)
        {
            sonidos.Victoria();
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Falta asignar PanelVictoria.");
        }

        Debug.Log("¡VICTORIA! Completaste todos los pedidos.");
    }

    // Termina la partida con derrota.
    private void PerderPartida()
    {
        TerminarPartida();

        if (textoResumenDerrota != null)
        {
            textoResumenDerrota.text = "Se fueron " + clientesPerdidos + " clientes enojados.\n\n" + ArmarResumen();
        }

        if (sonidos != null)
        {
            sonidos.Derrota();
        }

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Falta asignar PanelDerrota.");
        }

        Debug.Log("GAME OVER: perdiste " + maximoClientesPerdidos + " clientes.");
    }

    // Arma el texto con los números de la partida, para los paneles finales.
    // "\n" es un salto de línea.
    private string ArmarResumen()
    {
        return "Pedidos entregados: " + pedidosCompletados + " / " + pedidosParaGanar +
               "\nPropinas: $" + propinas +
               "\nClientes perdidos: " + clientesPerdidos + " / " + maximoClientesPerdidos;
    }

    // Lo que pasa siempre al terminar la partida, ganemos o perdamos.
    private void TerminarPartida()
    {
        // Marcamos que la partida terminó.
        // Esto evita seguir sumando pedidos o clientes perdidos.
        partidaTerminada = true;

        // Detenemos la paciencia de todos los clientes, recorriendo el array.
        for (int i = 0; i < clientes.Length; i++)
        {
            clientes[i].DetenerPaciencia();
        }

        // Pausamos el juego: con timeScale = 0 el tiempo deja de correr
        // y nada se mueve. Los botones de la UI siguen funcionando.
        Time.timeScale = 0f;
    }

    // Este método lo llama el botón "Volver a intentar".
    public void ReiniciarPartida()
    {
        // Volvemos el tiempo a la normalidad antes de recargar.
        Time.timeScale = 1f;

        // Cargamos nuevamente la escena actual.
        // Así se reinician los pedidos, los clientes y las máquinas.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Este método lo puede llamar un botón "Menú" de los paneles
    // de victoria y derrota.
    public void IrAlMenu()
    {
        // Volvemos el tiempo a la normalidad antes de cambiar de escena.
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}
