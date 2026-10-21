using UnityEngine;

// Tacho de basura: sirve para tirar lo que el barista lleva en la mano
// (por ejemplo, si agarró un ingrediente equivocado).
// Hereda de InteractiveStation, así que se usa con la tecla E.
public class TrashCan : InteractiveStation
{
    // Se ejecuta cuando el barista se acerca y presiona E.
    public override void Interact(PlayerInventory inventory)
    {
        // Si tiene las manos vacías, no hay nada para tirar.
        if (!inventory.HasItem())
        {
            Notify("No tenés nada para tirar.");
            return;
        }

        // Vaciamos la mano del barista.
        inventory.DropItem();

        SoundEffects s = GetSoundEffects();

        if (s != null)
        {
            s.Throw();
        }

        // false: no es un error, así que no suena el "error".
        Notify("Lo tiraste a la basura.", false);
    }
}
