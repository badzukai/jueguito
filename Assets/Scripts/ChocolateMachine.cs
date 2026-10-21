using UnityEngine;

// La chocolatera convierte el chocolate en chocolate preparado.
// Hereda de InteractiveStation, así que se usa con la tecla E.
public class ChocolateMachine : InteractiveStation
{
    // Segundos que tarda en preparar el chocolate.
    [SerializeField] private float preparationTime = 4f;

    // Dónde aparece el globo respecto de la chocolatera.
    [SerializeField] private Vector3 bubblePosition = new Vector3(0f, 0.85f, 0f);

    // Indica si la chocolatera está trabajando.
    private bool preparing = false;

    // Segundos que faltan para terminar.
    private float timeRemaining = 0f;

    // Indica si hay chocolate listo para retirar.
    private bool chocolateReady = false;

    // Globo que muestra lo que hay adentro de la chocolatera.
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
            Notify("La chocolatera está ocupada.");
            return;
        }

        // Si terminó, intentamos retirar el chocolate.
        if (chocolateReady)
        {
            // Solo la vaciamos si el barista pudo agarrarlo.
            if (inventory.PickUpItem(PlayerInventory.ItemType.HotChocolate))
            {
                chocolateReady = false;
                HideBubble();
            }

            return;
        }

        // Si está vacía, comprobamos el ingrediente.
        if (inventory.GetCurrentItem() == PlayerInventory.ItemType.Chocolate)
        {
            // El chocolate pasa de la mano a la máquina.
            inventory.DropItem();

            // Empezamos a contar el tiempo de preparación.
            preparing = true;
            timeRemaining = preparationTime;

            // Mostramos el chocolate medio transparente: se está preparando.
            ShowInBubble(PlayerInventory.ItemType.Chocolate, false);

            Debug.Log("Preparando chocolate...");
        }
        else
        {
            Notify("La chocolatera necesita chocolate.");
        }
    }

    // Update funciona como temporizador: resta el tiempo de cada frame.
    private void Update()
    {
        // Si no está preparando nada, no hay nada que contar.
        if (!preparing)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        // Cuando el tiempo llega a cero, el chocolate está listo.
        if (timeRemaining <= 0f)
        {
            preparing = false;
            chocolateReady = true;

            // Ahora mostramos el chocolate preparado, bien visible.
            ShowInBubble(PlayerInventory.ItemType.HotChocolate, true);

            Debug.Log("¡Chocolate preparado!");

            // Sonido de "¡listo!".
            SoundEffects s = GetSoundEffects();

            if (s != null)
            {
                s.MachineReady();
            }
        }
    }

    // Muestra un objeto en el globo de la chocolatera (si el globo existe).
    private void ShowInBubble(PlayerInventory.ItemType item, bool ready)
    {
        if (bubble != null)
        {
            bubble.Show(item, ready);
        }
    }

    // Oculta el globo de la chocolatera (si el globo existe).
    private void HideBubble()
    {
        if (bubble != null)
        {
            bubble.Hide();
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================
    // Funciones que solo devuelven información: otros scripts pueden
    // preguntar cómo está la chocolatera sin poder cambiarla.

    // Devuelve true mientras la chocolatera está preparando.
    public bool IsPreparing()
    {
        return preparing;
    }

    // Devuelve true si hay un chocolate listo para retirar.
    public bool HasChocolateReady()
    {
        return chocolateReady;
    }
}
