using UnityEngine;

/// <summary>
/// Caja de pedido en la trastienda de la MYPIME. Al interactuar, el dependiente
/// coge un lote de un producto y lo lleva hasta la estantería.
/// </summary>
public class DeliveryBox : MonoBehaviour, IInteractable
{
    public ProductData product;
    public int amount = 8;
    public bool empty = false;

    [Tooltip("Etiqueta de color que se apaga al vaciar la caja")]
    public GameObject labelVisual;
    [Tooltip("Esta caja contiene un pedido pagado en la computadora: el triciclo siempre la trae")]
    public bool pedidoPC;

    void Start()
    {
        if (GameManager.Instance != null && !GameManager.Instance.deliveryBoxes.Contains(this))
            GameManager.Instance.deliveryBoxes.Add(this);
    }

    /// <summary>Configura la caja con un pedido de la computadora.</summary>
    public void Asignar(ProductData producto, int unidades)
    {
        if (producto == null) return;
        product = producto;
        amount = Mathf.Max(1, unidades);
        empty = false;
        pedidoPC = true;
        if (labelVisual != null)
        {
            labelVisual.SetActive(true);
            Renderer r = labelVisual.GetComponent<Renderer>();
            Material m = producto.material != null ? producto.material : MaterialFactory.Get(producto.color);
            if (r != null && m != null) r.sharedMaterial = m;
        }
    }

    /// <summary>Vuelve a llenar la caja al empezar un día nuevo.</summary>
    public void Refill()
    {
        empty = false;
        // pedidoPC se mantiene hasta que el jugador recoge la caja: ya está pagado.
        if (labelVisual != null) labelVisual.SetActive(true);
    }

    /// <summary>Deja la caja vacía cuando no hay pedido pagado para ella.</summary>
    public void SinPedido()
    {
        empty = true;
        pedidoPC = false;
        if (labelVisual != null) labelVisual.SetActive(false);
    }

    public string GetPrompt(PlayerController player)
    {
        if (empty) return $"El proveedor no trajo {product.nombre} hoy";

        if (player.CarriedProduct != null && player.CarriedProduct != product && player.CarriedCount > 0)
            return "Termina primero el producto que llevas";

        return pedidoPC
            ? $"Recoger {amount}× {product.nombre} (pagado en la computadora)"
            : $"Coger {amount}× {product.nombre} ({Currency.Format(GameManager.CosteDeCantidad(product, amount))} CUP)";
    }

    public void Interact(PlayerController player)
    {
        if (empty || product == null) return;
        if (player.CarriedProduct != null && player.CarriedProduct != product && player.CarriedCount > 0) return;

        int coste = pedidoPC ? 0 : GameManager.CosteDeCantidad(product, amount);
        if (coste > 0 && GameManager.Instance != null && !GameManager.Instance.SpendMoney(coste))
        {
            if (HUD.Instance != null) HUD.Instance.ShowMessage("No te alcanza el dinero para el pedido");
            return;
        }

        bool yaPagado = pedidoPC;
        player.PickUp(product, amount);
        empty = true;
        pedidoPC = false;
        if (labelVisual != null) labelVisual.SetActive(false);
        SoundFx.Pickup();

        if (HUD.Instance != null)
            HUD.Instance.ShowMessage(yaPagado
                ? $"Recoges {amount}× {product.nombre} · pedido ya pagado en la computadora"
                : $"Compraste {amount}× {product.nombre} por {Currency.Format(coste)} CUP");
    }
}
