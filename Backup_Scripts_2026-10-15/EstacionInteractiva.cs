
using UnityEngine;

public class EstacionInteractiva : MonoBehaviour
{
    public virtual void Interactuar(PlayerInventory inventario)
    {
        Debug.Log("Interactuaste con: " + gameObject.name);
    }
}
