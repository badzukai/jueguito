using UnityEngine;

// Clase "base" de todas las estaciones que se usan con la tecla E:
// la bandeja, la cafetera, la chocolatera, la moledora, las repisas y el tacho.
//
// HERENCIA: cuando un script dice "public class Cafetera : EstacionInteractiva",
// significa que la Cafetera ES una EstacionInteractiva y recibe todo lo que
// tiene esta clase. Así, PlayerInteraction puede buscar "todas las estaciones"
// sin importar si son cafeteras, bandejas o repisas.
//
// Es la misma idea que ya usamos con MonoBehaviour: todos nuestros scripts
// "heredan" de MonoBehaviour, y por eso pueden tener Start(), Update(), etc.
public class EstacionInteractiva : MonoBehaviour
{
    // Referencia al HUD, donde mostramos los mensajes para el jugador.
    // La buscamos recién la primera vez que la necesitamos.
    private HUD hud;

    // Referencia a los sonidos del juego (también la buscamos la primera vez).
    private Sonidos sonidos;

    // "virtual" significa que cada estación puede reemplazar esta función
    // con su propia versión. Para hacerlo, la estación escribe la misma
    // función con la palabra "override".
    //
    // Si una estación NO la reemplaza, se ejecuta esta versión general,
    // que solamente muestra un mensaje en la Console.
    public virtual void Interactuar(PlayerInventory inventario)
    {
        Debug.Log("Interactuaste con: " + gameObject.name);
    }

    // Muestra un mensaje al jugador en el cartel del HUD
    // (y también en la Console, para nosotros).
    //
    // "protected" es parecido a "private", pero además deja que las
    // clases que heredan de esta (Cafetera, Repisa, etc.) la usen.
    // Así escribimos esta función una sola vez y la comparten todas.
    //
    // esError es un parámetro opcional: si no lo pasamos vale true y suena
    // el "error". Para avisos buenos (por ejemplo, el tacho) pasamos false.
    protected void Avisar(string mensaje, bool esError = true)
    {
        Debug.Log(mensaje);

        if (esError)
        {
            Sonidos s = ObtenerSonidos();

            if (s != null)
            {
                s.Error();
            }
        }

        // La primera vez buscamos el HUD en la escena.
        if (hud == null)
        {
            hud = FindFirstObjectByType<HUD>();
        }

        if (hud != null)
        {
            hud.MostrarMensaje(mensaje);
        }
    }

    // Devuelve los sonidos del juego, para que cada estación haga sonar el suyo.
    // Lo buscamos solo la primera vez (búsqueda "perezosa").
    // Puede devolver null si no hay sonidos en la escena: hay que comprobarlo.
    protected Sonidos ObtenerSonidos()
    {
        if (sonidos == null)
        {
            sonidos = FindFirstObjectByType<Sonidos>();
        }

        return sonidos;
    }
}
