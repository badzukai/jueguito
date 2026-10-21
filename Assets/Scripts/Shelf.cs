using UnityEngine;

// La repisa tiene varios espacios (por defecto, dos) para apoyar objetos.
// También sirve para combinar bebidas:
//   - Café preparado + leche espumada        -> café con leche.
//   - Chocolate preparado + leche espumada   -> chocolate con leche.
// Hereda de InteractiveStation, así que se usa con la tecla E.
//
// Qué pasa al apretar E:
//   - Si el barista lleva algo que combina con un objeto de la repisa,
//     se combinan en ese espacio.
//   - Si no combina, lo apoya en un espacio libre.
//   - Si tiene las manos vacías, agarra el objeto del espacio más cercano.
public class Shelf : InteractiveStation
{
    // Posición de cada espacio respecto del centro de la repisa.
    // La cantidad de elementos de este array define cuántos espacios hay.
    // Se puede cambiar desde el Inspector.
    [SerializeField]
    private Vector3[] slotPositions =
    {
        new Vector3(-0.4f, 0.3f, 0f),
        new Vector3(0.4f, 0.3f, 0f)
    };

    // Objeto apoyado en cada espacio. Ninguno significa que está libre.
    private PlayerInventory.ItemType[] slots;

    // Dibujo que muestra el objeto de cada espacio.
    private ItemBubble[] bubbles;

    private void Start()
    {
        // Creamos los arrays con un lugar por cada espacio.
        // Un array nuevo de ItemType arranca lleno de Ninguno (el valor 0),
        // así que todos los espacios empiezan libres.
        slots = new PlayerInventory.ItemType[slotPositions.Length];
        bubbles = new ItemBubble[slotPositions.Length];

        // Creamos un dibujo para cada espacio (sin el fondo de globo,
        // para que el objeto se vea apoyado en la repisa).
        ItemCatalog catalog = FindFirstObjectByType<ItemCatalog>();

        if (catalog != null)
        {
            for (int i = 0; i < slotPositions.Length; i++)
            {
                bubbles[i] = catalog.CreateBubble(transform, slotPositions[i], false);
            }
        }
    }

    // Se ejecuta cuando el barista se acerca y presiona E.
    public override void Interact(PlayerInventory inventory)
    {
        if (inventory.HasItem())
        {
            PlaceItem(inventory);
        }
        else
        {
            TakeItem(inventory);
        }
    }

    // El barista lleva algo: intentamos combinarlo o apoyarlo.
    private void PlaceItem(PlayerInventory inventory)
    {
        PlayerInventory.ItemType itemInHand = inventory.GetCurrentItem();

        // 1) ¿Combina con algo que ya está en la repisa?
        for (int i = 0; i < slots.Length; i++)
        {
            PlayerInventory.ItemType result = Combine(slots[i], itemInHand);

            if (result != PlayerInventory.ItemType.None)
            {
                slots[i] = result;
                inventory.DropItem();
                UpdateSlot(i);

                Debug.Log("¡Preparaste " + result + "!");

                // Sonido de "¡listo!" cuando se mezclan.
                SoundEffects s = GetSoundEffects();

                if (s != null)
                {
                    s.MachineReady();
                }

                return;
            }
        }

        // 2) Si no combina, lo apoyamos en el primer espacio libre.
        int freeSlot = FindFreeSlot();

        if (freeSlot == -1)
        {
            Notify("La repisa está llena.");
            return;
        }

        slots[freeSlot] = itemInHand;
        inventory.DropItem();
        UpdateSlot(freeSlot);

        Debug.Log("Guardaste: " + itemInHand);
    }

    // El barista tiene las manos vacías: agarra el objeto más cercano.
    private void TakeItem(PlayerInventory inventory)
    {
        int slot = FindNearestOccupiedSlot(inventory.transform.position);

        if (slot == -1)
        {
            Notify("La repisa está vacía.");
            return;
        }

        if (inventory.PickUpItem(slots[slot]))
        {
            slots[slot] = PlayerInventory.ItemType.None;
            UpdateSlot(slot);
        }
    }

    // Devuelve el número del primer espacio libre,
    // o -1 si están todos ocupados.
    private int FindFreeSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == PlayerInventory.ItemType.None)
            {
                return i;
            }
        }

        return -1;
    }

    // Devuelve el número del espacio ocupado más cercano a una posición,
    // o -1 si están todos libres.
    private int FindNearestOccupiedSlot(Vector3 baristaPosition)
    {
        int nearest = -1;
        float shortestDistance = 0f;

        for (int i = 0; i < slots.Length; i++)
        {
            // Los espacios libres no nos interesan.
            if (slots[i] == PlayerInventory.ItemType.None)
            {
                continue;
            }

            Vector3 slotPosition = transform.position + slotPositions[i];
            float distance = Vector2.Distance(baristaPosition, slotPosition);

            // Nos quedamos con el primero que encontramos,
            // o con este si está más cerca que el anterior.
            if (nearest == -1 || distance < shortestDistance)
            {
                nearest = i;
                shortestDistance = distance;
            }
        }

        return nearest;
    }

    // Devuelve la bebida que sale de mezclar a y b,
    // o Ninguno si esos dos objetos no se pueden combinar.
    private PlayerInventory.ItemType Combine(
        PlayerInventory.ItemType a,
        PlayerInventory.ItemType b)
    {
        if (IsPair(a, b,
            PlayerInventory.ItemType.Coffee,
            PlayerInventory.ItemType.FoamedMilk))
        {
            return PlayerInventory.ItemType.CoffeeWithMilk;
        }

        if (IsPair(a, b,
            PlayerInventory.ItemType.HotChocolate,
            PlayerInventory.ItemType.FoamedMilk))
        {
            return PlayerInventory.ItemType.ChocolateWithMilk;
        }

        return PlayerInventory.ItemType.None;
    }

    // Devuelve true si a y b son los productos x e y, en cualquier orden.
    // && significa Y, || significa O.
    private bool IsPair(
        PlayerInventory.ItemType a,
        PlayerInventory.ItemType b,
        PlayerInventory.ItemType x,
        PlayerInventory.ItemType y)
    {
        return (a == x && b == y) || (a == y && b == x);
    }

    // Actualiza el dibujo de un espacio según lo que tenga.
    private void UpdateSlot(int i)
    {
        if (bubbles[i] != null)
        {
            bubbles[i].Show(slots[i]);
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================

    // Devuelve true si alguno de los espacios tiene ese objeto.
    public bool Has(PlayerInventory.ItemType item)
    {
        // Antes de Start el array todavía no existe.
        if (slots == null)
        {
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == item)
            {
                return true;
            }
        }

        return false;
    }
}
