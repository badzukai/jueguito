using UnityEngine;

// Guarda el objeto que el barista lleva en la mano.
// El barista puede llevar un solo objeto a la vez.
public class PlayerInventory : MonoBehaviour
{
    // Un "enum" es una lista de opciones con nombre.
    // En lugar de usar números o textos sueltos, cada objeto del juego
    // tiene un nombre fijo, y así evitamos errores de tipeo.
    // Por dentro, Unity guarda cada opción como un número
    // (Ninguno = 0, Cafe = 1, Leche = 2, etc.).
    // Por eso NO conviene cambiar el orden de esta lista:
    // los valores guardados en la escena cambiarían de significado.
    public enum TipoObjeto
    {
        Ninguno,                    // Manos vacías.
        Cafe,                       // Granos de café (ingrediente).
        Leche,                      // Leche (ingrediente).
        Chocolate,                  // Chocolate (ingrediente).
        CafeMolido,                 // Sale de la moledora.
        CafePreparado,              // Sale de la cafetera.
        LecheEspumada,              // Sale de la cafetera al poner leche.
        ChocolatePreparado,         // Sale de la chocolatera.
        CafeConLeche,               // Café + leche espumada, combinados en la repisa.
        ChocolateConLeche,          // Chocolate + leche espumada, combinados en la repisa.
        GalletitaRedVelvet,         // Sale de la estación de galletitas (tecla Z).
        GalletitaVainillaChocolate, // Sale de la estación de galletitas (tecla X).
        GalletitaChocolate          // Sale de la estación de galletitas (tecla C).
        // Los objetos nuevos se agregan SIEMPRE al final de la lista.
    }

    // Objeto que el barista tiene en la mano en este momento.
    // Es privado, pero con [SerializeField] lo vemos en el Inspector,
    // lo que sirve para comprobar qué lleva mientras probamos el juego.
    [SerializeField] private TipoObjeto objetoActual = TipoObjeto.Ninguno;

    // Dónde aparece el globo respecto del barista (arriba de su cabeza).
    [SerializeField] private Vector3 posicionGlobo = new Vector3(0f, 0.7f, 0f);

    // Globo que muestra lo que lleva el barista en la mano.
    private GloboObjeto globo;

    // HUD, para mostrar mensajes en pantalla.
    private HUD hud;

    // Sonidos del juego (agarrar y apoyar cosas).
    private Sonidos sonidos;

    private void Start()
    {
        // Buscamos el catálogo de dibujos que hay en la escena.
        // FindFirstObjectByType es la versión nueva de FindObjectOfType:
        // busca el primer objeto con ese componente. Es lento,
        // por eso lo usamos una sola vez, en Start.
        CatalogoObjetos catalogo = FindFirstObjectByType<CatalogoObjetos>();

        if (catalogo != null)
        {
            globo = catalogo.CrearGlobo(transform, posicionGlobo, true);
        }
        else
        {
            Debug.LogWarning("Falta el CatalogoObjetos en la escena: no se verán los globos.");
        }

        // También buscamos el HUD y los sonidos una sola vez.
        hud = FindFirstObjectByType<HUD>();
        sonidos = FindFirstObjectByType<Sonidos>();
    }

    // Muestra en el globo lo que lleva el barista.
    // Si tiene las manos vacías, el globo se oculta solo.
    private void ActualizarGlobo()
    {
        if (globo != null)
        {
            globo.Mostrar(objetoActual);
        }
    }

    // Devuelve el objeto que lleva el barista.
    // Otros scripts pueden consultarlo, pero no cambiarlo directamente:
    // para eso tienen que usar RecogerObjeto() o SoltarObjeto().
    public TipoObjeto ObtenerObjetoActual()
    {
        return objetoActual;
    }

    // Devuelve true si el barista tiene algo en la mano.
    public bool TieneObjeto()
    {
        return objetoActual != TipoObjeto.Ninguno;
    }

    // Intenta poner un objeto en la mano del barista.
    // Devuelve true si pudo recogerlo y false si ya tenía algo.
    public bool RecogerObjeto(TipoObjeto nuevoObjeto)
    {
        // Si ya tiene las manos ocupadas, no puede agarrar otra cosa.
        if (TieneObjeto())
        {
            Debug.Log("Ya tenés un objeto en la mano.");

            if (hud != null)
            {
                hud.MostrarMensaje("Ya tenés algo en la mano. Dejalo en una repisa o tiralo al tacho.");
            }

            if (sonidos != null)
            {
                sonidos.Error();
            }

            return false;
        }

        objetoActual = nuevoObjeto;
        ActualizarGlobo();

        if (sonidos != null)
        {
            sonidos.Agarrar();
        }

        Debug.Log("Recogiste: " + objetoActual);
        return true;
    }

    // Deja las manos del barista vacías.
    public void SoltarObjeto()
    {
        objetoActual = TipoObjeto.Ninguno;
        ActualizarGlobo();

        if (sonidos != null)
        {
            sonidos.Apoyar();
        }
    }
}
