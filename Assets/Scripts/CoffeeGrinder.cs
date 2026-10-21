using UnityEngine;

// La moledora convierte los granos de café en café molido.
// Hereda de InteractiveStation, así que se usa con la tecla E.
public class CoffeeGrinder : InteractiveStation
{
    // Segundos que tarda en moler el café.
    [SerializeField] private float preparationTime = 3f;

    // Dónde aparece el globo respecto de la moledora.
    [SerializeField] private Vector3 bubblePosition = new Vector3(0f, 0.85f, 0f);

    // Indica si la moledora está trabajando.
    private bool preparing = false;

    // Segundos que faltan para terminar de moler.
    private float timeRemaining = 0f;

    // Indica si hay café molido listo para retirar.
    private bool coffeeReady = false;

    // Globo que muestra lo que hay adentro de la moledora.
    private ItemBubble bubble;

    private void Start()
    {
        // Buscamos el catálogo de dibujos una sola vez y creamos el globo.
        ItemCatalog catalog = FindFirstObjectByType<ItemCatalog>();

        if (catalog != null)
        {
            bubble = catalog.CreateBubble(transform, bubblePosition, true);
        }
    }

    // Se ejecuta cuando el barista se acerca y presiona E.
    public override void Interact(PlayerInventory inventory)
    {
        // Si está trabajando, no podemos usarla.
        if (preparing)
        {
            Notify("La moledora está trabajando...");
            return;
        }

        // Si ya terminó, intentamos retirar el café molido.
        if (coffeeReady)
        {
            // Solo vaciamos la moledora si el barista pudo agarrarlo
            // (por ejemplo, no puede si tiene las manos ocupadas).
            if (inventory.PickUpItem(PlayerInventory.ItemType.GroundCoffee))
            {
                coffeeReady = false;
                HideBubble();

                Debug.Log("Retiraste el café molido.");
            }

            return;
        }

        // Si está vacía, comprobamos que el barista traiga granos de café.
        if (inventory.GetCurrentItem() == PlayerInventory.ItemType.CoffeeBeans)
        {
            // Los granos pasan de la mano a la moledora.
            inventory.DropItem();

            // Empezamos a contar el tiempo de molienda.
            preparing = true;
            timeRemaining = preparationTime;

            // Mostramos los granos medio transparentes: se están moliendo.
            ShowInBubble(PlayerInventory.ItemType.CoffeeBeans, false);

            Debug.Log("Moliendo café...");
        }
        else
        {
            Notify("La moledora necesita granos de café.");
        }
    }

    // Update se ejecuta una vez por frame.
    // Lo usamos como un temporizador: le restamos el tiempo
    // que pasó desde el frame anterior hasta llegar a cero.
    private void Update()
    {
        // Si no está moliendo, no hay nada que contar.
        if (!preparing)
        {
            return;
        }

        // Time.deltaTime es el tiempo que pasó desde el último frame.
        timeRemaining -= Time.deltaTime;

        // Cuando el tiempo llega a cero, el café está listo.
        if (timeRemaining <= 0f)
        {
            preparing = false;
            coffeeReady = true;

            // Ahora mostramos el café molido, bien visible.
            ShowInBubble(PlayerInventory.ItemType.GroundCoffee, true);

            Debug.Log("¡Café molido listo!");

            // Sonido de "¡listo!".
            SoundEffects s = GetSoundEffects();

            if (s != null)
            {
                s.MachineReady();
            }
        }
    }

    // Muestra un objeto en el globo de la moledora (si el globo existe).
    private void ShowInBubble(PlayerInventory.ItemType item, bool ready)
    {
        if (bubble != null)
        {
            bubble.Show(item, ready);
        }
    }

    // Oculta el globo de la moledora (si el globo existe).
    private void HideBubble()
    {
        if (bubble != null)
        {
            bubble.Hide();
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================
    // Funciones que solo devuelven información: otros scripts pueden
    // preguntar cómo está la moledora sin poder cambiarla.

    // Devuelve true mientras la moledora está moliendo.
    public bool IsGrinding()
    {
        return preparing;
    }

    // Devuelve true si hay café molido listo para retirar.
    public bool HasCoffeeReady()
    {
        return coffeeReady;
    }
}
