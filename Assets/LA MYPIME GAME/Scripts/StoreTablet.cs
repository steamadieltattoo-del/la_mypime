using UnityEngine;

/// <summary>
/// Tablet de gestión del local. Al interactuar abre la tienda de muebles
/// y ampliaciones (la misma pantalla que los huecos de obra).
/// </summary>
public class StoreTablet : MonoBehaviour, IInteractable
{
    public string GetPrompt(PlayerController player)
    {
        return "Abrir tienda de muebles y ampliaciones";
    }

    public void Interact(PlayerController player)
    {
        if (StoreUI.Instance == null) return;
        StoreUI.Instance.OpenMuebles(null);
    }
}
