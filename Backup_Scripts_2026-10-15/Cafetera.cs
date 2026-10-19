
using System.Collections;
using UnityEngine;

public class Cafetera : EstacionInteractiva
{
    [SerializeField] private float tiempoCafe = 4f;
    [SerializeField] private float tiempoLeche = 3f;

    private bool preparando = false;

    private PlayerInventory.TipoObjeto productoListo =
        PlayerInventory.TipoObjeto.Ninguno;

    public override void Interactuar(PlayerInventory inventario)
    {
        if (preparando)
        {
            Debug.Log("La cafetera está ocupada.");
            return;
        }

        if (productoListo != PlayerInventory.TipoObjeto.Ninguno)
        {
            if (inventario.RecogerObjeto(productoListo))
            {
                productoListo = PlayerInventory.TipoObjeto.Ninguno;
            }

            return;
        }

        switch (inventario.ObjetoActual)
        {
            case PlayerInventory.TipoObjeto.CafeMolido:
                inventario.SoltarObjeto();
                StartCoroutine(Preparar(
                    PlayerInventory.TipoObjeto.CafePreparado,
                    tiempoCafe
                ));
                break;

            case PlayerInventory.TipoObjeto.Leche:
                inventario.SoltarObjeto();
                StartCoroutine(Preparar(
                    PlayerInventory.TipoObjeto.LecheEspumada,
                    tiempoLeche
                ));
                break;

            default:
                Debug.Log("Necesitás café molido o leche.");
                break;
        }
    }

    private IEnumerator Preparar(
        PlayerInventory.TipoObjeto resultado,
        float duracion)
    {
        preparando = true;
        Debug.Log("Preparando: " + resultado);

        yield return new WaitForSeconds(duracion);

        productoListo = resultado;
        preparando = false;

        Debug.Log("¡Listo para retirar: " + productoListo + "!");
    }
}
