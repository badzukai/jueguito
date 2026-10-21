using UnityEngine;

// La cafetera tiene dos usos:
//   - Café molido  -> café preparado.
//   - Leche        -> leche espumada.
// Hereda de InteractiveStation, así que se usa con la tecla E.
public class CoffeeMachine : InteractiveStation
{
    // Segundos que tarda en preparar el café.
    [SerializeField] private float coffeeTime = 4f;

    // Segundos que tarda en espumar la leche.
    [SerializeField] private float milkTime = 3f;

    // Dónde aparece el globo respecto de la cafetera.
    [SerializeField] private Vector3 bubblePosition = new Vector3(0f, 0.85f, 0f);

    // Indica si la cafetera está trabajando.
    private bool preparing = false;

    // Segundos que faltan para terminar.
    private float timeRemaining = 0f;

    // Producto que va a salir cuando termine de preparar.
    private PlayerInventory.ItemType productInProgress =
        PlayerInventory.ItemType.None;

    // Producto terminado que espera ser retirado.
    // Ninguno significa que la cafetera está vacía.
    private PlayerInventory.ItemType readyProduct =
        PlayerInventory.ItemType.None;

    // Globo que muestra lo que hay adentro de la cafetera.
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
            Notify("La cafetera está ocupada.");
            return;
        }

        // Si hay algo listo, intentamos retirarlo.
        if (readyProduct != PlayerInventory.ItemType.None)
        {
            // Solo la vaciamos si el barista pudo agarrarlo.
            if (inventory.PickUpItem(readyProduct))
            {
                readyProduct = PlayerInventory.ItemType.None;
                HideBubble();
            }

            return;
        }

        // Si está vacía, revisamos qué ingrediente trae el barista.
        // Usamos switch porque comparamos una misma variable
        // contra varios valores posibles.
        switch (inventory.GetCurrentItem())
        {
            case PlayerInventory.ItemType.GroundCoffee:
                inventory.DropItem();
                StartPreparing(
                    PlayerInventory.ItemType.GroundCoffee,
                    PlayerInventory.ItemType.Coffee,
                    coffeeTime);
                break;

            case PlayerInventory.ItemType.Milk:
                inventory.DropItem();
                StartPreparing(
                    PlayerInventory.ItemType.Milk,
                    PlayerInventory.ItemType.FoamedMilk,
                    milkTime);
                break;

            default:
                Notify("La cafetera necesita café molido o leche.");
                break;
        }
    }

    // Pone en marcha la cafetera.
    // ingrediente: lo que dejó el barista (se muestra mientras trabaja).
    // resultado: lo que va a salir al terminar.
    // duracion: cuántos segundos va a tardar.
    private void StartPreparing(
        PlayerInventory.ItemType ingredient,
        PlayerInventory.ItemType result,
        float duration)
    {
        productInProgress = result;
        timeRemaining = duration;
        preparing = true;

        // Mostramos el ingrediente medio transparente: se está preparando.
        ShowInBubble(ingredient, false);

        Debug.Log("Preparando: " + result);
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

        // Cuando el tiempo llega a cero, el producto queda listo para retirar.
        if (timeRemaining <= 0f)
        {
            preparing = false;
            readyProduct = productInProgress;
            productInProgress = PlayerInventory.ItemType.None;

            // Ahora mostramos el resultado, bien visible.
            ShowInBubble(readyProduct, true);

            Debug.Log("¡Listo para retirar: " + readyProduct + "!");

            // Sonido de "¡listo!".
            SoundEffects s = GetSoundEffects();

            if (s != null)
            {
                s.MachineReady();
            }
        }
    }

    // Muestra un objeto en el globo de la cafetera (si el globo existe).
    private void ShowInBubble(PlayerInventory.ItemType item, bool ready)
    {
        if (bubble != null)
        {
            bubble.Show(item, ready);
        }
    }

    // Oculta el globo de la cafetera (si el globo existe).
    private void HideBubble()
    {
        if (bubble != null)
        {
            bubble.Hide();
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================
    // Funciones que solo devuelven información: otros scripts pueden
    // preguntar cómo está la cafetera sin poder cambiarla.

    // Devuelve lo que está preparando ahora (Ninguno si no está trabajando).
    public PlayerInventory.ItemType GetProductInProgress()
    {
        return productInProgress;
    }

    // Devuelve lo que está listo para retirar (Ninguno si no hay nada).
    public PlayerInventory.ItemType GetReadyProduct()
    {
        return readyProduct;
    }
}
