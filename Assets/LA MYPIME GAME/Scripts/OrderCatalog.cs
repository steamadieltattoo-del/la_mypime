using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catálogo del proveedor: aquí se pone QUÉ productos se pueden pedir desde la computadora.
/// Se edita en el Inspector (Assets/LA MYPIME GAME/Data/CatalogoPedidos.asset)
/// y también se puede activar/desactivar cada producto desde la propia pantalla del juego.
/// </summary>
[CreateAssetMenu(fileName = "CatalogoPedidos", menuName = "Supermarket/Catálogo de pedidos")]
public class OrderCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public ProductData producto;
        [Tooltip("Si está desactivado, el producto no aparece en la computadora")]
        public bool activo = true;
        [Tooltip("Unidades que vienen en una caja")]
        public int lote = 8;
        [Tooltip("Máximo de unidades que se pueden pedir de una vez")]
        public int maximo = 32;
    }

    public List<Entry> productos = new List<Entry>();

    /// <summary>Busca la configuración de un producto por referencia.</summary>
    public Entry Find(ProductData p)
    {
        if (p == null) return null;
        foreach (Entry e in productos)
            if (e != null && e.producto == p) return e;
        return null;
    }

    /// <summary>Busca un producto del catálogo por nombre (para cargar la partida guardada).</summary>
    public ProductData FindProduct(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return null;
        foreach (Entry e in productos)
            if (e != null && e.producto != null && e.producto.nombre == nombre) return e.producto;
        return null;
    }

    /// <summary>Número de productos visibles en la computadora.</summary>
    public int Activos
    {
        get
        {
            int n = 0;
            foreach (Entry e in productos)
                if (e != null && e.activo && e.producto != null) n++;
            return n;
        }
    }
}
