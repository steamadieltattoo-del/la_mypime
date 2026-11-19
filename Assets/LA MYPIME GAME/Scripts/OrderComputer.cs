using UnityEngine;

/// <summary>
/// Computadora de pedidos del almacén. Al interactuar abre la pantalla del proveedor,
/// inspirada en Supermarket Simulator: eliges productos y cantidades y pagas al pedir.
/// </summary>
public class OrderComputer : MonoBehaviour, IInteractable
{
    [Tooltip("Productos que se pueden pedir. Se edita en Assets/LA MYPIME GAME/Data")]
    public OrderCatalog catalogo;

    public string GetPrompt(PlayerController player)
    {
        return "Abrir computadora de pedidos";
    }

    public void Interact(PlayerController player)
    {
        if (OrderComputerUI.Instance == null) return;
        if (catalogo == null && GameManager.Instance != null)
            catalogo = GameManager.Instance.catalogoPedidos;
        if (catalogo == null)
        {
            if (HUD.Instance != null) HUD.Instance.ShowMessage("Falta el catálogo de pedidos");
            return;
        }
        OrderComputerUI.Instance.Open(catalogo);
    }
}
