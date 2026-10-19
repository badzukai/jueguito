using UnityEngine;

// Muestra el dibujo de un objeto en un globito, encima de algo:
// el barista, una máquina o un espacio de la repisa.
//
// Este script va en el prefab "Globo". No lo ponemos a mano en la escena:
// lo crea CatalogoObjetos con Instantiate cuando empieza el juego.
public class GloboObjeto : MonoBehaviour
{
    // Imagen de fondo del globo (la nube blanca).
    [SerializeField] private SpriteRenderer fondo;

    // Imagen del objeto que se muestra dentro del globo.
    [SerializeField] private SpriteRenderer icono;

    // Transparencia del ícono cuando la máquina todavía está trabajando.
    // 1 = se ve completo, 0 = invisible.
    [SerializeField] private float transparenciaEnProceso = 0.5f;

    // Objeto al que sigue el globo (el barista, una máquina, una repisa).
    private Transform objetivo;

    // Distancia entre el objetivo y el globo.
    private Vector3 desplazamiento;

    // De dónde sacamos los dibujos de cada objeto.
    private CatalogoObjetos catalogo;

    // Prepara el globo. La llama CatalogoObjetos justo después de crearlo.
    // conFondo: true para el globito de diálogo, false para mostrar
    // solo el objeto (por ejemplo, apoyado en la repisa).
    public void Configurar(
        CatalogoObjetos catalogoObjetos,
        Transform objetivoASeguir,
        Vector3 distancia,
        bool conFondo)
    {
        catalogo = catalogoObjetos;
        objetivo = objetivoASeguir;
        desplazamiento = distancia;

        fondo.gameObject.SetActive(conFondo);

        // Al comenzar no muestra nada.
        Ocultar();
    }

    // LateUpdate se ejecuta al final de cada frame, después de que
    // todo se movió. Así el globo acompaña al barista sin quedarse atrás.
    private void LateUpdate()
    {
        SeguirObjetivo();
    }

    // Ubica el globo encima de su objetivo.
    private void SeguirObjetivo()
    {
        if (objetivo != null)
        {
            transform.position = objetivo.position + desplazamiento;
        }
    }

    // Muestra un objeto en el globo.
    // listo es un parámetro opcional: si no lo pasamos, vale true.
    // Con listo = false el ícono se ve medio transparente
    // (lo usamos mientras una máquina está trabajando).
    public void Mostrar(PlayerInventory.TipoObjeto objeto, bool listo = true)
    {
        // Si no hay objeto, no hay nada que mostrar.
        if (objeto == PlayerInventory.TipoObjeto.Ninguno)
        {
            Ocultar();
            return;
        }

        // Buscamos el dibujo de este objeto en el catálogo.
        icono.sprite = catalogo.ObtenerSprite(objeto);

        // Elegimos la transparencia según si está listo o no.
        // El cuarto valor de un Color es la transparencia (alfa).
        Color color = Color.white;

        if (!listo)
        {
            color.a = transparenciaEnProceso;
        }

        icono.color = color;

        // Lo ubicamos y lo hacemos visible.
        SeguirObjetivo();
        gameObject.SetActive(true);
    }

    // Oculta el globo.
    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}
