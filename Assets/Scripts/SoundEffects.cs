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
public class SoundEffects : MonoBehaviour
{
    // ===================== SONIDOS (Inspector) =====================

    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private AudioClip putDownSound;
    [SerializeField] private AudioClip machineReadySound;
    [SerializeField] private AudioClip deliverSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip angryCustomerSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;

    // Volumen de los efectos, de 0 (mudo) a 1 (máximo).
    [SerializeField] private float volume = 0.7f;

    // El AudioSource que está en el mismo objeto.
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("Sonidos necesita un AudioSource en el mismo objeto.");
        }
    }

    // ============== FUNCIONES QUE LLAMAN LOS OTROS SCRIPTS ==============

    public void PickUp()
    {
        PlaySound(pickUpSound);
    }

    public void PutDown()
    {
        PlaySound(putDownSound);
    }

    public void MachineReady()
    {
        PlaySound(machineReadySound);
    }

    public void Deliver()
    {
        PlaySound(deliverSound);
    }

    public void Error()
    {
        PlaySound(errorSound);
    }

    public void Throw()
    {
        PlaySound(throwSound);
    }

    public void AngryCustomer()
    {
        PlaySound(angryCustomerSound);
    }

    public void Victory()
    {
        PlaySound(victorySound);
    }

    public void Defeat()
    {
        PlaySound(defeatSound);
    }

    // Reproduce un sonido (si está asignado en el Inspector).
    private void PlaySound(AudioClip sound)
    {
        if (audioSource != null && sound != null)
        {
            audioSource.PlayOneShot(sound, volume);
        }
    }
}
