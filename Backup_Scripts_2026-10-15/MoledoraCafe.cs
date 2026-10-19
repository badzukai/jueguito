
using System.Collections;
using UnityEngine;

public class MoledoraCafe : EstacionInteractiva
{
    [SerializeField] private float tiempoPreparacion = 3f;

    private bool preparando = false;
    private bool cafeListo = false;

    public override void Interactuar(PlayerInventory inventario)
    {
        if (preparando)
        {
            Debug.Log("La moledora está trabajando...");
            return;
        }

        if (cafeListo)
        {
            if (inventario.RecogerObjeto(
                PlayerInventory.TipoObjeto.CafeMolido))
            {
                cafeListo = false;
                Debug.Log("Retiraste el café molido.");
            }

            return;
        }

        if (inventario.ObjetoActual == PlayerInventory.TipoObjeto.Cafe)
        {
            inventario.SoltarObjeto();
            StartCoroutine(MolerCafe());
        }
        else
        {
            Debug.Log("La moledora necesita granos de café.");
        }
    }

    private IEnumerator MolerCafe()
    {
        preparando = true;
        Debug.Log("Molinedo café...");

        yield return new WaitForSeconds(tiempoPreparacion);

        preparando = false;
        cafeListo = true;

        Debug.Log("¡Café molido listo!");
    }
}
