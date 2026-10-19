using UnityEngine;

// La repisa tiene varios espacios (por defecto, dos) para apoyar objetos.
// También sirve para combinar bebidas:
//   - Café preparado + leche espumada        -> café con leche.
//   - Chocolate preparado + leche espumada   -> chocolate con leche.
// Hereda de EstacionInteractiva, así que se usa con la tecla E.
//
// Qué pasa al apretar E:
//   - Si el barista lleva algo que combina con un objeto de la repisa,
//     se combinan en ese espacio.
//   - Si no combina, lo apoya en un espacio libre.
//   - Si tiene las manos vacías, agarra el objeto del espacio más cercano.
public class Repisa : EstacionInteractiva
{
    // Posición de cada espacio respecto del centro de la repisa.
    // La cantidad de elementos de este array define cuántos espacios hay.
    // Se puede cambiar desde el Inspector.
    [SerializeField]
    private Vector3[] posicionesEspacios =
    {
        new Vector3(-0.4f, 0.3f, 0f),
        new Vector3(0.4f, 0.3f, 0f)
    };

    // Objeto apoyado en cada espacio. Ninguno significa que está libre.
    private PlayerInventory.TipoObjeto[] espacios;

    // Dibujo que muestra el objeto de cada espacio.
    private GloboObjeto[] globos;

    private void Start()
    {
        // Creamos los arrays con un lugar por cada espacio.
        // Un array nuevo de TipoObjeto arranca lleno de Ninguno (el valor 0),
        // así que todos los espacios empiezan libres.
        espacios = new PlayerInventory.TipoObjeto[posicionesEspacios.Length];
        globos = new GloboObjeto[posicionesEspacios.Length];

        // Creamos un dibujo para cada espacio (sin el fondo de globo,
        // para que el objeto se vea apoyado en la repisa).
        CatalogoObjetos catalogo = FindFirstObjectByType<CatalogoObjetos>();

        if (catalogo != null)
        {
            for (int i = 0; i < posicionesEspacios.Length; i++)
            {
                globos[i] = catalogo.CrearGlobo(transform, posicionesEspacios[i], false);
            }
        }
    }

    // Se ejecuta cuando el barista se acerca y presiona E.
    public override void Interactuar(PlayerInventory inventario)
    {
        if (inventario.TieneObjeto())
        {
            DejarObjeto(inventario);
        }
        else
        {
            AgarrarObjeto(inventario);
        }
    }

    // El barista lleva algo: intentamos combinarlo o apoyarlo.
    private void DejarObjeto(PlayerInventory inventario)
    {
        PlayerInventory.TipoObjeto objetoEnMano = inventario.ObtenerObjetoActual();

        // 1) ¿Combina con algo que ya está en la repisa?
        for (int i = 0; i < espacios.Length; i++)
        {
            PlayerInventory.TipoObjeto resultado = Combinar(espacios[i], objetoEnMano);

            if (resultado != PlayerInventory.TipoObjeto.Ninguno)
            {
                espacios[i] = resultado;
                inventario.SoltarObjeto();
                ActualizarEspacio(i);

                Debug.Log("¡Preparaste " + resultado + "!");

                // Sonido de "¡listo!" cuando se mezclan.
                Sonidos s = ObtenerSonidos();

                if (s != null)
                {
                    s.MaquinaLista();
                }

                return;
            }
        }

        // 2) Si no combina, lo apoyamos en el primer espacio libre.
        int espacioLibre = BuscarEspacioLibre();

        if (espacioLibre == -1)
        {
            Avisar("La repisa está llena.");
            return;
        }

        espacios[espacioLibre] = objetoEnMano;
        inventario.SoltarObjeto();
        ActualizarEspacio(espacioLibre);

        Debug.Log("Guardaste: " + objetoEnMano);
    }

    // El barista tiene las manos vacías: agarra el objeto más cercano.
    private void AgarrarObjeto(PlayerInventory inventario)
    {
        int espacio = BuscarEspacioOcupadoMasCercano(inventario.transform.position);

        if (espacio == -1)
        {
            Avisar("La repisa está vacía.");
            return;
        }

        if (inventario.RecogerObjeto(espacios[espacio]))
        {
            espacios[espacio] = PlayerInventory.TipoObjeto.Ninguno;
            ActualizarEspacio(espacio);
        }
    }

    // Devuelve el número del primer espacio libre,
    // o -1 si están todos ocupados.
    private int BuscarEspacioLibre()
    {
        for (int i = 0; i < espacios.Length; i++)
        {
            if (espacios[i] == PlayerInventory.TipoObjeto.Ninguno)
            {
                return i;
            }
        }

        return -1;
    }

    // Devuelve el número del espacio ocupado más cercano a una posición,
    // o -1 si están todos libres.
    private int BuscarEspacioOcupadoMasCercano(Vector3 posicionBarista)
    {
        int masCercano = -1;
        float menorDistancia = 0f;

        for (int i = 0; i < espacios.Length; i++)
        {
            // Los espacios libres no nos interesan.
            if (espacios[i] == PlayerInventory.TipoObjeto.Ninguno)
            {
                continue;
            }

            Vector3 posicionEspacio = transform.position + posicionesEspacios[i];
            float distancia = Vector2.Distance(posicionBarista, posicionEspacio);

            // Nos quedamos con el primero que encontramos,
            // o con este si está más cerca que el anterior.
            if (masCercano == -1 || distancia < menorDistancia)
            {
                masCercano = i;
                menorDistancia = distancia;
            }
        }

        return masCercano;
    }

    // Devuelve la bebida que sale de mezclar a y b,
    // o Ninguno si esos dos objetos no se pueden combinar.
    private PlayerInventory.TipoObjeto Combinar(
        PlayerInventory.TipoObjeto a,
        PlayerInventory.TipoObjeto b)
    {
        if (SonEstosDos(a, b,
            PlayerInventory.TipoObjeto.CafePreparado,
            PlayerInventory.TipoObjeto.LecheEspumada))
        {
            return PlayerInventory.TipoObjeto.CafeConLeche;
        }

        if (SonEstosDos(a, b,
            PlayerInventory.TipoObjeto.ChocolatePreparado,
            PlayerInventory.TipoObjeto.LecheEspumada))
        {
            return PlayerInventory.TipoObjeto.ChocolateConLeche;
        }

        return PlayerInventory.TipoObjeto.Ninguno;
    }

    // Devuelve true si a y b son los productos x e y, en cualquier orden.
    // && significa Y, || significa O.
    private bool SonEstosDos(
        PlayerInventory.TipoObjeto a,
        PlayerInventory.TipoObjeto b,
        PlayerInventory.TipoObjeto x,
        PlayerInventory.TipoObjeto y)
    {
        return (a == x && b == y) || (a == y && b == x);
    }

    // Actualiza el dibujo de un espacio según lo que tenga.
    private void ActualizarEspacio(int i)
    {
        if (globos[i] != null)
        {
            globos[i].Mostrar(espacios[i]);
        }
    }

    // ===================== CONSULTAS (para el tutorial) =====================

    // Devuelve true si alguno de los espacios tiene ese objeto.
    public bool Tiene(PlayerInventory.TipoObjeto objeto)
    {
        // Antes de Start el array todavía no existe.
        if (espacios == null)
        {
            return false;
        }

        for (int i = 0; i < espacios.Length; i++)
        {
            if (espacios[i] == objeto)
            {
                return true;
            }
        }

        return false;
    }
}
