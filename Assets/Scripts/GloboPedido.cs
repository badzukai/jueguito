using UnityEngine;

// Globo que aparece arriba de un cliente con lo que pidió:
// la bebida y, si pidió, la galletita.
//   - Solo bebida      -> globo chico con un dibujo.
//   - Bebida + galletita -> globo ancho con los dos dibujos.
//
// Este script va en el prefab "GloboPedido". No lo ponemos a mano en la escena:
// lo crea CatalogoObjetos con Instantiate cuando empieza el juego.
public class GloboPedido : MonoBehaviour
{
    // Imagen de fondo del globo.
    [SerializeField] private SpriteRenderer fondo;

    // Dibujo de la bebida pedida.
    [SerializeField] private SpriteRenderer iconoBebida;

    // Dibujo de la galletita pedida.
    [SerializeField] private SpriteRenderer iconoGalletita;

    // Fondo para pedidos de un solo producto.
    [SerializeField] private Sprite fondoSimple;

    // Fondo más ancho para pedidos de dos productos.
    [SerializeField] private Sprite fondoDoble;

    // Dónde va la bebida cuando está sola (centrada).
    [SerializeField] private Vector3 posicionBebidaSola = new Vector3(0f, 0.0625f, 0f);

    // Dónde van los dos productos cuando hay galletita (uno a cada lado).
    [SerializeField] private Vector3 posicionBebidaConGalletita = new Vector3(-0.28f, 0.0625f, 0f);
    [SerializeField] private Vector3 posicionGalletita = new Vector3(0.28f, 0.0625f, 0f);

    // Objeto al que sigue el globo (el cliente).
    private Transform objetivo;

    // Distancia entre el cliente y el globo.
    private Vector3 desplazamiento;

    // De dónde sacamos los dibujos de cada objeto.
    private CatalogoObjetos catalogo;

    // Prepara el globo. La llama CatalogoObjetos justo después de crearlo.
    public void Configurar(
        CatalogoObjetos catalogoObjetos,
        Transform objetivoASeguir,
        Vector3 distancia)
    {
        catalogo = catalogoObjetos;
        objetivo = objetivoASeguir;
        desplazamiento = distancia;

        // Al comenzar no muestra nada.
        Ocultar();
    }

    // LateUpdate se ejecuta al final de cada frame, después de que
    // todo se movió. Así el globo acompaña al cliente.
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

    // Muestra el pedido en el globo.
    public void Mostrar(
        PlayerInventory.TipoObjeto bebida,
        PlayerInventory.TipoObjeto galletita)
    {
        // La bebida siempre se muestra.
        iconoBebida.sprite = catalogo.ObtenerSprite(bebida);

        if (galletita == PlayerInventory.TipoObjeto.Ninguno)
        {
            // Pedido de un solo producto: globo chico y bebida centrada.
            fondo.sprite = fondoSimple;
            iconoBebida.transform.localPosition = posicionBebidaSola;
            iconoGalletita.gameObject.SetActive(false);
        }
        else
        {
            // Pedido doble: globo ancho, bebida a la izquierda
            // y galletita a la derecha.
            fondo.sprite = fondoDoble;
            iconoBebida.transform.localPosition = posicionBebidaConGalletita;

            iconoGalletita.sprite = catalogo.ObtenerSprite(galletita);
            iconoGalletita.transform.localPosition = posicionGalletita;
            iconoGalletita.gameObject.SetActive(true);
        }

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
