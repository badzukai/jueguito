
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float distanciaInteraccion = 1.2f;

    private PlayerInventory inventario;

    private void Awake()
    {
        inventario = GetComponent<PlayerInventory>();
    }


    private void Update()
    {
        // Si presionamos E, interactuamos normalmente con una estación.
        if (Input.GetKeyDown(KeyCode.E))
        {
            BuscarEstacion(false);
        }

        // Si presionamos R, intentamos retirar un objeto de una bandeja.
        if (Input.GetKeyDown(KeyCode.R))
        {
            BuscarEstacion(true);
        }
    }


    // Recibe true cuando queremos retirar un producto con R.
    private void BuscarEstacion(bool retirar)
    {
        EstacionInteractiva[] estaciones =
            FindObjectsByType<EstacionInteractiva>(FindObjectsSortMode.None);

        EstacionInteractiva estacionCercana = null;
        float menorDistancia = distanciaInteraccion;

        foreach (EstacionInteractiva estacion in estaciones)
        {
            // Si presionamos R, solo buscamos objetos de tipo Bandeja.
            // Ignoramos las máquinas y las repisas.
            if (retirar && !(estacion is Bandeja))
            {
                continue;
            }

            // Calculamos la distancia entre el jugador y la estación.
            float distancia = Vector2.Distance(
                transform.position,
                estacion.transform.position
            );

            // Nos quedamos con la estación más cercana.
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                estacionCercana = estacion;
            }
        }

        // Si no encontramos ninguna estación cercana, terminamos.
        if (estacionCercana == null)
        {
            return;
        }

        // Si presionamos R y encontramos una bandeja,
        // intentamos retirar un producto.
        if (retirar && estacionCercana is Bandeja bandeja)
        {
            bandeja.RetirarProducto(inventario);
        }
        // Si presionamos E, usamos la interacción habitual.
        else if (!retirar)
        {
            estacionCercana.Interactuar(inventario);
        }

    }
}
