using UnityEngine;

// La moledora convierte los granos de café en café molido.
// Hereda de EstacionInteractiva, así que se usa con la tecla E.
public class MoledoraCafe : EstacionInteractiva
{
    // Segundos que tarda en moler el café.
    [SerializeField] private float tiempoPreparacion = 3f;

    // Dónde aparece el globo respecto de la moledora.
    [SerializeField] private Vector3 posicionGlobo = new Vector3(0f, 0.85f, 0f);

    // Indica si la moledora está trabajando.
    private bool preparando = false;

    // Segundos que faltan para terminar de moler.
    private float tiempoRestante = 0f;

    // Indica si hay café molido listo para retirar.
    private bool cafeListo = false;

    // Globo que muestra lo que hay adentro de la moledora.
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
            Avisar("La moledora está trabajando...");
            return;
        }

        // Si ya terminó, intentamos retirar el café molido.
        if (cafeListo)
        {
            // Solo vaciamos la moledora si el barista pudo agarrarlo
            // (por ejemplo, no puede si tiene las manos ocupadas).
            if (inventario.RecogerObjeto(PlayerInventory.TipoObjeto.CafeMolido))
            {
                cafeListo = false;
                OcultarGlobo();

                Debug.Log("Retiraste el café molido.");
            }

            return;
        }

        // Si está vacía, comprobamos que el barista traiga granos de café.
        if (inventario.ObtenerObjetoActual() == PlayerInventory.TipoObjeto.Cafe)
        {
            // Los granos pasan de la mano a la moledora.
            inventario.SoltarObjeto();

            // Empezamos a contar el tiempo de molienda.
            preparando = true;
            tiempoRestante = tiempoPreparacion;

            // Mostramos los granos medio transparentes: se están moliendo.
            MostrarEnGlobo(PlayerInventory.TipoObjeto.Cafe, false);

            Debug.Log("Moliendo café...");
        }
        else
        {
            Avisar("La moledora necesita granos de café.");
        }
    }

    // Update se ejecuta una vez por frame.
    // Lo usamos como un temporizador: le restamos el tiempo
    // que pasó desde el frame anterior hasta llegar a cero.
    private void Update()
    {
        // Si no está moliendo, no hay nada que contar.
        if (!preparando)
        {
            return;
        }

        // Time.deltaTime es el tiempo que pasó desde el último frame.
        tiempoRestante -= Time.deltaTime;

        // Cuando el tiempo llega a cero, el café está listo.
        if (tiempoRestante <= 0f)
        {
            preparando = false;
            cafeListo = true;

            // Ahora mostramos el café molido, bien visible.
            MostrarEnGlobo(PlayerInventory.TipoObjeto.CafeMolido, true);

            Debug.Log("¡Café molido listo!");

            // Sonido de "¡listo!".
            Sonidos s = ObtenerSonidos();

            if (s != null)
            {
                s.MaquinaLista();
            }
        }
    }

    // Muestra un objeto en el globo de la moledora (si el globo existe).
    private void MostrarEnGlobo(PlayerInventory.TipoObjeto objeto, bool listo)
    {
        if (globo != null)
        {
            globo.Mostrar(objeto, listo);
        }
    }

    // Oculta el globo de la moledora (si el globo existe).
    private void OcultarGlobo()
    {
        if (globo != null)
        {
            globo.Ocultar();
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================
    // Funciones que solo devuelven información: otros scripts pueden
    // preguntar cómo está la moledora sin poder cambiarla.

    // Devuelve true mientras la moledora está moliendo.
    public bool EstaMoliendo()
    {
        return preparando;
    }

    // Devuelve true si hay café molido listo para retirar.
    public bool TieneCafeListo()
    {
        return cafeListo;
    }
}
