using UnityEngine;

// Permite que el barista use las estaciones que tiene cerca:
//   E -> interactuar con la estación más cercana.
//   R -> retirar un producto de la bandeja más cercana.
// Además "resalta" (pinta de amarillo) la estación que se va a usar con la E,
// así el jugador sabe con qué va a interactuar antes de apretar.
public class PlayerInteraction : MonoBehaviour
{
    // Distancia máxima a la que puede estar una estación para usarla.
    [SerializeField] private float distanciaInteraccion = 1.2f;

    // Inventario del barista.
    private PlayerInventory inventario;

    // Array con todas las estaciones de la escena.
    private EstacionInteractiva[] estaciones;

    // Array "paralelo": en la posición i está el dibujo (SpriteRenderer)
    // de la estación i. Lo usamos para resaltarla.
    private SpriteRenderer[] dibujos;

    // Color con el que se pinta la estación más cercana.
    [SerializeField] private Color colorResaltado = new Color(1f, 0.88f, 0.55f);

    // Posición (en el array) de la estación resaltada. -1 = ninguna.
    private int indiceResaltado = -1;

    private void Awake()
    {
        inventario = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
        // Buscamos todas las estaciones UNA sola vez, al comenzar.
        // Buscar objetos en la escena es lento, por eso no conviene
        // hacerlo cada vez que apretamos una tecla.
        // FindObjectsInactive.Include hace que también encuentre las
        // estaciones desactivadas (las bandejas empiezan ocultas).
        estaciones = FindObjectsByType<EstacionInteractiva>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        // Guardamos el dibujo de cada estación, también una sola vez.
        dibujos = new SpriteRenderer[estaciones.Length];

        for (int i = 0; i < estaciones.Length; i++)
        {
            dibujos[i] = estaciones[i].GetComponent<SpriteRenderer>();
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
        Resaltar(BuscarIndiceCercano(false));

        // Si presionamos E, interactuamos con la estación más cercana.
        if (Input.GetKeyDown(KeyCode.E))
        {
            EstacionInteractiva estacion = BuscarEstacionCercana(false);

            if (estacion != null)
            {
                estacion.Interactuar(inventario);
            }
        }

        // Si presionamos R, intentamos retirar un producto de una bandeja.
        if (Input.GetKeyDown(KeyCode.R))
        {
            EstacionInteractiva estacion = BuscarEstacionCercana(true);

            if (estacion != null)
            {
                Bandeja bandeja = estacion.GetComponent<Bandeja>();
                bandeja.RetirarProducto(inventario);
            }
        }
    }

    // Pinta la estación de la posición "indice" y despinta la anterior.
    private void Resaltar(int indice)
    {
        // Si es la misma que ya estaba resaltada, no hay nada que cambiar.
        if (indice == indiceResaltado)
        {
            return;
        }

        // Devolvemos la anterior a su color normal (blanco = sin teñir).
        if (indiceResaltado != -1 && dibujos[indiceResaltado] != null)
        {
            dibujos[indiceResaltado].color = Color.white;
        }

        // Pintamos la nueva.
        if (indice != -1 && dibujos[indice] != null)
        {
            dibujos[indice].color = colorResaltado;
        }

        indiceResaltado = indice;
    }

    // Devuelve la estación más cercana al barista,
    // o null si no hay ninguna dentro de la distancia de interacción.
    // soloBandejas: si es true, ignora todo lo que no sea una Bandeja.
    private EstacionInteractiva BuscarEstacionCercana(bool soloBandejas)
    {
        int indice = BuscarIndiceCercano(soloBandejas);

        if (indice == -1)
        {
            return null;
        }

        return estaciones[indice];
    }

    // Igual que BuscarEstacionCercana, pero devuelve la POSICIÓN en el array
    // (o -1 si no hay ninguna cerca). Así también sabemos cuál es su dibujo.
    private int BuscarIndiceCercano(bool soloBandejas)
    {
        int indiceCercano = -1;

        // Arrancamos con la distancia máxima permitida.
        // Cada vez que encontramos algo más cerca, la achicamos.
        float menorDistancia = distanciaInteraccion;

        // Recorremos el array con un for, como vimos en clase.
        for (int i = 0; i < estaciones.Length; i++)
        {
            EstacionInteractiva estacion = estaciones[i];

            // Ignoramos las estaciones que están ocultas en este momento
            // (por ejemplo, la bandeja de un cliente que todavía no llegó).
            if (!estacion.gameObject.activeInHierarchy)
            {
                continue;
            }

            // Si buscamos solo bandejas, salteamos las que no lo son.
            // GetComponent devuelve null si el objeto no tiene ese componente.
            if (soloBandejas && estacion.GetComponent<Bandeja>() == null)
            {
                continue;
            }

            // Calculamos la distancia entre el barista y la estación.
            float distancia = Vector2.Distance(
                transform.position,
                estacion.transform.position
            );

            // Nos quedamos con la estación más cercana.
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                indiceCercano = i;
            }
        }

        return indiceCercano;
    }
}
