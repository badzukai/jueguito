using UnityEngine;

// Mueve al barista con las flechas o con WASD.
public class PlayerMovement : MonoBehaviour
{
    // Velocidad del barista. La podemos ajustar desde el Inspector.
    [SerializeField] private float velocidad = 4f;

    // Referencia al Rigidbody2D del barista.
    // Lo usamos para moverlo respetando las colisiones.
    private Rigidbody2D rb;

    // Dirección en la que el jugador quiere moverse.
    private Vector2 movimiento;

    private void Awake()
    {
        // Guardamos el Rigidbody2D una sola vez, al comenzar,
        // para no buscarlo en cada frame.
        rb = GetComponent<Rigidbody2D>();
    }

    // Update se ejecuta una vez por frame.
    // Acá leemos el teclado.
    private void Update()
    {
        // GetAxisRaw devuelve -1, 0 o 1 según la tecla presionada.
        // Horizontal: izquierda / derecha. Vertical: abajo / arriba.
        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");

        // normalized hace que en diagonal no vaya más rápido
        // que en línea recta.
        movimiento = movimiento.normalized;
    }

    // FixedUpdate se ejecuta en intervalos fijos.
    // Es el lugar indicado para mover objetos con física.
    private void FixedUpdate()
    {
        // Calculamos la nueva posición y movemos el Rigidbody2D.
        // Multiplicamos por Time.fixedDeltaTime para que la velocidad
        // sea la misma en cualquier computadora.
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}
