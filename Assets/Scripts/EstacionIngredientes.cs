using UnityEngine;

// Estación donde el barista agarra los ingredientes básicos:
//   Z -> granos de café
//   X -> leche
//   C -> chocolate
// No usa la tecla E, por eso no hereda de EstacionInteractiva.
public class EstacionIngredientes : MonoBehaviour
{
    // Distancia máxima a la que el barista puede estar para usarla.
    [SerializeField] private float distanciaInteraccion = 1.5f;

    // Referencia al barista. La asignamos desde el Inspector.
    [SerializeField] private Transform jugador;

    // Inventario del barista. Lo buscamos una sola vez en Awake.
    private PlayerInventory inventario;

    // Dibujo de la estación: lo pintamos de amarillo cuando el barista está cerca,
    // igual que las estaciones de la tecla E (ver PlayerInteraction).
    private SpriteRenderer dibujo;

    // Color con el que se resalta.
    [SerializeField] private Color colorResaltado = new Color(1f, 0.88f, 0.55f);

    private void Awake()
    {
        dibujo = GetComponent<SpriteRenderer>();

        if (jugador != null)
        {
            inventario = jugador.GetComponent<PlayerInventory>();
        }
        else
        {
            Debug.LogWarning("Falta asignar el jugador en " + gameObject.name);
        }
    }

    private void Update()
    {
        // Si no tenemos el inventario, no podemos hacer nada.
        if (inventario == null)
        {
            return;
        }

        // En la pausa (o con la partida terminada) no dejamos agarrar nada.
        if (Time.timeScale == 0f)
        {
            return;
        }

        // Si el barista está cerca, la estación se pinta; si no, vuelve a su color.
        if (dibujo != null)
        {
            if (JugadorEstaCerca())
            {
                dibujo.color = colorResaltado;
            }
            else
            {
                dibujo.color = Color.white;
            }
        }

        // Revisamos las teclas (la distancia se vuelve a comprobar al entregar).
        if (Input.GetKeyDown(KeyCode.Z))
        {
            EntregarIngrediente(PlayerInventory.TipoObjeto.Cafe);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            EntregarIngrediente(PlayerInventory.TipoObjeto.Leche);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            EntregarIngrediente(PlayerInventory.TipoObjeto.Chocolate);
        }
    }

    // Le da el ingrediente al barista, solo si está cerca.
    private void EntregarIngrediente(PlayerInventory.TipoObjeto ingrediente)
    {
        if (JugadorEstaCerca())
        {
            inventario.RecogerObjeto(ingrediente);
        }
    }

    // Devuelve true si el barista está a menos de distanciaInteraccion.
    // Vector2.Distance calcula la distancia entre dos posiciones.
    private bool JugadorEstaCerca()
    {
        float distancia = Vector2.Distance(transform.position, jugador.position);
        return distancia <= distanciaInteraccion;
    }
}
