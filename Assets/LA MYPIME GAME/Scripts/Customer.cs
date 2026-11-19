using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cliente con lista de la compra (1-3 productos) y máquina de estados:
/// entra -> busca estantería -> coge producto -> vuelve a buscar -> va a caja
/// -> espera a que escaneen y cobren -> sale.
/// Movimiento directo (sin NavMesh) para mantener el MVP fácil de entender.
/// </summary>
public class Customer : MonoBehaviour
{
    public enum CustomerState
    {
        BuscandoProducto,
        YendoACaja,
        EnCola,
        EsperandoEscaneo,
        Saliendo
    }

    public CustomerState state = CustomerState.BuscandoProducto;
    public Transform exitPoint;
    public float speed = 1.8f;
    public List<ProductData> carro = new List<ProductData>();

    [Tooltip("Máximo que está dispuesto a pagar este cliente")]
    public int presupuesto;

    [Tooltip("Segundos que espera en cola antes de irse")]
    public float paciencia = 22f;

    [Header("Preferencia")]
    [Tooltip("Producto que este cliente quiere encontrar")]
    public ProductData favorito;
    public bool favoritoConseguido;

    /// <summary>El cobro da bonus de reputación solo si encontró su producto favorito.</summary>
    public bool Satisfecho => favoritoConseguido;

    private GameObject globoFavorito;
    private float tiempoEspera;
    private GameObject barraFondo;
    private Transform barraRelleno;
    private Renderer barraRenderer;

    private Vector3 destination;
    private List<Vector3> path = new List<Vector3>();
    private int pathIndex;
    private Shelf targetShelf;
    private Shelf ultimaEstanteria;
    private int productosPorCoger;
    private int intentos;
    private GameObject cesta;

    void Awake()
    {
        BuildVisual();
    }

    void Start()
    {
        if (GameManager.Instance != null && !GameManager.Instance.customers.Contains(this))
            GameManager.Instance.customers.Add(this);

        productosPorCoger = Random.Range(1, 4); // 1, 2 o 3 productos
        ChooseFavorito();
        ChooseShelf();
    }

    /// <summary>Crea el cuerpo, la cabeza y la cesta con primitivas.</summary>
    void BuildVisual()
    {
        Color[] camisas =
        {
            new Color(0.20f, 0.50f, 0.82f),
            new Color(0.85f, 0.45f, 0.20f),
            new Color(0.35f, 0.70f, 0.42f),
            new Color(0.70f, 0.32f, 0.60f)
        };

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Cuerpo";
        body.transform.SetParent(transform, false);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        Paint(body, camisas[Random.Range(0, camisas.Length)]);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Cabeza";
        head.transform.SetParent(transform, false);
        head.transform.localPosition = new Vector3(0f, 1.42f, 0f);
        head.transform.localScale = Vector3.one * 0.34f;
        Paint(head, new Color(0.92f, 0.76f, 0.60f));

        cesta = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cesta.name = "Cesta";
        cesta.transform.SetParent(transform, false);
        cesta.transform.localPosition = new Vector3(0f, 0.55f, 0.42f);
        cesta.transform.localScale = new Vector3(0.42f, 0.26f, 0.30f);
        Paint(cesta, new Color(0.85f, 0.62f, 0.20f));

        globoFavorito = GameObject.CreatePrimitive(PrimitiveType.Cube);
        globoFavorito.name = "GloboFavorito";
        globoFavorito.transform.SetParent(transform, false);
        globoFavorito.transform.localPosition = new Vector3(0f, 2.35f, 0f);
        globoFavorito.transform.localScale = Vector3.one * 0.24f;
        Paint(globoFavorito, Color.white);

        BuildPatienceBar();
    }

    /// <summary>Elige qué producto quiere, ponderado por el campo demanda.</summary>
    void ChooseFavorito()
    {
        if (GameManager.Instance == null) return;

        List<Shelf> opciones = new List<Shelf>();
        foreach (Shelf s in GameManager.Instance.shelves)
            if (s != null && s.product != null) opciones.Add(s);
        if (opciones.Count == 0) return;

        float total = 0f;
        foreach (Shelf s in opciones) total += Mathf.Max(0.01f, s.product.demanda);

        float r = Random.value * total;
        favorito = opciones[opciones.Count - 1].product;
        foreach (Shelf s in opciones)
        {
            r -= Mathf.Max(0.01f, s.product.demanda);
            if (r <= 0f) { favorito = s.product; break; }
        }

        if (globoFavorito != null)
        {
            Renderer gr = globoFavorito.GetComponent<Renderer>();
            if (gr != null && favorito != null) gr.sharedMaterial = MaterialFactory.Get(favorito.color);
        }
    }

    void MarcarFavoritoConseguido()
    {
        favoritoConseguido = true;
        if (globoFavorito != null) globoFavorito.SetActive(false);
    }

    /// <summary>Barra sobre la cabeza que se acorta mientras espera.</summary>
    void BuildPatienceBar()
    {
        barraFondo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barraFondo.name = "Paciencia";
        barraFondo.transform.SetParent(transform, false);
        barraFondo.transform.localPosition = new Vector3(0f, 1.95f, 0f);
        barraFondo.transform.localScale = new Vector3(1f, 0.12f, 0.06f);
        Paint(barraFondo, new Color(0.12f, 0.12f, 0.12f));

        GameObject relleno = GameObject.CreatePrimitive(PrimitiveType.Cube);
        relleno.name = "Relleno";
        relleno.transform.SetParent(barraFondo.transform, false);
        relleno.transform.localPosition = Vector3.zero;
        relleno.transform.localScale = new Vector3(0.96f, 0.8f, 1.2f);
        Paint(relleno, new Color(0.25f, 0.90f, 0.35f));
        barraRelleno = relleno.transform;
        barraRenderer = relleno.GetComponent<Renderer>();

        barraFondo.SetActive(false);
    }

    void ActualizarBarra()
    {
        if (barraFondo == null || barraRelleno == null) return;

        float t = Mathf.Clamp01(1f - tiempoEspera / Mathf.Max(0.1f, paciencia));
        barraRelleno.localScale = new Vector3(Mathf.Max(0.01f, 0.96f * t), 0.8f, 1.2f);
        barraRelleno.localPosition = new Vector3(-0.48f * (1f - t), 0f, 0f);

        if (barraRenderer != null)
        {
            Color c = t > 0.4f ? new Color(0.25f, 0.90f, 0.35f) : new Color(0.95f, 0.25f, 0.22f);
            barraRenderer.sharedMaterial = MaterialFactory.Get(c);
        }
    }

    void MostrarBarra(bool visible)
    {
        if (barraFondo != null) barraFondo.SetActive(visible);
    }

    static void Paint(GameObject go, Color color)
    {
        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        Material mat = MaterialFactory.Get(color);
        Renderer renderer = go.GetComponent<Renderer>();
        if (mat != null && renderer != null) renderer.sharedMaterial = mat;
    }

    void ChooseShelf()
    {
        intentos++;
        if (GameManager.Instance == null || intentos > 8)
        {
            GoToExit();
            return;
        }

        // Primero intenta su producto favorito; es lo que lo hace satisfecho.
        if (!favoritoConseguido && favorito != null)
        {
            foreach (Shelf s in GameManager.Instance.shelves)
            {
                if (s == null || !s.Disponible || s.product != favorito) continue;
                targetShelf = s;
                ultimaEstanteria = s;
                state = CustomerState.BuscandoProducto;
                SetDestination(s.customerSpot != null
                    ? s.customerSpot.position
                    : s.transform.position + Vector3.back * 1.5f);
                return;
            }
        }

        List<Shelf> candidatos = new List<Shelf>();
        foreach (Shelf shelf in GameManager.Instance.shelves)
        {
            if (shelf == null || !shelf.Disponible) continue;
            if (shelf == ultimaEstanteria && HayOtraOpcion(shelf)) continue;
            candidatos.Add(shelf);
        }

        if (candidatos.Count == 0)
        {
            GoToExit();
            return;
        }

        targetShelf = candidatos[Random.Range(0, candidatos.Count)];
        ultimaEstanteria = targetShelf;
        state = CustomerState.BuscandoProducto;
        SetDestination(targetShelf.customerSpot != null
            ? targetShelf.customerSpot.position
            : targetShelf.transform.position + Vector3.back * 1.5f);
    }

    /// <summary>Pide una ruta al grid. Si no hay grid, se mueve en línea recta.</summary>
    void SetDestination(Vector3 target)
    {
        destination = target;
        path.Clear();
        pathIndex = 0;

        if (NavigationGrid.Instance != null)
        {
            List<Vector3> ruta = NavigationGrid.Instance.FindPath(transform.position, target);
            if (ruta != null) path.AddRange(ruta);
        }
    }

    /// <summary>Tolerancia personal al precio, calculada al llegar a la caja.</summary>
    void CalcularPresupuesto()
    {
        float sumaBase = 0f;
        foreach (ProductData p in carro)
            if (p != null) sumaBase += GameManager.PrecioDe(p);

        presupuesto = Mathf.Max(1, Mathf.RoundToInt(sumaBase * Random.Range(1.08f, 1.45f)));
    }

    /// <summary>El cliente se va sin comprar porque le parece caro.</summary>
    public void Rechazar()
    {
        state = CustomerState.Saliendo;
        MostrarBarra(false);
        SetDestination(exitPoint != null
            ? exitPoint.position
            : transform.position + Vector3.back * 6f);
    }

    /// <summary>Se va por cansancio de esperar en la cola o en la caja.</summary>
    public void Abandonar()
    {
        if (state != CustomerState.EnCola && state != CustomerState.EsperandoEscaneo) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            if (gm.checkout != null) gm.checkout.Cancel(this);
            gm.RegisterAbandon();
        }
        if (HUD.Instance != null) HUD.Instance.ShowMessage("Un cliente se fue por la espera");
        MostrarBarra(false);
        GoToExit();
    }

    bool HayOtraOpcion(Shelf excluida)
    {
        if (GameManager.Instance == null) return false;
        foreach (Shelf s in GameManager.Instance.shelves)
            if (s != null && s != excluida && s.Disponible) return true;
        return false;
    }

    void Update()
    {
        // La paciencia corre mientras espera en la cola o ya en la caja.
        if (state == CustomerState.EnCola || state == CustomerState.EsperandoEscaneo)
        {
            tiempoEspera += Time.deltaTime;
            ActualizarBarra();
            if (tiempoEspera >= paciencia)
            {
                Abandonar();
                return;
            }
        }

        if (state == CustomerState.EsperandoEscaneo) return;

        if (state == CustomerState.EnCola)
        {
            Checkout caja = GameManager.Instance != null ? GameManager.Instance.checkout : null;
            if (caja != null && caja.IsFree)
            {
                CalcularPresupuesto();
                if (caja.Queue(this, new List<ProductData>(carro)))
                {
                    state = CustomerState.EsperandoEscaneo;
                    tiempoEspera = 0f;
                    MostrarBarra(true);
                }
            }
            return; // espera quieto en la cola
        }

        Move();

        Vector3 a = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 b = new Vector3(destination.x, 0f, destination.z);
        bool rutaTerminada = pathIndex >= path.Count;
        if (rutaTerminada && Vector3.Distance(a, b) < 0.5f) OnArrived();
    }

    void Move()
    {
        Vector3 target = pathIndex < path.Count ? path[pathIndex] : destination;
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.04f)
        {
            if (pathIndex < path.Count) pathIndex++;
            return;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            8f * Time.deltaTime);
    }

    void OnArrived()
    {
        switch (state)
        {
            case CustomerState.BuscandoProducto:
                if (targetShelf == null || !targetShelf.Disponible)
                {
                    ChooseShelf();
                    return;
                }

                targetShelf.TakeOne();
                carro.Add(targetShelf.product);
                productosPorCoger--;

                if (targetShelf.product == favorito) MarcarFavoritoConseguido();

                if (cesta != null)
                    cesta.transform.localScale = new Vector3(0.42f, 0.26f + carro.Count * 0.06f, 0.30f);

                if (productosPorCoger > 0)
                {
                    ChooseShelf();
                    return;
                }

                Checkout caja = GameManager.Instance != null ? GameManager.Instance.checkout : null;
                if (caja == null || caja.customerSpot == null)
                {
                    GoToExit();
                    return;
                }

                state = CustomerState.YendoACaja;
                SetDestination(caja.customerSpot.position);
                break;

            case CustomerState.YendoACaja:
                Checkout c = GameManager.Instance != null ? GameManager.Instance.checkout : null;
                if (c == null)
                {
                    GoToExit();
                    return;
                }

                if (c.IsFree)
                {
                    CalcularPresupuesto();
                    if (c.Queue(this, new List<ProductData>(carro)))
                    {
                        state = CustomerState.EsperandoEscaneo;
                        tiempoEspera = 0f;
                        MostrarBarra(true);
                    }
                }
                else
                {
                    state = CustomerState.EnCola;
                    tiempoEspera = 0f;
                    MostrarBarra(true);
                }
                break;

            case CustomerState.Saliendo:
                Leave();
                break;
        }
    }

    void GoToExit()
    {
        state = CustomerState.Saliendo;
        MostrarBarra(false);
        SetDestination(exitPoint != null
            ? exitPoint.position
            : transform.position + Vector3.back * 6f);
    }

    /// <summary>Lo llama la caja cuando el jugador termina de cobrar.</summary>
    public void Pay()
    {
        if (state == CustomerState.EsperandoEscaneo) GoToExit();
    }

    void Leave()
    {
        if (GameManager.Instance != null)
        {
            Checkout caja = GameManager.Instance.checkout;
            if (caja != null) caja.Cancel(this);
            GameManager.Instance.customers.Remove(this);
        }
        Object.Destroy(gameObject);
    }
}
