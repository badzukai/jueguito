using UnityEngine;

// Pausa del juego: con Esc se abre un panel con Continuar, Reiniciar y Menú.
// Va en el prefab HUD; el panel de pausa es un hijo del HUD.
//
// Para pausar usamos Time.timeScale = 0, como al terminar la partida:
// el tiempo deja de correr, así que no se mueve nada ni bajan los temporizadores.
public class PauseMenu : MonoBehaviour
{
    // Panel que se muestra mientras el juego está en pausa.
    [SerializeField] private GameObject pausePanel;

    // true mientras el juego está pausado.
    private bool paused = false;

    // El GameManager sabe reiniciar la partida y volver al menú.
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        pausePanel.SetActive(false);
    }

    private void Update()
    {
        // Esc abre la pausa, y si ya estaba abierta, la cierra.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Abre la pausa (salvo que la partida ya haya terminado).
    public void Pause()
    {
        if (gameManager != null && gameManager.IsGameOver())
        {
            return;
        }

        paused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Botón "Continuar": cierra la pausa y el tiempo vuelve a correr.
    public void Resume()
    {
        paused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Botón "Reiniciar".
    public void Restart()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    // Botón "Menú".
    public void GoToMenu()
    {
        if (gameManager != null)
        {
            gameManager.GoToMenu();
        }
    }
}
