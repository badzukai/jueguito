using UnityEngine;

// Guarda el dibujo (Sprite) de cada objeto del juego y crea los globos.
//
// Es el ÚNICO lugar donde se eligen los dibujos de los objetos.
// Cuando tengamos el pixel art definitivo, alcanza con:
//   - reemplazar los archivos PNG de Assets/Sprites/Objetos
//     por otros con el mismo nombre, o
//   - arrastrar los nuevos sprites a estos campos en el Inspector.
//
// Va en el prefab "CatalogoObjetos", que tiene que estar UNA vez en la escena.
public class CatalogoObjetos : MonoBehaviour
{
    // Prefab del globo que aparece encima de las cosas.
    [SerializeField] private GloboObjeto prefabGlobo;

    // Prefab del globo con el pedido de cada cliente.
    [SerializeField] private GloboPedido prefabGloboPedido;

    // ----- Ingredientes -----
    [SerializeField] private Sprite spriteCafe;
    [SerializeField] private Sprite spriteLeche;
    [SerializeField] private Sprite spriteChocolate;

    // ----- Productos intermedios -----
    [SerializeField] private Sprite spriteCafeMolido;
    [SerializeField] private Sprite spriteLecheEspumada;

    // ----- Bebidas terminadas -----
    [SerializeField] private Sprite spriteCafePreparado;
    [SerializeField] private Sprite spriteChocolatePreparado;
    [SerializeField] private Sprite spriteCafeConLeche;
    [SerializeField] private Sprite spriteChocolateConLeche;

    // ----- Galletitas -----
    [SerializeField] private Sprite spriteGalletitaRedVelvet;
    [SerializeField] private Sprite spriteGalletitaVainillaChocolate;
    [SerializeField] private Sprite spriteGalletitaChocolate;

    // Devuelve el dibujo que corresponde a cada objeto.
    // Usamos switch porque comparamos una misma variable contra muchos valores.
    public Sprite ObtenerSprite(PlayerInventory.TipoObjeto objeto)
    {
        switch (objeto)
        {
            case PlayerInventory.TipoObjeto.Cafe:
                return spriteCafe;

            case PlayerInventory.TipoObjeto.Leche:
                return spriteLeche;

            case PlayerInventory.TipoObjeto.Chocolate:
                return spriteChocolate;

            case PlayerInventory.TipoObjeto.CafeMolido:
                return spriteCafeMolido;

            case PlayerInventory.TipoObjeto.LecheEspumada:
                return spriteLecheEspumada;

            case PlayerInventory.TipoObjeto.CafePreparado:
                return spriteCafePreparado;

            case PlayerInventory.TipoObjeto.ChocolatePreparado:
                return spriteChocolatePreparado;

            case PlayerInventory.TipoObjeto.CafeConLeche:
                return spriteCafeConLeche;

            case PlayerInventory.TipoObjeto.ChocolateConLeche:
                return spriteChocolateConLeche;

            case PlayerInventory.TipoObjeto.GalletitaRedVelvet:
                return spriteGalletitaRedVelvet;

            case PlayerInventory.TipoObjeto.GalletitaVainillaChocolate:
                return spriteGalletitaVainillaChocolate;

            case PlayerInventory.TipoObjeto.GalletitaChocolate:
                return spriteGalletitaChocolate;

            default:
                return null;
        }
    }

    // Crea un globo nuevo que sigue a "objetivo".
    // desplazamiento: dónde va el globo respecto del objetivo
    //                 (por ejemplo, (0, 1, 0) es una unidad más arriba).
    // conFondo: true para el globito de diálogo, false para mostrar solo el objeto.
    // Devuelve el globo creado, para que quien lo pidió lo pueda usar.
    public GloboObjeto CrearGlobo(Transform objetivo, Vector3 desplazamiento, bool conFondo)
    {
        // Instantiate crea una copia del prefab en la escena.
        // Quaternion.identity significa "sin rotación".
        GloboObjeto globo = Instantiate(
            prefabGlobo,
            objetivo.position + desplazamiento,
            Quaternion.identity
        );

        globo.Configurar(this, objetivo, desplazamiento, conFondo);

        return globo;
    }

    // Crea el globo de pedido de un cliente. Funciona igual que CrearGlobo.
    public GloboPedido CrearGloboPedido(Transform objetivo, Vector3 desplazamiento)
    {
        GloboPedido globo = Instantiate(
            prefabGloboPedido,
            objetivo.position + desplazamiento,
            Quaternion.identity
        );

        globo.Configurar(this, objetivo, desplazamiento);

        return globo;
    }
}
