using System.Collections.Generic;
using UnityEngine;

/// <summary>Tipo de mueble que se puede comprar en la tienda del local.</summary>
public enum TipoMueble
{
    Estante = 0,
    Gondola = 1,
    Nevera = 2
}

/// <summary>
/// Gestiona la tienda de muebles y las ampliaciones del local.
/// Reproduce la mecánica de Supermarket Simulator: los muebles se compran con
/// dinero, se colocan en huecos de obra y cada ampliación añade superficie,
/// huecos nuevos, más clientes y alquiler diario.
/// </summary>
public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    [Header("Precios base (se ajustan con el índice del día)")]
    public int precioEstante = Currency.CUP(9f);
    public int precioGondola = Currency.CUP(15f);
    public int precioNevera = Currency.CUP(24f);
    public int precioAmpliacion1 = Currency.CUP(55f);
    public int precioAmpliacion2 = Currency.CUP(85f);
    [Tooltip("Alquiler diario extra por cada ampliación comprada")]
    public int alquilerExtraPorAmpliacion = Currency.CUP(6f);

    [Header("Huecos y ampliaciones de la escena")]
    public List<BuildSpot> spots = new List<BuildSpot>();
    public List<ExpansionZone> expansiones = new List<ExpansionZone>();

    [Tooltip("Muebles construidos por el jugador")]
    public List<Shelf> muebles = new List<Shelf>();

    [Tooltip("Todos los productos que se pueden asignar a un mueble nuevo")]
    public List<ProductData> productos = new List<ProductData>();

    [Tooltip("Clientes simultáneos antes de ampliar")]
    public int clientesBase = 4;

    public int AmpliacionesCompradas
    {
        get
        {
            if (expansiones == null) return 0;
            int n = 0;
            foreach (ExpansionZone e in expansiones)
                if (e != null && e.comprada) n++;
            return n;
        }
    }

    /// <summary>Alquiler extra total del día, ya ajustado por la inflación.</summary>
    public int AlquilerExtra
    {
        get
        {
            int n = AmpliacionesCompradas;
            if (n <= 0) return 0;
            float indice = GameManager.Instance != null ? GameManager.Instance.indicePrecios : 1f;
            int unitario = Mathf.Max(1, Mathf.RoundToInt(alquilerExtraPorAmpliacion * indice));
            return unitario * n;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ActualizarClientes();
        if (GameManager.Instance != null) GameManager.Instance.RecalcularGastos();
    }

    public void RegistrarSpot(BuildSpot spot)
    {
        if (spot == null || spots == null) return;
        if (!spots.Contains(spot)) spots.Add(spot);
    }

    public void RegistrarExpansion(ExpansionZone zona)
    {
        if (zona == null || expansiones == null) return;
        if (!expansiones.Contains(zona)) expansiones.Add(zona);
    }

    // ------------------------------------------------------------- precios

    public string NombreMueble(TipoMueble tipo)
    {
        switch (tipo)
        {
            case TipoMueble.Gondola: return "Góndola doble";
            case TipoMueble.Nevera: return "Nevera";
            default: return "Estante básico";
        }
    }

    public int CapacidadMueble(TipoMueble tipo)
    {
        switch (tipo)
        {
            case TipoMueble.Gondola: return 12;
            case TipoMueble.Nevera: return 8;
            default: return 8;
        }
    }

    public int PrecioMueble(TipoMueble tipo)
    {
        int basePrecio = tipo == TipoMueble.Gondola ? precioGondola
            : tipo == TipoMueble.Nevera ? precioNevera
            : precioEstante;

        float indice = GameManager.Instance != null ? GameManager.Instance.indicePrecios : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(basePrecio * indice));
    }

    public int PrecioAmpliacion(int index)
    {
        int basePrecio = index == 0 ? precioAmpliacion1 : precioAmpliacion2;
        float indice = GameManager.Instance != null ? GameManager.Instance.indicePrecios : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(basePrecio * indice));
    }

    public string NombreAmpliacion(int index)
    {
        if (expansiones != null && index >= 0 && index < expansiones.Count && expansiones[index] != null)
            return expansiones[index].nombre;
        return "la ampliación anterior";
    }

    public bool PuedeComprarAmpliacion(int index)
    {
        if (expansiones == null || index < 0 || index >= expansiones.Count) return false;
        ExpansionZone zona = expansiones[index];
        if (zona == null || zona.comprada) return false;

        if (zona.indiceRequerido >= 0)
        {
            if (zona.indiceRequerido >= expansiones.Count) return false;
            ExpansionZone previa = expansiones[zona.indiceRequerido];
            if (previa == null || !previa.comprada) return false;
        }
        return true;
    }

    // ------------------------------------------------------------- compras

    public BuildSpot BuscarSpotLibre()
    {
        if (spots == null) return null;
        foreach (BuildSpot s in spots)
            if (s != null && s.gameObject.activeInHierarchy && !s.ocupado) return s;
        return null;
    }

    public BuildSpot BuscarSpot(int id)
    {
        if (spots == null) return null;
        foreach (BuildSpot s in spots)
            if (s != null && s.id == id) return s;
        return null;
    }

    /// <summary>Compra y coloca un mueble en un hueco libre.</summary>
    public bool ComprarMueble(BuildSpot spot, TipoMueble tipo, ProductData producto)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return false;

        if (spot == null || spot.ocupado)
        {
            Aviso("Ese hueco ya está ocupado. Elige otro.");
            return false;
        }
        if (producto == null)
        {
            Aviso("Elige un producto para el mueble nuevo.");
            return false;
        }

        int precio = PrecioMueble(tipo);
        if (!gm.SpendMoney(precio))
        {
            Aviso($"No te alcanza: necesitas {Currency.Format(precio)} CUP.");
            return false;
        }

        Shelf shelf = spot.Construir(tipo, producto);
        if (shelf == null)
        {
            gm.dinero += precio; // devolución defensiva si algo falla
            Aviso("No se pudo construir el mueble.");
            return false;
        }

        if (muebles == null) muebles = new List<Shelf>();
        muebles.Add(shelf);

        RecalcularTodo(true);
        SaveSystem.Save(gm);
        Aviso($"{NombreMueble(tipo)} de {producto.nombre} construido · -{Currency.Format(precio)} CUP");
        if (HUD.Instance != null)
            HUD.Instance.ShowMessage($"{NombreMueble(tipo)} de {producto.nombre} listo en {spot.etiqueta}");
        return true;
    }

    /// <summary>Compra una ampliación y abre su ala del local.</summary>
    public bool ComprarAmpliacion(int index)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return false;
        if (expansiones == null || index < 0 || index >= expansiones.Count) return false;

        ExpansionZone zona = expansiones[index];
        if (zona == null || zona.comprada)
        {
            Aviso("Esa ampliación ya está abierta.");
            return false;
        }
        if (!PuedeComprarAmpliacion(index))
        {
            Aviso($"Primero compra {NombreAmpliacion(zona.indiceRequerido)}.");
            return false;
        }

        int precio = PrecioAmpliacion(index);
        if (!gm.SpendMoney(precio))
        {
            Aviso($"No te alcanza: necesitas {Currency.Format(precio)} CUP.");
            return false;
        }

        zona.Abrir();
        RecalcularTodo(true);
        SaveSystem.Save(gm);

        Aviso($"{zona.nombre} abierta · {zona.spots.Count} huecos nuevos · +{zona.clientesExtra} cliente(s) · alquiler +{Currency.Format(AlquilerExtra)} CUP/día");
        if (HUD.Instance != null)
            HUD.Instance.ShowMessage($"¡{zona.nombre} abierta! Ya puedes comprar muebles en el ala nueva");
        return true;
    }

    /// <summary>Recalcula alquiler, clientes y rejilla de navegación.</summary>
    void RecalcularTodo(bool reconstruirGrid)
    {
        GameManager gm = GameManager.Instance;
        ActualizarClientes();
        if (gm != null) gm.RecalcularGastos();

        if (reconstruirGrid && NavigationGrid.Instance != null)
            NavigationGrid.Instance.Build();

        GameManager.Notify();
    }

    public void ActualizarClientes()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.spawner == null) return;
        gm.spawner.maxCustomers = Mathf.Max(1, clientesBase + AmpliacionesCompradas);
    }

    void Aviso(string texto)
    {
        if (StoreUI.Instance != null) StoreUI.Instance.Aviso(texto);
        else if (HUD.Instance != null) HUD.Instance.ShowMessage(texto);
    }

    // ------------------------------------------------------------- guardado

    /// <summary>Reconstruye las compras guardadas antes de restaurar los stocks.</summary>
    public void AplicarGuardado(List<int> spotIds, List<int> tipos, List<string> productosGuardados, List<bool> ampliadas)
    {
        if (ampliadas != null)
        {
            for (int i = 0; i < ampliadas.Count && i < expansiones.Count; i++)
                if (ampliadas[i] && expansiones[i] != null && !expansiones[i].comprada)
                    expansiones[i].Abrir();
        }

        if (spotIds != null && tipos != null)
        {
            for (int i = 0; i < spotIds.Count; i++)
            {
                BuildSpot spot = BuscarSpot(spotIds[i]);
                if (spot == null || spot.ocupado) continue;

                TipoMueble tipo = i < tipos.Count ? (TipoMueble)tipos[i] : TipoMueble.Estante;
                string nombreProducto = productosGuardados != null && i < productosGuardados.Count
                    ? productosGuardados[i] : "";
                ProductData producto = BuscarProducto(nombreProducto);
                if (producto == null) continue;

                Shelf shelf = spot.Construir(tipo, producto);
                if (shelf != null)
                {
                    if (muebles == null) muebles = new List<Shelf>();
                    if (!muebles.Contains(shelf)) muebles.Add(shelf);
                }
            }
        }

        ActualizarClientes();
        GameManager gm = GameManager.Instance;
        if (gm != null) gm.RecalcularGastos();
        if (NavigationGrid.Instance != null) NavigationGrid.Instance.Build();
        GameManager.Notify();
    }

    ProductData BuscarProducto(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return null;

        GameManager gm = GameManager.Instance;
        if (gm != null && gm.catalogoPedidos != null)
        {
            ProductData p = gm.catalogoPedidos.FindProduct(nombre);
            if (p != null) return p;
        }

        if (productos != null)
            foreach (ProductData p in productos)
                if (p != null && p.nombre == nombre) return p;

        if (gm != null && gm.shelves != null)
            foreach (Shelf s in gm.shelves)
                if (s != null && s.product != null && s.product.nombre == nombre) return s.product;

        return null;
    }

    // ------------------------------------------------------------- fábrica

    /// <summary>
    /// Construye en tiempo de ejecución un mueble jugable con primitivas.
    /// Mantiene la misma lógica que las estanterías de la escena (slots,
    /// punto de cliente, nevera y materiales compartidos).
    /// </summary>
    public static Shelf CrearMueble(TipoMueble tipo, Vector3 pos, float yaw, ProductData producto)
    {
        GameObject go = new GameObject("Mueble nuevo");
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(2.4f, 1.9f, 0.85f);
        col.center = new Vector3(0f, 0.95f, 0f);

        bool nevera = tipo == TipoMueble.Nevera;
        int filas = tipo == TipoMueble.Gondola ? 3 : 2;
        const int columnas = 4;

        Color estructuraColor = nevera
            ? new Color(0.82f, 0.89f, 0.93f)
            : (tipo == TipoMueble.Gondola
                ? new Color(0.58f, 0.42f, 0.26f)
                : new Color(0.50f, 0.36f, 0.22f));
        Color tableroColor = nevera
            ? new Color(0.92f, 0.95f, 0.97f)
            : (tipo == TipoMueble.Gondola
                ? new Color(0.78f, 0.62f, 0.42f)
                : new Color(0.64f, 0.47f, 0.30f));

        Cubo(go, "LateralIzq", new Vector3(-1.15f, 0.95f, 0f), new Vector3(0.10f, 1.90f, 0.85f), estructuraColor);
        Cubo(go, "LateralDer", new Vector3(1.15f, 0.95f, 0f), new Vector3(0.10f, 1.90f, 0.85f), estructuraColor);
        Cubo(go, "Base", new Vector3(0f, 0.10f, 0f), new Vector3(2.30f, 0.20f, 0.85f), estructuraColor);
        Cubo(go, "Techo", new Vector3(0f, 1.87f, 0f), new Vector3(2.30f, 0.10f, 0.85f), estructuraColor);
        Cubo(go, "Fondo", new Vector3(0f, 1.00f, 0.40f), new Vector3(2.20f, 1.75f, 0.04f), tableroColor);

        float[] baldas = filas == 3
            ? new[] { 0.35f, 0.85f, 1.35f }
            : new[] { 0.50f, 1.15f };

        for (int f = 0; f < baldas.Length; f++)
            Cubo(go, "Balda " + (f + 1), new Vector3(0f, baldas[f], -0.05f),
                new Vector3(2.20f, 0.06f, 0.75f), tableroColor);

        // Cartel de producto sobre el mueble (se ve desde el pasillo).
        if (producto != null)
        {
            Cubo(go, "CartelProducto", new Vector3(0f, 1.68f, -0.44f),
                new Vector3(1.70f, 0.22f, 0.03f), producto.color);
            GameObject textoGo = new GameObject("TextoProducto");
            textoGo.transform.SetParent(go.transform, false);
            textoGo.transform.localPosition = new Vector3(0f, 1.68f, -0.47f);
            textoGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            textoGo.transform.localScale = Vector3.one * 0.10f;
            TextMesh texto = textoGo.AddComponent<TextMesh>();
            texto.text = producto.nombre.ToUpperInvariant();
            texto.fontSize = 48;
            texto.characterSize = 0.08f;
            texto.anchor = TextAnchor.MiddleCenter;
            texto.alignment = TextAlignment.Center;
            texto.color = new Color(1f, 1f, 1f, 0.95f);
        }

        GameObject indicador = null;
        if (nevera)
        {
            indicador = Cubo(go, "IndicadorNevera", new Vector3(0.98f, 1.72f, -0.38f),
                new Vector3(0.14f, 0.14f, 0.14f), new Color(0.20f, 0.90f, 0.35f));
        }

        Transform[] slots = new Transform[filas * columnas];
        int idx = 0;
        for (int f = 0; f < filas; f++)
        {
            float y = baldas[f] + 0.16f;
            for (int c = 0; c < columnas; c++)
            {
                GameObject slot = new GameObject("Slot_" + idx);
                slot.transform.SetParent(go.transform, false);
                slot.transform.localPosition = new Vector3(-0.42f + c * 0.28f, y, -0.32f);
                slots[idx++] = slot.transform;
            }
        }

        GameObject punto = new GameObject("PuntoCliente");
        punto.transform.SetParent(go.transform, false);
        punto.transform.localPosition = new Vector3(0f, 0f, -1.5f);

        Shelf shelf = go.AddComponent<Shelf>();
        shelf.product = producto;
        shelf.capacity = slots.Length;
        shelf.stock = 0;
        shelf.slots = slots;
        shelf.customerSpot = punto.transform;
        if (nevera)
        {
            shelf.esNevera = true;
            shelf.costeLuz = Currency.CUP(0.5f);
            shelf.indicadorLuz = indicador;
        }

        shelf.RefreshVisuals();
        return shelf;
    }

    static GameObject Cubo(GameObject padre, string nombre, Vector3 pos, Vector3 escala, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;

        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        Renderer r = go.GetComponent<Renderer>();
        Material mat = MaterialFactory.Get(color);
        if (r != null && mat != null) r.sharedMaterial = mat;
        return go;
    }
}
