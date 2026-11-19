using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Caja registradora con minijuego de escaneo:
/// el cliente deja sus productos en la cinta, el jugador escanea uno a uno
/// y después cobra el total.
/// </summary>
public class Checkout : MonoBehaviour, IInteractable
{
    public Customer currentCustomer;
    public int total;

    [Tooltip("Donde se coloca el cliente mientras espera")]
    public Transform customerSpot;

    [Tooltip("Huecos de la cinta donde aparecen los productos por escanear")]
    public Transform[] scanSlots;

    public bool IsFree => currentCustomer == null;

    private readonly List<ProductData> pendientes = new List<ProductData>();
    private readonly List<ProductData> escaneados = new List<ProductData>();
    private readonly List<GameObject> itemVisuals = new List<GameObject>();

    public string GetPrompt(PlayerController player)
    {
        if (currentCustomer == null) return "Caja libre";
        if (pendientes.Count > 0) return $"Escanear {pendientes[0].nombre} ({pendientes.Count} pendientes)";
        return $"Cobrar {Currency.Format(total)} CUP";
    }

    public void Interact(PlayerController player)
    {
        if (currentCustomer == null) return;

        if (pendientes.Count > 0) ScanNext();
        else Charge();
    }

    /// <summary>Devuelve false si el cliente rechaza el precio y se marcha.</summary>
    public bool Queue(Customer customer, List<ProductData> productos)
    {
        pendientes.Clear();
        escaneados.Clear();
        if (productos != null) pendientes.AddRange(productos);

        total = 0;
        foreach (ProductData p in pendientes)
            total += GameManager.PrecioDe(p);

        if (customer != null && total > customer.presupuesto)
        {
            pendientes.Clear();
            if (GameManager.Instance != null) GameManager.Instance.RegisterRejection();
            customer.Rechazar();
            if (HUD.Instance != null) HUD.Instance.ShowMessage($"Se fue: {Currency.Format(total)} CUP le pareció caro");
            GameManager.Notify();
            return false;
        }

        currentCustomer = customer;
        CreateItemVisuals();
        GameManager.Notify();
        return true;
    }

    void ScanNext()
    {
        ProductData producto = pendientes[0];
        pendientes.RemoveAt(0);
        escaneados.Add(producto);

        if (itemVisuals.Count > 0)
        {
            GameObject visual = itemVisuals[0];
            itemVisuals.RemoveAt(0);
            if (visual != null) Destroy(visual);
        }

        SoundFx.Scan();
        if (HUD.Instance != null) HUD.Instance.ShowMessage($"Escaneado: {producto.nombre}");
        GameManager.Notify();
    }

    void Charge()
    {
        Customer cliente = currentCustomer;
        int importe = total;

        SalesAnalytics.RegisterSale(escaneados, importe);
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterSale(importe, cliente != null && cliente.Satisfecho);

        currentCustomer = null;
        total = 0;
        escaneados.Clear();
        ClearItemVisuals();
        cliente.Pay();

        SoundFx.Cash();
        if (HUD.Instance != null) HUD.Instance.ShowMessage($"Cobrado: {Currency.Format(importe)} CUP");
        GameManager.Notify();
    }

    public void Cancel(Customer customer)
    {
        if (currentCustomer != customer) return;
        currentCustomer = null;
        total = 0;
        pendientes.Clear();
        escaneados.Clear();
        ClearItemVisuals();
        GameManager.Notify();
    }

    void CreateItemVisuals()
    {
        ClearItemVisuals();
        if (scanSlots == null) return;

        for (int i = 0; i < pendientes.Count && i < scanSlots.Length; i++)
        {
            ProductData p = pendientes[i];
            if (p == null || scanSlots[i] == null) continue;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Producto cinta - " + p.nombre;
            go.transform.SetParent(scanSlots[i], false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = p.tamaño;

            Collider col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Material mat = p.material != null ? p.material : MaterialFactory.Get(p.color);
            Renderer renderer = go.GetComponent<Renderer>();
            if (mat != null && renderer != null) renderer.sharedMaterial = mat;

            itemVisuals.Add(go);
        }
    }

    void ClearItemVisuals()
    {
        foreach (GameObject go in itemVisuals)
            if (go != null) Destroy(go);
        itemVisuals.Clear();
    }
}
