
using UnityEngine;

// La bandeja es una estación interactiva.
// Por eso hereda de EstacionInteractiva y utiliza la tecla E.
public class Bandeja : EstacionInteractiva
{
    // Guardamos una referencia al cliente
    // al que pertenece esta bandeja.
    [SerializeField] private Cliente cliente;

    // Aquí guardamos la bebida colocada en la bandeja.
    // Al comenzar, la bandeja no tiene ninguna bebida.
    [SerializeField]
    private PlayerInventory.TipoObjeto bebida =
        PlayerInventory.TipoObjeto.Ninguno;

    // Aquí guardamos la galletita colocada en la bandeja.
    // También comienza vacía.
    [SerializeField]
    private PlayerInventory.TipoObjeto galletita =
        PlayerInventory.TipoObjeto.Ninguno;

    // Este método se ejecuta cuando el jugador
    // se acerca a la bandeja y presiona E.
    public override void Interactuar(PlayerInventory inventario)
    {
        // Primero comprobamos que haya un cliente asignado.
        // Si no lo hay, no podemos verificar ningún pedido.
        if (cliente == null)
        {
            Debug.Log("Esta bandeja no tiene cliente.");
            return;
        }

        // Consultamos qué objeto lleva el barista.
        PlayerInventory.TipoObjeto objeto = inventario.ObjetoActual;

        // CASO 1: El barista tiene las manos vacías.
        // En ese caso, intentamos entregar el pedido.
        if (objeto == PlayerInventory.TipoObjeto.Ninguno)
        {
            // Comprobamos si los productos coinciden
            // con lo que pidió el cliente.

            if (PedidoCorrecto())
            {
                // Primero vaciamos los dos espacios de la bandeja.
                bebida = PlayerInventory.TipoObjeto.Ninguno;
                galletita = PlayerInventory.TipoObjeto.Ninguno;

                // Después avisamos al cliente.
                // El cliente notificará al GameManager,
                // que generará el siguiente pedido.
                cliente.RecibirPedido();
            }

            else
            {
                // El pedido está incompleto o tiene
                // algún producto diferente del solicitado.
                Debug.Log("El pedido todavía no está completo o es incorrecto.");
            }

            // Terminamos esta interacción.
            return;
        }

        // CASO 2: El barista lleva una bebida.
        // EsBebida() nos dice si el objeto es una bebida terminada.
        if (EsBebida(objeto))
        {
            // Si ya hay una bebida, no podemos colocar otra.
            if (bebida != PlayerInventory.TipoObjeto.Ninguno)
            {
                Debug.Log("Ya hay una bebida en la bandeja.");
                return;
            }

            // Guardamos la bebida en la bandeja.
            bebida = objeto;

            // Quitamos la bebida de las manos del barista.
            inventario.SoltarObjeto();

            Debug.Log("Colocaste: " + bebida);
        }

        // CASO 3: El barista lleva una galletita.
        else if (EsGalletita(objeto))
        {
            // Si ya hay una galletita, no aceptamos otra.
            if (galletita != PlayerInventory.TipoObjeto.Ninguno)
            {
                Debug.Log("Ya hay una galletita en la bandeja.");
                return;
            }

            // Guardamos la galletita en la bandeja.
            galletita = objeto;

            // Vaciamos las manos del barista.
            inventario.SoltarObjeto();

            Debug.Log("Colocaste: " + galletita);
        }

        // CASO 4: El barista lleva algo que no se puede servir.
        else
        {
            Debug.Log("Este producto no se puede servir.");
        }
    }

    // Comprueba si la bandeja contiene exactamente
    // los productos que pidió el cliente.
    private bool PedidoCorrecto()
    {
        // && significa Y: ambas condiciones deben ser verdaderas.
        // Si el cliente no pidió galletita, ambos valores
        // de galletita deben ser Ninguno.
        return bebida == cliente.BebidaPedida &&
               galletita == cliente.GalletitaPedida;
    }

    // Devuelve true si el objeto es una bebida lista para servir.
    // Devuelve false para cualquier otro objeto.
    private bool EsBebida(PlayerInventory.TipoObjeto objeto)
    {
        // || significa O: alcanza con que una condición sea verdadera.
        return objeto == PlayerInventory.TipoObjeto.CafePreparado ||
               objeto == PlayerInventory.TipoObjeto.CafeConLeche ||
               objeto == PlayerInventory.TipoObjeto.ChocolatePreparado ||
               objeto == PlayerInventory.TipoObjeto.ChocolateConLeche;
    }

    // Comprueba si el objeto pertenece a uno
    // de nuestros dos tipos de galletitas.
    private bool EsGalletita(PlayerInventory.TipoObjeto objeto)
    {
        return objeto == PlayerInventory.TipoObjeto.GalletitaRedVelvet ||
               objeto == PlayerInventory.TipoObjeto.GalletitaVainillaChocolate;
    }


    // Permite retirar un producto de la bandeja.
    // Lo usaremos cuando el jugador se equivoque de pedido.
    public void RetirarProducto(PlayerInventory inventario)
    {
        // No podemos retirar nada si tenemos las manos ocupadas.
        if (inventario.TieneObjeto())
        {
            Debug.Log("Necesitás tener las manos vacías.");
            return;
        }

        // Si hay una galletita, intentamos retirarla primero.
        if (galletita != PlayerInventory.TipoObjeto.Ninguno)
        {
            // Si el inventario acepta la galletita,
            // vaciamos su espacio en la bandeja.
            if (inventario.RecogerObjeto(galletita))
            {
                galletita = PlayerInventory.TipoObjeto.Ninguno;
            }

            return;
        }

        // Si no hay galletita, intentamos retirar la bebida.
        if (bebida != PlayerInventory.TipoObjeto.Ninguno)
        {
            // Transferimos la bebida al inventario del jugador.
            if (inventario.RecogerObjeto(bebida))
            {
                bebida = PlayerInventory.TipoObjeto.Ninguno;
            }

            return;
        }

        // Si no hay ningún producto, mostramos este mensaje.
        Debug.Log("La bandeja está vacía.");
    }

    // Vacía la bandeja cuando un cliente abandona su pedido.
    public void VaciarBandeja()
    {
        // Eliminamos la bebida que estuviera colocada.
        bebida = PlayerInventory.TipoObjeto.Ninguno;

        // Eliminamos la galletita que estuviera colocada.
        galletita = PlayerInventory.TipoObjeto.Ninguno;

        // Mostramos un mensaje para comprobarlo.
        Debug.Log("Se vació la bandeja de " + gameObject.name);
    }

}
