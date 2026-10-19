using TMPro;          // Permite usar textos de TextMeshPro.
using UnityEngine;
using UnityEngine.UI; // Permite usar Image (la barra de progreso y los corazones).

// HUD de la partida: lo que se ve "encima" del juego.
//   - Barra de madera arriba (como en los juegos de Papa's):
//       a la izquierda, una taza con los pedidos "3 / 12" y una barra que se va llenando;
//       a la derecha, la plata de las propinas y un corazón por cada cliente que podemos perder.
//       En el centro va la guía del tutorial (script GuiaTutorial).
//   - Abajo: un cartel con mensajes para el jugador que se borra solo.
//
// Va en el prefab "HUD", que tiene que estar UNA vez en la escena Cafeteria.
// Los demás scripts lo buscan con FindFirstObjectByType.
public class HUD : MonoBehaviour
{
    // Texto con la cantidad de pedidos completados ("3 / 12").
    [SerializeField] private TMP_Text textoPedidos;

    // Texto con los clientes perdidos. Ahora usamos corazones,
    // así que puede quedar vacío (sin asignar) en el Inspector.
    [SerializeField] private TMP_Text textoPerdidos;

    // Cartel de mensajes (el fondo). Lo ocultamos cuando no hay mensaje.
    [SerializeField] private GameObject cartelMensaje;

    // Texto que va adentro del cartel.
    [SerializeField] private TMP_Text textoMensaje;

    // Segundos que se muestra un mensaje si no se indica otra cosa.
    [SerializeField] private float duracionMensaje = 2.5f;

    // Relleno de la barra de progreso de pedidos.
    // Es una Image de tipo "Filled": con fillAmount (de 0 a 1)
    // elegimos qué parte de la imagen se ve.
    [SerializeField] private Image rellenoProgreso;

    // Un corazón por cada cliente que podemos perder.
    [SerializeField] private Image[] corazones;

    // Dibujos del corazón lleno y del corazón vacío (cliente perdido).
    [SerializeField] private Sprite corazonLleno;
    [SerializeField] private Sprite corazonVacio;

    // Texto con la plata juntada en propinas ("$ 35").
    [SerializeField] private TMP_Text textoPropinas;

    // Segundos que le quedan al mensaje actual.
    private float tiempoMensaje = 0f;

    // Mensaje que espera a que termine el actual ("" = no hay ninguno esperando).
    private string mensajeEnEspera = "";
    private float segundosEnEspera = 0f;

    private void Awake()
    {
        // Al comenzar no hay ningún mensaje.
        cartelMensaje.SetActive(false);
    }

    // Update funciona como temporizador del mensaje.
    private void Update()
    {
        // Si no hay mensaje visible, no hay nada que contar.
        if (!cartelMensaje.activeSelf)
        {
            return;
        }

        tiempoMensaje -= Time.deltaTime;

        // Cuando se termina el tiempo, ocultamos el cartel...
        if (tiempoMensaje <= 0f)
        {
            cartelMensaje.SetActive(false);

            // ...salvo que haya otro mensaje esperando: en ese caso lo mostramos.
            if (mensajeEnEspera != "")
            {
                string siguiente = mensajeEnEspera;
                mensajeEnEspera = "";
                MostrarMensaje(siguiente, segundosEnEspera);
            }
        }
    }

    // Muestra un mensaje en el cartel.
    // segundos es opcional: si no lo pasamos, usamos duracionMensaje.
    // (Usamos -1 como valor "especial" que significa "el de siempre".)
    // esperar también es opcional: si es true y ya hay un mensaje en pantalla,
    // este espera a que el otro termine (no lo pisa).
    public void MostrarMensaje(string mensaje, float segundos = -1f, bool esperar = false)
    {
        if (esperar && cartelMensaje.activeSelf)
        {
            mensajeEnEspera = mensaje;
            segundosEnEspera = segundos;
            return;
        }

        textoMensaje.text = mensaje;

        if (segundos < 0f)
        {
            tiempoMensaje = duracionMensaje;
        }
        else
        {
            tiempoMensaje = segundos;
        }

        cartelMensaje.SetActive(true);
    }

    // Muestra la plata de las propinas.
    public void ActualizarPropinas(int total)
    {
        if (textoPropinas != null)
        {
            textoPropinas.text = "$ " + total;
        }
    }

    // Actualiza los contadores de arriba.
    // La llama el GameManager cada vez que cambia algún número.
    public void ActualizarContadores(
        int pedidosCompletados,
        int pedidosParaGanar,
        int clientesPerdidos,
        int maximoClientesPerdidos)
    {
        textoPedidos.text = pedidosCompletados + " / " + pedidosParaGanar;

        // La barra se llena según cuántos pedidos llevamos.
        // Ponemos (float) para que la división dé con decimales:
        // entre dos int, 3 / 12 daría 0 en lugar de 0.25.
        if (rellenoProgreso != null)
        {
            rellenoProgreso.fillAmount = (float)pedidosCompletados / pedidosParaGanar;
        }

        // Si todavía existe el texto viejo, también lo actualizamos.
        if (textoPerdidos != null)
        {
            textoPerdidos.text = "Clientes perdidos: " + clientesPerdidos + " / " + maximoClientesPerdidos;
        }

        // Recorremos los corazones: los primeros quedan llenos y los últimos
        // se vacían, uno por cada cliente perdido.
        // Ejemplo con 3 corazones y 1 cliente perdido: lleno, lleno, vacío.
        for (int i = 0; i < corazones.Length; i++)
        {
            if (i < maximoClientesPerdidos - clientesPerdidos)
            {
                corazones[i].sprite = corazonLleno;
            }
            else
            {
                corazones[i].sprite = corazonVacio;
            }
        }
    }
}
