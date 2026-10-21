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
    [SerializeField] private GameObject defeatPanel;

    // Panel que aparece cuando ganamos.
    [SerializeField] private GameObject victoryPanel;

    // Textos de los paneles finales donde escribimos el resumen de la partida.
    [SerializeField] private TMP_Text victorySummaryText;
    [SerializeField] private TMP_Text defeatSummaryText;

    // Los tres clientes de la cafetería.
    [SerializeField] private Customer customer1;
    [SerializeField] private Customer customer2;
    [SerializeField] private Customer customer3;

    // ======================= REGLAS DEL JUEGO =======================

    // Cantidad de pedidos correctos necesarios para ganar.
    [SerializeField] private int ordersToWin = 15;

    // Cantidad de pedidos necesarios para desbloquear el tercer puesto.
    [SerializeField] private int ordersForThirdCustomer = 10;

    // Cantidad máxima de clientes que podemos perder.
    [SerializeField] private int maxLostCustomers = 3;

    // Propina por cada pedido: una parte fija y una extra según la rapidez.
    // Si el cliente todavía tiene toda la paciencia, deja la extra completa;
    // si le queda la mitad, deja la mitad de la extra, etc.
    [SerializeField] private int baseTip = 5;
    [SerializeField] private int maxExtraTip = 10;

    // Cantidad de pedidos que tiene el tutorial (uno por cliente).
    // Es una constante porque no cambia durante el juego.
    private const int TutorialOrders = 3;

    // ======================== ESTADO DE LA PARTIDA ========================
    // Los dejamos con [SerializeField] para ver los contadores
    // en el Inspector mientras probamos el juego.

    // Cantidad total de pedidos entregados correctamente (contador).
    [SerializeField] private int completedOrders = 0;

    // Cantidad de clientes que perdieron la paciencia (contador).
    [SerializeField] private int lostCustomers = 0;

    // Plata juntada en propinas (acumulador).
    [SerializeField] private int tips = 0;

    // Para explicar las galletitas una sola vez (el tutorial no las usa).
    private bool cookiesExplained = false;

    // Indica si ya terminamos los tres pedidos iniciales.
    private bool tutorialFinished = false;

    // Evita activar el tercer puesto más de una vez.
    private bool thirdCustomerActive = false;

    // Indica si la partida ya terminó (por victoria o derrota).
    private bool gameOver = false;

    // Array con los tres clientes, para recorrerlos con un for.
    private Customer[] customers;

    // HUD con los contadores y los mensajes en pantalla.
    private HUD hud;

    // Guía que explica el tutorial de a un paso (en la barra de arriba).
    private TutorialGuide tutorialGuide;

    // Sonidos del juego.
    private SoundEffects soundEffects;

    // Bebidas que pueden aparecer en pedidos aleatorios.
    private PlayerInventory.ItemType[] drinks =
    {
        PlayerInventory.ItemType.Coffee,
        PlayerInventory.ItemType.CoffeeWithMilk,
        PlayerInventory.ItemType.HotChocolate,
        PlayerInventory.ItemType.ChocolateWithMilk
    };

    // Opciones de galletitas.
    // Ninguno permite generar pedidos sin galletita.
    private PlayerInventory.ItemType[] cookies =
    {
        PlayerInventory.ItemType.None,
        PlayerInventory.ItemType.RedVelvetCookie,
        PlayerInventory.ItemType.VanillaChocolateCookie,
        PlayerInventory.ItemType.ChocolateCookie
    };

    // ============================ INICIO ============================

    // Start se ejecuta al comenzar la partida.
    private void Start()
    {
        // Comprobamos que todos los clientes estén asignados.
        if (customer1 == null || customer2 == null || customer3 == null)
        {
            Debug.LogError("Faltan clientes en el GameManager.");
            enabled = false;
            return;
        }

        // Guardamos los tres clientes en un array.
        customers = new Customer[] { customer1, customer2, customer3 };

        // Nos aseguramos de que el juego corra a velocidad normal
        // (al terminar la partida lo pausamos con timeScale = 0).
        Time.timeScale = 1f;

        // Buscamos el HUD una sola vez y mostramos los contadores en cero.
        hud = FindFirstObjectByType<HUD>();
        UpdateHUD();

        // También buscamos la guía del tutorial y los sonidos (vienen en el prefab HUD).
        tutorialGuide = FindFirstObjectByType<TutorialGuide>();
        soundEffects = FindFirstObjectByType<SoundEffects>();

        // El segundo y el tercer cliente comienzan ocultos.
        // Van a aparecer durante el tutorial.
        customer2.gameObject.SetActive(false);
        customer3.gameObject.SetActive(false);

        // Empezamos por el primer pedido del tutorial.
        GenerateTutorialOrder();
    }

    // ==================== AVISOS DE LOS CLIENTES ====================

    // Se ejecuta cuando un cliente recibe su pedido correcto.
    public void OrderCompleted(Customer servedCustomer)
    {
        // Si la partida terminó, ignoramos nuevas entregas.
        if (gameOver)
        {
            return;
        }

        // Sumamos un pedido al contador y la propina al acumulador.
        completedOrders++;
        int tip = CalculateTip(servedCustomer);
        tips += tip;
        UpdateHUD();

        Debug.Log("Pedidos completados: " + completedOrders + " | propina: $" + tip);
        Message("¡Pedido entregado! +$" + tip + " de propina", 2f);

        if (soundEffects != null)
        {
            soundEffects.Deliver();
        }

        // Durante el tutorial, le avisamos a la guía que ya se entregó.
        if (!tutorialFinished && tutorialGuide != null)
        {
            tutorialGuide.OrderDelivered();
        }

        // Comprobamos si ya alcanzamos la cantidad necesaria para ganar.
        if (completedOrders >= ordersToWin)
        {
            WinGame();
        }
    }

    // Se ejecuta cuando un cliente terminó de salir y pasó la pausa.
    // Acá decidimos quién entra después.
    public void CustomerLeft(Customer leavingCustomer)
    {
        // Si la partida terminó, no entra nadie más.
        if (gameOver)
        {
            return;
        }

        // ----- Durante el tutorial -----
        if (!tutorialFinished)
        {
            // El cliente que se fue desaparece hasta nuevo aviso.
            leavingCustomer.gameObject.SetActive(false);

            // Si todavía faltan pedidos del tutorial, entra el siguiente.
            // Si no, termina el tutorial.
            if (completedOrders < TutorialOrders)
            {
                GenerateTutorialOrder();
            }
            else
            {
                FinishTutorial();
            }

            return;
        }

        // ----- Durante la partida normal -----
        // Llega un cliente nuevo al puesto que quedó libre.
        BringNewCustomer(leavingCustomer);

        // Si alcanzamos la cantidad necesaria, desbloqueamos el tercer puesto.
        if (!thirdCustomerActive && completedOrders >= ordersForThirdCustomer)
        {
            ActivateThirdCustomer();
        }
    }

    // Se ejecuta cuando un cliente agota su paciencia.
    public void CustomerOutOfPatience(Customer customer)
    {
        // Solo contamos clientes perdidos después del tutorial
        // y mientras la partida siga en juego.
        if (!tutorialFinished || gameOver)
        {
            return;
        }

        // Aumentamos la cantidad de clientes perdidos.
        lostCustomers++;
        UpdateHUD();

        Debug.Log("Clientes perdidos: " + lostCustomers);

        // Comprobamos si llegamos al máximo de clientes perdidos.
        if (lostCustomers >= maxLostCustomers)
        {
            LoseGame();
            return;
        }

        Message("¡Un cliente se cansó de esperar y se fue enojado!");

        if (soundEffects != null)
        {
            soundEffects.AngryCustomer();
        }

        // Si todavía no perdimos, el cliente se va enojado.
        // Cuando termine de salir y pase la pausa, va a llamar a
        // CustomerLeft() y entrará un cliente nuevo a ese puesto.
        customer.LeaveAngry();
    }

    // Permite que otros scripts consulten si terminó el tutorial,
    // sin poder modificarlo.
    public bool IsTutorialFinished()
    {
        return tutorialFinished;
    }

    // Permite consultar si la partida ya terminó (la usa la pausa).
    public bool IsGameOver()
    {
        return gameOver;
    }

    // Calcula la propina de un pedido.
    // En el tutorial no hay paciencia, así que deja solo la parte fija.
    private int CalculateTip(Customer customer)
    {
        if (!tutorialFinished)
        {
            return baseTip;
        }

        // GetPatiencePercent devuelve un número entre 0 y 1.
        // (int) corta los decimales: por ejemplo, (int)7.8 da 7.
        return baseTip + (int)(maxExtraTip * customer.GetPatiencePercent());
    }

    // =========================== TUTORIAL ===========================

    // Define los tres pedidos iniciales en un orden fijo.
    // Usamos switch porque comparamos la misma variable
    // (completedOrders) contra varios valores.
    private void GenerateTutorialOrder()
    {
        // Primero elegimos qué cliente entra y qué bebida pide.
        Customer customer = null;
        PlayerInventory.ItemType drink = PlayerInventory.ItemType.None;

        switch (completedOrders)
        {
            // Primer pedido: Cliente1 pide un café.
            case 0:
                customer = customer1;
                drink = PlayerInventory.ItemType.Coffee;
                break;

            // Segundo pedido: Cliente2 pide café con leche.
            case 1:
                customer = customer2;
                drink = PlayerInventory.ItemType.CoffeeWithMilk;
                break;

            // Tercer pedido: Cliente3 pide chocolate.
            case 2:
                customer = customer3;
                drink = PlayerInventory.ItemType.HotChocolate;
                break;
        }

        if (customer == null)
        {
            return;
        }

        // En el tutorial los pedidos no llevan galletita.
        customer.AssignOrder(drink, PlayerInventory.ItemType.None);
        customer.Enter();

        // La guía va a explicar la receta de a un paso.
        // Le sumamos 1 porque para el jugador el primer pedido es el "1".
        if (tutorialGuide != null)
        {
            tutorialGuide.StartOrder(customer, drink, completedOrders + 1, TutorialOrders);
        }
    }

    // Se ejecuta cuando completamos los tres pedidos del tutorial.
    private void FinishTutorial()
    {
        tutorialFinished = true;

        // Ya no hace falta la guía del tutorial.
        if (tutorialGuide != null)
        {
            tutorialGuide.Finish();
        }

        Debug.Log("¡Tutorial terminado! Ahora hay dos clientes.");
        Message("¡Terminó el tutorial! Ahora los clientes tienen paciencia: " +
                "¡no los hagas esperar!", 5f);

        // Vuelven a entrar el primer y el segundo cliente,
        // cada uno con un pedido al azar.
        // El tercero espera hasta que se desbloquee su puesto.
        BringNewCustomer(customer1);
        BringNewCustomer(customer2);
    }

    // ======================== PARTIDA NORMAL ========================

    // Le da un pedido al azar al cliente y lo hace entrar.
    private void BringNewCustomer(Customer customer)
    {
        PlayerInventory.ItemType cookie = GenerateRandomOrder(customer);
        customer.Enter();

        // La primera vez que alguien pide galletita, la explicamos
        // (el tutorial no las usa). true = esperar a que termine el mensaje anterior.
        if (cookie != PlayerInventory.ItemType.None && !cookiesExplained)
        {
            cookiesExplained = true;

            if (hud != null)
            {
                hud.ShowMessage("¡Un cliente también quiere galletita! Agarrala en Galletitas (Z, X o C) " +
                                   "y ponela en su bandeja junto con la bebida.", 7f, true);
            }
        }
    }

    // Genera una bebida y una galletita opcional al azar.
    // Devuelve la galletita elegida (puede ser Ninguno).
    private PlayerInventory.ItemType GenerateRandomOrder(Customer customer)
    {
        // Random.Range elige un número al azar entre el primero (incluido)
        // y el segundo (NO incluido). Así obtenemos una posición válida
        // dentro de cada array.
        int drinkIndex = Random.Range(0, drinks.Length);
        int cookieIndex = Random.Range(0, cookies.Length);

        // Le asignamos el nuevo pedido al cliente indicado.
        customer.AssignOrder(drinks[drinkIndex], cookies[cookieIndex]);

        return cookies[cookieIndex];
    }

    // Desbloquea el tercer puesto de atención.
    private void ActivateThirdCustomer()
    {
        thirdCustomerActive = true;

        BringNewCustomer(customer3);

        Debug.Log("¡Tercer cliente desbloqueado!");
        Message("¡Se abrió un tercer puesto! Ahora vienen más clientes.", 4f);
    }

    // ============================== HUD ==============================

    // Muestra los contadores actualizados en el HUD (si hay HUD en la escena).
    private void UpdateHUD()
    {
        if (hud != null)
        {
            hud.UpdateCounters(
                completedOrders,
                ordersToWin,
                lostCustomers,
                maxLostCustomers);
            hud.UpdateTips(tips);
        }
    }

    // Muestra un mensaje en el cartel del HUD (si hay HUD en la escena).
    // segundos es opcional: si no lo pasamos, el HUD usa su duración normal.
    private void Message(string text, float seconds = -1f)
    {
        Debug.Log(text);

        if (hud != null)
        {
            hud.ShowMessage(text, seconds);
        }
    }

    // ======================== FIN DE PARTIDA ========================

    // Termina la partida con victoria.
    private void WinGame()
    {
        EndGame();

        // Un comentario distinto según cuántos clientes se fueron enojados.
        string comment;

        if (lostCustomers == 0)
        {
            comment = "¡Barista perfecto! No se te fue nadie.";
        }
        else if (lostCustomers == 1)
        {
            comment = "¡Muy bien! Se te fue un solo cliente.";
        }
        else
        {
            comment = "¡Lo lograste, por poquito!";
        }

        if (victorySummaryText != null)
        {
            victorySummaryText.text = comment + "\n\n" + BuildSummary();
        }

        if (soundEffects != null)
        {
            soundEffects.Victory();
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Falta asignar PanelVictoria.");
        }

        Debug.Log("¡VICTORIA! Completaste todos los pedidos.");
    }

    // Termina la partida con derrota.
    private void LoseGame()
    {
        EndGame();

        if (defeatSummaryText != null)
        {
            defeatSummaryText.text = "Se fueron " + lostCustomers + " clientes enojados.\n\n" + BuildSummary();
        }

        if (soundEffects != null)
        {
            soundEffects.Defeat();
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Falta asignar PanelDerrota.");
        }

        Debug.Log("GAME OVER: perdiste " + maxLostCustomers + " clientes.");
    }

    // Arma el texto con los números de la partida, para los paneles finales.
    // "\n" es un salto de línea.
    private string BuildSummary()
    {
        return "Pedidos entregados: " + completedOrders + " / " + ordersToWin +
               "\nPropinas: $" + tips +
               "\nClientes perdidos: " + lostCustomers + " / " + maxLostCustomers;
    }

    // Lo que pasa siempre al terminar la partida, ganemos o perdamos.
    private void EndGame()
    {
        // Marcamos que la partida terminó.
        // Esto evita seguir sumando pedidos o clientes perdidos.
        gameOver = true;

        // Detenemos la paciencia de todos los clientes, recorriendo el array.
        for (int i = 0; i < customers.Length; i++)
        {
            customers[i].StopPatience();
        }

        // Pausamos el juego: con timeScale = 0 el tiempo deja de correr
        // y nada se mueve. Los botones de la UI siguen funcionando.
        Time.timeScale = 0f;
    }

    // Este método lo llama el botón "Volver a intentar".
    public void RestartGame()
    {
        // Volvemos el tiempo a la normalidad antes de recargar.
        Time.timeScale = 1f;

        // Cargamos nuevamente la escena actual.
        // Así se reinician los pedidos, los clientes y las máquinas.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Este método lo puede llamar un botón "Menú" de los paneles
    // de victoria y derrota.
    public void GoToMenu()
    {
        // Volvemos el tiempo a la normalidad antes de cambiar de escena.
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}
