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
public class MenuPrincipal : MonoBehaviour
{
    // Panel con los botones principales.
    [SerializeField] private GameObject panelPrincipal;

    // Panel con las instrucciones.
    [SerializeField] private GameObject panelComoJugar;

    // Panel con los créditos.
    [SerializeField] private GameObject panelCreditos;

    // Páginas de "Cómo jugar". Las guardamos en un array:
    // la posición 0 es la primera página, la 1 la segunda, etc.
    [SerializeField] private GameObject[] paginasComoJugar;

    // Texto que muestra en qué página estamos ("1 / 3").
    [SerializeField] private TMP_Text textoPagina;

    // Número de la página que se está viendo (empieza en 0).
    private int paginaActual = 0;

    private void Start()
    {
        // Si venimos de una partida que terminó, el tiempo puede haber
        // quedado en pausa (timeScale = 0). Lo volvemos a la normalidad.
        Time.timeScale = 1f;

        // Al abrir el menú, mostramos el panel principal.
        MostrarPanel(panelPrincipal);
    }

    // Carga la escena con el nombre indicado.
    // La usa el botón "Jugar", que le pasa el texto "Cafeteria".
    // La escena tiene que estar agregada en la lista de escenas
    // (File > Build Profiles > Scene List).
    public void CargarEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    // Botón "Cómo jugar": siempre arranca por la primera página.
    public void MostrarComoJugar()
    {
        MostrarPanel(panelComoJugar);
        MostrarPagina(0);
    }

    // Botón "Siguiente" de Cómo jugar.
    public void SiguientePagina()
    {
        MostrarPagina(paginaActual + 1);
    }

    // Botón "Anterior" de Cómo jugar.
    public void AnteriorPagina()
    {
        MostrarPagina(paginaActual - 1);
    }

    // Botón "Créditos".
    public void MostrarCreditos()
    {
        MostrarPanel(panelCreditos);
    }

    // Botón "Volver" de los paneles de instrucciones y créditos.
    public void VolverAlMenu()
    {
        MostrarPanel(panelPrincipal);
    }

    // Botón "Salir".
    // Application.Quit cierra el juego, pero solo funciona en el juego
    // ya compilado: dentro del editor de Unity no hace nada.
    // Por eso dejamos un mensaje en la Console para comprobarlo.
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // Muestra una sola página de "Cómo jugar" y oculta las demás.
    private void MostrarPagina(int numero)
    {
        // Si nos pasamos de la última, volvemos a la primera (y al revés).
        if (numero >= paginasComoJugar.Length)
        {
            numero = 0;
        }
        else if (numero < 0)
        {
            numero = paginasComoJugar.Length - 1;
        }

        paginaActual = numero;

        // Recorremos el array: solo queda activa la página elegida.
        for (int i = 0; i < paginasComoJugar.Length; i++)
        {
            paginasComoJugar[i].SetActive(i == paginaActual);
        }

        // Le sumamos 1 porque para el jugador la primera página es la "1".
        if (textoPagina != null)
        {
            textoPagina.text = (paginaActual + 1) + " / " + paginasComoJugar.Length;
        }
    }

    // Muestra un solo panel y oculta los demás.
    // panel == panelPrincipal da true solamente para el panel elegido,
    // así que ese queda activo y los otros dos se ocultan.
    private void MostrarPanel(GameObject panel)
    {
        panelPrincipal.SetActive(panel == panelPrincipal);
        panelComoJugar.SetActive(panel == panelComoJugar);
        panelCreditos.SetActive(panel == panelCreditos);
    }
}
