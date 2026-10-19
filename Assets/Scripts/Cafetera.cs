using UnityEngine;

// La cafetera tiene dos usos:
//   - Café molido  -> café preparado.
//   - Leche        -> leche espumada.
// Hereda de EstacionInteractiva, así que se usa con la tecla E.
public class Cafetera : EstacionInteractiva
{
    // Segundos que tarda en preparar el café.
    [SerializeField] private float tiempoCafe = 4f;

    // Segundos que tarda en espumar la leche.
    [SerializeField] private float tiempoLeche = 3f;

    // Dónde aparece el globo respecto de la cafetera.
    [SerializeField] private Vector3 posicionGlobo = new Vector3(0f, 0.85f, 0f);

    // Indica si la cafetera está trabajando.
    private bool preparando = false;

    // Segundos que faltan para terminar.
    private float tiempoRestante = 0f;

    // Producto que va a salir cuando termine de preparar.
    private PlayerInventory.TipoObjeto productoEnPreparacion =
        PlayerInventory.TipoObjeto.Ninguno;

    // Producto terminado que espera ser retirado.
    // Ninguno significa que la cafetera está vacía.
    private PlayerInventory.TipoObjeto productoListo =
        PlayerInventory.TipoObjeto.Ninguno;

    // Globo que muestra lo que hay adentro de la cafetera.
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
            Avisar("La cafetera está ocupada.");
            return;
        }

        // Si hay algo listo, intentamos retirarlo.
        if (productoListo != PlayerInventory.TipoObjeto.Ninguno)
        {
            // Solo la vaciamos si el barista pudo agarrarlo.
            if (inventario.RecogerObjeto(productoListo))
            {
                productoListo = PlayerInventory.TipoObjeto.Ninguno;
                OcultarGlobo();
            }

            return;
        }

        // Si está vacía, revisamos qué ingrediente trae el barista.
        // Usamos switch porque comparamos una misma variable
        // contra varios valores posibles.
        switch (inventario.ObtenerObjetoActual())
        {
            case PlayerInventory.TipoObjeto.CafeMolido:
                inventario.SoltarObjeto();
                EmpezarPreparacion(
                    PlayerInventory.TipoObjeto.CafeMolido,
                    PlayerInventory.TipoObjeto.CafePreparado,
                    tiempoCafe);
                break;

            case PlayerInventory.TipoObjeto.Leche:
                inventario.SoltarObjeto();
                EmpezarPreparacion(
                    PlayerInventory.TipoObjeto.Leche,
                    PlayerInventory.TipoObjeto.LecheEspumada,
                    tiempoLeche);
                break;

            default:
                Avisar("La cafetera necesita café molido o leche.");
                break;
        }
    }

    // Pone en marcha la cafetera.
    // ingrediente: lo que dejó el barista (se muestra mientras trabaja).
    // resultado: lo que va a salir al terminar.
    // duracion: cuántos segundos va a tardar.
    private void EmpezarPreparacion(
        PlayerInventory.TipoObjeto ingrediente,
        PlayerInventory.TipoObjeto resultado,
        float duracion)
    {
        productoEnPreparacion = resultado;
        tiempoRestante = duracion;
        preparando = true;

        // Mostramos el ingrediente medio transparente: se está preparando.
        MostrarEnGlobo(ingrediente, false);

        Debug.Log("Preparando: " + resultado);
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

        // Cuando el tiempo llega a cero, el producto queda listo para retirar.
        if (tiempoRestante <= 0f)
        {
            preparando = false;
            productoListo = productoEnPreparacion;
            productoEnPreparacion = PlayerInventory.TipoObjeto.Ninguno;

            // Ahora mostramos el resultado, bien visible.
            MostrarEnGlobo(productoListo, true);

            Debug.Log("¡Listo para retirar: " + productoListo + "!");

            // Sonido de "¡listo!".
            Sonidos s = ObtenerSonidos();

            if (s != null)
            {
                s.MaquinaLista();
            }
        }
    }

    // Muestra un objeto en el globo de la cafetera (si el globo existe).
    private void MostrarEnGlobo(PlayerInventory.TipoObjeto objeto, bool listo)
    {
        if (globo != null)
        {
            globo.Mostrar(objeto, listo);
        }
    }

    // Oculta el globo de la cafetera (si el globo existe).
    private void OcultarGlobo()
    {
        if (globo != null)
        {
            globo.Ocultar();
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================
    // Funciones que solo devuelven información: otros scripts pueden
    // preguntar cómo está la cafetera sin poder cambiarla.

    // Devuelve lo que está preparando ahora (Ninguno si no está trabajando).
    public PlayerInventory.TipoObjeto ObtenerProductoEnPreparacion()
    {
        return productoEnPreparacion;
    }

    // Devuelve lo que está listo para retirar (Ninguno si no hay nada).
    public PlayerInventory.TipoObjeto ObtenerProductoListo()
    {
        return productoListo;
    }
}
