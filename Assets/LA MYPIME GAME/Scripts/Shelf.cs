using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estantería: guarda un tipo de producto, controla el stock y dibuja las unidades.
/// El jugador rellena el stock desde la zona de pedidos; los clientes lo vacían.
/// </summary>
public class Shelf : MonoBehaviour, IInteractable
{
    public ProductData product;
    public int capacity = 8;
    public int stock = 4;

    [Tooltip("Puntos donde aparece cada unidad visible")]
    public Transform[] slots;

    [Tooltip("Donde se coloca el cliente para coger un producto")]
    public Transform customerSpot;

    [Header("Nevera")]
    [Tooltip("Esta unidad enfría la mercancía")]
    public bool esNevera;
    [Tooltip("Coste diario de electricidad cuando está encendida")]
    public int costeLuz = Currency.CUP(0.5f);
    public bool encendida = true;
    [Tooltip("Segundos que tarda en estropearse un producto frío sin nevera")]
    public float tiempoDesperdicio = 45f;
    [Tooltip("Cubito indicador: verde = encendida, rojo = apagada")]
    public GameObject indicadorLuz;

    private float tiempoSinFrio;
    private bool estropeado;

    /// <summary>Los clientes solo cogen mercancía fresca y disponible.</summary>
    public bool Disponible => product != null && stock > 0 && !estropeado;
    public bool Estropeado => estropeado;
    private bool NecesitaFrio => product != null && product.necesitaFrio;

    /// <summary>Restaura el estado guardado de la nevera al cargar la partida.</summary>
    public void ApplySavedState(bool encendidaGuardada, bool estropeadoGuardado)
    {
        encendida = encendidaGuardada;
        estropeado = estropeadoGuardado;
        tiempoSinFrio = 0f;
        RefreshVisuals();
        RefreshFridge();
    }

    [SerializeField] private List<GameObject> visuals = new List<GameObject>();

    void Start()
    {
        if (GameManager.Instance != null && !GameManager.Instance.shelves.Contains(this))
            GameManager.Instance.shelves.Add(this);

        RefreshVisuals();
        RefreshFridge();
        GameManager.Notify();
    }

    void Update()
    {
        if (!NecesitaFrio || stock <= 0) return;

        if (esNevera && encendida)
        {
            if (estropeado)
            {
                estropeado = false;
                RefreshVisuals();
                GameManager.Notify();
            }
            tiempoSinFrio = 0f;
            return;
        }

        if (estropeado) return;

        tiempoSinFrio += Time.deltaTime;
        if (tiempoSinFrio < tiempoDesperdicio) return;

        estropeado = true;
        if (HUD.Instance != null)
            HUD.Instance.ShowMessage($"¡{product.nombre} estropeado! Enciende la nevera y repón");
        RefreshVisuals();
        GameManager.Notify();
    }

    void RefreshFridge()
    {
        if (indicadorLuz == null) return;
        Renderer r = indicadorLuz.GetComponent<Renderer>();
        if (r == null) return;
        r.sharedMaterial = MaterialFactory.Get(encendida
            ? new Color(0.20f, 0.90f, 0.35f)
            : new Color(0.95f, 0.22f, 0.20f));
    }

    public string GetPrompt(PlayerController player)
    {
        if (product == null) return "Estantería sin producto asignado";

        // Con las manos vacías, una nevera se enciende y se apaga.
        if (esNevera && (player.CarriedProduct == null || player.CarriedCount <= 0))
            return encendida ? "Apagar nevera (ahorrar luz)" : "Encender nevera";

        if (estropeado) return $"{product.nombre} estropeado: repón o enciende la nevera";

        if (NecesitaFrio && (!esNevera || !encendida))
            return $"¡Sin frío! {product.nombre} se estropeará";

        if (player.CarriedProduct == product && player.CarriedCount > 0)
        {
            int n = Mathf.Min(player.CarriedCount, capacity - stock);
            return n > 0 ? $"Reponer {product.nombre} ×{n}" : $"{product.nombre}: lleno";
        }

        return stock >= capacity
            ? $"{product.nombre}: lleno"
            : $"Trae {product.nombre} de la zona de pedidos";
    }

    public void Interact(PlayerController player)
    {
        if (product == null) return;

        if (esNevera && (player.CarriedProduct == null || player.CarriedCount <= 0))
        {
            encendida = !encendida;
            RefreshFridge();
            GameManager.Notify();
            SoundFx.Pickup();
            if (HUD.Instance != null)
                HUD.Instance.ShowMessage(encendida
                    ? "Nevera encendida"
                    : "Nevera apagada: ahorras luz, pero la mercancía se estropeará");
            return;
        }

        if (player.CarriedProduct != product || player.CarriedCount <= 0) return;

        int n = Mathf.Min(player.CarriedCount, capacity - stock);
        if (n <= 0) return;

        if (estropeado)
        {
            estropeado = false;
            tiempoSinFrio = 0f;
        }

        stock += n;
        player.RemoveCarried(n);    // se lo quitamos al jugador y actualizamos mano + HUD
        RefreshVisuals();
        GameManager.Notify();
        SoundFx.Pickup();

        if (HUD.Instance != null) HUD.Instance.ShowMessage($"Repuestos {n}× {product.nombre}");
    }

    /// <summary>Lo llama un empleado cuando repone un lote ya pagado al proveedor.</summary>
    public void Restock(int n)
    {
        if (product == null || n <= 0) return;
        int cantidad = Mathf.Min(n, capacity - stock);
        if (cantidad <= 0) return;

        if (estropeado)
        {
            estropeado = false;
            tiempoSinFrio = 0f;
        }

        stock += cantidad;
        RefreshVisuals();
        RefreshFridge();
        GameManager.Notify();
    }

    /// <summary>Lo llama un cliente cuando coge una unidad.</summary>
    public void TakeOne()
    {
        if (!Disponible) return;
        stock--;
        RefreshVisuals();
        GameManager.Notify();
    }

    /// <summary>Crea o destruye los cubos que representan el stock.</summary>
    public void RefreshVisuals()
    {
        foreach (GameObject go in visuals)
        {
            if (go == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(go);
            else Object.Destroy(go);
#else
            Object.Destroy(go);
#endif
        }
        visuals.Clear();

        if (product == null || slots == null) return;

        for (int i = 0; i < stock && i < slots.Length; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"{product.nombre}_{i + 1}";
            go.transform.SetParent(slots[i], false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = product.tamaño;

            // Sin collider: así el raycast del jugador siempre da en la estantería.
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) Object.DestroyImmediate(col);
                else Object.Destroy(col);
#else
                Object.Destroy(col);
#endif
            }

            Material mat = estropeado
                ? MaterialFactory.Get(new Color(0.34f, 0.32f, 0.27f))
                : (product.material != null ? product.material : MaterialFactory.Get(product.color));

            Renderer renderer = go.GetComponent<Renderer>();
            if (mat != null && renderer != null) renderer.sharedMaterial = mat;

            visuals.Add(go);
        }
    }
}
