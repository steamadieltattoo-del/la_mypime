using UnityEngine;

/// <summary>
/// Define un producto del supermercado.
/// Crea nuevos productos con: Botón derecho > Create > Supermarket > Producto
/// </summary>
[CreateAssetMenu(fileName = "Producto", menuName = "Supermarket/Producto")]
public class ProductData : ScriptableObject
{
    [Header("Datos visibles")]
    public string nombre = "Producto";
    [Tooltip("Precio base en USD de referencia; se convierte a CUP con la tasa del juego")]
    public float precioUSD = 1f;
    [Tooltip("Coste del pedido en USD de referencia (caja de 8 unidades)")]
    public float costePedidoUSD = 2f;
    [Tooltip("Precio al que se vende al cliente, ya en CUP")]
    public int precio = 930;
    [Tooltip("Coste de pedir una caja de 8 unidades al proveedor, ya en CUP")]
    public int costePedido = 1860;
    public Color color = new Color(0.85f, 0.3f, 0.25f);

    [Header("Preferencia de clientes")]
    [Tooltip("Peso con el que los clientes eligen este producto. 1 = normal, 3 = muy demandado")]
    public float demanda = 1f;

    [Header("Conservación")]
    [Tooltip("Necesita nevera: se estropea si se guarda sin frío")]
    public bool necesitaFrio;

    [Header("Formato en la estantería")]
    public Vector3 tamaño = new Vector3(0.22f, 0.36f, 0.22f);

    [Tooltip("Material creado por el generador de escena. Si está vacío se usa el color de arriba.")]
    public Material material;
}
