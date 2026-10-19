
using System.Collections;
using UnityEngine;

public class Chocolatera : EstacionInteractiva
{
    [SerializeField] private float tiempoPreparacion = 4f;

    private bool preparando = false;
    private bool chocolateListo = false;

    public override void Interactuar(PlayerInventory inventario)
    {
        // Si está trabajando, no podemos usarla.
        if (preparando)
        {
            Debug.Log("La chocolatera está ocupada.");
            return;
        }

        // Si terminó, intentamos retirar el chocolate.
        if (chocolateListo)
        {
            if (inventario.RecogerObjeto(
                PlayerInventory.TipoObjeto.ChocolatePreparado))
            {
                chocolateListo = false;
            }

            return;
        }

        // Si está vacía, comprobamos el ingrediente.
        if (inventario.ObjetoActual ==
            PlayerInventory.TipoObjeto.Chocolate)
        {
            inventario.SoltarObjeto();
            StartCoroutine(PrepararChocolate());
        }
        else
        {
            Debug.Log("La chocolatera necesita chocolate.");
        }
    }

    private IEnumerator PrepararChocolate()
    {
        preparando = true;
        Debug.Log("Preparando chocolate...");

        yield return new WaitForSeconds(tiempoPreparacion);

        preparando = false;
        chocolateListo = true;

        Debug.Log("¡Chocolate preparado!");
    }
}
