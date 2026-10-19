using TMPro;          // Permite usar textos de TextMeshPro.
using UnityEngine;
using UnityEngine.UI; // Permite usar Image (el dibujo del pedido).

// Tutorial guiado "de a un paso".
// En el centro de la barra de arriba muestra UNA sola instrucción
// (por ejemplo "Andá a Ingredientes y apretá Z") y una flecha que rebota
// arriba de la estación a la que hay que ir.
//
// ¿Cómo sabe qué paso mostrar? En cada frame se fija cómo están las cosas
// (qué tiene el barista en la mano, si la moledora está trabajando, etc.)
// y elige el paso con una cadena de if / else if, empezando por el final
// de la receta: "¿ya está en la bandeja? ¿lo tengo en la mano? ¿está listo
// en la máquina?...". Así, si el jugador se equivoca o tira algo,
// la guía se acomoda sola.
//
// Va en el prefab HUD. El GameManager lo busca en Start y le avisa
// cuándo empieza cada pedido del tutorial, cuándo se entregó
// y cuándo terminó el tutorial.
public class GuiaTutorial : MonoBehaviour
{
    // ===================== REFERENCIAS (Inspector) =====================

    // Cartel del centro de la barra de arriba (se oculta al terminar el tutorial).
    [SerializeField] private GameObject cartelTutorial;

    // Renglón chico de arriba: "Tu cliente quiere un café".
    [SerializeField] private TMP_Text textoTitulo;

    // Renglón grande: la instrucción del paso actual.
    [SerializeField] private TMP_Text textoInstruccion;

    // Dibujo de la bebida que pidió el cliente.
    [SerializeField] private Image imagenPedido;

    // Prefab de la flecha que señala la estación (un SpriteRenderer).
    [SerializeField] private GameObject prefabFlecha;

    // ============================ CONSTANTES ============================

    // El dibujo de la flecha apunta hacia ABAJO. Para que apunte a otro lado
    // la giramos: 90 = hacia la derecha, -90 = hacia la izquierda, 180 = hacia arriba.
    private const float AnguloAbajo = 0f;
    private const float AnguloDerecha = 90f;
    private const float AnguloIzquierda = -90f;
    private const float AnguloArriba = 180f;

    // Altura (en el mundo) de la flecha cuando señala algo que está sobre la barra:
    // justo debajo de la barra, donde se para el barista.
    private const float AlturaFlechaBarra = -0.85f;

    // Qué tan rápido y qué tan alto rebota la flecha.
    private const float VelocidadRebote = 6f;
    private const float AlturaRebote = 0.12f;

    // Duración (en segundos) del "saltito" del texto cuando cambia el paso.
    private const float DuracionSaltito = 0.25f;

    // Color de fondo de las teclas (#RRGGBBAA, medio transparente).
    private const string ColorTecla = "#F2C14E88";

    // ======================== ESTADO DEL TUTORIAL ========================

    // true mientras hay un pedido del tutorial en curso.
    private bool activo = false;

    // true cuando ya se entregó el pedido actual (esperamos al siguiente cliente).
    private bool entregado = false;

    // Cliente del pedido actual y la bebida que pidió.
    private Cliente clienteActual;
    private PlayerInventory.TipoObjeto bebidaPedida = PlayerInventory.TipoObjeto.Ninguno;

    // Número del pedido (1, 2 o 3) y cantidad total del tutorial.
    private int numeroPedido = 0;
    private int totalPedidos = 0;

    // Último texto que mostramos, para saber cuándo cambia el paso.
    private string instruccionActual = "";

    // Tiempo que le queda al "saltito" del texto (temporizador).
    private float tiempoSaltito = 0f;

    // ========================= OTRAS REFERENCIAS =========================
    // Las buscamos una sola vez en Start.

    private PlayerInventory inventario;
    private EstacionIngredientes ingredientes;
    private MoledoraCafe moledora;
    private Cafetera cafetera;
    private Chocolatera chocolatera;
    private Tacho tacho;
    private Repisa[] repisas;
    private CatalogoObjetos catalogo;

    // La flecha que creamos con Instantiate.
    private Transform flecha;

    // Hacia dónde apunta la flecha (null = no se muestra).
    private Transform destinoFlecha;

    // Dónde va la flecha respecto del destino y hacia qué lado apunta.
    private Vector3 desplazamientoFlecha = Vector3.zero;
    private float anguloFlecha = 0f;

    // ============================ INICIO ============================

    private void Start()
    {
        // Buscamos cada estación una sola vez (FindFirstObjectByType es lento).
        inventario = FindFirstObjectByType<PlayerInventory>();
        ingredientes = FindFirstObjectByType<EstacionIngredientes>();
        moledora = FindFirstObjectByType<MoledoraCafe>();
        cafetera = FindFirstObjectByType<Cafetera>();
        chocolatera = FindFirstObjectByType<Chocolatera>();
        tacho = FindFirstObjectByType<Tacho>();

        // Hay dos repisas: FindObjectsByType (en plural) devuelve un array con todas.
        repisas = FindObjectsByType<Repisa>(FindObjectsSortMode.None);

        // Creamos la flecha a partir del prefab y la escondemos hasta que haga falta.
        if (prefabFlecha != null)
        {
            GameObject nuevaFlecha = Instantiate(prefabFlecha);
            flecha = nuevaFlecha.transform;
            nuevaFlecha.SetActive(false);
        }

        // Si el GameManager todavía no empezó un pedido, el cartel queda oculto.
        cartelTutorial.SetActive(activo);
    }

    // Update se ejecuta en cada frame: elegimos el paso y movemos la flecha.
    private void Update()
    {
        if (!activo)
        {
            return;
        }

        ElegirPaso();
        MoverFlecha();
        AnimarSaltito();
    }

    // ===================== AVISOS DEL GAMEMANAGER =====================

    // Empieza un pedido del tutorial.
    // numero: 1, 2 o 3. total: cantidad de pedidos del tutorial.
    public void EmpezarPedido(Cliente cliente, PlayerInventory.TipoObjeto bebida, int numero, int total)
    {
        clienteActual = cliente;
        bebidaPedida = bebida;
        numeroPedido = numero;
        totalPedidos = total;
        entregado = false;
        activo = true;

        cartelTutorial.SetActive(true);
        MostrarDibujo(PlayerInventory.TipoObjeto.Ninguno);
    }

    // El cliente recibió su pedido.
    public void PedidoEntregado()
    {
        entregado = true;
    }

    // Terminó el tutorial: escondemos el cartel y la flecha.
    public void Terminar()
    {
        activo = false;
        cartelTutorial.SetActive(false);
        destinoFlecha = null;
        MoverFlecha();
    }

    // ======================= ELEGIR EL PASO ACTUAL =======================

    private void ElegirPaso()
    {
        // 1) Ya entregamos: esperamos al próximo cliente.
        if (entregado)
        {
            MostrarTitulo("¡Pedido " + numeroPedido + " de " + totalPedidos + " listo!");
            Indicar("¡Muy bien! Ya viene el próximo cliente...", null);
            return;
        }

        // 2) El cliente todavía está entrando: esperamos a que llegue.
        if (!clienteActual.PuedeRecibirPedido())
        {
            MostrarTitulo("Tutorial · Pedido " + numeroPedido + " de " + totalPedidos);
            MostrarDibujo(PlayerInventory.TipoObjeto.Ninguno);
            Indicar("¡Llega un cliente! Esperalo en el mostrador", null);
            return;
        }

        // 3) El cliente llegó: nos dice qué quiere.
        MostrarTitulo("Tu cliente quiere <b>" + NombreDe(bebidaPedida) + "</b>");
        MostrarDibujo(bebidaPedida);

        PlayerInventory.TipoObjeto enMano = inventario.ObtenerObjetoActual();
        Bandeja bandeja = clienteActual.ObtenerBandeja();

        // 4) La bebida ya está en la bandeja: falta entregarla.
        if (bandeja.ObtenerBebida() == bebidaPedida)
        {
            if (enMano == PlayerInventory.TipoObjeto.Ninguno)
            {
                Indicar("Apretá " + Tecla("E") + " en la bandeja para <b>entregar</b>", bandeja.transform);
            }
            else
            {
                Indicar("Para entregar, primero dejá lo que tenés en una repisa", RepisaMasCercana());
            }
            return;
        }

        // 5) Tenemos la bebida en la mano: a la bandeja.
        if (enMano == bebidaPedida)
        {
            Indicar("¡Ya está! Llevalo a la <b>bandeja</b> y apretá " + Tecla("E"), bandeja.transform);
            return;
        }

        // 6) Hay otra bebida en la bandeja: hay que sacarla.
        if (bandeja.ObtenerBebida() != PlayerInventory.TipoObjeto.Ninguno && enMano == PlayerInventory.TipoObjeto.Ninguno)
        {
            Indicar("Esa no es la bebida que pidió: sacala con " + Tecla("R"), bandeja.transform);
            return;
        }

        // 7) Si no, seguimos la receta de la bebida pedida.
        switch (bebidaPedida)
        {
            case PlayerInventory.TipoObjeto.CafePreparado:
                GuiarCafe(enMano);
                break;

            case PlayerInventory.TipoObjeto.CafeConLeche:
                GuiarCafeConLeche(enMano);
                break;

            case PlayerInventory.TipoObjeto.ChocolatePreparado:
                GuiarChocolate(enMano);
                break;
        }
    }

    // ============================ RECETAS ============================
    // Cada receta revisa los pasos desde el FINAL hacia el principio.
    // El primero que se cumple es el que mostramos.

    // Café: granos (Z) > moledora > café molido > cafetera > café.
    private void GuiarCafe(PlayerInventory.TipoObjeto enMano)
    {
        if (cafetera.ObtenerProductoListo() == PlayerInventory.TipoObjeto.CafePreparado)
        {
            Agarrar("¡El café está listo! Agarralo con " + Tecla("E"), cafetera.transform, enMano);
        }
        else if (cafetera.ObtenerProductoEnPreparacion() == PlayerInventory.TipoObjeto.CafePreparado)
        {
            Indicar("La cafetera está haciendo el café... ¡esperá!", cafetera.transform);
        }
        else if (enMano == PlayerInventory.TipoObjeto.CafeMolido)
        {
            Indicar("Ahora llevá el café molido a la <b>cafetera</b> y apretá " + Tecla("E"), cafetera.transform);
        }
        else if (moledora.TieneCafeListo())
        {
            Agarrar("¡Ya está molido! Agarralo con " + Tecla("E"), moledora.transform, enMano);
        }
        else if (moledora.EstaMoliendo())
        {
            Indicar("La moledora está moliendo... ¡esperá!", moledora.transform);
        }
        else if (enMano == PlayerInventory.TipoObjeto.Cafe)
        {
            Indicar("Llevá los granos a la <b>moledora</b> y apretá " + Tecla("E"), moledora.transform);
        }
        else if (RepisaCon(PlayerInventory.TipoObjeto.CafeMolido) != null)
        {
            Agarrar("Agarrá el café molido de la repisa con " + Tecla("E"),
                    RepisaCon(PlayerInventory.TipoObjeto.CafeMolido), enMano);
        }
        else if (enMano != PlayerInventory.TipoObjeto.Ninguno)
        {
            ManosOcupadas();
        }
        else
        {
            Indicar("Andá a <b>Ingredientes</b> y apretá " + Tecla("Z") + " para agarrar granos de café",
                    ingredientes.transform);
        }
    }

    // Leche espumada: leche (X) > cafetera > leche espumada.
    private void GuiarLeche(PlayerInventory.TipoObjeto enMano)
    {
        if (cafetera.ObtenerProductoListo() == PlayerInventory.TipoObjeto.LecheEspumada)
        {
            Agarrar("¡La leche está espumada! Agarrala con " + Tecla("E"), cafetera.transform, enMano);
        }
        else if (cafetera.ObtenerProductoEnPreparacion() == PlayerInventory.TipoObjeto.LecheEspumada)
        {
            Indicar("La cafetera está espumando la leche... ¡esperá!", cafetera.transform);
        }
        else if (enMano == PlayerInventory.TipoObjeto.Leche)
        {
            Indicar("Llevá la leche a la <b>cafetera</b> y apretá " + Tecla("E") + " para espumarla",
                    cafetera.transform);
        }
        else if (enMano != PlayerInventory.TipoObjeto.Ninguno)
        {
            ManosOcupadas();
        }
        else
        {
            Indicar("Ahora la leche: andá a <b>Ingredientes</b> y apretá " + Tecla("X"),
                    ingredientes.transform);
        }
    }

    // Café con leche: café + leche espumada, juntos en una repisa.
    private void GuiarCafeConLeche(PlayerInventory.TipoObjeto enMano)
    {
        // ¿En qué repisa está cada cosa? (null = en ninguna)
        Transform repisaConMezcla = RepisaCon(PlayerInventory.TipoObjeto.CafeConLeche);
        Transform repisaConCafe = RepisaCon(PlayerInventory.TipoObjeto.CafePreparado);
        Transform repisaConLeche = RepisaCon(PlayerInventory.TipoObjeto.LecheEspumada);

        if (repisaConMezcla != null)
        {
            Agarrar("¡Se mezclaron! Agarrá el café con leche con " + Tecla("E"), repisaConMezcla, enMano);
        }
        else if (enMano == PlayerInventory.TipoObjeto.CafePreparado && repisaConLeche != null)
        {
            Indicar("Poné el café en la repisa con la leche: ¡se mezclan solos!", repisaConLeche);
        }
        else if (enMano == PlayerInventory.TipoObjeto.LecheEspumada && repisaConCafe != null)
        {
            Indicar("Poné la leche en la repisa con el café: ¡se mezclan solos!", repisaConCafe);
        }
        else if (enMano == PlayerInventory.TipoObjeto.CafePreparado ||
                 enMano == PlayerInventory.TipoObjeto.LecheEspumada)
        {
            // Solo se puede llevar una cosa a la vez: la apoyamos para tener las manos libres.
            Indicar("Dejalo en una <b>repisa</b> con " + Tecla("E") + " para tener las manos libres",
                    RepisaMasCercana());
        }
        else if (repisaConCafe != null && repisaConLeche != null)
        {
            // Están en repisas distintas: hay que juntarlas.
            Agarrar("Agarrá la leche y ponela en la repisa del café", repisaConLeche, enMano);
        }
        else if (repisaConCafe != null ||
                 cafetera.ObtenerProductoListo() == PlayerInventory.TipoObjeto.LecheEspumada ||
                 cafetera.ObtenerProductoEnPreparacion() == PlayerInventory.TipoObjeto.LecheEspumada)
        {
            // El café ya está (o la leche está en marcha): seguimos con la leche.
            GuiarLeche(enMano);
        }
        else
        {
            // Todavía falta el café: lo hacemos igual que en el primer pedido.
            GuiarCafe(enMano);
        }
    }

    // Chocolate: chocolate (C) > chocolatera > chocolate preparado.
    private void GuiarChocolate(PlayerInventory.TipoObjeto enMano)
    {
        if (chocolatera.TieneChocolateListo())
        {
            Agarrar("¡El chocolate está listo! Agarralo con " + Tecla("E"), chocolatera.transform, enMano);
        }
        else if (chocolatera.EstaPreparando())
        {
            Indicar("La chocolatera está trabajando... ¡esperá!", chocolatera.transform);
        }
        else if (enMano == PlayerInventory.TipoObjeto.Chocolate)
        {
            Indicar("Llevá el chocolate a la <b>chocolatera</b> y apretá " + Tecla("E"), chocolatera.transform);
        }
        else if (enMano != PlayerInventory.TipoObjeto.Ninguno)
        {
            ManosOcupadas();
        }
        else
        {
            Indicar("Andá a <b>Ingredientes</b> y apretá " + Tecla("C") + " para agarrar chocolate",
                    ingredientes.transform);
        }
    }

    // ======================== FUNCIONES DE AYUDA ========================

    // Muestra una instrucción y apunta la flecha a un destino (null = sin flecha).
    private void Indicar(string texto, Transform destino)
    {
        // Si el paso cambió, hacemos que el texto dé un saltito para que se note.
        if (texto != instruccionActual)
        {
            instruccionActual = texto;
            textoInstruccion.text = texto;
            tiempoSaltito = DuracionSaltito;
        }

        destinoFlecha = destino;

        if (destino != null)
        {
            UbicarFlecha(destino);
        }
    }

    // Elige dónde va la flecha según qué estación es el destino.
    private void UbicarFlecha(Transform destino)
    {
        if (destino == clienteActual.ObtenerBandeja().transform)
        {
            // La bandeja está arriba de la barra: la flecha va debajo de la barra
            // (donde se para el barista), apuntando hacia arriba.
            desplazamientoFlecha = new Vector3(0f, AlturaFlechaBarra - destino.position.y, 0f);
            anguloFlecha = AnguloArriba;
        }
        else if (destino == ingredientes.transform)
        {
            // La despensa está pegada a la pared izquierda: la flecha va a su derecha.
            desplazamientoFlecha = new Vector3(1.55f, 0.3f, 0f);
            anguloFlecha = AnguloIzquierda;
        }
        else if (tacho != null && destino == tacho.transform)
        {
            // El tacho está al lado de las galletitas: la flecha va a su izquierda.
            desplazamientoFlecha = new Vector3(-0.95f, 0f, 0f);
            anguloFlecha = AnguloDerecha;
        }
        else
        {
            // Máquinas y repisas de la fila de abajo: su posición es el borde de arriba
            // (donde se para el barista). La flecha va justo arriba, apuntando hacia abajo.
            desplazamientoFlecha = new Vector3(0f, 0.45f, 0f);
            anguloFlecha = AnguloAbajo;
        }
    }

    // Cambia el título solo si es distinto del que ya está
    // (así no le pedimos a TextMeshPro que redibuje el texto en cada frame).
    private void MostrarTitulo(string texto)
    {
        if (textoTitulo.text != texto)
        {
            textoTitulo.text = texto;
        }
    }

    // Para agarrar algo hacen falta las manos vacías.
    private void Agarrar(string texto, Transform lugar, PlayerInventory.TipoObjeto enMano)
    {
        if (enMano == PlayerInventory.TipoObjeto.Ninguno)
        {
            Indicar(texto, lugar);
        }
        else
        {
            ManosOcupadas();
        }
    }

    // El barista tiene algo que no sirve para este paso.
    private void ManosOcupadas()
    {
        if (tacho != null)
        {
            Indicar("Eso no sirve ahora: tiralo en el <b>tacho</b> con " + Tecla("E"), tacho.transform);
        }
        else
        {
            Indicar("Eso no sirve ahora: dejalo en una <b>repisa</b> con " + Tecla("E"), RepisaMasCercana());
        }
    }

    // Devuelve la repisa que tiene ese objeto (o null si ninguna lo tiene).
    private Transform RepisaCon(PlayerInventory.TipoObjeto objeto)
    {
        for (int i = 0; i < repisas.Length; i++)
        {
            if (repisas[i].Tiene(objeto))
            {
                return repisas[i].transform;
            }
        }

        return null;
    }

    // Devuelve la repisa más cercana al barista (igual que buscar un mínimo).
    private Transform RepisaMasCercana()
    {
        Transform masCercana = null;
        float menorDistancia = 0f;

        for (int i = 0; i < repisas.Length; i++)
        {
            float distancia = Vector2.Distance(inventario.transform.position, repisas[i].transform.position);

            if (masCercana == null || distancia < menorDistancia)
            {
                masCercana = repisas[i].transform;
                menorDistancia = distancia;
            }
        }

        return masCercana;
    }

    // Nombre de cada bebida, para el título.
    private string NombreDe(PlayerInventory.TipoObjeto bebida)
    {
        switch (bebida)
        {
            case PlayerInventory.TipoObjeto.CafePreparado:
                return "un café";
            case PlayerInventory.TipoObjeto.CafeConLeche:
                return "un café con leche";
            case PlayerInventory.TipoObjeto.ChocolatePreparado:
                return "un chocolate";
            default:
                return "algo rico";
        }
    }

    // Muestra el dibujo de la bebida (o lo oculta si es Ninguno).
    private void MostrarDibujo(PlayerInventory.TipoObjeto bebida)
    {
        // Búsqueda "perezosa": buscamos el catálogo la primera vez que hace falta.
        if (catalogo == null)
        {
            catalogo = FindFirstObjectByType<CatalogoObjetos>();
        }

        Sprite dibujo = null;

        if (catalogo != null && bebida != PlayerInventory.TipoObjeto.Ninguno)
        {
            dibujo = catalogo.ObtenerSprite(bebida);
        }

        // Solo lo cambiamos si es distinto del que ya está.
        if (imagenPedido.sprite != dibujo)
        {
            imagenPedido.sprite = dibujo;
        }

        // Sin dibujo, ocultamos la imagen para que no se vea un cuadrado blanco.
        imagenPedido.enabled = dibujo != null;
    }

    // Devuelve una tecla "dibujada": la letra en negrita con un fondo amarillo.
    // <mark> es una etiqueta de TextMeshPro que pinta un rectángulo detrás del texto.
    private string Tecla(string tecla)
    {
        return "<mark=" + ColorTecla + "><b> " + tecla + " </b></mark>";
    }

    // ========================= ANIMACIONES =========================

    // Pone la flecha al lado del destino, rebotando.
    private void MoverFlecha()
    {
        if (flecha == null)
        {
            return;
        }

        if (destinoFlecha == null)
        {
            flecha.gameObject.SetActive(false);
            return;
        }

        flecha.gameObject.SetActive(true);

        // Giramos la flecha (solo en el eje Z, porque el juego es 2D).
        flecha.eulerAngles = new Vector3(0f, 0f, anguloFlecha);

        // Mathf.Sin da un número que va y viene entre -1 y 1 a medida que pasa
        // el tiempo: lo usamos para que la flecha vaya y venga sola.
        float rebote = Mathf.Sin(Time.time * VelocidadRebote) * AlturaRebote;

        // flecha.up es la dirección "arriba" de la flecha, que gira con ella:
        // así rebota siempre a lo largo de hacia donde apunta.
        flecha.position = destinoFlecha.position + desplazamientoFlecha + flecha.up * rebote;
    }

    // Cuando cambia el paso, el texto se agranda un poquito y vuelve a su tamaño.
    private void AnimarSaltito()
    {
        if (tiempoSaltito <= 0f)
        {
            return;
        }

        tiempoSaltito -= Time.deltaTime;

        // Va de 1.08 (recién cambiado) a 1 (tamaño normal).
        float escala = 1f + 0.08f * (tiempoSaltito / DuracionSaltito);

        if (tiempoSaltito <= 0f)
        {
            escala = 1f;
        }

        textoInstruccion.transform.localScale = new Vector3(escala, escala, 1f);
    }
}
