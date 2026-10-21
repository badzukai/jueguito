using TMPro;                       // Permite usar textos de TextMeshPro.
using UnityEngine;
using UnityEngine.SceneManagement; // Permite cambiar de escena.

// Controla la escena del menú principal.
// Cada botón del menú llama a una de estas funciones desde su OnClick().
//
// El menú tiene tres paneles, y se ve uno a la vez:
//   - Principal: Jugar, Cómo jugar, Créditos y Salir.
//   - Cómo jugar: tiene varias páginas (controles, máquinas, pedidos)
//     y se pasa de una a otra con los botones Anterior y Siguiente.
//   - Créditos.
public class MainMenu : MonoBehaviour
{
    // Panel con los botones principales.
    [SerializeField] private GameObject mainPanel;

    // Panel con las instrucciones.
    [SerializeField] private GameObject howToPlayPanel;

    // Panel con los créditos.
    [SerializeField] private GameObject creditsPanel;

    // Páginas de "Cómo jugar". Las guardamos en un array:
    // la posición 0 es la primera página, la 1 la segunda, etc.
    [SerializeField] private GameObject[] howToPlayPages;

    // Texto que muestra en qué página estamos ("1 / 3").
    [SerializeField] private TMP_Text pageText;

    // Número de la página que se está viendo (empieza en 0).
    private int currentPage = 0;

    private void Start()
    {
        // Si venimos de una partida que terminó, el tiempo puede haber
        // quedado en pausa (timeScale = 0). Lo volvemos a la normalidad.
        Time.timeScale = 1f;

        // Al abrir el menú, mostramos el panel principal.
        ShowPanel(mainPanel);
    }

    // Carga la escena con el nombre indicado.
    // La usa el botón "Jugar", que le pasa el texto "Cafeteria".
    // La escena tiene que estar agregada en la lista de escenas
    // (File > Build Profiles > Scene List).
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Botón "Cómo jugar": siempre arranca por la primera página.
    public void ShowHowToPlay()
    {
        ShowPanel(howToPlayPanel);
        ShowPage(0);
    }

    // Botón "Siguiente" de Cómo jugar.
    public void NextPage()
    {
        ShowPage(currentPage + 1);
    }

    // Botón "Anterior" de Cómo jugar.
    public void PreviousPage()
    {
        ShowPage(currentPage - 1);
    }

    // Botón "Créditos".
    public void ShowCredits()
    {
        ShowPanel(creditsPanel);
    }

    // Botón "Volver" de los paneles de instrucciones y créditos.
    public void BackToMenu()
    {
        ShowPanel(mainPanel);
    }

    // Botón "Salir".
    // Application.Quit cierra el juego, pero solo funciona en el juego
    // ya compilado: dentro del editor de Unity no hace nada.
    // Por eso dejamos un mensaje en la Console para comprobarlo.
    public void Exit()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // Muestra una sola página de "Cómo jugar" y oculta las demás.
    private void ShowPage(int number)
    {
        // Si nos pasamos de la última, volvemos a la primera (y al revés).
        if (number >= howToPlayPages.Length)
        {
            number = 0;
        }
        else if (number < 0)
        {
            number = howToPlayPages.Length - 1;
        }

        currentPage = number;

        // Recorremos el array: solo queda activa la página elegida.
        for (int i = 0; i < howToPlayPages.Length; i++)
        {
            howToPlayPages[i].SetActive(i == currentPage);
        }

        // Le sumamos 1 porque para el jugador la primera página es la "1".
        if (pageText != null)
        {
            pageText.text = (currentPage + 1) + " / " + howToPlayPages.Length;
        }
    }

    // Muestra un solo panel y oculta los demás.
    // panel == mainPanel da true solamente para el panel elegido,
    // así que ese queda activo y los otros dos se ocultan.
    private void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        howToPlayPanel.SetActive(panel == howToPlayPanel);
        creditsPanel.SetActive(panel == creditsPanel);
    }
}
