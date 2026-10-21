using UnityEngine;

// Estación donde el barista agarra las galletitas:
//   Z -> galletita red velvet
//   X -> galletita de vainilla y chocolate
//   C -> galletita de chocolate
// No usa la tecla E, por eso no hereda de InteractiveStation.
public class CookieStation : MonoBehaviour
{
    // Referencia al barista. La asignamos desde el Inspector.
    [SerializeField] private Transform player;

    // Distancia máxima a la que el barista puede estar para usarla.
    [SerializeField] private float interactionDistance = 1.5f;

    // Inventario del barista. Lo buscamos una sola vez en Awake.
    private PlayerInventory inventory;

    // Dibujo de la estación: lo pintamos de amarillo cuando el barista está cerca,
    // igual que las estaciones de la tecla E (ver PlayerInteraction).
    private SpriteRenderer drawing;

    // Color con el que se resalta.
    [SerializeField] private Color highlightColor = new Color(1f, 0.88f, 0.55f);

    private void Awake()
    {
        drawing = GetComponent<SpriteRenderer>();

        if (player != null)
        {
            inventory = player.GetComponent<PlayerInventory>();
        }
        else
        {
            Debug.LogWarning("Falta asignar el jugador en " + gameObject.name);
        }
    }

    private void Update()
    {
        // Si no tenemos el inventario, no podemos hacer nada.
        if (inventory == null)
        {
            return;
        }

        // En la pausa (o con la partida terminada) no dejamos agarrar nada.
        if (Time.timeScale == 0f)
        {
            return;
        }

        // Si el barista está cerca, la estación se pinta; si no, vuelve a su color.
        if (drawing != null)
        {
            if (IsPlayerNear())
            {
                drawing.color = highlightColor;
            }
            else
            {
                drawing.color = Color.white;
            }
        }

        // Revisamos las teclas (la distancia se vuelve a comprobar al entregar).
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GiveCookie(PlayerInventory.ItemType.RedVelvetCookie);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            GiveCookie(PlayerInventory.ItemType.VanillaChocolateCookie);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            GiveCookie(PlayerInventory.ItemType.ChocolateCookie);
        }
    }

    // Le da la galletita al barista, solo si está cerca.
    private void GiveCookie(PlayerInventory.ItemType cookie)
    {
        if (IsPlayerNear())
        {
            inventory.PickUpItem(cookie);
        }
    }

    // Devuelve true si el barista está a menos de interactionDistance.
    // Vector2.Distance calcula la distancia entre dos posiciones.
    private bool IsPlayerNear()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= interactionDistance;
    }
}
