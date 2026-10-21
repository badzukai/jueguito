using UnityEngine;

// Permite que el barista use las estaciones que tiene cerca:
//   E -> interactuar con la estación más cercana.
//   R -> retirar un producto de la bandeja más cercana.
// Además "resalta" (pinta de amarillo) la estación que se va a usar con la E,
// así el jugador sabe con qué va a interactuar antes de apretar.
public class PlayerInteraction : MonoBehaviour
{
    // Distancia máxima entre el barista y el BORDE de una estación para usarla.
    // Medimos hasta el borde (y no hasta el centro) porque las máquinas
    // son grandes: así hay que estar pegado a la máquina para usarla.
    [SerializeField] private float reach = 0.8f;

    // Las estaciones sin collider (las bandejas) se miden hasta su centro.
    // Les "restamos" este radio para hacer de cuenta que medimos hasta su borde.
    [SerializeField] private float radiusWithoutCollider = 0.6f;

    // Inventario del barista.
    private PlayerInventory inventory;

    // Array con todas las estaciones de la escena.
    private InteractiveStation[] stations;

    // Array "paralelo": en la posición i está el dibujo (SpriteRenderer)
    // de la estación i. Lo usamos para resaltarla.
    private SpriteRenderer[] drawings;

    // Otro array paralelo: el collider de cada estación (puede ser null
    // si la estación no tiene, como las bandejas).
    private Collider2D[] colliders;

    // Color con el que se pinta la estación más cercana.
    [SerializeField] private Color highlightColor = new Color(1f, 0.88f, 0.55f);

    // Posición (en el array) de la estación resaltada. -1 = ninguna.
    private int highlightedIndex = -1;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
        // Buscamos todas las estaciones UNA sola vez, al comenzar.
        // Buscar objetos en la escena es lento, por eso no conviene
        // hacerlo cada vez que apretamos una tecla.
        // FindObjectsInactive.Include hace que también encuentre las
        // estaciones desactivadas (las bandejas empiezan ocultas).
        stations = FindObjectsByType<InteractiveStation>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        // Guardamos el dibujo de cada estación, también una sola vez.
        drawings = new SpriteRenderer[stations.Length];
        colliders = new Collider2D[stations.Length];

        for (int i = 0; i < stations.Length; i++)
        {
            drawings[i] = stations[i].GetComponent<SpriteRenderer>();
            colliders[i] = stations[i].GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        // En la pausa (o con la partida terminada) el tiempo está en 0:
        // no dejamos usar nada.
        if (Time.timeScale == 0f)
        {
            return;
        }

        // Resaltamos la estación que se usaría con la E.
        Highlight(FindNearestIndex(false));

        // Si presionamos E, interactuamos con la estación más cercana.
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractiveStation station = FindNearestStation(false);

            if (station != null)
            {
                station.Interact(inventory);
            }
        }

        // Si presionamos R, intentamos retirar un producto de una bandeja.
        if (Input.GetKeyDown(KeyCode.R))
        {
            InteractiveStation station = FindNearestStation(true);

            if (station != null)
            {
                Tray tray = station.GetComponent<Tray>();
                tray.RemoveProduct(inventory);
            }
        }
    }

    // Pinta la estación de la posición "indice" y despinta la anterior.
    private void Highlight(int index)
    {
        // Si es la misma que ya estaba resaltada, no hay nada que cambiar.
        if (index == highlightedIndex)
        {
            return;
        }

        // Devolvemos la anterior a su color normal (blanco = sin teñir).
        if (highlightedIndex != -1 && drawings[highlightedIndex] != null)
        {
            drawings[highlightedIndex].color = Color.white;
        }

        // Pintamos la nueva.
        if (index != -1 && drawings[index] != null)
        {
            drawings[index].color = highlightColor;
        }

        highlightedIndex = index;
    }

    // Devuelve la estación más cercana al barista,
    // o null si no hay ninguna dentro de la distancia de interacción.
    // onlyTrays: si es true, ignora todo lo que no sea una Bandeja.
    private InteractiveStation FindNearestStation(bool onlyTrays)
    {
        int index = FindNearestIndex(onlyTrays);

        if (index == -1)
        {
            return null;
        }

        return stations[index];
    }

    // Igual que FindNearestStation, pero devuelve la POSICIÓN en el array
    // (o -1 si no hay ninguna cerca). Así también sabemos cuál es su dibujo.
    private int FindNearestIndex(bool onlyTrays)
    {
        int nearestIndex = -1;

        // Arrancamos con la distancia máxima permitida.
        // Cada vez que encontramos algo más cerca, la achicamos.
        float shortestDistance = reach;

        // Recorremos el array con un for, como vimos en clase.
        for (int i = 0; i < stations.Length; i++)
        {
            InteractiveStation station = stations[i];

            // Ignoramos las estaciones que están ocultas en este momento
            // (por ejemplo, la bandeja de un cliente que todavía no llegó).
            if (!station.gameObject.activeInHierarchy)
            {
                continue;
            }

            // Si buscamos solo bandejas, salteamos las que no lo son.
            // GetComponent devuelve null si el objeto no tiene ese componente.
            if (onlyTrays && station.GetComponent<Tray>() == null)
            {
                continue;
            }

            // Punto de la estación al que medimos la distancia.
            // Si tiene collider, usamos ClosestPoint: devuelve el punto del
            // borde del collider más cercano al barista.
            // Si no tiene, usamos su posición.
            Vector2 point = station.transform.position;

            if (colliders[i] != null)
            {
                point = colliders[i].ClosestPoint(transform.position);
            }

            // Calculamos la distancia entre el barista y ese punto.
            float distance = Vector2.Distance(transform.position, point);

            if (colliders[i] == null)
            {
                distance = distance - radiusWithoutCollider;
            }

            // Nos quedamos con la estación más cercana.
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}
