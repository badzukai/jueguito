using TMPro;          // Permite usar textos de TextMeshPro.
using UnityEngine;
using UnityEngine.UI; // Permite usar Image (la barra de progreso y los corazones).

// HUD de la partida: lo que se ve "encima" del juego.
//   - Barra de madera arriba (como en los juegos de Papa's):
//       a la izquierda, una taza con los pedidos "3 / 12" y una barra que se va llenando;
//       a la derecha, la plata de las propinas y un corazón por cada cliente que podemos perder.
//       En el centro va la guía del tutorial (script TutorialGuide).
//   - Abajo: un cartel con mensajes para el jugador que se borra solo.
//
// Va en el prefab "HUD", que tiene que estar UNA vez en la escena Cafeteria.
// Los demás scripts lo buscan con FindFirstObjectByType.
public class HUD : MonoBehaviour
{
    // Texto con la cantidad de pedidos completados ("3 / 12").
    [SerializeField] private TMP_Text ordersText;

    // Texto con los clientes perdidos. Ahora usamos corazones,
    // así que puede quedar vacío (sin asignar) en el Inspector.
    [SerializeField] private TMP_Text lostText;

    // Cartel de mensajes (el fondo). Lo ocultamos cuando no hay mensaje.
    [SerializeField] private GameObject messageBox;

    // Texto que va adentro del cartel.
    [SerializeField] private TMP_Text messageText;

    // Segundos que se muestra un mensaje si no se indica otra cosa.
    [SerializeField] private float messageDuration = 2.5f;

    // Relleno de la barra de progreso de pedidos.
    // Es una Image de tipo "Filled": con fillAmount (de 0 a 1)
    // elegimos qué parte de la imagen se ve.
    [SerializeField] private Image progressFill;

    // Un corazón por cada cliente que podemos perder.
    [SerializeField] private Image[] hearts;

    // Dibujos del corazón lleno y del corazón vacío (cliente perdido).
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    // Texto con la plata juntada en propinas ("$ 35").
    [SerializeField] private TMP_Text tipsText;

    // Segundos que le quedan al mensaje actual.
    private float messageTimer = 0f;

    // Mensaje que espera a que termine el actual ("" = no hay ninguno esperando).
    private string queuedMessage = "";
    private float queuedSeconds = 0f;

    private void Awake()
    {
        // Al comenzar no hay ningún mensaje.
        messageBox.SetActive(false);
    }

    // Update funciona como temporizador del mensaje.
    private void Update()
    {
        // Si no hay mensaje visible, no hay nada que contar.
        if (!messageBox.activeSelf)
        {
            return;
        }

        messageTimer -= Time.deltaTime;

        // Cuando se termina el tiempo, ocultamos el cartel...
        if (messageTimer <= 0f)
        {
            messageBox.SetActive(false);

            // ...salvo que haya otro mensaje esperando: en ese caso lo mostramos.
            if (queuedMessage != "")
            {
                string next = queuedMessage;
                queuedMessage = "";
                ShowMessage(next, queuedSeconds);
            }
        }
    }

    // Muestra un mensaje en el cartel.
    // segundos es opcional: si no lo pasamos, usamos messageDuration.
    // (Usamos -1 como valor "especial" que significa "el de siempre".)
    // esperar también es opcional: si es true y ya hay un mensaje en pantalla,
    // este espera a que el otro termine (no lo pisa).
    public void ShowMessage(string message, float seconds = -1f, bool wait = false)
    {
        if (wait && messageBox.activeSelf)
        {
            queuedMessage = message;
            queuedSeconds = seconds;
            return;
        }

        messageText.text = message;

        if (seconds < 0f)
        {
            messageTimer = messageDuration;
        }
        else
        {
            messageTimer = seconds;
        }

        messageBox.SetActive(true);
    }

    // Muestra la plata de las propinas.
    public void UpdateTips(int total)
    {
        if (tipsText != null)
        {
            tipsText.text = "$ " + total;
        }
    }

    // Actualiza los contadores de arriba.
    // La llama el GameManager cada vez que cambia algún número.
    public void UpdateCounters(
        int completedOrders,
        int ordersToWin,
        int lostCustomers,
        int maxLostCustomers)
    {
        ordersText.text = completedOrders + " / " + ordersToWin;

        // La barra se llena según cuántos pedidos llevamos.
        // Ponemos (float) para que la división dé con decimales:
        // entre dos int, 3 / 12 daría 0 en lugar de 0.25.
        if (progressFill != null)
        {
            progressFill.fillAmount = (float)completedOrders / ordersToWin;
        }

        // Si todavía existe el texto viejo, también lo actualizamos.
        if (lostText != null)
        {
            lostText.text = "Clientes perdidos: " + lostCustomers + " / " + maxLostCustomers;
        }

        // Recorremos los corazones: los primeros quedan llenos y los últimos
        // se vacían, uno por cada cliente perdido.
        // Ejemplo con 3 corazones y 1 cliente perdido: lleno, lleno, vacío.
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < maxLostCustomers - lostCustomers)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}
