using UnityEngine;

// Mueve al barista con las flechas o con WASD.
public class PlayerMovement : MonoBehaviour
{
    // Velocidad del barista. La podemos ajustar desde el Inspector.
    [SerializeField] private float speed = 4f;

    // Referencia al Rigidbody2D del barista.
    // Lo usamos para moverlo respetando las colisiones.
    private Rigidbody2D rb;

    // Dirección en la que el jugador quiere moverse.
    private Vector2 movement;

    // Animator del barista: reproduce las animaciones de caminar
    // (los estados Walk_Down, Walk_Up, Walk_Left y Walk_Right de Player.controller).
    private Animator animator;

    // Nombre de la animación que corresponde a la última dirección.
    // Arranca mirando para abajo (hacia la cámara).
    private string currentAnimation = "Walk_Down";

    private void Awake()
    {
        // Guardamos el Rigidbody2D una sola vez, al comenzar,
        // para no buscarlo en cada frame.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update se ejecuta una vez por frame.
    // Acá leemos el teclado.
    private void Update()
    {
        // GetAxisRaw devuelve -1, 0 o 1 según la tecla presionada.
        // Horizontal: izquierda / derecha. Vertical: abajo / arriba.
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // normalized hace que en diagonal no vaya más rápido
        // que en línea recta.
        movement = movement.normalized;

        UpdateAnimation();
    }

    // Elige qué animación mostrar según hacia dónde camina el barista.
    private void UpdateAnimation()
    {
        // Si el barista no tiene Animator, no hacemos nada.
        if (animator == null)
        {
            return;
        }

        // Elegimos la dirección. Si va en diagonal, gana el costado.
        if (movement.x > 0)
        {
            currentAnimation = "Walk_Right";
        }
        else if (movement.x < 0)
        {
            currentAnimation = "Walk_Left";
        }
        else if (movement.y > 0)
        {
            currentAnimation = "Walk_Up";
        }
        else if (movement.y < 0)
        {
            currentAnimation = "Walk_Down";
        }

        bool isWalking = movement.x != 0 || movement.y != 0;

        if (isWalking)
        {
            // Play cambia al estado con ese nombre. Si ya está en ese estado,
            // sigue reproduciéndolo (no lo reinicia).
            animator.speed = 1f;
            animator.Play(currentAnimation);
        }
        else
        {
            // Quieto: volvemos al primer cuadro (el barista parado)
            // y frenamos la animación con speed = 0.
            // El 0 es la capa (Base Layer) y el 0f es el comienzo del clip.
            animator.Play(currentAnimation, 0, 0f);
            animator.speed = 0f;
        }
    }

    // FixedUpdate se ejecuta en intervalos fijos.
    // Es el lugar indicado para mover objetos con física.
    private void FixedUpdate()
    {
        // Calculamos la nueva posición y movemos el Rigidbody2D.
        // Multiplicamos por Time.fixedDeltaTime para que la velocidad
        // sea la misma en cualquier computadora.
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
