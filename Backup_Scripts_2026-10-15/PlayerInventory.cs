
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public enum TipoObjeto
    {
        Ninguno,
        Cafe,
        Leche,
        Chocolate,
        CafeMolido,
        CafePreparado,
        LecheEspumada,
        ChocolatePreparado,
        CafeConLeche,
        ChocolateConLeche,
        GalletitaRedVelvet,
        GalletitaVainillaChocolate
    }

    [SerializeField] private TipoObjeto objetoActual = TipoObjeto.Ninguno;

    public TipoObjeto ObjetoActual
    {
        get { return objetoActual; }
    }

    public bool TieneObjeto()
    {
        return objetoActual != TipoObjeto.Ninguno;
    }

    public bool RecogerObjeto(TipoObjeto nuevoObjeto)
    {
        if (TieneObjeto())
        {
            Debug.Log("Ya tenés un objeto en la mano.");
            return false;
        }

        objetoActual = nuevoObjeto;
        Debug.Log("Recogiste: " + objetoActual);
        return true;
    }

    public void SoltarObjeto()
    {
        objetoActual = TipoObjeto.Ninguno;
    }
}
