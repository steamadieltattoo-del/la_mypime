using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de la tienda del local (tablet). Copia la mecánica de
/// Supermarket Simulator: pestaña de MUEBLES para comprar estantes, góndolas
/// y neveras, y pestaña de AMPLIACIONES para abrir nuevas alas del local.
/// No usa EventSystem: detecta toques y clics igual que el resto del juego.
/// </summary>
public class StoreUI : MonoBehaviour
{
    public static StoreUI Instance { get; private set; }
    public static bool Abierto => Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    /// <summary>Evita que la E que cierra la tienda vuelva a interactuar en el mismo frame.</summary>
    public static bool CerroAhora { get; private set; }

    enum Tab { Muebles, Ampliaciones }

    class FilaMueble
    {
        public TipoMueble tipo;
        public Text info;
        public RectTransform boton;
        public Text botonTexto;
    }

    class FilaAmpliacion
    {
        public ExpansionZone zona;
        public Text info;
        public RectTransform boton;
        public Text botonTexto;
    }

    readonly List<FilaMueble> filasMueble = new List<FilaMueble>();
    readonly List<FilaAmpliacion> filasAmpliacion = new List<FilaAmpliacion>();
    readonly List<RectTransform> botonesProducto = new List<RectTransform>();
    readonly List<ProductData> productos = new List<ProductData>();

    GameObject panel;
    GameObject contenidoMuebles;
    GameObject contenidoAmpliaciones;
    RectTransform tabMuebles;
    RectTransform tabAmpliaciones;
    Text dineroTexto;
    Text productoTexto;
    Text infoMuebles;
    Text huecoTexto;
    Text infoAmpliaciones;
    RectTransform botonPedidos;
    RectTransform botonCerrar;
    Font font;

    Tab tab = Tab.Muebles;
    BuildSpot spotObjetivo;
    ProductData productoSeleccionado;
    bool ignorarInputUnFrame;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        CerroAhora = false;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (panel == null) BuildUI();
        if (panel != null) panel.SetActive(false);
    }

    // ------------------------------------------------------------- abrir

    public void OpenMuebles(BuildSpot spot)
    {
        if (panel == null) BuildUI();
        spotObjetivo = spot;
        tab = Tab.Muebles;
        panel.SetActive(true);
        RebuildProductos();
        Refresh();
        ignorarInputUnFrame = true;
    }

    public void OpenAmpliaciones(int foco)
    {
        if (panel == null) BuildUI();
        spotObjetivo = null;
        tab = Tab.Ampliaciones;
        panel.SetActive(true);
        Refresh();
        if (foco >= 0 && foco < filasAmpliacion.Count && filasAmpliacion[foco].zona != null)
            Aviso(filasAmpliacion[foco].zona.nombre + ": " + filasAmpliacion[foco].zona.descripcion);
        ignorarInputUnFrame = true;
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        CerroAhora = true;
    }

    public void Aviso(string texto)
    {
        if (texto == null) return;
        if (tab == Tab.Muebles && infoMuebles != null) infoMuebles.text = texto;
        else if (infoAmpliaciones != null) infoAmpliaciones.text = texto;
    }

    // ------------------------------------------------------------- update

    void Update()
    {
        if (!Abierto) return;
        if (ignorarInputUnFrame) { ignorarInputUnFrame = false; return; }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            Close();
            return;
        }

        if (Pulsado(botonCerrar)) { Close(); return; }

        if (Pulsado(tabMuebles) && tab != Tab.Muebles) { tab = Tab.Muebles; Refresh(); return; }
        if (Pulsado(tabAmpliaciones) && tab != Tab.Ampliaciones) { tab = Tab.Ampliaciones; Refresh(); return; }

        if (tab == Tab.Muebles)
        {
            for (int i = 0; i < botonesProducto.Count && i < productos.Count; i++)
            {
                if (!Pulsado(botonesProducto[i])) continue;
                productoSeleccionado = productos[i];
                Refresh();
                return;
            }

            foreach (FilaMueble fila in filasMueble)
            {
                if (Pulsado(fila.boton))
                {
                    ComprarMueble(fila.tipo);
                    return;
                }
            }

            if (Pulsado(botonPedidos))
            {
                OrderCatalog catalogo = GameManager.Instance != null ? GameManager.Instance.catalogoPedidos : null;
                Close();
                if (catalogo != null && OrderComputerUI.Instance != null)
                    OrderComputerUI.Instance.Open(catalogo);
                return;
            }
        }
        else
        {
            foreach (FilaAmpliacion fila in filasAmpliacion)
            {
                if (Pulsado(fila.boton))
                {
                    ComprarAmpliacion(fila.zona);
                    return;
                }
            }
        }
    }

    // ------------------------------------------------------------- compras

    void ComprarMueble(TipoMueble tipo)
    {
        StoreManager tienda = StoreManager.Instance;
        if (tienda == null) return;
        if (productoSeleccionado == null)
        {
            Aviso("Elige primero el producto que venderá el mueble.");
            return;
        }

        BuildSpot spot = spotObjetivo;
        if (spot == null || spot.ocupado) spot = tienda.BuscarSpotLibre();
        if (spot == null)
        {
            Aviso("No hay huecos libres. Compra una AMPLIACIÓN para ganar espacio.");
            return;
        }

        if (tienda.ComprarMueble(spot, tipo, productoSeleccionado))
        {
            spotObjetivo = null;
            productoSeleccionado = null;
            RebuildProductos();
            Refresh();
        }
    }

    void ComprarAmpliacion(ExpansionZone zona)
    {
        if (zona == null || StoreManager.Instance == null) return;
        StoreManager.Instance.ComprarAmpliacion(zona.id);
        Refresh();
    }

    // ------------------------------------------------------------- refresco

    void Refresh()
    {
        if (panel == null) return;
        GameManager gm = GameManager.Instance;
        StoreManager tienda = StoreManager.Instance;

        if (dineroTexto != null)
            dineroTexto.text = gm != null
                ? $"Dinero: {Currency.Format(gm.dinero)} CUP   ·   Alquiler extra: {Currency.Format(tienda != null ? tienda.AlquilerExtra : 0)} CUP/día"
                : "Dinero: —";

        if (tabMuebles != null)
            tabMuebles.GetComponent<Image>().color = tab == Tab.Muebles
                ? new Color(0.20f, 0.56f, 0.72f, 0.98f)
                : new Color(0.12f, 0.20f, 0.26f, 0.92f);
        if (tabAmpliaciones != null)
            tabAmpliaciones.GetComponent<Image>().color = tab == Tab.Ampliaciones
                ? new Color(0.58f, 0.44f, 0.16f, 0.98f)
                : new Color(0.12f, 0.20f, 0.26f, 0.92f);

        if (contenidoMuebles != null) contenidoMuebles.SetActive(tab == Tab.Muebles);
        if (contenidoAmpliaciones != null) contenidoAmpliaciones.SetActive(tab == Tab.Ampliaciones);
        if (botonPedidos != null) botonPedidos.gameObject.SetActive(tab == Tab.Muebles);

        if (tab == Tab.Muebles) RefreshMuebles(tienda, gm);
        else RefreshAmpliaciones(tienda, gm);
    }

    void RefreshMuebles(StoreManager tienda, GameManager gm)
    {
        if (tienda == null) return;

        if (productoTexto != null)
            productoTexto.text = "Producto para el mueble nuevo: " +
                (productoSeleccionado != null ? productoSeleccionado.nombre : "elige uno arriba");

        for (int i = 0; i < botonesProducto.Count && i < productos.Count; i++)
        {
            Image img = botonesProducto[i].GetComponent<Image>();
            if (img == null) continue;
            img.color = productos[i] == productoSeleccionado
                ? new Color(0.85f, 0.66f, 0.18f, 0.98f)
                : new Color(0.14f, 0.26f, 0.32f, 0.96f);
        }

        foreach (FilaMueble fila in filasMueble)
        {
            if (fila.info == null) continue;
            int precio = tienda.PrecioMueble(fila.tipo);
            string extra = fila.tipo == TipoMueble.Nevera ? " · frío · +luz" : "";
            fila.info.text = $"{tienda.NombreMueble(fila.tipo)}   ·   {tienda.CapacidadMueble(fila.tipo)} huecos{extra}   ·   {Currency.Format(precio)} CUP";
            if (fila.botonTexto != null) fila.botonTexto.text = "COMPRAR";
        }

        if (huecoTexto != null)
        {
            BuildSpot libre = spotObjetivo != null && !spotObjetivo.ocupado
                ? spotObjetivo
                : tienda.BuscarSpotLibre();
            if (libre == null)
                huecoTexto.text = "Sin huecos libres: compra una ampliación en la pestaña AMPLIACIONES.";
            else
                huecoTexto.text = $"Se construirá en: {libre.etiqueta}   ·   huecos libres: {ContarLibres(tienda)}";
        }
    }

    static int ContarLibres(StoreManager tienda)
    {
        int n = 0;
        foreach (BuildSpot s in tienda.spots)
            if (s != null && s.gameObject.activeInHierarchy && !s.ocupado) n++;
        return n;
    }

    void RefreshAmpliaciones(StoreManager tienda, GameManager gm)
    {
        if (tienda == null) return;

        foreach (FilaAmpliacion fila in filasAmpliacion)
        {
            if (fila.zona == null) continue;
            ExpansionZone zona = fila.zona;
            int precio = tienda.PrecioAmpliacion(zona.id);
            bool puede = tienda.PuedeComprarAmpliacion(zona.id);

            if (fila.info != null)
            {
                string estado = zona.comprada
                    ? "ABIERTA"
                    : puede ? $"{Currency.Format(precio)} CUP" : "BLOQUEADA";
                fila.info.text = $"{zona.nombre}   ·   {zona.descripcion}\n" +
                    $"+{zona.clientesExtra} cliente(s) simultáneo(s)   ·   {zona.spots.Count} huecos   ·   {estado}";
            }

            if (fila.botonTexto != null)
                fila.botonTexto.text = zona.comprada ? "ABIERTA" : puede ? "AMPLIAR" : "BLOQUEADA";

            if (fila.boton != null)
            {
                Image img = fila.boton.GetComponent<Image>();
                if (img != null)
                    img.color = zona.comprada
                        ? new Color(0.20f, 0.42f, 0.28f, 0.75f)
                        : puede
                            ? new Color(0.62f, 0.44f, 0.16f, 0.95f)
                            : new Color(0.24f, 0.24f, 0.26f, 0.75f);
            }
        }

        if (infoAmpliaciones != null && string.IsNullOrEmpty(infoAmpliaciones.text))
            infoAmpliaciones.text = "Cada ampliación añade superficie, huecos de muebles, clientes y alquiler diario.";
    }

    // ------------------------------------------------------------- interfaz

    void BuildUI()
    {
        if (panel != null) return;

        font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null) { try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            GameObject ui = GameObject.Find("UI");
            if (ui != null) canvas = ui.GetComponent<Canvas>();
        }
        if (canvas == null) return;

        panel = Panel(canvas.transform, "TiendaMuebles", Vector2.zero, new Vector2(1120f, 700f),
            new Color(0.04f, 0.08f, 0.12f, 0.98f));
        Panel(panel.transform, "Marco", Vector2.zero, new Vector2(1080f, 660f),
            new Color(0.09f, 0.16f, 0.21f, 1f));

        Texto(panel.transform, "Titulo", "TIENDA LA MYPIME · MUEBLES Y AMPLIACIONES",
            new Vector2(0f, 310f), new Vector2(1040f, 46f), 26, TextAnchor.MiddleCenter,
            new Color(1f, 0.86f, 0.42f));

        dineroTexto = Texto(panel.transform, "Dinero", "", new Vector2(0f, 274f), new Vector2(1040f, 32f),
            18, TextAnchor.MiddleCenter, new Color(0.65f, 1f, 0.72f));

        tabMuebles = Boton(panel.transform, "TabMuebles", new Vector2(-120f, 233f), new Vector2(220f, 42f),
            new Color(0.20f, 0.56f, 0.72f, 0.98f), out _, "MUEBLES", 18);
        tabAmpliaciones = Boton(panel.transform, "TabAmpliaciones", new Vector2(120f, 233f), new Vector2(220f, 42f),
            new Color(0.12f, 0.20f, 0.26f, 0.92f), out _, "AMPLIACIONES", 18);

        // ------------------------------------------------ pestaña muebles
        contenidoMuebles = Panel(panel.transform, "ContenidoMuebles", Vector2.zero, new Vector2(1080f, 600f),
            new Color(0f, 0f, 0f, 0f));

        productoTexto = Texto(contenidoMuebles.transform, "ProductoTexto", "",
            new Vector2(-250f, 190f), new Vector2(560f, 34f), 17, TextAnchor.MiddleLeft,
            new Color(0.92f, 0.96f, 1f));

        float[] ys = { 88f, 28f, -32f };
        TipoMueble[] tipos = { TipoMueble.Estante, TipoMueble.Gondola, TipoMueble.Nevera };
        for (int i = 0; i < tipos.Length; i++)
        {
            GameObject fila = Panel(contenidoMuebles.transform, "FilaMueble_" + i,
                new Vector2(0f, ys[i]), new Vector2(1040f, 54f),
                i % 2 == 0 ? new Color(0.12f, 0.20f, 0.26f, 0.96f) : new Color(0.10f, 0.16f, 0.21f, 0.96f));
            FilaMueble fm = new FilaMueble { tipo = tipos[i] };
            fm.info = Texto(fila.transform, "Info", "", new Vector2(-190f, 0f), new Vector2(640f, 48f),
                17, TextAnchor.MiddleLeft, new Color(0.92f, 0.96f, 1f));
            fm.boton = Boton(fila.transform, "Comprar", new Vector2(405f, 0f), new Vector2(170f, 40f),
                new Color(0.20f, 0.58f, 0.30f, 0.95f), out fm.botonTexto, "COMPRAR", 17);
            filasMueble.Add(fm);
        }

        huecoTexto = Texto(contenidoMuebles.transform, "Hueco", "",
            new Vector2(0f, -96f), new Vector2(1040f, 30f), 16, TextAnchor.MiddleCenter,
            new Color(1f, 0.90f, 0.52f));

        infoMuebles = Texto(contenidoMuebles.transform, "InfoMuebles",
            "Elige un producto, compra el mueble y se colocará en el primer hueco libre. Luego pide mercancía en la computadora.",
            new Vector2(0f, -132f), new Vector2(1040f, 56f), 16, TextAnchor.MiddleCenter,
            new Color(0.78f, 0.88f, 0.96f));
        infoMuebles.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoMuebles.verticalOverflow = VerticalWrapMode.Overflow;

        botonPedidos = Boton(contenidoMuebles.transform, "BotonPedidos", new Vector2(-300f, -200f),
            new Vector2(270f, 48f), new Color(0.18f, 0.42f, 0.58f, 0.95f), out _, "PEDIR MERCANCÍA", 16);

        // ------------------------------------------------ pestaña ampliaciones
        contenidoAmpliaciones = Panel(panel.transform, "ContenidoAmpliaciones", Vector2.zero,
            new Vector2(1080f, 600f), new Color(0f, 0f, 0f, 0f));

        infoAmpliaciones = Texto(contenidoAmpliaciones.transform, "InfoAmpliaciones",
            "Cada ampliación añade superficie, huecos de muebles, clientes y alquiler diario.",
            new Vector2(0f, 150f), new Vector2(1000f, 60f), 17, TextAnchor.MiddleCenter,
            new Color(0.80f, 0.90f, 0.98f));
        infoAmpliaciones.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoAmpliaciones.verticalOverflow = VerticalWrapMode.Overflow;

        if (StoreManager.Instance != null)
        {
            float y = 55f;
            foreach (ExpansionZone zona in StoreManager.Instance.expansiones)
            {
                if (zona == null) continue;
                GameObject fila = Panel(contenidoAmpliaciones.transform, "FilaAmpliacion_" + zona.id,
                    new Vector2(0f, y), new Vector2(1040f, 90f),
                    new Color(0.12f, 0.20f, 0.26f, 0.96f));
                FilaAmpliacion fa = new FilaAmpliacion { zona = zona };
                fa.info = Texto(fila.transform, "Info", "", new Vector2(-180f, 0f), new Vector2(650f, 80f),
                    16, TextAnchor.MiddleLeft, new Color(0.92f, 0.96f, 1f));
                fa.info.horizontalOverflow = HorizontalWrapMode.Wrap;
                fa.info.verticalOverflow = VerticalWrapMode.Overflow;
                fa.boton = Boton(fila.transform, "Ampliar", new Vector2(405f, 0f), new Vector2(180f, 54f),
                    new Color(0.62f, 0.44f, 0.16f, 0.95f), out fa.botonTexto, "AMPLIAR", 18);
                filasAmpliacion.Add(fa);
                y -= 105f;
            }
        }

        // ------------------------------------------------ cerrar
        botonCerrar = Boton(panel.transform, "BotonCerrar", new Vector2(300f, -272f), new Vector2(220f, 46f),
            new Color(0.55f, 0.22f, 0.22f, 0.92f), out _, "CERRAR (E)", 16);
    }

    void RebuildProductos()
    {
        if (contenidoMuebles == null) return;

        foreach (Transform child in contenidoMuebles.transform)
            if (child.name.StartsWith("ProdBtn")) Destroy(child.gameObject);
        botonesProducto.Clear();
        productos.Clear();

        GameManager gm = GameManager.Instance;
        if (gm != null && gm.catalogoPedidos != null)
        {
            foreach (OrderCatalog.Entry e in gm.catalogoPedidos.productos)
                if (e != null && e.producto != null && !productos.Contains(e.producto))
                    productos.Add(e.producto);
        }
        if (productos.Count == 0 && StoreManager.Instance != null)
            foreach (ProductData p in StoreManager.Instance.productos)
                if (p != null && !productos.Contains(p)) productos.Add(p);

        if (productoSeleccionado == null || !productos.Contains(productoSeleccionado))
            productoSeleccionado = productos.Count > 0 ? productos[0] : null;

        int n = productos.Count;
        for (int i = 0; i < n; i++)
        {
            float x = (i - (n - 1) * 0.5f) * 130f;
            RectTransform boton = Boton(contenidoMuebles.transform, "ProdBtn_" + i,
                new Vector2(x, 150f), new Vector2(124f, 38f),
                new Color(0.14f, 0.26f, 0.32f, 0.96f), out _, productos[i].nombre, 14);
            botonesProducto.Add(boton);
        }
    }

    // ------------------------------------------------------------- helpers UI

    static GameObject Panel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    Text Texto(Transform parent, string name, string content, Vector2 pos, Vector2 size,
        int fontSize, TextAnchor align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text text = go.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    RectTransform Boton(Transform parent, string name, Vector2 pos, Vector2 size, Color color,
        out Text label, string contenido, int fontSize)
    {
        GameObject go = Panel(parent, name, pos, size, color);
        label = Texto(go.transform, "Texto", contenido, Vector2.zero, size, fontSize,
            TextAnchor.MiddleCenter, Color.white);
        return go.GetComponent<RectTransform>();
    }

    static bool Pulsado(RectTransform rt)
    {
        if (rt == null) return false;
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
                if (t.phase == TouchPhase.Began && Dentro(rt, t.position)) return true;
            return false;
        }
        return Input.GetMouseButtonDown(0) && Dentro(rt, Input.mousePosition);
    }

    static bool Dentro(RectTransform rt, Vector2 screen)
    {
        Vector2 local;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screen, null, out local)
            && rt.rect.Contains(local);
    }
}
