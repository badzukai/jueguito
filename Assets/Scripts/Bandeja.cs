using UnityEngine;

// La bandeja es una estación interactiva.
// Por eso hereda de EstacionInteractiva y utiliza la tecla E.
// Cada bandeja pertenece a un cliente: ahí se arma su pedido.
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
        PlayerInventory.TipoObjeto objeto = inventario.ObtenerObjetoActual();

        // CASO 1: El barista tiene las manos vacías.
        // En ese caso, intentamos entregar el pedido.
        if (objeto == PlayerInventory.TipoObjeto.Ninguno)
        {
            IntentarEntregarPedido();
            return;
        }

        // CASO 2: El barista lleva una bebida.
        if (EsBebida(objeto))
        {
            // Si ya hay una bebida, no podemos colocar otra.
            if (bebida != PlayerInventory.TipoObjeto.Ninguno)
            {
                Avisar("Ya hay una bebida en la bandeja.");
                return;
            }

            // Pasamos la bebida de la mano a la bandeja.
            bebida = objeto;
            inventario.SoltarObjeto();

            Debug.Log("Colocaste: " + bebida);
        }
        // CASO 3: El barista lleva una galletita.
        else if (EsGalletita(objeto))
        {
            // Si ya hay una galletita, no aceptamos otra.
            if (galletita != PlayerInventory.TipoObjeto.Ninguno)
            {
                Avisar("Ya hay una galletita en la bandeja.");
                return;
            }

            // Pasamos la galletita de la mano a la bandeja.
            galletita = objeto;
            inventario.SoltarObjeto();

            Debug.Log("Colocaste: " + galletita);
        }
        // CASO 4: El barista lleva algo que no se puede servir
        // (por ejemplo, granos de café o leche sin preparar).
        else
        {
            Avisar("Eso no se puede servir: primero hay que prepararlo.");
        }
    }

    // Revisa la bandeja y, si el pedido está bien, se lo da al cliente.
    private void IntentarEntregarPedido()
    {
        // Si el cliente todavía no llegó o ya se está yendo,
        // no puede recibir nada.
        if (!cliente.PuedeRecibirPedido())
        {
            Avisar("El cliente no está esperando un pedido.");
            return;
        }

        // Le preguntamos al cliente si esto es lo que pidió.
        if (cliente.EsPedidoCorrecto(bebida, galletita))
        {
            // Primero vaciamos los dos espacios de la bandeja.
            VaciarBandeja();

            // Después avisamos al cliente.
            // El cliente notificará al GameManager.
            cliente.RecibirPedido();
        }
        else
        {
            // El pedido está incompleto o tiene
            // algún producto diferente del solicitado.
            Avisar("El pedido no está completo o no es lo que pidió.");
        }
    }

    // Devuelve true si el objeto es una bebida lista para servir.
    // || significa O: alcanza con que una condición sea verdadera.
    private bool EsBebida(PlayerInventory.TipoObjeto objeto)
    {
        return objeto == PlayerInventory.TipoObjeto.CafePreparado ||
               objeto == PlayerInventory.TipoObjeto.CafeConLeche ||
               objeto == PlayerInventory.TipoObjeto.ChocolatePreparado ||
               objeto == PlayerInventory.TipoObjeto.ChocolateConLeche;
    }

    // Devuelve true si el objeto es uno
    // de nuestros tres tipos de galletitas.
    private bool EsGalletita(PlayerInventory.TipoObjeto objeto)
    {
        return objeto == PlayerInventory.TipoObjeto.GalletitaRedVelvet ||
               objeto == PlayerInventory.TipoObjeto.GalletitaVainillaChocolate ||
               objeto == PlayerInventory.TipoObjeto.GalletitaChocolate;
    }

    // Permite retirar un producto de la bandeja con la tecla R.
    // Sirve cuando el jugador se equivocó de pedido.
    public void RetirarProducto(PlayerInventory inventario)
    {
        // No podemos retirar nada si tenemos las manos ocupadas.
        if (inventario.TieneObjeto())
        {
            Avisar("Para sacar algo de la bandeja necesitás las manos vacías.");
            return;
        }

        // Si hay una galletita, la retiramos primero.
        if (galletita != PlayerInventory.TipoObjeto.Ninguno)
        {
            inventario.RecogerObjeto(galletita);
            galletita = PlayerInventory.TipoObjeto.Ninguno;
            return;
        }

        // Si no hay galletita, retiramos la bebida.
        if (bebida != PlayerInventory.TipoObjeto.Ninguno)
        {
            inventario.RecogerObjeto(bebida);
            bebida = PlayerInventory.TipoObjeto.Ninguno;
            return;
        }

        // Si no hay ningún producto, mostramos este mensaje.
        Avisar("La bandeja está vacía.");
    }

    // Deja la bandeja vacía.
    // Se usa al entregar un pedido y cuando un cliente pierde la paciencia.
    public void VaciarBandeja()
    {
        bebida = PlayerInventory.TipoObjeto.Ninguno;
        galletita = PlayerInventory.TipoObjeto.Ninguno;
    }

    // ===================== CONSULTAS (para el tutorial) =====================

    // Devuelve la bebida que está en la bandeja (Ninguno si no hay).
    public PlayerInventory.TipoObjeto ObtenerBebida()
    {
        return bebida;
    }
}
