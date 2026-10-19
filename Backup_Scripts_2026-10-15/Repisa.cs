
using UnityEngine;

public class Repisa : EstacionInteractiva
{
    [SerializeField]
    private PlayerInventory.TipoObjeto objetoGuardado =
        PlayerInventory.TipoObjeto.Ninguno;

    public override void Interactuar(PlayerInventory inventario)
    {
        // Si la repisa está vacía, podemos guardar un objeto.
        if (objetoGuardado == PlayerInventory.TipoObjeto.Ninguno)
        {
            if (inventario.TieneObjeto())
            {
                objetoGuardado = inventario.ObjetoActual;
                inventario.SoltarObjeto();

                Debug.Log("Guardaste: " + objetoGuardado);
            }

            return;
        }

        // Si el jugador no lleva nada, retira el objeto guardado.
        if (!inventario.TieneObjeto())
        {
            if (inventario.RecogerObjeto(objetoGuardado))
            {
                objetoGuardado = PlayerInventory.TipoObjeto.Ninguno;
            }

            return;
        }

        // Si ambos tienen objetos, intentamos combinarlos.
        CombinarProductos(inventario);
    }

    private void CombinarProductos(PlayerInventory inventario)
    {
        PlayerInventory.TipoObjeto objetoEnMano = inventario.ObjetoActual;

        bool esCafeConLeche =
            (objetoGuardado == PlayerInventory.TipoObjeto.CafePreparado &&
             objetoEnMano == PlayerInventory.TipoObjeto.LecheEspumada)
            ||
            (objetoGuardado == PlayerInventory.TipoObjeto.LecheEspumada &&
             objetoEnMano == PlayerInventory.TipoObjeto.CafePreparado);

        bool esChocolateConLeche =
            (objetoGuardado == PlayerInventory.TipoObjeto.ChocolatePreparado &&
             objetoEnMano == PlayerInventory.TipoObjeto.LecheEspumada)
            ||
            (objetoGuardado == PlayerInventory.TipoObjeto.LecheEspumada &&
             objetoEnMano == PlayerInventory.TipoObjeto.ChocolatePreparado);

        if (esCafeConLeche)
        {
            objetoGuardado = PlayerInventory.TipoObjeto.CafeConLeche;
            inventario.SoltarObjeto();

            Debug.Log("¡Preparaste café con leche!");
        }
        else if (esChocolateConLeche)
        {
            objetoGuardado = PlayerInventory.TipoObjeto.ChocolateConLeche;
            inventario.SoltarObjeto();

            Debug.Log("¡Preparaste chocolate con leche!");
        }
        else
        {
            Debug.Log("Estos productos no se pueden combinar.");
        }
    }
}
