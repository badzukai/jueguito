using UnityEngine;

// Pausa del juego: con Esc se abre un panel con Continuar, Reiniciar y Menú.
// Va en el prefab HUD; el panel de pausa es un hijo del HUD.
//
// Para pausar usamos Time.timeScale = 0, como al terminar la partida:
// el tiempo deja de correr, así que no se mueve nada ni bajan los temporizadores.
public class MenuPausa : MonoBehaviour
{
    // Panel que se muestra mientras el juego está en pausa.
    [SerializeField] private GameObject panelPausa;

    // true mientras el juego está pausado.
    private bool enPausa = false;

    // El GameManager sabe reiniciar la partida y volver al menú.
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        panelPausa.SetActive(false);
    }

    private void Update()
    {
        // Esc abre la pausa, y si ya estaba abierta, la cierra.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enPausa)
            {
                Continuar();
            }
            else
            {
                Pausar();
            }
        }
    }

    // Abre la pausa (salvo que la partida ya haya terminado).
    public void Pausar()
    {
        if (gameManager != null && gameManager.EstaTerminadaPartida())
        {
            return;
        }

        enPausa = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
    }

    // Botón "Continuar": cierra la pausa y el tiempo vuelve a correr.
    public void Continuar()
    {
        enPausa = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
    }

    // Botón "Reiniciar".
    public void Reiniciar()
    {
        if (gameManager != null)
        {
            gameManager.ReiniciarPartida();
        }
    }

    // Botón "Menú".
    public void IrAlMenu()
    {
        if (gameManager != null)
        {
            gameManager.IrAlMenu();
        }
    }
}
