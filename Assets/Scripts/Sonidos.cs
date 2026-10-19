using UnityEngine;

// Reproduce los efectos de sonido del juego.
// Va en el prefab HUD, junto con un AudioSource.
//
// AudioSource es el "parlante" de un objeto: reproduce sonidos (AudioClip).
// PlayOneShot reproduce un sonido una vez, sin cortar el que ya estaba sonando,
// así pueden sonar varios a la vez (por ejemplo, la máquina y una entrega).
// Los sonidos siguen andando aunque Time.timeScale sea 0 (en la pausa o al terminar).
//
// Los demás scripts lo buscan una vez con FindFirstObjectByType
// y llaman a la función que corresponda, por ejemplo sonidos.Entregar().
public class Sonidos : MonoBehaviour
{
    // ===================== SONIDOS (Inspector) =====================

    [SerializeField] private AudioClip sonidoAgarrar;
    [SerializeField] private AudioClip sonidoApoyar;
    [SerializeField] private AudioClip sonidoMaquinaLista;
    [SerializeField] private AudioClip sonidoEntregar;
    [SerializeField] private AudioClip sonidoError;
    [SerializeField] private AudioClip sonidoTirar;
    [SerializeField] private AudioClip sonidoClienteEnojado;
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private AudioClip sonidoDerrota;

    // Volumen de los efectos, de 0 (mudo) a 1 (máximo).
    [SerializeField] private float volumen = 0.7f;

    // El AudioSource que está en el mismo objeto.
    private AudioSource parlante;

    private void Awake()
    {
        parlante = GetComponent<AudioSource>();

        if (parlante == null)
        {
            Debug.LogWarning("Sonidos necesita un AudioSource en el mismo objeto.");
        }
    }

    // ============== FUNCIONES QUE LLAMAN LOS OTROS SCRIPTS ==============

    public void Agarrar()
    {
        Reproducir(sonidoAgarrar);
    }

    public void Apoyar()
    {
        Reproducir(sonidoApoyar);
    }

    public void MaquinaLista()
    {
        Reproducir(sonidoMaquinaLista);
    }

    public void Entregar()
    {
        Reproducir(sonidoEntregar);
    }

    public void Error()
    {
        Reproducir(sonidoError);
    }

    public void Tirar()
    {
        Reproducir(sonidoTirar);
    }

    public void ClienteEnojado()
    {
        Reproducir(sonidoClienteEnojado);
    }

    public void Victoria()
    {
        Reproducir(sonidoVictoria);
    }

    public void Derrota()
    {
        Reproducir(sonidoDerrota);
    }

    // Reproduce un sonido (si está asignado en el Inspector).
    private void Reproducir(AudioClip sonido)
    {
        if (parlante != null && sonido != null)
        {
            parlante.PlayOneShot(sonido, volumen);
        }
    }
}
