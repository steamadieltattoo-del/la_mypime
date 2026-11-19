using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de la computadora de pedidos. Muestra los productos configurados en el
/// catálogo, permite cambiar cantidades, activar/desactivar productos y enviar el
/// pedido al proveedor. El coste se paga al confirmar y el triciclo sale de inmediato.
/// No usa EventSystem: detecta toques y clics igual que el resto de la interfaz.
/// </summary>
public class OrderComputerUI : MonoBehaviour
{
    public static OrderComputerUI Instance { get; private set; }
    public static bool Abierto => Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    /// <summary>Se activa el frame en que se cierra con E para que el jugador no vuelva a interactuar.</summary>
    public static bool CerroAhora { get; private set; }

    void LateUpdate()
    {
        CerroAhora = false;
    }

    class Row
    {
        public OrderCatalog.Entry entry;
        public int cantidad;
        public Text nombre;
        public Text cantidadTexto;
        public Text activoTexto;
        public RectTransform menos;
        public RectTransform mas;
        public RectTransform activo;
    }

    readonly List<Row> filas = new List<Row>();
    OrderCatalog catalogo;
    GameObject panel;
    Text totalTexto;
    Text avisoTexto;
    RectTransform botonPedir;
    RectTransform botonCerrar;
    Font font;
    bool ignorarInputUnFrame;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (panel == null) BuildUI();
        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Open(OrderCatalog cat)
    {
        if (panel == null) BuildUI();
        catalogo = cat;
        BuildRows();
        panel.SetActive(true);
        Refresh();
        // La tecla E que abrió la pantalla no debe cerrarla en el mismo frame.
        ignorarInputUnFrame = true;
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        CerroAhora = true;
    }

    void Update()
    {
        if (!Abierto) return;
        if (ignorarInputUnFrame) { ignorarInputUnFrame = false; return; }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            CerroAhora = true;
            Close();
            return;
        }

        if (Pulsado(botonCerrar)) { Close(); return; }
        if (Pulsado(botonPedir)) { Confirmar(); return; }

        bool cambio = false;
        foreach (Row f in filas)
        {
            if (f.entry == null || f.entry.producto == null) continue;
            if (Pulsado(f.activo))
            {
                f.entry.activo = !f.entry.activo;
                if (f.entry.activo == false) f.cantidad = 0;
                cambio = true;
            }
            int max = Mathf.Max(1, f.entry.maximo);
            if (Pulsado(f.menos)) { f.cantidad = Mathf.Max(0, f.cantidad - f.entry.lote); cambio = true; }
            if (Pulsado(f.mas)) { f.cantidad = Mathf.Min(max, f.cantidad + f.entry.lote); cambio = true; }
        }
        if (cambio)
        {
            if (GameManager.Instance != null) SaveSystem.Save(GameManager.Instance);
            Refresh();
        }
    }

    /// <summary>Cobra el pedido y hace salir el triciclo hacia el almacén.</summary>
    void Confirmar()
    {
        if (GameManager.Instance == null) return;
        int pedidos = 0;
        int unidades = 0;
        foreach (Row f in filas)
        {
            if (f.entry == null || !f.entry.activo || f.cantidad <= 0) continue;
            if (GameManager.Instance.HacerPedido(f.entry.producto, f.cantidad, f.entry.lote))
            {
                unidades += f.cantidad;
                pedidos++;
                f.cantidad = 0;
            }
        }
        if (pedidos > 0)
        {
            if (HUD.Instance != null)
                HUD.Instance.ShowMessage($"Pedido enviado: {unidades} unidades de {pedidos} producto(s). El triciclo va por la carretera al almacén.");
            Refresh();
        }
    }

    void Refresh()
    {
        if (totalTexto == null) return;
        int total = 0;
        int preparadas = 0;
        foreach (Row f in filas)
        {
            if (f.entry == null || f.entry.producto == null) continue;
            int unitario = GameManager.CostePorUnidad(f.entry.producto, f.entry.lote);
            f.nombre.text = f.entry.activo
                ? $"{f.entry.producto.nombre}   ·   {Currency.Format(unitario)} CUP/unidad"
                : $"{f.entry.producto.nombre}   ·   NO DISPONIBLE";
            f.nombre.color = f.entry.activo ? new Color(0.92f, 0.96f, 1f) : new Color(0.55f, 0.58f, 0.62f);
            f.cantidadTexto.text = f.cantidad.ToString();
            f.activoTexto.text = f.entry.activo ? "SÍ" : "NO";
            f.activoTexto.color = f.entry.activo ? new Color(0.55f, 1f, 0.65f) : new Color(1f, 0.55f, 0.55f);
            f.menos.gameObject.SetActive(f.entry.activo);
            f.mas.gameObject.SetActive(f.entry.activo);
            if (f.entry.activo)
            {
                total += unitario * f.cantidad;
                if (f.cantidad > 0) preparadas++;
            }
        }
        totalTexto.text = $"Total del pedido: {Currency.Format(total)} CUP   ·   {preparadas} producto(s)   ·   Dinero: {Currency.Format(GameManager.Instance != null ? GameManager.Instance.dinero : 0)} CUP";
        if (avisoTexto != null)
        {
            int pendientes = GameManager.Instance != null ? GameManager.Instance.PedidosPendientesCount : 0;
            avisoTexto.text = pendientes > 0
                ? $"Hay {pendientes} pedido(s) en camino: saldrán cuando el triciclo termine el viaje actual."
                : "El pedido se paga ahora y el triciclo sale enseguida hacia el almacén.";
        }
    }

    void BuildRows()
    {
        foreach (Transform child in panel.transform)
            if (child.name.StartsWith("Fila_")) Destroy(child.gameObject);
        filas.Clear();
        if (catalogo == null) return;

        float y = 205f;
        for (int i = 0; i < catalogo.productos.Count && i < 8; i++)
        {
            OrderCatalog.Entry e = catalogo.productos[i];
            if (e == null || e.producto == null) continue;
            GameObject fila = Panel(panel.transform, "Fila_" + i, new Vector2(0f, y), new Vector2(1050f, 56f),
                i % 2 == 0 ? new Color(0.12f, 0.19f, 0.25f, 0.95f) : new Color(0.09f, 0.15f, 0.20f, 0.95f));
            Row r = new Row { entry = e };

            r.activo = Boton(fila.transform, "Activo", new Vector2(-455f, 0f), new Vector2(100f, 42f),
                new Color(0.20f, 0.42f, 0.52f), out r.activoTexto, "", 16);
            r.nombre = Texto(fila.transform, "Nombre", "", new Vector2(-335f, 0f), new Vector2(510f, 44f),
                18, TextAnchor.MiddleLeft, new Color(0.92f, 0.96f, 1f));
            r.menos = Boton(fila.transform, "Menos", new Vector2(105f, 0f), new Vector2(52f, 42f),
                new Color(0.55f, 0.25f, 0.25f), out _, "−", 24);
            r.cantidadTexto = Texto(fila.transform, "Cantidad", "0", new Vector2(185f, 0f), new Vector2(70f, 44f),
                20, TextAnchor.MiddleCenter, Color.white);
            r.mas = Boton(fila.transform, "Mas", new Vector2(265f, 0f), new Vector2(52f, 42f),
                new Color(0.22f, 0.52f, 0.30f), out _, "+", 24);
            Texto(fila.transform, "Lote", $"caja de {e.lote}", new Vector2(410f, 0f), new Vector2(140f, 44f),
                14, TextAnchor.MiddleRight, new Color(0.70f, 0.80f, 0.88f));
            filas.Add(r);
            y -= 60f;
        }
    }

    void BuildUI()
    {
        if (panel != null) return;
        font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null) { try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }

        Canvas canvas = null;
        GameObject canvasGo = GameObject.Find("UI");
        if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvasGo = new GameObject("UI");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
        }

        panel = Panel(canvasGo.transform, "ComputadoraPedidos", Vector2.zero, new Vector2(1120f, 700f),
            new Color(0.04f, 0.08f, 0.12f, 0.97f));
        Panel(panel.transform, "Marco", Vector2.zero, new Vector2(1080f, 660f),
            new Color(0.10f, 0.18f, 0.24f, 1f));

        Texto(panel.transform, "Titulo", "COMPUTADORA DE PEDIDOS · PROVEEDOR LA MYPIME",
            new Vector2(0f, 302f), new Vector2(1020f, 50f), 26, TextAnchor.MiddleCenter,
            new Color(1f, 0.86f, 0.42f));
        Texto(panel.transform, "Ayuda", "Toca SÍ/NO para poner los productos que se pueden pedir · −/+ para elegir cajas",
            new Vector2(0f, 262f), new Vector2(1020f, 36f), 16, TextAnchor.MiddleCenter,
            new Color(0.70f, 0.86f, 0.96f));

        totalTexto = Texto(panel.transform, "Total", "", new Vector2(-150f, -272f), new Vector2(760f, 44f),
            19, TextAnchor.MiddleLeft, new Color(0.65f, 1f, 0.72f));
        avisoTexto = Texto(panel.transform, "Aviso", "", new Vector2(0f, -308f), new Vector2(1020f, 34f),
            15, TextAnchor.MiddleCenter, new Color(0.80f, 0.88f, 0.96f));

        Text t;
        botonPedir = Boton(panel.transform, "BotonPedir", new Vector2(300f, -272f), new Vector2(210f, 58f),
            new Color(0.20f, 0.58f, 0.30f), out t, "HACER PEDIDO", 19);
        botonCerrar = Boton(panel.transform, "BotonCerrar", new Vector2(300f, -338f), new Vector2(210f, 44f),
            new Color(0.55f, 0.22f, 0.22f), out t, "CERRAR (E)", 16);
    }

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
