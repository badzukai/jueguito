using UnityEngine;

// Globo que aparece arriba de un cliente con lo que pidió:
// la bebida y, si pidió, la galletita.
//   - Solo bebida      -> globo chico con un dibujo.
//   - Bebida + galletita -> globo ancho con los dos dibujos.
//
// Este script va en el prefab "OrderBubble". No lo ponemos a mano en la escena:
// lo crea ItemCatalog con Instantiate cuando empieza el juego.
public class OrderBubble : MonoBehaviour
{
    // Imagen de fondo del globo.
    [SerializeField] private SpriteRenderer background;

    // Dibujo de la bebida pedida.
    [SerializeField] private SpriteRenderer drinkIcon;

    // Dibujo de la galletita pedida.
    [SerializeField] private SpriteRenderer cookieIcon;

    // Fondo para pedidos de un solo producto.
    [SerializeField] private Sprite singleBackground;

    // Fondo más ancho para pedidos de dos productos.
    [SerializeField] private Sprite doubleBackground;

    // Dónde va la bebida cuando está sola (centrada).
    [SerializeField] private Vector3 drinkOnlyPosition = new Vector3(0f, 0.0625f, 0f);

    // Dónde van los dos productos cuando hay galletita (uno a cada lado).
    [SerializeField] private Vector3 drinkWithCookiePosition = new Vector3(-0.28f, 0.0625f, 0f);
    [SerializeField] private Vector3 cookiePosition = new Vector3(0.28f, 0.0625f, 0f);

    // Objeto al que sigue el globo (el cliente).
    private Transform target;

    // Distancia entre el cliente y el globo.
    private Vector3 offset;

    // De dónde sacamos los dibujos de cada objeto.
    private ItemCatalog catalog;

    // Prepara el globo. La llama ItemCatalog justo después de crearlo.
    public void Setup(
        ItemCatalog itemCatalog,
        Transform targetToFollow,
        Vector3 distance)
    {
        catalog = itemCatalog;
        target = targetToFollow;
        offset = distance;

        // Al comenzar no muestra nada.
        Hide();
    }

    // LateUpdate se ejecuta al final de cada frame, después de que
    // todo se movió. Así el globo acompaña al cliente.
    private void LateUpdate()
    {
        FollowTarget();
    }

    // Ubica el globo encima de su objetivo.
    private void FollowTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    // Muestra el pedido en el globo.
    public void Show(
        PlayerInventory.ItemType drink,
        PlayerInventory.ItemType cookie)
    {
        // La bebida siempre se muestra.
        drinkIcon.sprite = catalog.GetSprite(drink);

        if (cookie == PlayerInventory.ItemType.None)
        {
            // Pedido de un solo producto: globo chico y bebida centrada.
            background.sprite = singleBackground;
            drinkIcon.transform.localPosition = drinkOnlyPosition;
            cookieIcon.gameObject.SetActive(false);
        }
        else
        {
            // Pedido doble: globo ancho, bebida a la izquierda
            // y galletita a la derecha.
            background.sprite = doubleBackground;
            drinkIcon.transform.localPosition = drinkWithCookiePosition;

            cookieIcon.sprite = catalog.GetSprite(cookie);
            cookieIcon.transform.localPosition = cookiePosition;
            cookieIcon.gameObject.SetActive(true);
        }

        // Lo ubicamos y lo hacemos visible.
        FollowTarget();
        gameObject.SetActive(true);
    }

    // Oculta el globo.
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
