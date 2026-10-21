using UnityEngine;

// Guarda el dibujo (Sprite) de cada objeto del juego y crea los globos.
//
// Es el ÚNICO lugar donde se eligen los dibujos de los objetos.
// Cuando tengamos el pixel art definitivo, alcanza con:
//   - reemplazar los archivos PNG de Assets/Sprites/Objetos
//     por otros con el mismo nombre, o
//   - arrastrar los nuevos sprites a estos campos en el Inspector.
//
// Va en el prefab "ItemCatalog", que tiene que estar UNA vez en la escena.
public class ItemCatalog : MonoBehaviour
{
    // Prefab del globo que aparece encima de las cosas.
    [SerializeField] private ItemBubble bubblePrefab;

    // Prefab del globo con el pedido de cada cliente.
    [SerializeField] private OrderBubble orderBubblePrefab;

    // ----- Ingredientes -----
    [SerializeField] private Sprite coffeeBeansSprite;
    [SerializeField] private Sprite milkSprite;
    [SerializeField] private Sprite chocolateSprite;

    // ----- Productos intermedios -----
    [SerializeField] private Sprite groundCoffeeSprite;
    [SerializeField] private Sprite foamedMilkSprite;

    // ----- Bebidas terminadas -----
    [SerializeField] private Sprite coffeeSprite;
    [SerializeField] private Sprite hotChocolateSprite;
    [SerializeField] private Sprite coffeeWithMilkSprite;
    [SerializeField] private Sprite chocolateWithMilkSprite;

    // ----- Galletitas -----
    [SerializeField] private Sprite redVelvetCookieSprite;
    [SerializeField] private Sprite vanillaChocolateCookieSprite;
    [SerializeField] private Sprite chocolateCookieSprite;

    // Devuelve el dibujo que corresponde a cada objeto.
    // Usamos switch porque comparamos una misma variable contra muchos valores.
    public Sprite GetSprite(PlayerInventory.ItemType item)
    {
        switch (item)
        {
            case PlayerInventory.ItemType.CoffeeBeans:
                return coffeeBeansSprite;

            case PlayerInventory.ItemType.Milk:
                return milkSprite;

            case PlayerInventory.ItemType.Chocolate:
                return chocolateSprite;

            case PlayerInventory.ItemType.GroundCoffee:
                return groundCoffeeSprite;

            case PlayerInventory.ItemType.FoamedMilk:
                return foamedMilkSprite;

            case PlayerInventory.ItemType.Coffee:
                return coffeeSprite;

            case PlayerInventory.ItemType.HotChocolate:
                return hotChocolateSprite;

            case PlayerInventory.ItemType.CoffeeWithMilk:
                return coffeeWithMilkSprite;

            case PlayerInventory.ItemType.ChocolateWithMilk:
                return chocolateWithMilkSprite;

            case PlayerInventory.ItemType.RedVelvetCookie:
                return redVelvetCookieSprite;

            case PlayerInventory.ItemType.VanillaChocolateCookie:
                return vanillaChocolateCookieSprite;

            case PlayerInventory.ItemType.ChocolateCookie:
                return chocolateCookieSprite;

            default:
                return null;
        }
    }

    // Crea un globo nuevo que sigue a "objetivo".
    // desplazamiento: dónde va el globo respecto del objetivo
    //                 (por ejemplo, (0, 1, 0) es una unidad más arriba).
    // withBackground: true para el globito de diálogo, false para mostrar solo el objeto.
    // Devuelve el globo creado, para que quien lo pidió lo pueda usar.
    public ItemBubble CreateBubble(Transform target, Vector3 offset, bool withBackground)
    {
        // Instantiate crea una copia del prefab en la escena.
        // Quaternion.identity significa "sin rotación".
        ItemBubble bubble = Instantiate(
            bubblePrefab,
            target.position + offset,
            Quaternion.identity
        );

        bubble.Setup(this, target, offset, withBackground);

        return bubble;
    }

    // Crea el globo de pedido de un cliente. Funciona igual que CreateBubble.
    public OrderBubble CreateOrderBubble(Transform target, Vector3 offset)
    {
        OrderBubble bubble = Instantiate(
            orderBubblePrefab,
            target.position + offset,
            Quaternion.identity
        );

        bubble.Setup(this, target, offset);

        return bubble;
    }
}
