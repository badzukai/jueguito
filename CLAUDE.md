# Barista Caótico — contexto para Claude

Proyecto de **Helena** para el parcial de **Programación de Videojuegos 1** (T.U. en Videojuegos, UTN, ago/nov 2026).
Helena es estudiante: nunca había programado videojuegos y está aprendiendo Unity. **Ella tiene que defender el código**, así que todo tiene que poder explicarse con lo que vio en clase.

- Motor: **Unity 6000.3.26f1**, plantilla URP 2D. Input: el viejo `Input` (GetKeyDown / GetAxisRaw); activeInputHandler = Both.
- Carpeta del proyecto: `C:\Users\PC\Documents\HELENA\Jueguito\Jueguito2`
- Backup de los scripts originales (antes de la refactorización): `Backup_Scripts_2026-10-15/` (fuera de Assets a propósito).
- Idioma: español rioplatense (voseo) en código, comentarios, mensajes y en la charla con Helena.

## El juego

Cafetería 2D vista desde arriba. El barista (Player) prepara bebidas y las sirve en la bandeja de cada cliente.

- **Ingredientes** (EstacionIngredientes): Z café en grano · X leche · C chocolate.
- **Galletitas** (EstacionGalletitas): Z red velvet · X vainilla/chocolate · C chocolate.
- **E** usa la estación más cercana (moledora, cafetera, chocolatera, repisas, bandejas, tacho). **R** saca un producto de una bandeja.
- Recetas: granos → moledora → café molido → cafetera → café. Leche → cafetera → leche espumada. Chocolate → chocolatera.
  Café/chocolate + leche espumada combinados en una **repisa** (2 espacios) → café con leche / chocolate con leche.
- **Tutorial**: 3 pedidos fijos, un cliente distinto cada uno (Cliente1 café, Cliente2 café con leche, Cliente3 chocolate), sin paciencia, con mensajes guía en el HUD.
- **Partida normal**: vuelven Cliente1 y Cliente2 con pedidos al azar y paciencia. A los `pedidosParaTercerCliente` (10) se abre el puesto 3.
  Si a un cliente se le acaba la paciencia, se va enojado y entra otro. Se gana con `pedidosParaGanar` (12 en la escena) y se pierde al perder 3 clientes.
  Al terminar, `Time.timeScale = 0`.
- Los clientes entran y salen caminando (MovimientoCliente) y tienen una pausa de 2 s entre uno y otro.

## Criterio de clase (MUY importante)

Material visto (Unidades 1 a 3): variables (int/float/bool/string/char), `const`, camelCase, funciones (con return, void, parámetros, **parámetros opcionales**, ref/out), if/else, && || !, **switch**, for/while/do-while, break/continue, contadores y acumuladores, **arrays** (también 2D), y en Unity: MonoBehaviour, Awake/Start/Update/FixedUpdate/LateUpdate, public/private/[SerializeField], Vector2/3, Transform, Translate, Instantiate/Destroy y prefabs, Input, Time.deltaTime y Time.timeScale, SpriteRenderer, Animator, Tags/CompareTag, Layers, GetComponent guardado en Start, FindObjectOfType solo en Start, Rigidbody2D, colliders, triggers, UI (Canvas, TMP, Button OnClick, Image, Slider) y SceneManager.LoadScene.

Reglas que venimos siguiendo:
- **No usar**: corrutinas (IEnumerator/yield), propiedades `{ get; }`, `foreach`, `is`, LINQ, eventos/delegates, `static` propio. Los temporizadores se hacen en `Update` con `Time.deltaTime`, y en lugar de propiedades se usan funciones (`ObtenerX()`, `EstaX()`).
- **Se mantienen** (Helena eligió el nivel "intermedio"): `enum` (`PlayerInventory.TipoObjeto`) y **herencia** (`EstacionInteractiva` con virtual/override, y `protected Avisar()`). Están explicados en comentarios.
- `FindFirstObjectByType<T>()` se usa como "versión nueva de FindObjectOfType", siempre en Start o de forma perezosa (lazy), nunca en Update.
- **Comentarios didácticos en cada bloque**, en español y sin `/// <summary>`. Cuando se usa algo que no vieron en clase, explicarlo en el comentario.
- Mensajes para el jugador: `Avisar("...")` en las estaciones o `hud.MostrarMensaje(...)`. Los `Debug.Log` quedan para lo interno.

## Scripts (Assets/Scripts)

| Script | Qué hace |
|---|---|
| PlayerMovement | Mueve con Rigidbody2D.MovePosition en FixedUpdate. |
| PlayerInventory | `enum TipoObjeto` (**solo se agregan valores al FINAL**: se serializa como int). Objeto en la mano y globo encima del barista. |
| PlayerInteraction | E/R: busca la estación más cercana (`BuscarIndiceCercano`, y `BuscarEstacionCercana` que la usa). Cachea `FindObjectsByType<EstacionInteractiva>(FindObjectsInactive.Include, …)` y un array paralelo `dibujos[]` (SpriteRenderer) en Start; saltea las inactivas. **Resalta** (tiñe con `colorResaltado`) la estación que se usaría con la E. No hace nada si `Time.timeScale == 0` (pausa o fin). |
| EstacionInteractiva | Clase base con `virtual Interactuar(inventario)`, `protected Avisar(mensaje, esError = true)` (manda el mensaje al HUD y hace sonar el error) y `protected ObtenerSonidos()` (búsqueda perezosa). |
| MoledoraCafe / Cafetera / Chocolatera | Máquinas con temporizador en Update. El globo muestra el ingrediente medio transparente mientras trabaja y el resultado al terminar. Consultas para el tutorial: `EstaMoliendo()`/`TieneCafeListo()`, `ObtenerProductoEnPreparacion()`/`ObtenerProductoListo()`, `EstaPreparando()`/`TieneChocolateListo()`. |
| Repisa | Arrays `posicionesEspacios` (Inspector), `espacios[]` y `globos[]`. Primero combina; si no, apoya en un espacio libre. Con manos vacías agarra el espacio ocupado más cercano. `Tiene(objeto)`. |
| Bandeja | Arma el pedido (bebida + galletita) y entrega con E y manos vacías. R retira productos. `ObtenerBebida()`. |
| Cliente | Pedido, globo de pedido, barra de paciencia (Slider), pausa al salir. La paciencia del pedido = `pacienciaMaxima` + `extraConLeche` + `extraGalletita`; la barra se pinta verde/amarilla/roja (`PintarBarra`). `Entrar()`, `RecibirPedido()`, `IrseEnojado()`, `Retirarse()`, `TerminoDeSalir()`, `ObtenerBandeja()`, `ObtenerPorcentajePaciencia()`. |
| MovimientoCliente | Camina entre `puntoEntrada` y `puntoEspera`. Llama a `cliente.MostrarPedido()` al llegar y a `cliente.TerminoDeSalir()` al salir. |
| GameManager | Tutorial (switch sobre pedidosCompletados), pedidos al azar (arrays), tercer cliente, **propinas** (`propinaBase` + `propinaExtraMaxima` × paciencia restante), pista de galletitas la primera vez, victoria/derrota con resumen (`textoResumenVictoria/Derrota`), sonidos, `EstaTerminadaPartida()`, `ReiniciarPartida()` y `IrAlMenu()`. |
| EstacionIngredientes / EstacionGalletitas | Teclas Z/X/C por distancia al jugador (no heredan de EstacionInteractiva). Se resaltan cuando el barista está cerca. No hacen nada en pausa. |
| CatalogoObjetos | **Único lugar con los sprites de cada objeto** (switch). Crea los globos con Instantiate (`CrearGlobo`, `CrearGloboPedido`). |
| GloboObjeto / GloboPedido | Globos que siguen a su objetivo en LateUpdate (no son hijos, para no heredar escalas raras). |
| HUD | Barra de madera arriba estilo Papa's: taza + "3 / 15" + barra de progreso (`Image` Filled) a la izquierda; moneda + propinas (`textoPropinas`, `ActualizarPropinas`) y 3 corazones a la derecha. Abajo, cartel de mensajes temporales; `MostrarMensaje(texto, segundos = -1, esperar = false)` puede dejar un mensaje esperando a que termine el anterior. |
| GuiaTutorial | En el root del prefab HUD. Tutorial **de a un paso**: en el centro de la barra muestra "Tu cliente quiere…" + UNA instrucción, y una flecha (prefab `Flecha`, Instantiate en Start) rebota sobre la estación. En cada Update elige el paso con if/else **desde el final de la receta** (bandeja → en la mano → listo en máquina → trabajando → ingrediente), así se acomoda sola si el jugador se equivoca. GameManager llama `EmpezarPedido(cliente, bebida, n, total)`, `PedidoEntregado()` y `Terminar()`. Reemplazó a PanelTutorial (borrado: Helena dijo que era demasiada info y letra chica). |
| Tacho | E tira lo que tenés en la mano (suena Tirar). |
| MenuPausa | En el root del HUD. Esc abre/cierra `PanelPausa` (Canvas propio con Override Sorting 20) con Continuar / Reiniciar / Menú; usa `Time.timeScale`. No pausa si la partida terminó. |
| Sonidos | En el root del HUD, con un AudioSource. `PlayOneShot` de 9 efectos (Agarrar, Apoyar, MaquinaLista, Entregar, Error, Tirar, ClienteEnojado, Victoria, Derrota) en `Assets/Audio/*.wav`, generados con `Herramientas/ArteProvisorio/sonidos.py`. |
| MenuPrincipal | Escena Menu: paneles Principal / Cómo jugar / Créditos con SetActive, `CargarEscena(string)` y `Salir()`. Cómo jugar tiene 3 páginas en `GameObject[] paginasComoJugar` (Controles, Máquinas y recetas con íconos, Pedidos y reglas) con `SiguientePagina()`/`AnteriorPagina()` (dan la vuelta) y `textoPagina` "1 / 3". |

## Escenas, prefabs y assets

- `Scenes/Menu.unity` (la UI la genera `Herramientas/ArteProvisorio/menu.py`: conserva cámara, EventSystem, Canvas y el objeto Menu, y rehace todo lo demás; fondo = render de la cafetería `Sprites/Menu/FondoMenu.png` hecho con `vista.py ... sinhud`) y `Scenes/Cafeteria.unity` (el juego).
- Prefabs: `CatalogoObjetos`, `HUD` y `Tacho` (los tres en la escena Cafeteria), `Globo`, `GloboPedido`, `Flecha` (la crea GuiaTutorial).
- `Sprites/Objetos/*.png`: íconos pixel 32×32 a 64 PPU (Point), más `Globo.png` y `GloboDoble.png`; el Tacho a 32 PPU. Los originales están en `Herramientas/ArteProvisorio/iconos_originales/` (se recuperaron después de que la versión vectorial los pisara).
- **Arte provisorio: PIXEL ART** (32 px por unidad, filtro Point). Helena probó el estilo vectorial tipo Club Penguin (`Herramientas/ArteCP`, ya no se usa) y prefirió volver al pixel. Se genera con `Herramientas/ArteProvisorio`: `arte.py` (piezas: personajes, despensas, bandeja, HUD), `arte60.py` (Fondo = el cuarto entero en una imagen, Mostrador, aparatos, `*_fila` = máquinas y repisa con su mueble) y `python exportar_pixel.py` (exporta todo con .meta, pivotes y guid; `exportar_pixel.py vista` arma `vista_60.png` sin Unity). El HUD se arma con `hud.py` (plantilla `plantilla_HUD.prefab`) y el menú con `menu.py`.
  Los guid están en `Herramientas/ArteProvisorio/guids.json`. Generadores vigentes: `exportar_pixel.py`, `escena5.py` (layout), `hud.py` (prefab HUD: barra, guía, propinas, pausa, sonidos; plantilla `plantilla_HUD.prefab`), `paneles_fin.py` (paneles de Victoria/Derrota y estilo de las barras de paciencia), `menu.py` y `sonidos.py`. Los demás (`exportar.py`, `escena*.py` viejos) son de versiones anteriores.
  **Para el pixel art definitivo**, Helena reemplaza los PNG con el mismo nombre (y mismo tamaño en unidades).
- **Encuadre de Cafeteria (layout v5, 60 % cocina / 40 % café, vista cenital a 3/4)**: cámara ortográfica 5 centrada en (0, 0) (ve x ±8.89, y −5…5). La barra del HUD (108 px) tapa y 4…5. **Las máquinas van en la fila de mesada de abajo, NUNCA sobre la barra** (Helena lo pidió así; la v4 las había puesto en la barra y no le gustó). Ojo con líneas YAML duplicadas como `orthographic size: orthographic size: 5`: Unity las ignora.
  Café (y 0.4…4): todo dibujado en `Escenario/Fondo` (pared con puerta al centro, ventana, pizarra, cuadros, lámparas; sillones, mesitas con puffs, plantas). Clientes en (−5/0/5, 0.75) detrás de la barra; EntradaClientes en la puerta (0, 3). Barra: sprite 569×51 px en y 0.197 (la tapa empieza en y 0.4), collider 17.78×1.0 con offset −0.297; arriba solo las bandejas (−5/0/5, 0.2), más la caja registradora y el frasco dibujados. Cocina (y −0.6…−5): fila de abajo con **pivote arriba en y −2.5** (sprites de 2.5 de alto): Moledora (−4.75), RepisaIzquierda (−2.35), Cafetera (0), RepisaDerecha (2.35), Chocolatera (4.75); repisas con espacios en (±0.41, −0.85). Despensas subidas, al lado de la barra: Ingredientes (−7.75, −1.9) y Galletitas (7.75, −1.9). Tacho en (6.1, −1.3). Plantas de adorno en las esquinas de abajo. Se arma con `Herramientas/ArteProvisorio/escena5.py`.
  Todos los objetos a escala 1: el tamaño lo da el sprite y el BoxCollider2D tiene el tamaño a mano. Barista y clientes 24×34 px; barista con collider en los pies (0.6×0.4, offset −0.33). Orden de dibujo: fondo −10, clientes 0, estaciones de la cocina 3, barra 5, bandejas y máquinas 6, barista 7, flecha 30, globos 100.
  Alcances: PlayerInteraction 1.5, Ingredientes/Galletitas 1.8. Globos: clientes 1.0, barista 1.0, máquinas −0.3 (desde el borde de arriba). Flecha del tutorial (GuiaTutorial.UbicarFlecha): bandejas desde abajo de la barra (y −0.85) apuntando arriba; Ingredientes desde su derecha; tacho desde su izquierda; máquinas y repisas desde arriba.
- En Cafeteria, cada `BarraPaciencia1/2/3` es **hija** de `TextoPedido1/2/3` (Canvas). Los TextoPedido ya no se usan (tienen el componente TMP desactivado), pero **no hay que borrarlos**, porque se borran las barras.
  El Canvas de la escena está en Scale With Screen Size 1920×1080; los TextoPedido están en x = ±540 / 0, y = −272 (las barras quedan sobre el globo de cada cliente).
  Si se pierden las referencias, hay que reasignar Cliente → Barra Paciencia en el Inspector.
- Cada Cliente tiene asignada su `bandejaDelCliente` y su `barraPaciencia`.

## Cómo probar rápido (puente de pruebas)

**No manejar Unity con capturas de pantalla: es lentísimo.** Usar el puente:
- `Packages/com.claude.puente` (script de editor, NO es parte del parcial; se borra antes de entregar junto con la carpeta `PuenteClaude/`).
- Desde la raíz: `python Herramientas/puente.py "jugar" "velocidad 3" "tp -6.45 0.45" "tecla Z" "estado" "captura nombre" "parar"`.
  Funciona con Unity en segundo plano. Cada pedido tarda segundos; el resultado incluye los Debug.Log y los errores.
- Comandos: `refrescar` (AssetDatabase.Refresh + espera la compilación), `jugar`, `parar`, `esperar s`, `velocidad x` (timeScale),
  `captura nombre` (PNG 1920×1080 en `PuenteClaude/capturas/`; en Play incluye el HUD, fuera de Play solo la cámara),
  `tp x y` (mueve al barista), `tecla E|R|Z|X|C` (llama a las mismas funciones que el juego), `estado` (barista, mano, pedidos, guía, HUD, clientes),
  `escena ruta|vacia` (no cambia si hay cambios sin guardar), `llamar Tipo Metodo args`, `leer Tipo campo`.
- Antes de editar una escena por fuera: `escena vacia` → editar el YAML → `refrescar` → `escena Assets/Scenes/X.unity` (así no aparece el cartel de "Reload").
- Para chequear compilación sin Unity: `dotnet build` de un csproj armado desde `Assembly-CSharp.csproj` (rutas absolutas + `Assets/Scripts/*.cs`).
- Posiciones útiles para `tp` (layout v5): Ingredientes (−6.3, −1.6), Galletitas (6.3, −2.2), fila de abajo (x, −1.95) con x = −4.75/−2.35/0/2.35/4.75, Bandejas (−5/0/5, −0.6), Tacho (5.2, −1.3).
- `captura nombre sinui` saca la foto sin la interfaz (así se hizo `Sprites/Menu/FondoMenu.png`).

## Convenciones técnicas al tocar archivos

- Los `.cs` van en **UTF-8 sin BOM con CRLF**. Los `.meta`, `.prefab` y `.unity` van en LF.
- **No renombrar campos `[SerializeField]`**, porque se pierden las asignaciones de la escena.
- Si se crean assets a mano: escribir el `.meta` **antes** que el archivo y mantener los guid existentes.
  Para referenciar un sprite Single se usa `{fileID: 21300000, guid: …, type: 3}`.
- Unity suele estar abierto. **No editar `Cafeteria.unity` por fuera** si Helena puede tener cambios sin guardar: mejor darle pasos en el Editor. Para chequear la escena alcanza con leer el YAML.
- Antes de pisar un script, comparar con la versión en disco por si Helena lo editó.

## Estado al 16/10/2026

Hecho y probado por Helena: refactorización completa, flujo del tutorial, clientes que se van enojados, globos (barista, máquinas, repisas, pedidos), galletita de chocolate, menú.

HUD y Tacho ya están en la escena (Helena los agregó y guardó).

Recién agregado (15/10, segunda y tercera parte), **probado por Claude en Play pero no por Helena**:
- Barra superior estilo Papa's, tutorial de a un paso con flecha, cafetería en pixel art con layout 60/40 (v5), menú con Cómo jugar ilustrado. Además (16/10): propinas, paciencia según el pedido con barra de colores, resaltado de la estación cercana, pausa con Esc, pantallas de Victoria/Derrota con resumen y botón Menú, pista de galletitas y sonidos. Capturas en `Capturas/v6/`.
- Recorrido probado con el puente: tutorial completo, propinas, pausa, pista de galletitas, barras de paciencia, Victoria/Derrota forzadas con `llamar GameManager GanarPartida/PerderPartida`, menú. En la escena alguien puso pedidosParaGanar 15, tercer puesto a los 12 y paciencias base de 35–40 s (se respetó). Las capturas del puente muestran el fondo oscuro de los paneles más claro que en el juego (efecto de la captura).
- La escena se editó por fuera con Unity abierto (sin cambios pendientes) y se recargó con "Reload".

## Pendientes / ideas

1. Que Helena juegue una partida entera con teclado y ajuste números en el Inspector (paciencias, extras, propinas, pedidos para ganar).
2. Animator para barista y clientes: Helena bajó personajes LPC (`HELENA/lpc_female_*.zip`, con animaciones); hay que revisar la licencia (CC-BY-SA/GPL: va en Créditos).
3. Música de fondo suave (con un AudioSource en loop) y un control de volumen en la pausa.
4. Dificultad que sube: menos paciencia o más clientes a medida que avanza; o "días" como en Papa's.
5. Feedback visual: texto flotante "+$7" sobre el cliente, carita feliz/enojada, partículas (no se vieron en clase).
6. **Machete para la defensa**: qué hace cada script y cómo explicar enum, herencia/protected, arrays paralelos, temporizadores con deltaTime, timeScale, AudioSource, la guía del tutorial.
7. Antes de entregar: borrar `Packages/com.claude.puente` y la carpeta `PuenteClaude/` (herramienta de pruebas de Claude).
