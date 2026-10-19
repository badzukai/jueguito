using UnityEngine;

// Tacho de basura: sirve para tirar lo que el barista lleva en la mano
// (por ejemplo, si agarró un ingrediente equivocado).
// Hereda de EstacionInteractiva, así que se usa con la tecla E.
public class Tacho : EstacionInteractiva
{
    // Se ejecuta cuando el barista se acerca y presiona E.
    public override void Interactuar(PlayerInventory inventario)
    {
        // Si tiene las manos vacías, no hay nada para tirar.
        if (!inventario.TieneObjeto())
        {
            Avisar("No tenés nada para tirar.");
            return;
        }

        // Vaciamos la mano del barista.
        inventario.SoltarObjeto();

        Sonidos s = ObtenerSonidos();

        if (s != null)
        {
            s.Tirar();
        }

        // false: no es un error, así que no suena el "error".
        Avisar("Lo tiraste a la basura.", false);
    }
}
