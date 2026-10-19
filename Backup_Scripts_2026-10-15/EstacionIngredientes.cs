
using UnityEngine;

public class EstacionIngredientes : MonoBehaviour
{
    [SerializeField] private float distanciaInteraccion = 1.5f;
    [SerializeField] private Transform jugador;

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
            inventario.RecogerObjeto(PlayerInventory.TipoObjeto.Cafe);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            inventario.RecogerObjeto(PlayerInventory.TipoObjeto.Leche);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            inventario.RecogerObjeto(PlayerInventory.TipoObjeto.Chocolate);
        }
    }
}
