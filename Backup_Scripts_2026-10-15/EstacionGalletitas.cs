
using UnityEngine;

public class EstacionGalletitas : MonoBehaviour
{
    [SerializeField] private Transform jugador;
    [SerializeField] private float distanciaInteraccion = 1.5f;

    private PlayerInventory inventario;

    private void Awake()
    {
        if (jugador != null)
        {
            inventario = jugador.GetComponent<PlayerInventory>();
        }
    }

    private void Update()
    {
        if (jugador == null || inventario == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            jugador.position
        );

        if (distancia > distanciaInteraccion)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            inventario.RecogerObjeto(
                PlayerInventory.TipoObjeto.GalletitaRedVelvet
            );
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            inventario.RecogerObjeto(
                PlayerInventory.TipoObjeto.GalletitaVainillaChocolate
            );
        }
    }
}
