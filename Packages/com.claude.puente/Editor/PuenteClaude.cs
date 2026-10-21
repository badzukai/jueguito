// =====================================================================
//  PUENTE DE PRUEBAS PARA CLAUDE  (NO ES PARTE DEL PARCIAL)
// =====================================================================
// Herramienta del editor para que Claude pueda probar el juego rápido,
// sin manejar el mouse ni sacar fotos de la pantalla.
//
// Claude escribe una lista de comandos en PuenteClaude/comando.txt
// (carpeta al lado de Assets) y este script los ejecuta uno por uno:
// refrescar, dar Play, sacar capturas a PNG, mover al barista,
// "apretar" teclas llamando a las mismas funciones del juego, etc.
// El resultado (y lo que salga en la Console) queda en resultado.txt.
//
// Como es solo del editor, no entra en el juego compilado.
// Para sacarlo antes de entregar: borrar la carpeta Packages/com.claude.puente
// y la carpeta PuenteClaude.
// =====================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PuenteClaude
{
    static readonly string Carpeta = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PuenteClaude"));
    static string ArchivoComando => Path.Combine(Carpeta, "comando.txt");
    static string ArchivoPendiente => Path.Combine(Carpeta, "pendiente.txt");
    static string ArchivoParcial => Path.Combine(Carpeta, "parcial.txt");
    static string ArchivoResultado => Path.Combine(Carpeta, "resultado.txt");
    static string CarpetaCapturas => Path.Combine(Carpeta, "capturas");

    const BindingFlags Todo = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    static int framesEnPlay;

    static PuenteClaude()
    {
        Directory.CreateDirectory(Carpeta);
        Directory.CreateDirectory(CarpetaCapturas);
        EditorApplication.update += Tick;
        Application.logMessageReceived += AlLog;
        CompilationPipeline.assemblyCompilationFinished += AlCompilar;
    }

    // ------------------------------------------------------------ ciclo
    static void Tick()
    {
        try
        {
            if (!File.Exists(ArchivoPendiente))
            {
                if (!File.Exists(ArchivoComando)) return;
                Iniciar();
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            List<string> lineas = File.ReadAllLines(ArchivoPendiente).ToList();
            if (lineas.Count <= 1)
            {
                Terminar(lineas.Count > 0 ? lineas[0] : "id ?");
                return;
            }

            string cmd = lineas[1].Trim();
            bool listo;
            string nuevo = null;
            if (cmd.Length == 0 || cmd.StartsWith("#"))
            {
                listo = true;
            }
            else
            {
                try { listo = Ejecutar(cmd, out nuevo); }
                catch (Exception e)
                {
                    Escribir("ERROR en '" + cmd + "': " + (e.InnerException ?? e).Message);
                    listo = true;
                }
            }

            if (listo) lineas.RemoveAt(1);
            else if (nuevo != null) lineas[1] = nuevo;
            File.WriteAllLines(ArchivoPendiente, lineas);

            if (EditorApplication.isPlaying)
            {
                // Que el juego no se frene aunque Unity esté en segundo plano
                // (solo dura lo que dura el Play; no cambia la configuración del proyecto).
                Application.runInBackground = true;
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
        catch (IOException)
        {
            // El archivo estaba en uso: lo intentamos en el próximo update.
        }
    }

    static void Iniciar()
    {
        string[] lineas = File.ReadAllLines(ArchivoComando);
        File.Delete(ArchivoComando);
        if (File.Exists(ArchivoResultado)) File.Delete(ArchivoResultado);
        List<string> l = lineas.ToList();
        if (l.Count == 0 || !l[0].StartsWith("id ")) l.Insert(0, "id 0");
        File.WriteAllText(ArchivoParcial, l[0] + "\n");
        File.WriteAllLines(ArchivoPendiente, l);
    }

    static void Terminar(string id)
    {
        string texto = File.Exists(ArchivoParcial) ? File.ReadAllText(ArchivoParcial) : id + "\n";
        texto += "FIN " + id.Replace("id ", "") + "\n";
        string tmp = ArchivoResultado + ".tmp";
        File.WriteAllText(tmp, texto);
        if (File.Exists(ArchivoResultado)) File.Delete(ArchivoResultado);
        File.Move(tmp, ArchivoResultado);
        File.Delete(ArchivoPendiente);
        if (File.Exists(ArchivoParcial)) File.Delete(ArchivoParcial);
    }

    static void Escribir(string s)
    {
        File.AppendAllText(ArchivoParcial, s + "\n");
    }

    static double T() { return EditorApplication.timeSinceStartup; }
    static double D(string s) { return double.Parse(s, Inv); }
    static float F(string s) { return float.Parse(s, Inv); }

    // ------------------------------------------------------- comandos
    static bool Ejecutar(string cmd, out string nuevo)
    {
        nuevo = null;
        string[] p = cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string op = p[0].ToLowerInvariant();

        switch (op)
        {
            case "refrescar":
                AssetDatabase.Refresh();
                nuevo = "refrescando " + T().ToString(Inv);
                return false;

            case "refrescando":
                if (T() < D(p[1]) + 1.0) return false;
                Escribir(EditorUtility.scriptCompilationFailed ? "refrescar: HAY ERRORES DE COMPILACION" : "refrescar: ok");
                return true;

            case "jugar":
                if (EditorApplication.isPlaying) { Escribir("jugar: ya estaba en Play"); return true; }
                framesEnPlay = 0;
                EditorApplication.isPlaying = true;
                nuevo = "jugando " + T().ToString(Inv);
                return false;

            case "jugando":
                if (!EditorApplication.isPlaying)
                {
                    if (T() > D(p[1]) + 60) { Escribir("jugar: no entró en Play"); return true; }
                    return false;
                }
                framesEnPlay++;
                if (framesEnPlay < 15) return false;
                Escribir("jugar: en Play (" + SceneManager.GetActiveScene().name + ")");
                return true;

            case "parar":
                EditorApplication.isPlaying = false;
                nuevo = "parando";
                return false;

            case "parando":
                if (EditorApplication.isPlaying) return false;
                Escribir("parar: ok");
                return true;

            case "esperar":
                nuevo = "esperar_hasta " + (T() + D(p[1])).ToString(Inv);
                return false;

            case "esperar_hasta":
                return T() >= D(p[1]);

            case "velocidad":
                Time.timeScale = F(p[1]);
                Escribir("velocidad: " + p[1]);
                return true;

            case "captura":
            {
                string ruta = Path.Combine(CarpetaCapturas, p[1] + ".png");
                if (File.Exists(ruta)) File.Delete(ruta);
                // Dibujamos la cámara a mano (con la UI): no depende de que la ventana Game se vea.
                // "captura nombre sinui" saca la foto sin la interfaz.
                CapturarCamara(ruta, !(p.Length > 2 && p[2] == "sinui"));
                nuevo = "captura_esperando " + p[1] + " " + (T() + 15).ToString(Inv);
                return false;
            }

            case "captura_esperando":
            {
                string ruta = Path.Combine(CarpetaCapturas, p[1] + ".png");
                if (File.Exists(ruta) && new FileInfo(ruta).Length > 0)
                {
                    Escribir("captura: " + ruta);
                    return true;
                }
                if (T() > D(p[2])) { Escribir("captura: FALLÓ (" + p[1] + ")"); return true; }
                if (EditorApplication.isPlaying) EditorApplication.QueuePlayerLoopUpdate();
                return false;
            }

            case "tp":
                Teletransportar(F(p[1]), F(p[2]));
                return true;

            case "tecla":
                Tecla(p[1].ToUpperInvariant());
                return true;

            case "estado":
                Estado();
                return true;

            case "escena":
                AbrirEscena(p.Length > 1 ? p[1] : null);
                return true;

            case "llamar":
                Llamar(p);
                return true;

            case "leer":
                Leer(p[1], p[2]);
                return true;

            default:
                Escribir("comando desconocido: " + cmd);
                return true;
        }
    }

    // ----------------------------------------------------- utilidades
    static Type TipoJuego(string nombre)
    {
        foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = a.GetType(nombre);
            if (t != null) return t;
        }
        throw new Exception("No encontré el tipo " + nombre);
    }

    static UnityEngine.Object[] Buscar(string tipo)
    {
        return UnityEngine.Object.FindObjectsByType(TipoJuego(tipo), FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    static UnityEngine.Object Primero(string tipo)
    {
        return UnityEngine.Object.FindFirstObjectByType(TipoJuego(tipo));
    }

    static object Campo(object obj, string nombre)
    {
        if (obj == null) return null;
        Type t = obj.GetType();
        while (t != null)
        {
            FieldInfo f = t.GetField(nombre, Todo);
            if (f != null) return f.GetValue(obj);
            PropertyInfo pr = t.GetProperty(nombre, Todo);
            if (pr != null) return pr.GetValue(obj);
            t = t.BaseType;
        }
        return "(no existe " + nombre + ")";
    }

    static object Metodo(object obj, string nombre, params object[] args)
    {
        Type t = obj.GetType();
        while (t != null)
        {
            foreach (MethodInfo m in t.GetMethods(Todo))
            {
                if (m.Name == nombre && m.GetParameters().Length == args.Length)
                {
                    return m.Invoke(obj, args);
                }
            }
            t = t.BaseType;
        }
        throw new Exception("No encontré el método " + nombre + " en " + obj.GetType().Name);
    }

    static string TextoTMP(object tmp)
    {
        if (tmp == null) return "(null)";
        UnityEngine.Object u = tmp as UnityEngine.Object;
        if (u == null) return "(null)";
        Component c = tmp as Component;
        string activo = c != null && !c.gameObject.activeInHierarchy ? " [oculto]" : "";
        return Campo(tmp, "text") + activo;
    }

    static string EnLaMano()
    {
        UnityEngine.Object inv = Primero("PlayerInventory");
        return inv == null ? "?" : Metodo(inv, "GetCurrentItem").ToString();
    }

    // -------------------------------------------------------- acciones
    static void Teletransportar(float x, float y)
    {
        Component jugador = Primero("PlayerMovement") as Component;
        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = new Vector2(x, y);
        jugador.transform.position = new Vector3(x, y, jugador.transform.position.z);
        Escribir("tp: barista en (" + x.ToString(Inv) + ", " + y.ToString(Inv) + ")");
    }

    static void Tecla(string k)
    {
        Type tipoObjeto = TipoJuego("PlayerInventory").GetNestedType("ItemType");
        UnityEngine.Object inv = Primero("PlayerInventory");

        if (k == "E" || k == "R")
        {
            UnityEngine.Object pi = Primero("PlayerInteraction");
            object est = Metodo(pi, "FindNearestStation", k == "R");
            if (est == null)
            {
                Escribir("tecla " + k + ": no hay ninguna estación cerca");
                return;
            }
            Component ce = (Component)est;
            if (k == "E")
            {
                Metodo(est, "Interact", inv);
            }
            else
            {
                Component bandeja = ce.GetComponent(TipoJuego("Tray"));
                Metodo(bandeja, "RemoveProduct", inv);
            }
            Escribir("tecla " + k + " -> " + ce.name + " | en la mano: " + EnLaMano());
            return;
        }

        string ingrediente = k == "Z" ? "CoffeeBeans" : (k == "X" ? "Milk" : "Chocolate");
        string galletita = k == "Z" ? "RedVelvetCookie" : (k == "X" ? "VanillaChocolateCookie" : "ChocolateCookie");
        foreach (UnityEngine.Object e in Buscar("IngredientStation"))
        {
            Metodo(e, "GiveIngredient", Enum.Parse(tipoObjeto, ingrediente));
        }
        foreach (UnityEngine.Object e in Buscar("CookieStation"))
        {
            Metodo(e, "GiveCookie", Enum.Parse(tipoObjeto, galletita));
        }
        Escribir("tecla " + k + " | en la mano: " + EnLaMano());
    }

    static void Estado()
    {
        if (!EditorApplication.isPlaying)
        {
            Escribir("estado: (no está en Play) escena " + SceneManager.GetActiveScene().path);
            return;
        }
        Component jugador = Primero("PlayerMovement") as Component;
        if (jugador != null)
        {
            Vector3 pos = jugador.transform.position;
            Escribir("estado: barista (" + pos.x.ToString("0.00", Inv) + ", " + pos.y.ToString("0.00", Inv) + ") | en la mano: " + EnLaMano());
        }
        UnityEngine.Object gm = Primero("GameManager");
        if (gm != null)
        {
            Escribir("  pedidos " + Campo(gm, "completedOrders") + " | perdidos " + Campo(gm, "lostCustomers") +
                     " | tutorial terminado " + Campo(gm, "tutorialFinished") + " | timeScale " + Time.timeScale);
        }
        UnityEngine.Object guia = Primero("TutorialGuide");
        if (guia != null)
        {
            Escribir("  guía: " + TextoTMP(Campo(guia, "titleText")) + " || " + TextoTMP(Campo(guia, "instructionText")));
        }
        UnityEngine.Object hud = Primero("HUD");
        if (hud != null)
        {
            Escribir("  HUD: " + TextoTMP(Campo(hud, "ordersText")) + " | mensaje: " + TextoTMP(Campo(hud, "messageText")));
        }
        foreach (UnityEngine.Object c in Buscar("Customer"))
        {
            Component cc = (Component)c;
            if (!cc.gameObject.activeInHierarchy) { Escribir("  " + cc.name + ": oculto"); continue; }
            Escribir("  " + cc.name + ": en (" + cc.transform.position.x.ToString("0.0", Inv) + ", " + cc.transform.position.y.ToString("0.0", Inv) +
                     ") | puede recibir " + Metodo(c, "CanReceiveOrder"));
        }
    }

    static void AbrirEscena(string ruta)
    {
        if (EditorApplication.isPlaying) { Escribir("escena: primero hay que parar el Play"); return; }
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isDirty)
            {
                Escribir("escena: HAY CAMBIOS SIN GUARDAR en " + SceneManager.GetSceneAt(i).name + ", no cambio de escena");
                return;
            }
        }
        if (string.IsNullOrEmpty(ruta) || ruta == "vacia")
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Escribir("escena: vacía");
        }
        else
        {
            EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            Escribir("escena: " + ruta);
        }
    }

    // llamar Tipo Metodo [args...] -> lo llama en todos los objetos de ese tipo
    static void Llamar(string[] p)
    {
        foreach (UnityEngine.Object o in Buscar(p[1]))
        {
            foreach (MethodInfo m in o.GetType().GetMethods(Todo))
            {
                ParameterInfo[] ps = m.GetParameters();
                if (m.Name != p[2] || ps.Length != p.Length - 3) continue;
                object[] args = new object[ps.Length];
                for (int i = 0; i < ps.Length; i++)
                {
                    Type tp = ps[i].ParameterType;
                    string s = p[3 + i];
                    if (tp.IsEnum) args[i] = Enum.Parse(tp, s);
                    else if (tp == typeof(int)) args[i] = int.Parse(s, Inv);
                    else if (tp == typeof(float)) args[i] = float.Parse(s, Inv);
                    else if (tp == typeof(bool)) args[i] = bool.Parse(s);
                    else args[i] = s;
                }
                object r = m.Invoke(o, args);
                Escribir("llamar " + o.name + "." + p[2] + " -> " + (r ?? "ok"));
                break;
            }
        }
    }

    static void Leer(string tipo, string campo)
    {
        foreach (UnityEngine.Object o in Buscar(tipo))
        {
            Escribir("leer " + o.name + "." + campo + " = " + Campo(o, campo));
        }
    }

    // Dibuja la cámara principal en una textura de 1920x1080 y la guarda como PNG.
    // Los Canvas "Overlay" no los dibuja ninguna cámara, así que durante la captura
    // los pasamos a "Screen Space - Camera" con esta cámara y después los dejamos como estaban.
    static void CapturarCamara(string ruta, bool conUI = true)
    {
        Camera cam = Camera.main;
        if (cam == null) { Escribir("captura: no hay Main Camera"); return; }
        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        RenderTexture anterior = cam.targetTexture;
        cam.targetTexture = rt;

        Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        List<Canvas> cambiados = new List<Canvas>();
        List<Canvas> subidos = new List<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (conUI && c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay && c.gameObject.activeInHierarchy)
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = cam.nearClipPlane + 0.5f;
                cambiados.Add(c);
            }
        }
        // En "Camera" la UI se ordena junto con los sprites; en Overlay va siempre arriba.
        // Para que la foto sea fiel, subimos el orden de todos los Canvas mientras dibujamos.
        foreach (Canvas c in canvases)
        {
            if (conUI && (c.isRootCanvas || c.overrideSorting))
            {
                c.sortingOrder += 1000;
                subidos.Add(c);
            }
        }
        Canvas.ForceUpdateCanvases();
        cam.Render();
        foreach (Canvas c in subidos)
        {
            c.sortingOrder -= 1000;
        }
        foreach (Canvas c in cambiados)
        {
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.worldCamera = null;
        }
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();
        cam.targetTexture = anterior;
        RenderTexture.active = null;
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        rt.Release();
        UnityEngine.Object.DestroyImmediate(rt);
    }

    // ------------------------------------------------ logs y compilación
    static void AlLog(string mensaje, string pila, LogType tipo)
    {
        if (!File.Exists(ArchivoPendiente)) return;
        try
        {
            string linea = "[" + tipo + "] " + mensaje.Split('\n')[0];
            if (tipo == LogType.Error || tipo == LogType.Exception || tipo == LogType.Assert)
            {
                string primera = (pila ?? "").Split('\n').FirstOrDefault(l => l.Contains("Assets/") || l.Contains("Packages/"));
                if (!string.IsNullOrEmpty(primera)) linea += "  @ " + primera.Trim();
            }
            File.AppendAllText(ArchivoParcial, linea + "\n");
        }
        catch (IOException) { }
    }

    static void AlCompilar(string ensamblado, CompilerMessage[] mensajes)
    {
        List<string> errores = new List<string>();
        foreach (CompilerMessage m in mensajes)
        {
            if (m.type == CompilerMessageType.Error) errores.Add("ERROR DE COMPILACION: " + m.message);
        }
        File.WriteAllLines(Path.Combine(Carpeta, "compilacion.txt"), errores);
        if (errores.Count > 0 && File.Exists(ArchivoPendiente))
        {
            File.AppendAllText(ArchivoParcial, string.Join("\n", errores) + "\n");
        }
    }
}
