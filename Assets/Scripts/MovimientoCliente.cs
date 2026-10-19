using UnityEngine;

// Controla la entrada y salida de un cliente.
// Solo se encarga de caminar; cuando llega a un destino,
// le avisa al script Cliente para que haga lo que corresponda.
public class MovimientoCliente : MonoBehaviour
{
    // Punto por donde el cliente entra y sale.
    [SerializeField] private Transform puntoEntrada;

    // Lugar donde el cliente espera su pedido.
    [SerializeField] private Transform puntoEspera;

    // Velocidad del movimiento.
    [SerializeField] private float velocidad = 2f;

    // Constante: distancia a la que consideramos que ya llegó.
    // Usamos const porque este valor nunca cambia durante el juego.
    private const float DistanciaLlegada = 0.01f;

    // Indica si está caminando hacia el mostrador.
    private bool entrando = false;

    // Indica si está caminando hacia la salida.
    private bool saliendo = false;

    // Indica si está parado en su puesto, esperando.
    private bool enPuesto = false;

    // Referencia al script Cliente del mismo objeto.
    private Cliente cliente;

    private void Awake()
    {
        // Guardamos el Cliente una sola vez.
        cliente = GetComponent<Cliente>();

        // Verificamos que los puntos estén asignados.
        if (puntoEntrada == null || puntoEspera == null)
        {
            Debug.LogError("Faltan puntos de movimiento en " + gameObject.name);
            enabled = false;
            return;
        }

        // Colocamos al cliente fuera de la cafetería.
        // Recién va a caminar cuando el GameManager le diga que entre.
        transform.position = puntoEntrada.position;
    }

    private void Update()
    {
        // Mientras entra, avanza hacia el mostrador.
        if (entrando)
        {
            MoverHacia(puntoEspera.position);

            // Comprobamos si llegó al puesto.
            if (LlegoA(puntoEspera.position))
            {
                entrando = false;
                enPuesto = true;

                Debug.Log(gameObject.name + " llegó al mostrador.");

                // Le avisamos al cliente para que muestre su pedido.
                cliente.MostrarPedido();
            }
        }
        // Mientras sale, avanza hacia la salida.
        else if (saliendo)
        {
            MoverHacia(puntoEntrada.position);

            // Comprobamos si llegó a la salida.
            if (LlegoA(puntoEntrada.position))
            {
                saliendo = false;

                Debug.Log(gameObject.name + " salió de la cafetería.");

                // Le avisamos al cliente que ya se fue.
                cliente.TerminoDeSalir();
            }
        }
    }

    // Empieza a caminar desde la entrada hacia el mostrador.
    public void Entrar()
    {
        enPuesto = false;
        saliendo = false;

        // Colocamos al cliente en la entrada.
        transform.position = puntoEntrada.position;

        entrando = true;
    }

    // Empieza a caminar desde el mostrador hacia la salida.
    public void Salir()
    {
        enPuesto = false;
        entrando = false;
        saliendo = true;
    }

    // Devuelve true si el cliente está parado en su puesto.
    public bool EstaEnPuesto()
    {
        return enPuesto;
    }

    // Mueve al cliente un poquito hacia el destino en cada frame.
    // MoveTowards avanza como máximo "velocidad * Time.deltaTime",
    // así el movimiento es suave y no se pasa del destino.
    private void MoverHacia(Vector3 destino)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );
    }

    // Devuelve true si el cliente ya está prácticamente en el destino.
    private bool LlegoA(Vector3 destino)
    {
        return Vector3.Distance(transform.position, destino) < DistanciaLlegada;
    }
}
