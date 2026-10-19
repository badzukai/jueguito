
using UnityEngine;

// Controla la entrada y salida de un cliente.
public class MovimientoCliente : MonoBehaviour
{
    // Punto por donde el cliente entra y sale.
    [SerializeField] private Transform puntoEntrada;

    // Lugar donde el cliente espera su pedido.
    [SerializeField] private Transform puntoEspera;

    // Velocidad del movimiento.
    [SerializeField] private float velocidad = 2f;

    // Estados posibles del movimiento del cliente.
    private enum EstadoMovimiento
    {
        Quieto,
        Entrando,
        Saliendo
    }

    // Estado en el que se encuentra actualmente.
    private EstadoMovimiento estadoActual = EstadoMovimiento.Quieto;

    // Indica si llegó a su puesto.
    public bool EnPuesto { get; private set; }

    // Indica si terminó de salir.
    public bool EstaEnSalida { get; private set; }

    private void Start()
    {
        // Verificamos que los puntos estén asignados.
        if (puntoEntrada == null || puntoEspera == null)
        {
            Debug.LogError("Faltan puntos de movimiento en " + gameObject.name);
            enabled = false;
            return;
        }

        // Colocamos al cliente fuera de la cafetería.
        transform.position = puntoEntrada.position;

        // Iniciamos su entrada.
        Entrar();
    }

    private void Update()
    {
        // Mientras entra, avanza hacia el mostrador.
        if (estadoActual == EstadoMovimiento.Entrando)
        {
            MoverHacia(puntoEspera.position);

            // Comprobamos si llegó al puesto.
            if (Vector3.Distance(
                transform.position, puntoEspera.position) < 0.01f)
            {
                // Detenemos el movimiento.
                estadoActual = EstadoMovimiento.Quieto;

                // Marcamos que ya está en su puesto.
                EnPuesto = true;

                Debug.Log(gameObject.name + " llegó al mostrador.");

                // Mostramos el pedido al llegar.
                Cliente cliente = GetComponent<Cliente>();

                if (cliente != null)
                {
                    cliente.MostrarPedido();
                }
            }
        }

        // Mientras sale, avanza hacia la salida.
        else if (estadoActual == EstadoMovimiento.Saliendo)
        {
            MoverHacia(puntoEntrada.position);

            // Comprobamos si llegó a la salida.
            if (Vector3.Distance(
                transform.position, puntoEntrada.position) < 0.01f)
            {
                // Detenemos el movimiento.
                estadoActual = EstadoMovimiento.Quieto;

                // Avisamos que terminó de salir.
                EstaEnSalida = true;

                Debug.Log(gameObject.name + " salió de la cafetería.");
            }
        }
    }

    // Inicia el movimiento de entrada.
    public void Entrar()
    {
        // Reiniciamos los indicadores del movimiento.
        EnPuesto = false;
        EstaEnSalida = false;

        // Colocamos al cliente en la entrada.
        transform.position = puntoEntrada.position;

        // Empezamos a caminar hacia el mostrador.
        estadoActual = EstadoMovimiento.Entrando;
    }

    // Inicia el movimiento de salida.
    public void Salir()
    {
        // El cliente deja de estar disponible en su puesto.
        EnPuesto = false;
        EstaEnSalida = false;

        // Comenzamos a caminar hacia la salida.
        estadoActual = EstadoMovimiento.Saliendo;
    }

    // Mueve al cliente hacia el destino indicado.
    private void MoverHacia(Vector3 destino)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );
    }
}
