using UnityEngine;

// Estación donde el barista agarra los ingredientes básicos:
//   Z -> granos de café
//   X -> leche
//   C -> chocolate
// No usa la tecla E, por eso no hereda de InteractiveStation.
public class IngredientStation : MonoBehaviour
{
    // Distancia máxima a la que el barista puede estar para usarla.
    [SerializeField] private float interactionDistance = 1.5f;

    // Referencia al barista. La asignamos desde el Inspector.
    [SerializeField] private Transform player;

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
            GiveIngredient(PlayerInventory.ItemType.CoffeeBeans);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            GiveIngredient(PlayerInventory.ItemType.Milk);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            GiveIngredient(PlayerInventory.ItemType.Chocolate);
        }
    }

    // Le da el ingrediente al barista, solo si está cerca.
    private void GiveIngredient(PlayerInventory.ItemType ingredient)
    {
        if (IsPlayerNear())
        {
            inventory.PickUpItem(ingredient);
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
