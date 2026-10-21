using TMPro;          // Permite usar textos de TextMeshPro.
using UnityEngine;
using UnityEngine.UI; // Permite usar Image (el dibujo del pedido).

// Tutorial guiado "de a un paso".
// En el centro de la barra de arriba muestra UNA sola instrucción
// (por ejemplo "Andá a Ingredientes y apretá Z") y una flecha que rebota
// arriba de la estación a la que hay que ir.
//
// ¿Cómo sabe qué paso mostrar? En cada frame se fija cómo están las cosas
// (qué tiene el barista en la mano, si la moledora está trabajando, etc.)
// y elige el paso con una cadena de if / else if, empezando por el final
// de la receta: "¿ya está en la bandeja? ¿lo tengo en la mano? ¿está listo
// en la máquina?...". Así, si el jugador se equivoca o tira algo,
// la guía se acomoda sola.
//
// Va en el prefab HUD. El GameManager lo busca en Start y le avisa
// cuándo empieza cada pedido del tutorial, cuándo se entregó
// y cuándo terminó el tutorial.
public class TutorialGuide : MonoBehaviour
{
    // ===================== REFERENCIAS (Inspector) =====================

    // Cartel del centro de la barra de arriba (se oculta al terminar el tutorial).
    [SerializeField] private GameObject tutorialBox;

    // Renglón chico de arriba: "Tu cliente quiere un café".
    [SerializeField] private TMP_Text titleText;

    // Renglón grande: la instrucción del paso actual.
    [SerializeField] private TMP_Text instructionText;

    // Dibujo de la bebida que pidió el cliente.
    [SerializeField] private Image orderImage;

    // Prefab de la flecha que señala la estación (un SpriteRenderer).
    [SerializeField] private GameObject arrowPrefab;

    // ============================ CONSTANTES ============================

    // El dibujo de la flecha apunta hacia ABAJO. Para que apunte a otro lado
    // la giramos: 90 = hacia la derecha, -90 = hacia la izquierda, 180 = hacia arriba.
    private const float AngleDown = 0f;
    private const float AngleRight = 90f;
    private const float AngleLeft = -90f;
    private const float AngleUp = 180f;

    // Altura (en el mundo) de la flecha cuando señala algo que está sobre la barra:
    // justo debajo de la barra, donde se para el barista.
    private const float ArrowHeightAtBar = -0.35f;

    // Qué tan rápido y qué tan alto rebota la flecha.
    private const float BounceSpeed = 6f;
    private const float BounceHeight = 0.12f;

    // Duración (en segundos) del "saltito" del texto cuando cambia el paso.
    private const float HopDuration = 0.25f;

    // Color de fondo de las teclas (#RRGGBBAA, medio transparente).
    private const string KeyColor = "#F2C14E88";

    // ======================== ESTADO DEL TUTORIAL ========================

    // true mientras hay un pedido del tutorial en curso.
    private bool active = false;

    // true cuando ya se entregó el pedido actual (esperamos al siguiente cliente).
    private bool delivered = false;

    // Cliente del pedido actual y la bebida que pidió.
    private Customer currentCustomer;
    private PlayerInventory.ItemType orderedDrink = PlayerInventory.ItemType.None;

    // Número del pedido (1, 2 o 3) y cantidad total del tutorial.
    private int orderNumber = 0;
    private int totalOrders = 0;

    // Último texto que mostramos, para saber cuándo cambia el paso.
    private string currentInstruction = "";

    // Tiempo que le queda al "saltito" del texto (temporizador).
    private float hopTimer = 0f;

    // ========================= OTRAS REFERENCIAS =========================
    // Las buscamos una sola vez en Start.

    private PlayerInventory inventory;
    private IngredientStation ingredients;
    private CoffeeGrinder grinder;
    private CoffeeMachine coffeeMachine;
    private ChocolateMachine chocolateMachine;
    private TrashCan trashCan;
    private Shelf[] shelves;
    private ItemCatalog catalog;

    // La flecha que creamos con Instantiate.
    private Transform arrow;

    // Hacia dónde apunta la flecha (null = no se muestra).
    private Transform arrowDestination;

    // Dónde va la flecha respecto del destino y hacia qué lado apunta.
    private Vector3 arrowOffset = Vector3.zero;
    private float arrowAngle = 0f;

    // ============================ INICIO ============================

    private void Start()
    {
        // Buscamos cada estación una sola vez (FindFirstObjectByType es lento).
        inventory = FindFirstObjectByType<PlayerInventory>();
        ingredients = FindFirstObjectByType<IngredientStation>();
        grinder = FindFirstObjectByType<CoffeeGrinder>();
        coffeeMachine = FindFirstObjectByType<CoffeeMachine>();
        chocolateMachine = FindFirstObjectByType<ChocolateMachine>();
        trashCan = FindFirstObjectByType<TrashCan>();

        // Hay dos repisas: FindObjectsByType (en plural) devuelve un array con todas.
        shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);

        // Creamos la flecha a partir del prefab y la escondemos hasta que haga falta.
        if (arrowPrefab != null)
        {
            GameObject newArrow = Instantiate(arrowPrefab);
            arrow = newArrow.transform;
            newArrow.SetActive(false);
        }

        // Si el GameManager todavía no empezó un pedido, el cartel queda oculto.
        tutorialBox.SetActive(active);
    }

    // Update se ejecuta en cada frame: elegimos el paso y movemos la flecha.
    private void Update()
    {
        if (!active)
        {
            return;
        }

        ChooseStep();
        MoveArrow();
        AnimateHop();
    }

    // ===================== AVISOS DEL GAMEMANAGER =====================

    // Empieza un pedido del tutorial.
    // numero: 1, 2 o 3. total: cantidad de pedidos del tutorial.
    public void StartOrder(Customer customer, PlayerInventory.ItemType drink, int number, int total)
    {
        currentCustomer = customer;
        orderedDrink = drink;
        orderNumber = number;
        totalOrders = total;
        delivered = false;
        active = true;

        tutorialBox.SetActive(true);
        ShowDrawing(PlayerInventory.ItemType.None);
    }

    // El cliente recibió su pedido.
    public void OrderDelivered()
    {
        delivered = true;
    }

    // Terminó el tutorial: escondemos el cartel y la flecha.
    public void Finish()
    {
        active = false;
        tutorialBox.SetActive(false);
        arrowDestination = null;
        MoveArrow();
    }

    // ======================= ELEGIR EL PASO ACTUAL =======================

    private void ChooseStep()
    {
        // 1) Ya entregamos: esperamos al próximo cliente.
        if (delivered)
        {
            ShowTitle("¡Pedido " + orderNumber + " de " + totalOrders + " listo!");
            ShowStep("¡Muy bien! Ya viene el próximo cliente...", null);
            return;
        }

        // 2) El cliente todavía está entrando: esperamos a que llegue.
        if (!currentCustomer.CanReceiveOrder())
        {
            ShowTitle("Tutorial · Pedido " + orderNumber + " de " + totalOrders);
            ShowDrawing(PlayerInventory.ItemType.None);
            ShowStep("¡Llega un cliente! Esperalo en el mostrador", null);
            return;
        }

        // 3) El cliente llegó: nos dice qué quiere.
        ShowTitle("Tu cliente quiere <b>" + DrinkName(orderedDrink) + "</b>");
        ShowDrawing(orderedDrink);

        PlayerInventory.ItemType inHand = inventory.GetCurrentItem();
        Tray tray = currentCustomer.GetTray();

        // 4) La bebida ya está en la bandeja: falta entregarla.
        if (tray.GetDrink() == orderedDrink)
        {
            if (inHand == PlayerInventory.ItemType.None)
            {
                ShowStep("Apretá " + KeyLabel("E") + " en la bandeja para <b>entregar</b>", tray.transform);
            }
            else
            {
                ShowStep("Para entregar, primero dejá lo que tenés en una repisa", NearestShelf());
            }
            return;
        }

        // 5) Tenemos la bebida en la mano: a la bandeja.
        if (inHand == orderedDrink)
        {
            ShowStep("¡Ya está! Llevalo a la <b>bandeja</b> y apretá " + KeyLabel("E"), tray.transform);
            return;
        }

        // 6) Hay otra bebida en la bandeja: hay que sacarla.
        if (tray.GetDrink() != PlayerInventory.ItemType.None && inHand == PlayerInventory.ItemType.None)
        {
            ShowStep("Esa no es la bebida que pidió: sacala con " + KeyLabel("R"), tray.transform);
            return;
        }

        // 7) Si no, seguimos la receta de la bebida pedida.
        switch (orderedDrink)
        {
            case PlayerInventory.ItemType.Coffee:
                GuideCoffee(inHand);
                break;

            case PlayerInventory.ItemType.CoffeeWithMilk:
                GuideCoffeeWithMilk(inHand);
                break;

            case PlayerInventory.ItemType.HotChocolate:
                GuideChocolate(inHand);
                break;
        }
    }

    // ============================ RECETAS ============================
    // Cada receta revisa los pasos desde el FINAL hacia el principio.
    // El primero que se cumple es el que mostramos.

    // Café: granos (Z) > moledora > café molido > cafetera > café.
    private void GuideCoffee(PlayerInventory.ItemType inHand)
    {
        if (coffeeMachine.GetReadyProduct() == PlayerInventory.ItemType.Coffee)
        {
            PickUp("¡El café está listo! Agarralo con " + KeyLabel("E"), coffeeMachine.transform, inHand);
        }
        else if (coffeeMachine.GetProductInProgress() == PlayerInventory.ItemType.Coffee)
        {
            ShowStep("La cafetera está haciendo el café... ¡esperá!", coffeeMachine.transform);
        }
        else if (inHand == PlayerInventory.ItemType.GroundCoffee)
        {
            ShowStep("Ahora llevá el café molido a la <b>cafetera</b> y apretá " + KeyLabel("E"), coffeeMachine.transform);
        }
        else if (grinder.HasCoffeeReady())
        {
            PickUp("¡Ya está molido! Agarralo con " + KeyLabel("E"), grinder.transform, inHand);
        }
        else if (grinder.IsGrinding())
        {
            ShowStep("La moledora está moliendo... ¡esperá!", grinder.transform);
        }
        else if (inHand == PlayerInventory.ItemType.CoffeeBeans)
        {
            ShowStep("Llevá los granos a la <b>moledora</b> y apretá " + KeyLabel("E"), grinder.transform);
        }
        else if (ShelfWith(PlayerInventory.ItemType.GroundCoffee) != null)
        {
            PickUp("Agarrá el café molido de la repisa con " + KeyLabel("E"),
                    ShelfWith(PlayerInventory.ItemType.GroundCoffee), inHand);
        }
        else if (inHand != PlayerInventory.ItemType.None)
        {
            HandsFull();
        }
        else
        {
            ShowStep("Andá a <b>Ingredientes</b> y apretá " + KeyLabel("Z") + " para agarrar granos de café",
                    ingredients.transform);
        }
    }

    // Leche espumada: leche (X) > cafetera > leche espumada.
    private void GuideMilk(PlayerInventory.ItemType inHand)
    {
        if (coffeeMachine.GetReadyProduct() == PlayerInventory.ItemType.FoamedMilk)
        {
            PickUp("¡La leche está espumada! Agarrala con " + KeyLabel("E"), coffeeMachine.transform, inHand);
        }
        else if (coffeeMachine.GetProductInProgress() == PlayerInventory.ItemType.FoamedMilk)
        {
            ShowStep("La cafetera está espumando la leche... ¡esperá!", coffeeMachine.transform);
        }
        else if (inHand == PlayerInventory.ItemType.Milk)
        {
            ShowStep("Llevá la leche a la <b>cafetera</b> y apretá " + KeyLabel("E") + " para espumarla",
                    coffeeMachine.transform);
        }
        else if (inHand != PlayerInventory.ItemType.None)
        {
            HandsFull();
        }
        else
        {
            ShowStep("Ahora la leche: andá a <b>Ingredientes</b> y apretá " + KeyLabel("X"),
                    ingredients.transform);
        }
    }

    // Café con leche: café + leche espumada, juntos en una repisa.
    private void GuideCoffeeWithMilk(PlayerInventory.ItemType inHand)
    {
        // ¿En qué repisa está cada cosa? (null = en ninguna)
        Transform shelfWithMix = ShelfWith(PlayerInventory.ItemType.CoffeeWithMilk);
        Transform shelfWithCoffee = ShelfWith(PlayerInventory.ItemType.Coffee);
        Transform shelfWithMilk = ShelfWith(PlayerInventory.ItemType.FoamedMilk);

        if (shelfWithMix != null)
        {
            PickUp("¡Se mezclaron! Agarrá el café con leche con " + KeyLabel("E"), shelfWithMix, inHand);
        }
        else if (inHand == PlayerInventory.ItemType.Coffee && shelfWithMilk != null)
        {
            ShowStep("Poné el café en la repisa con la leche: ¡se mezclan solos!", shelfWithMilk);
        }
        else if (inHand == PlayerInventory.ItemType.FoamedMilk && shelfWithCoffee != null)
        {
            ShowStep("Poné la leche en la repisa con el café: ¡se mezclan solos!", shelfWithCoffee);
        }
        else if (inHand == PlayerInventory.ItemType.Coffee ||
                 inHand == PlayerInventory.ItemType.FoamedMilk)
        {
            // Solo se puede llevar una cosa a la vez: la apoyamos para tener las manos libres.
            ShowStep("Dejalo en una <b>repisa</b> con " + KeyLabel("E") + " para tener las manos libres",
                    NearestShelf());
        }
        else if (shelfWithCoffee != null && shelfWithMilk != null)
        {
            // Están en repisas distintas: hay que juntarlas.
            PickUp("Agarrá la leche y ponela en la repisa del café", shelfWithMilk, inHand);
        }
        else if (shelfWithCoffee != null ||
                 coffeeMachine.GetReadyProduct() == PlayerInventory.ItemType.FoamedMilk ||
                 coffeeMachine.GetProductInProgress() == PlayerInventory.ItemType.FoamedMilk)
        {
            // El café ya está (o la leche está en marcha): seguimos con la leche.
            GuideMilk(inHand);
        }
        else
        {
            // Todavía falta el café: lo hacemos igual que en el primer pedido.
            GuideCoffee(inHand);
        }
    }

    // Chocolate: chocolate (C) > chocolatera > chocolate preparado.
    private void GuideChocolate(PlayerInventory.ItemType inHand)
    {
        if (chocolateMachine.HasChocolateReady())
        {
            PickUp("¡El chocolate está listo! Agarralo con " + KeyLabel("E"), chocolateMachine.transform, inHand);
        }
        else if (chocolateMachine.IsPreparing())
        {
            ShowStep("La chocolatera está trabajando... ¡esperá!", chocolateMachine.transform);
        }
        else if (inHand == PlayerInventory.ItemType.Chocolate)
        {
            ShowStep("Llevá el chocolate a la <b>chocolatera</b> y apretá " + KeyLabel("E"), chocolateMachine.transform);
        }
        else if (inHand != PlayerInventory.ItemType.None)
        {
            HandsFull();
        }
        else
        {
            ShowStep("Andá a <b>Ingredientes</b> y apretá " + KeyLabel("C") + " para agarrar chocolate",
                    ingredients.transform);
        }
    }

    // ======================== FUNCIONES DE AYUDA ========================

    // Muestra una instrucción y apunta la flecha a un destino (null = sin flecha).
    private void ShowStep(string text, Transform destination)
    {
        // Si el paso cambió, hacemos que el texto dé un saltito para que se note.
        if (text != currentInstruction)
        {
            currentInstruction = text;
            instructionText.text = text;
            hopTimer = HopDuration;
        }

        arrowDestination = destination;

        if (destination != null)
        {
            PlaceArrow(destination);
        }
    }

    // Elige dónde va la flecha según qué estación es el destino.
    private void PlaceArrow(Transform destination)
    {
        if (destination == currentCustomer.GetTray().transform)
        {
            // La bandeja está arriba de la barra: la flecha va debajo de la barra
            // (donde se para el barista), apuntando hacia arriba.
            arrowOffset = new Vector3(0f, ArrowHeightAtBar - destination.position.y, 0f);
            arrowAngle = AngleUp;
        }
        else if (destination == ingredients.transform)
        {
            // La despensa está pegada a la pared izquierda: la flecha va a su derecha.
            arrowOffset = new Vector3(1.55f, 0.3f, 0f);
            arrowAngle = AngleLeft;
        }
        else if (trashCan != null && destination == trashCan.transform)
        {
            // El tacho está al lado de las galletitas: la flecha va a su izquierda.
            arrowOffset = new Vector3(-0.95f, 0f, 0f);
            arrowAngle = AngleRight;
        }
        else
        {
            // Máquinas y repisas de la fila de abajo: su posición es el borde de arriba
            // (donde se para el barista). La flecha va justo arriba, apuntando hacia abajo.
            arrowOffset = new Vector3(0f, 0.45f, 0f);
            arrowAngle = AngleDown;
        }
    }

    // Cambia el título solo si es distinto del que ya está
    // (así no le pedimos a TextMeshPro que redibuje el texto en cada frame).
    private void ShowTitle(string text)
    {
        if (titleText.text != text)
        {
            titleText.text = text;
        }
    }

    // Para agarrar algo hacen falta las manos vacías.
    private void PickUp(string text, Transform place, PlayerInventory.ItemType inHand)
    {
        if (inHand == PlayerInventory.ItemType.None)
        {
            ShowStep(text, place);
        }
        else
        {
            HandsFull();
        }
    }

    // El barista tiene algo que no sirve para este paso.
    private void HandsFull()
    {
        if (trashCan != null)
        {
            ShowStep("Eso no sirve ahora: tiralo en el <b>tacho</b> con " + KeyLabel("E"), trashCan.transform);
        }
        else
        {
            ShowStep("Eso no sirve ahora: dejalo en una <b>repisa</b> con " + KeyLabel("E"), NearestShelf());
        }
    }

    // Devuelve la repisa que tiene ese objeto (o null si ninguna lo tiene).
    private Transform ShelfWith(PlayerInventory.ItemType item)
    {
        for (int i = 0; i < shelves.Length; i++)
        {
            if (shelves[i].Has(item))
            {
                return shelves[i].transform;
            }
        }

        return null;
    }

    // Devuelve la repisa más cercana al barista (igual que buscar un mínimo).
    private Transform NearestShelf()
    {
        Transform nearest = null;
        float shortestDistance = 0f;

        for (int i = 0; i < shelves.Length; i++)
        {
            float distance = Vector2.Distance(inventory.transform.position, shelves[i].transform.position);

            if (nearest == null || distance < shortestDistance)
            {
                nearest = shelves[i].transform;
                shortestDistance = distance;
            }
        }

        return nearest;
    }

    // Nombre de cada bebida, para el título.
    private string DrinkName(PlayerInventory.ItemType drink)
    {
        switch (drink)
        {
            case PlayerInventory.ItemType.Coffee:
                return "un café";
            case PlayerInventory.ItemType.CoffeeWithMilk:
                return "un café con leche";
            case PlayerInventory.ItemType.HotChocolate:
                return "un chocolate";
            default:
                return "algo rico";
        }
    }

    // Muestra el dibujo de la bebida (o lo oculta si es Ninguno).
    private void ShowDrawing(PlayerInventory.ItemType drink)
    {
        // Búsqueda "perezosa": buscamos el catálogo la primera vez que hace falta.
        if (catalog == null)
        {
            catalog = FindFirstObjectByType<ItemCatalog>();
        }

        Sprite drawing = null;

        if (catalog != null && drink != PlayerInventory.ItemType.None)
        {
            drawing = catalog.GetSprite(drink);
        }

        // Solo lo cambiamos si es distinto del que ya está.
        if (orderImage.sprite != drawing)
        {
            orderImage.sprite = drawing;
        }

        // Sin dibujo, ocultamos la imagen para que no se vea un cuadrado blanco.
        orderImage.enabled = drawing != null;
    }

    // Devuelve una tecla "dibujada": la letra en negrita con un fondo amarillo.
    // <mark> es una etiqueta de TextMeshPro que pinta un rectángulo detrás del texto.
    private string KeyLabel(string key)
    {
        return "<mark=" + KeyColor + "><b> " + key + " </b></mark>";
    }

    // ========================= ANIMACIONES =========================

    // Pone la flecha al lado del destino, rebotando.
    private void MoveArrow()
    {
        if (arrow == null)
        {
            return;
        }

        if (arrowDestination == null)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);

        // Giramos la flecha (solo en el eje Z, porque el juego es 2D).
        arrow.eulerAngles = new Vector3(0f, 0f, arrowAngle);

        // Mathf.Sin da un número que va y viene entre -1 y 1 a medida que pasa
        // el tiempo: lo usamos para que la flecha vaya y venga sola.
        float bounce = Mathf.Sin(Time.time * BounceSpeed) * BounceHeight;

        // flecha.up es la dirección "arriba" de la flecha, que gira con ella:
        // así rebota siempre a lo largo de hacia donde apunta.
        arrow.position = arrowDestination.position + arrowOffset + arrow.up * bounce;
    }

    // Cuando cambia el paso, el texto se agranda un poquito y vuelve a su tamaño.
    private void AnimateHop()
    {
        if (hopTimer <= 0f)
        {
            return;
        }

        hopTimer -= Time.deltaTime;

        // Va de 1.08 (recién cambiado) a 1 (tamaño normal).
        float scale = 1f + 0.08f * (hopTimer / HopDuration);

        if (hopTimer <= 0f)
        {
            scale = 1f;
        }

        instructionText.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
