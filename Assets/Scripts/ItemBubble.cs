using UnityEngine;

// Muestra el dibujo de un objeto en un globito, encima de algo:
// el barista, una máquina o un espacio de la repisa.
//
// Este script va en el prefab "Globo". No lo ponemos a mano en la escena:
// lo crea ItemCatalog con Instantiate cuando empieza el juego.
public class ItemBubble : MonoBehaviour
{
    // Imagen de fondo del globo (la nube blanca).
    [SerializeField] private SpriteRenderer background;

    // Imagen del objeto que se muestra dentro del globo.
    [SerializeField] private SpriteRenderer icon;

    // Transparencia del ícono cuando la máquina todavía está trabajando.
    // 1 = se ve completo, 0 = invisible.
    [SerializeField] private float inProgressAlpha = 0.5f;

    // Objeto al que sigue el globo (el barista, una máquina, una repisa).
    private Transform target;

    // Distancia entre el objetivo y el globo.
    private Vector3 offset;

    // De dónde sacamos los dibujos de cada objeto.
    private ItemCatalog catalog;

    // Prepara el globo. La llama ItemCatalog justo después de crearlo.
    // withBackground: true para el globito de diálogo, false para mostrar
    // solo el objeto (por ejemplo, apoyado en la repisa).
    public void Setup(
        ItemCatalog itemCatalog,
        Transform targetToFollow,
        Vector3 distance,
        bool withBackground)
    {
        catalog = itemCatalog;
        target = targetToFollow;
        offset = distance;

        background.gameObject.SetActive(withBackground);

        // Al comenzar no muestra nada.
        Hide();
    }

    // LateUpdate se ejecuta al final de cada frame, después de que
    // todo se movió. Así el globo acompaña al barista sin quedarse atrás.
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

    // Muestra un objeto en el globo.
    // listo es un parámetro opcional: si no lo pasamos, vale true.
    // Con listo = false el ícono se ve medio transparente
    // (lo usamos mientras una máquina está trabajando).
    public void Show(PlayerInventory.ItemType item, bool ready = true)
    {
        // Si no hay objeto, no hay nada que mostrar.
        if (item == PlayerInventory.ItemType.None)
        {
            Hide();
            return;
        }

        // Buscamos el dibujo de este objeto en el catálogo.
        icon.sprite = catalog.GetSprite(item);

        // Elegimos la transparencia según si está listo o no.
        // El cuarto valor de un Color es la transparencia (alfa).
        Color color = Color.white;

        if (!ready)
        {
            color.a = inProgressAlpha;
        }

        icon.color = color;

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
