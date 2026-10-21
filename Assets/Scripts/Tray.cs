using UnityEngine;

// La bandeja es una estación interactiva.
// Por eso hereda de InteractiveStation y utiliza la tecla E.
// Cada bandeja pertenece a un cliente: ahí se arma su pedido.
public class Tray : InteractiveStation
{
    // Guardamos una referencia al cliente
    // al que pertenece esta bandeja.
    [SerializeField] private Customer customer;

    // Aquí guardamos la bebida colocada en la bandeja.
    // Al comenzar, la bandeja no tiene ninguna bebida.
    [SerializeField]
    private PlayerInventory.ItemType drink =
        PlayerInventory.ItemType.None;

    // Aquí guardamos la galletita colocada en la bandeja.
    // También comienza vacía.
    [SerializeField]
    private PlayerInventory.ItemType cookie =
        PlayerInventory.ItemType.None;

    // Dónde se dibujan la bebida y la galletita, respecto de la bandeja.
    // Se pueden acomodar desde el Inspector.
    [SerializeField] private Vector3 drinkPosition = new Vector3(-0.22f, 0.12f, 0f);
    [SerializeField] private Vector3 cookiePosition = new Vector3(0.25f, 0.06f, 0f);

    // Dibujos de lo que está apoyado en la bandeja (sin la nube de fondo),
    // así el jugador ve qué dejó y si se equivocó.
    private ItemBubble drinkBubble;
    private ItemBubble cookieBubble;

    private void Start()
    {
        // Los dibujos los crea el catálogo, como los de la repisa.
        ItemCatalog catalog = FindFirstObjectByType<ItemCatalog>();

        if (catalog != null)
        {
            drinkBubble = catalog.CreateBubble(transform, drinkPosition, false);
            cookieBubble = catalog.CreateBubble(transform, cookiePosition, false);
        }

        UpdateDrawings();
    }

    // Muestra sobre la bandeja lo que tiene apoyado.
    // Mostrar(Ninguno) oculta el dibujo, así que sirve también para vaciar.
    private void UpdateDrawings()
    {
        if (drinkBubble == null || cookieBubble == null)
        {
            return;
        }

        drinkBubble.Show(drink);
        cookieBubble.Show(cookie);
    }

    // Este método se ejecuta cuando el jugador
    // se acerca a la bandeja y presiona E.
    public override void Interact(PlayerInventory inventory)
    {
        // Primero comprobamos que haya un cliente asignado.
        // Si no lo hay, no podemos verificar ningún pedido.
        if (customer == null)
        {
            Debug.Log("Esta bandeja no tiene cliente.");
            return;
        }

        // Consultamos qué objeto lleva el barista.
        PlayerInventory.ItemType item = inventory.GetCurrentItem();

        // CASO 1: El barista tiene las manos vacías.
        // En ese caso, intentamos entregar el pedido.
        if (item == PlayerInventory.ItemType.None)
        {
            TryDeliverOrder();
            return;
        }

        // CASO 2: El barista lleva una bebida.
        if (IsDrink(item))
        {
            // Si ya hay una bebida, no podemos colocar otra.
            if (drink != PlayerInventory.ItemType.None)
            {
                Notify("Ya hay una bebida en la bandeja.");
                return;
            }

            // Pasamos la bebida de la mano a la bandeja.
            drink = item;
            inventory.DropItem();
            UpdateDrawings();

            Debug.Log("Colocaste: " + drink);
        }
        // CASO 3: El barista lleva una galletita.
        else if (IsCookie(item))
        {
            // Si ya hay una galletita, no aceptamos otra.
            if (cookie != PlayerInventory.ItemType.None)
            {
                Notify("Ya hay una galletita en la bandeja.");
                return;
            }

            // Pasamos la galletita de la mano a la bandeja.
            cookie = item;
            inventory.DropItem();
            UpdateDrawings();

            Debug.Log("Colocaste: " + cookie);
        }
        // CASO 4: El barista lleva algo que no se puede servir
        // (por ejemplo, granos de café o leche sin preparar).
        else
        {
            Notify("Eso no se puede servir: primero hay que prepararlo.");
        }
    }

    // Revisa la bandeja y, si el pedido está bien, se lo da al cliente.
    private void TryDeliverOrder()
    {
        // Si el cliente todavía no llegó o ya se está yendo,
        // no puede recibir nada.
        if (!customer.CanReceiveOrder())
        {
            Notify("El cliente no está esperando un pedido.");
            return;
        }

        // Le preguntamos al cliente si esto es lo que pidió.
        if (customer.IsCorrectOrder(drink, cookie))
        {
            // Primero vaciamos los dos espacios de la bandeja.
            EmptyTray();

            // Después avisamos al cliente.
            // El cliente notificará al GameManager.
            customer.ReceiveOrder();
        }
        else
        {
            // El pedido está incompleto o tiene
            // algún producto diferente del solicitado.
            Notify("El pedido no está completo o no es lo que pidió.");
        }
    }

    // Devuelve true si el objeto es una bebida lista para servir.
    // || significa O: alcanza con que una condición sea verdadera.
    private bool IsDrink(PlayerInventory.ItemType item)
    {
        return item == PlayerInventory.ItemType.Coffee ||
               item == PlayerInventory.ItemType.CoffeeWithMilk ||
               item == PlayerInventory.ItemType.HotChocolate ||
               item == PlayerInventory.ItemType.ChocolateWithMilk;
    }

    // Devuelve true si el objeto es uno
    // de nuestros tres tipos de galletitas.
    private bool IsCookie(PlayerInventory.ItemType item)
    {
        return item == PlayerInventory.ItemType.RedVelvetCookie ||
               item == PlayerInventory.ItemType.VanillaChocolateCookie ||
               item == PlayerInventory.ItemType.ChocolateCookie;
    }

    // Permite retirar un producto de la bandeja con la tecla R.
    // Sirve cuando el jugador se equivocó de pedido.
    public void RemoveProduct(PlayerInventory inventory)
    {
        // No podemos retirar nada si tenemos las manos ocupadas.
        if (inventory.HasItem())
        {
            Notify("Para sacar algo de la bandeja necesitás las manos vacías.");
            return;
        }

        // Si hay una galletita, la retiramos primero.
        if (cookie != PlayerInventory.ItemType.None)
        {
            inventory.PickUpItem(cookie);
            cookie = PlayerInventory.ItemType.None;
            UpdateDrawings();
            return;
        }

        // Si no hay galletita, retiramos la bebida.
        if (drink != PlayerInventory.ItemType.None)
        {
            inventory.PickUpItem(drink);
            drink = PlayerInventory.ItemType.None;
            UpdateDrawings();
            return;
        }

        // Si no hay ningún producto, mostramos este mensaje.
        Notify("La bandeja está vacía.");
    }

    // Deja la bandeja vacía.
    // Se usa al entregar un pedido y cuando un cliente pierde la paciencia.
    public void EmptyTray()
    {
        drink = PlayerInventory.ItemType.None;
        cookie = PlayerInventory.ItemType.None;
        UpdateDrawings();
    }

    // ===================== CONSULTAS (para el tutorial) =====================

    // Devuelve la bebida que está en la bandeja (Ninguno si no hay).
    public PlayerInventory.ItemType GetDrink()
    {
        return drink;
    }
}
