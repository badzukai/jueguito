using UnityEngine;

// Controla la entrada y salida de un cliente.
// Solo se encarga de caminar; cuando llega a un destino,
// le avisa al script Cliente para que haga lo que corresponda.
public class CustomerMovement : MonoBehaviour
{
    // Punto por donde el cliente entra y sale.
    [SerializeField] private Transform entryPoint;

    // Lugar donde el cliente espera su pedido.
    [SerializeField] private Transform waitPoint;

    // Velocidad del movimiento.
    [SerializeField] private float speed = 2f;

    // Constante: distancia a la que consideramos que ya llegó.
    // Usamos const porque este valor nunca cambia durante el juego.
    private const float ArrivalDistance = 0.01f;

    // Indica si está caminando hacia el mostrador.
    private bool entering = false;

    // Indica si está caminando hacia la salida.
    private bool leaving = false;

    // Indica si está parado en su puesto, esperando.
    private bool atSpot = false;

    // Referencia al script Cliente del mismo objeto.
    private Customer customer;

    private void Awake()
    {
        // Guardamos el Cliente una sola vez.
        customer = GetComponent<Customer>();

        // Verificamos que los puntos estén asignados.
        if (entryPoint == null || waitPoint == null)
        {
            Debug.LogError("Faltan puntos de movimiento en " + gameObject.name);
            enabled = false;
            return;
        }

        // Colocamos al cliente fuera de la cafetería.
        // Recién va a caminar cuando el GameManager le diga que entre.
        transform.position = entryPoint.position;
    }

    private void Update()
    {
        // Mientras entra, avanza hacia el mostrador.
        if (entering)
        {
            MoveTowardsPoint(waitPoint.position);

            // Comprobamos si llegó al puesto.
            if (HasReached(waitPoint.position))
            {
                entering = false;
                atSpot = true;

                Debug.Log(gameObject.name + " llegó al mostrador.");

                // Le avisamos al cliente para que muestre su pedido.
                customer.ShowOrder();
            }
        }
        // Mientras sale, avanza hacia la salida.
        else if (leaving)
        {
            MoveTowardsPoint(entryPoint.position);

            // Comprobamos si llegó a la salida.
            if (HasReached(entryPoint.position))
            {
                leaving = false;

                Debug.Log(gameObject.name + " salió de la cafetería.");

                // Le avisamos al cliente que ya se fue.
                customer.FinishedLeaving();
            }
        }
    }

    // Empieza a caminar desde la entrada hacia el mostrador.
    public void Enter()
    {
        atSpot = false;
        leaving = false;

        // Colocamos al cliente en la entrada.
        transform.position = entryPoint.position;

        entering = true;
    }

    // Empieza a caminar desde el mostrador hacia la salida.
    public void Exit()
    {
        atSpot = false;
        entering = false;
        leaving = true;
    }

    // Devuelve true si el cliente está parado en su puesto.
    public bool IsAtSpot()
    {
        return atSpot;
    }

    // Mueve al cliente un poquito hacia el destino en cada frame.
    // MoveTowards avanza como máximo "velocidad * Time.deltaTime",
    // así el movimiento es suave y no se pasa del destino.
    private void MoveTowardsPoint(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * Time.deltaTime
        );
    }

    // Devuelve true si el cliente ya está prácticamente en el destino.
    private bool HasReached(Vector3 destination)
    {
        return Vector3.Distance(transform.position, destination) < ArrivalDistance;
    }
}
