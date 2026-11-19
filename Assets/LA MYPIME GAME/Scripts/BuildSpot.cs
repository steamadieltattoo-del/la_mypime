using UnityEngine;

/// <summary>
/// Hueco de obra donde el jugador puede comprar y colocar un mueble nuevo
/// (estante, góndola o nevera), igual que en Supermarket Simulator.
/// Mientras está libre muestra un cartel de obra; al construir el mueble
/// el cartel desaparece y queda la estantería jugable.
/// </summary>
public class BuildSpot : MonoBehaviour, IInteractable
{
    [Tooltip("Identificador fijo para guardar la partida")]
    public int id;
    [Tooltip("Nombre visible del hueco")]
    public string etiqueta = "Hueco de obra";

    [Tooltip("Cartel y marca de obra que se ocultan al construir")]
    public GameObject obra;
    [Tooltip("Zona táctil del cartel (se desactiva al construir)")]
    public BoxCollider zona;

    [Header("Estado")]
    public bool ocupado;
    public TipoMueble tipoConstruido;
    public ProductData productoConstruido;
    public Shelf mueble;

    public bool Libre => !ocupado;

    void Start()
    {
        if (StoreManager.Instance != null) StoreManager.Instance.RegistrarSpot(this);
    }

    public string GetPrompt(PlayerController player)
    {
        if (ocupado) return "";
        return $"Construir mueble · {etiqueta}";
    }

    public void Interact(PlayerController player)
    {
        if (ocupado) return;
        if (StoreUI.Instance == null) return;
        StoreUI.Instance.OpenMuebles(this);
    }

    /// <summary>Crea el mueble, ocupa el hueco y lo registra en la partida.</summary>
    public Shelf Construir(TipoMueble tipo, ProductData producto)
    {
        if (ocupado) return null;

        Shelf nuevo = StoreManager.CrearMueble(tipo, transform.position, transform.eulerAngles.y, producto);
        if (nuevo == null) return null;

        nuevo.name = $"Mueble_{id}_{(producto != null ? producto.nombre : "Vacio")}";
        ocupado = true;
        tipoConstruido = tipo;
        productoConstruido = producto;
        mueble = nuevo;

        if (obra != null) obra.SetActive(false);
        if (zona != null) zona.enabled = false;

        GameManager gm = GameManager.Instance;
        if (gm != null && !gm.shelves.Contains(nuevo)) gm.shelves.Add(nuevo);

        return nuevo;
    }
}
