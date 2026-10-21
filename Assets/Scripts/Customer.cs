using UnityEngine;
using UnityEngine.UI;  // Permite trabajar con componentes de interfaz, como Slider.

// Representa a un cliente: guarda su pedido, lo muestra en un globo,
// controla su paciencia y avisa al GameManager cuando lo atendemos.
//
// Recorrido de un cliente:
//   1. El GameManager le asigna un pedido y llama a Entrar().
//   2. Camina hasta el mostrador (CustomerMovement) y muestra su pedido.
//   3. Recibe el pedido correcto y se va caminando hacia la salida.
//   4. Cuando sale, espera unos segundos con el puesto libre.
//   5. Le avisa al GameManager, que decide quién entra después.
public class Customer : MonoBehaviour
{
    // ===================== REFERENCIAS (Inspector) =====================

    // Barra visual de paciencia de este cliente.
    [SerializeField] private Slider patienceBar;

    // Bebida solicitada.
    [SerializeField]
    private PlayerInventory.ItemType orderedDrink =
        PlayerInventory.ItemType.Coffee;

    // Galletita solicitada. Puede ser Ninguno.
    [SerializeField]
    private PlayerInventory.ItemType orderedCookie =
        PlayerInventory.ItemType.None;

    // Referencia al controlador de la partida.
    [SerializeField] private GameManager gameManager;

    // Dónde aparece el globo del pedido respecto del cliente.
    [SerializeField] private Vector3 bubblePosition = new Vector3(0f, 0.7f, 0f);

    // Bandeja que pertenece a este cliente.
    // La mostramos solamente cuando llega a su puesto.
    [SerializeField] private Tray customerTray;

    // Tiempo base (en segundos) que el cliente está dispuesto a esperar.
    [SerializeField] private float maxPatience = 90f;

    // Segundos extra de paciencia según lo difícil que es el pedido:
    // las bebidas "con leche" llevan más pasos, y la galletita es un paso más.
    [SerializeField] private float extraWithMilk = 10f;
    [SerializeField] private float extraCookie = 6f;

    // Segundos que el puesto queda libre entre un cliente y el siguiente.
    [SerializeField] private float pauseBetweenCustomers = 2f;

    // ========================= ESTADO INTERNO =========================

    // Tiempo que le queda al cliente antes de perder la paciencia.
    private float currentPatience;

    // Paciencia total para el pedido actual (la base más los extras).
    private float orderPatience = 1f;

    // Relleno de la barra de paciencia: le cambiamos el color según cuánto queda.
    private Image barFill;

    // Indica si el tiempo de espera está corriendo.
    private bool patienceActive = false;

    // Indica que el cliente ya fue atendido y se está yendo.
    // Evita que se entregue el mismo pedido dos veces.
    private bool changingCustomer = false;

    // Indica si el cliente ya salió y estamos esperando la pausa.
    private bool paused = false;

    // Segundos que faltan para terminar la pausa.
    private float pauseRemaining = 0f;

    // Referencia al script que mueve al cliente.
    private CustomerMovement movement;

    // Globo que muestra el pedido arriba del cliente.
    private OrderBubble orderBubble;

    // ============================ INICIO ============================

    private void Awake()
    {
        // Buscamos el componente que controla el movimiento.
        movement = GetComponent<CustomerMovement>();

        if (movement == null)
        {
            Debug.LogError(gameObject.name + " necesita el script MovimientoCliente.");
        }

        // Avisamos si olvidamos asignar la bandeja en el Inspector.
        if (customerTray == null)
        {
            Debug.LogWarning("Falta asignar la bandeja de " + gameObject.name);
        }

        // Al comenzar, el cliente todavía no llegó:
        // ocultamos su pedido, su bandeja y su barra de paciencia.
        HideOrder();
    }

    private void Start()
    {
        // Buscamos el catálogo de dibujos una sola vez y creamos el globo.
        ItemCatalog catalog = FindFirstObjectByType<ItemCatalog>();

        if (catalog != null)
        {
            orderBubble = catalog.CreateOrderBubble(transform, bubblePosition);
        }
        else
        {
            Debug.LogWarning("Falta el CatalogoObjetos en la escena: no se verán los pedidos.");
        }

        // El Slider tiene un "relleno" (fillRect): guardamos su Image para pintarla.
        if (patienceBar != null && patienceBar.fillRect != null)
        {
            barFill = patienceBar.fillRect.GetComponent<Image>();
        }
    }

    // ============================ PEDIDO ============================

    // Cambia el pedido del cliente.
    public void AssignOrder(
        PlayerInventory.ItemType newDrink,
        PlayerInventory.ItemType newCookie)
    {
        // Guardamos los nuevos productos.
        orderedDrink = newDrink;
        orderedCookie = newCookie;

        // Calculamos cuánta paciencia tiene para este pedido (acumulador).
        orderPatience = maxPatience;

        if (orderedDrink == PlayerInventory.ItemType.CoffeeWithMilk ||
            orderedDrink == PlayerInventory.ItemType.ChocolateWithMilk)
        {
            orderPatience += extraWithMilk;
        }

        if (orderedCookie != PlayerInventory.ItemType.None)
        {
            orderPatience += extraCookie;
        }

        // Mostramos también el pedido en la consola.
        Debug.Log("Nuevo pedido: " + orderedDrink + " + " + orderedCookie);
    }

    // Devuelve true si la bebida y la galletita recibidas
    // son exactamente las que pidió el cliente.
    // && significa Y: ambas condiciones deben ser verdaderas.
    // Si el cliente no pidió galletita, ambos valores deben ser Ninguno.
    public bool IsCorrectOrder(
        PlayerInventory.ItemType drink,
        PlayerInventory.ItemType cookie)
    {
        return drink == orderedDrink && cookie == orderedCookie;
    }

    // ===================== LLEGADA Y SALIDA =====================

    // El cliente aparece en la entrada y camina hasta el mostrador.
    // La llama el GameManager.
    public void Enter()
    {
        // Lo activamos por si estaba oculto.
        gameObject.SetActive(true);

        // Es un cliente nuevo: todavía no fue atendido.
        changingCustomer = false;
        paused = false;

        movement.Enter();
    }

    // Muestra el pedido y la bandeja cuando el cliente llega al mostrador.
    // La llama CustomerMovement al llegar.
    public void ShowOrder()
    {
        // Activamos la bandeja correspondiente a este puesto.
        if (customerTray != null)
        {
            customerTray.gameObject.SetActive(true);
        }

        // Mostramos el pedido en el globo.
        if (orderBubble != null)
        {
            orderBubble.Show(orderedDrink, orderedCookie);
        }

        // Solo comenzamos a descontar paciencia después del tutorial.
        if (gameManager != null && gameManager.IsTutorialFinished())
        {
            StartPatience();
        }
    }

    // Devuelve la bandeja de este cliente (la usa el tutorial para señalarla).
    public Tray GetTray()
    {
        return customerTray;
    }

    // Devuelve true si el cliente está en el mostrador esperando.
    // La usa la bandeja antes de entregar el pedido.
    public bool CanReceiveOrder()
    {
        return movement.IsAtSpot() && !changingCustomer;
    }

    // Se ejecuta cuando entregamos correctamente el pedido.
    public void ReceiveOrder()
    {
        // Evitamos que una misma entrega se cuente dos veces.
        if (!CanReceiveOrder())
        {
            return;
        }

        Debug.Log("¡Pedido entregado correctamente!");

        // Avisamos al GameManager para que sume el pedido.
        if (gameManager != null)
        {
            gameManager.OrderCompleted(this);
        }

        // El cliente se va contento.
        Leave();
    }

    // Se ejecuta cuando el cliente perdió la paciencia.
    // La llama el GameManager.
    public void LeaveAngry()
    {
        Debug.Log(gameObject.name + " se fue enojado.");

        // Vaciamos su bandeja: lo que había preparado ya no sirve.
        if (customerTray != null)
        {
            customerTray.EmptyTray();
        }

        // Se va igual que cuando lo atendemos, pero sin sumar pedido.
        Leave();
    }

    // Lo que pasa siempre que un cliente se va,
    // ya sea contento (lo atendimos) o enojado (perdió la paciencia).
    // Así no repetimos el mismo código en dos lugares.
    private void Leave()
    {
        // Bloqueamos nuevas entregas mientras el cliente se va.
        changingCustomer = true;

        // El cliente deja de esperar: ocultamos su interfaz y su bandeja.
        StopPatience();
        HideOrder();

        // IMPORTANTE: damos la orden de caminar hacia la salida.
        // Cuando llegue, CustomerMovement llamará a FinishedLeaving().
        movement.Exit();
    }

    // Se ejecuta cuando el cliente terminó de salir.
    // La llama CustomerMovement al llegar a la salida.
    public void FinishedLeaving()
    {
        // Empezamos la pausa con el puesto libre.
        paused = true;
        pauseRemaining = pauseBetweenCustomers;
    }

    // Oculta el globo del pedido, la bandeja y la barra de paciencia.
    private void HideOrder()
    {
        // En Awake el globo todavía no existe (se crea en Start),
        // por eso comprobamos que no sea null.
        if (orderBubble != null)
        {
            orderBubble.Hide();
        }

        if (customerTray != null)
        {
            customerTray.gameObject.SetActive(false);
        }

        if (patienceBar != null)
        {
            patienceBar.gameObject.SetActive(false);
        }
    }

    // =========================== PACIENCIA ===========================

    // Inicia o reinicia el tiempo de espera del cliente.
    public void StartPatience()
    {
        // Recupera toda su paciencia y activa el temporizador.
        currentPatience = orderPatience;
        patienceActive = true;

        // Hacemos visible la barra y la llenamos.
        // 1 representa el 100% de paciencia.
        if (patienceBar != null)
        {
            patienceBar.gameObject.SetActive(true);
            patienceBar.value = 1f;
        }

        PaintBar(1f);

        Debug.Log(gameObject.name + " empieza con " + currentPatience + " segundos.");
    }

    // Detiene el temporizador del cliente.
    public void StopPatience()
    {
        patienceActive = false;
    }

    // Devuelve cuánta paciencia le queda, de 0 (nada) a 1 (toda).
    // La usa el GameManager para calcular la propina.
    public float GetPatiencePercent()
    {
        return currentPatience / orderPatience;
    }

    // Pinta la barra según cuánta paciencia queda:
    // verde (más de la mitad), amarillo (más de un cuarto) o rojo.
    private void PaintBar(float percent)
    {
        if (barFill == null)
        {
            return;
        }

        if (percent > 0.5f)
        {
            barFill.color = new Color(0.45f, 0.8f, 0.35f);
        }
        else if (percent > 0.25f)
        {
            barFill.color = new Color(0.95f, 0.76f, 0.3f);
        }
        else
        {
            barFill.color = new Color(0.85f, 0.3f, 0.25f);
        }
    }

    // ============================ UPDATE ============================

    // Se ejecuta una vez por frame. Lleva dos temporizadores:
    // la pausa después de salir y la paciencia en el mostrador.
    private void Update()
    {
        UpdatePause();
        UpdatePatience();
    }

    // Cuenta la pausa entre que el cliente sale y llega el siguiente.
    private void UpdatePause()
    {
        if (!paused)
        {
            return;
        }

        pauseRemaining -= Time.deltaTime;

        if (pauseRemaining <= 0f)
        {
            paused = false;

            // Avisamos que el puesto quedó libre.
            // El GameManager decide qué cliente entra después.
            if (gameManager != null)
            {
                gameManager.CustomerLeft(this);
            }
        }
    }

    // Descuenta la paciencia del cliente mientras espera su pedido.
    private void UpdatePatience()
    {
        // Si la paciencia no está activa, no hacemos nada.
        if (!patienceActive)
        {
            return;
        }

        // Restamos el tiempo transcurrido desde el último frame.
        // Mathf.Max evita que la paciencia quede negativa.
        currentPatience -= Time.deltaTime;
        currentPatience = Mathf.Max(currentPatience, 0f);

        // Actualizamos visualmente la barra de paciencia.
        // Por ejemplo: 45 / 90 = 0.5 = 50%.
        float percent = currentPatience / orderPatience;

        if (patienceBar != null)
        {
            patienceBar.value = percent;
        }

        PaintBar(percent);

        // Si la paciencia llegó a cero...
        if (currentPatience <= 0f)
        {
            // Detenemos el temporizador para no avisar más de una vez.
            patienceActive = false;

            Debug.Log(gameObject.name + " perdió la paciencia.");

            // Avisamos al GameManager que este cliente se cansó.
            if (gameManager != null)
            {
                gameManager.CustomerOutOfPatience(this);
            }
        }
    }
}
