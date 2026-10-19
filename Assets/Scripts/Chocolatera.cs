using UnityEngine;

// La chocolatera convierte el chocolate en chocolate preparado.
// Hereda de EstacionInteractiva, así que se usa con la tecla E.
public class Chocolatera : EstacionInteractiva
{
    // Segundos que tarda en preparar el chocolate.
    [SerializeField] private float tiempoPreparacion = 4f;

    // Dónde aparece el globo respecto de la chocolatera.
    [SerializeField] private Vector3 posicionGlobo = new Vector3(0f, 0.85f, 0f);

    // Indica si la chocolatera está trabajando.
    private bool preparando = false;

    // Segundos que faltan para terminar.
    private float tiempoRestante = 0f;

    // Indica si hay chocolate listo para retirar.
    private bool chocolateListo = false;

    // Globo que muestra lo que hay adentro de la chocolatera.
    private GloboObjeto globo;

    private void Start()
    {
        // Buscamos el catálogo de dibujos una sola vez y creamos el globo.
        CatalogoObjetos catalogo = FindFirstObjectByType<CatalogoObjetos>();

        if (catalogo != null)
        {
            globo = catalogo.CrearGlobo(transform, posicionGlobo, true);
        }
    }

    // Se ejecuta cuando el barista se acerca y presiona E.
    public override void Interactuar(PlayerInventory inventario)
    {
        // Si está trabajando, no podemos usarla.
        if (preparando)
        {
            Avisar("La chocolatera está ocupada.");
            return;
        }

        // Si terminó, intentamos retirar el chocolate.
        if (chocolateListo)
        {
            // Solo la vaciamos si el barista pudo agarrarlo.
            if (inventario.RecogerObjeto(PlayerInventory.TipoObjeto.ChocolatePreparado))
            {
                chocolateListo = false;
                OcultarGlobo();
            }

            return;
        }

        // Si está vacía, comprobamos el ingrediente.
        if (inventario.ObtenerObjetoActual() == PlayerInventory.TipoObjeto.Chocolate)
        {
            // El chocolate pasa de la mano a la máquina.
            inventario.SoltarObjeto();

            // Empezamos a contar el tiempo de preparación.
            preparando = true;
            tiempoRestante = tiempoPreparacion;

            // Mostramos el chocolate medio transparente: se está preparando.
            MostrarEnGlobo(PlayerInventory.TipoObjeto.Chocolate, false);

            Debug.Log("Preparando chocolate...");
        }
        else
        {
            Avisar("La chocolatera necesita chocolate.");
        }
    }

    // Update funciona como temporizador: resta el tiempo de cada frame.
    private void Update()
    {
        // Si no está preparando nada, no hay nada que contar.
        if (!preparando)
        {
            return;
        }

        tiempoRestante -= Time.deltaTime;

        // Cuando el tiempo llega a cero, el chocolate está listo.
        if (tiempoRestante <= 0f)
        {
            preparando = false;
            chocolateListo = true;

            // Ahora mostramos el chocolate preparado, bien visible.
            MostrarEnGlobo(PlayerInventory.TipoObjeto.ChocolatePreparado, true);

            Debug.Log("¡Chocolate preparado!");

            // Sonido de "¡listo!".
            Sonidos s = ObtenerSonidos();

            if (s != null)
            {
                s.MaquinaLista();
            }
        }
    }

    // Muestra un objeto en el globo de la chocolatera (si el globo existe).
    private void MostrarEnGlobo(PlayerInventory.TipoObjeto objeto, bool listo)
    {
        if (globo != null)
        {
            globo.Mostrar(objeto, listo);
        }
    }

    // Oculta el globo de la chocolatera (si el globo existe).
    private void OcultarGlobo()
    {
        if (globo != null)
        {
            globo.Ocultar();
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================
    // Funciones que solo devuelven información: otros scripts pueden
    // preguntar cómo está la chocolatera sin poder cambiarla.

    // Devuelve true mientras la chocolatera está preparando.
    public bool EstaPreparando()
    {
        return preparando;
    }

    // Devuelve true si hay un chocolate listo para retirar.
    public bool TieneChocolateListo()
    {
        return chocolateListo;
    }
}
