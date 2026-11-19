using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Guarda la partida en un JSON dentro de la carpeta de datos persistente del dispositivo.
/// En Android/iOS esa carpeta es privada de la app y sobrevive a las actualizaciones.
/// </summary>
[Serializable]
public class SaveData
{
    [Tooltip("Versión del formato: 2 = economía convertida con USD × 930")]
    public int version = 2;
    public float tasaUSD = Currency.TasaReferencia;

    public int dia = 1;
    public int dinero = Currency.CUP(50f);
    public int ventasHoy;
    public float tiempoRestante;
    public float indicePrecios = 1f;
    public int reputacion = 70;
    public bool gameOver;
    public int empleados;

    // Campos añadidos sin cambiar la versión: partidas antiguas los leen como false.
    // Campos obsoletos del antiguo horario de dos viajes: se conservan para poder
    // leer partidas viejas, pero el triciclo ya no usa horarios.
    public bool entregaMananaHecha;
    public bool entregaCierreHecha;

    public List<string> nombresEstanterias = new List<string>();
    public List<int> stocks = new List<int>();
    public List<string> nombresPedidos = new List<string>();
    public List<bool> pedidosVacios = new List<bool>();
    public List<bool> neverasEncendidas = new List<bool>();
    public List<bool> estanteriasEstropeadas = new List<bool>();

    // Pedidos de la computadora y catálogo configurado (campos nuevos, partidas antiguas los leen vacíos).
    public List<string> pedidosPCProductos = new List<string>();
    public List<int> pedidosPCCCantidad = new List<int>();
    public List<int> pedidosPCLotes = new List<int>();
    public List<int> pedidosPCTotal = new List<int>();
    public List<string> catalogoProductosActivos = new List<string>();
    public bool catalogoConfigurado;

    // Tienda de muebles y ampliaciones (campos nuevos; las partidas antiguas los leen vacíos).
    public List<int> mueblesSpot = new List<int>();
    public List<int> mueblesTipo = new List<int>();
    public List<string> mueblesProducto = new List<string>();
    public List<bool> ampliacionesCompradas = new List<bool>();

    public string analyticsJson = "";
}

public static class SaveSystem
{
    private static string Ruta => Path.Combine(Application.persistentDataPath, "supermercado_guardado.json");

    public static void Save(GameManager gm)
    {
        if (gm == null) return;

        SaveData data = new SaveData
        {
            version = 2,
            tasaUSD = Currency.TasaUSD,
            dia = gm.dia,
            dinero = gm.dinero,
            ventasHoy = gm.ventasHoy,
            tiempoRestante = gm.tiempoRestante,
            indicePrecios = gm.indicePrecios,
            reputacion = gm.reputacion,
            gameOver = gm.gameOver,
            empleados = gm.empleados,
            analyticsJson = SalesAnalytics.Serialize()
        };

        foreach (Shelf shelf in gm.shelves)
        {
            if (shelf == null) continue;
            data.nombresEstanterias.Add(shelf.name);
            data.stocks.Add(shelf.stock);
            data.neverasEncendidas.Add(shelf.encendida);
            data.estanteriasEstropeadas.Add(shelf.Estropeado);
        }

        foreach (DeliveryBox box in gm.deliveryBoxes)
        {
            if (box == null) continue;
            data.nombresPedidos.Add(box.name);
            data.pedidosVacios.Add(box.empty);
        }

        if (gm.pedidosPendientes != null)
        {
            foreach (GameManager.PedidoPendiente p in gm.pedidosPendientes)
            {
                if (p == null) continue;
                data.pedidosPCProductos.Add(p.producto);
                data.pedidosPCCCantidad.Add(p.cantidad);
                data.pedidosPCLotes.Add(p.lote);
                data.pedidosPCTotal.Add(p.totalPagado);
            }
        }

        if (gm.catalogoPedidos != null)
        {
            data.catalogoConfigurado = true;
            foreach (OrderCatalog.Entry e in gm.catalogoPedidos.productos)
                if (e != null && e.activo && e.producto != null)
                    data.catalogoProductosActivos.Add(e.producto.nombre);
        }

        if (gm.tienda != null)
        {
            foreach (BuildSpot spot in gm.tienda.spots)
            {
                if (spot == null || !spot.ocupado) continue;
                data.mueblesSpot.Add(spot.id);
                data.mueblesTipo.Add((int)spot.tipoConstruido);
                data.mueblesProducto.Add(spot.productoConstruido != null ? spot.productoConstruido.nombre : "");
            }

            foreach (ExpansionZone zona in gm.tienda.expansiones)
                data.ampliacionesCompradas.Add(zona != null && zona.comprada);
        }

        try
        {
            File.WriteAllText(Ruta, JsonUtility.ToJson(data, true));
            Debug.Log("Partida guardada en: " + Ruta);
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo guardar: " + e.Message);
        }
    }

    public static bool Load(GameManager gm)
    {
        if (gm == null || !File.Exists(Ruta)) return false;

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Ruta));
            if (data == null || data.version < 2) return false; // partida antigua, se ignora

            Currency.TasaUSD = Mathf.Clamp(data.tasaUSD, 850f, 1050f);
            gm.dia = data.dia;
            gm.dinero = data.dinero;
            gm.ventasHoy = data.ventasHoy;
            gm.tiempoRestante = Mathf.Clamp(data.tiempoRestante, 1f, gm.duracionDia);
            gm.indicePrecios = Mathf.Max(1f, data.indicePrecios);
            gm.reputacion = Mathf.Clamp(data.reputacion, 0, 100);
            gm.gameOver = data.gameOver;
            gm.empleados = Mathf.Clamp(data.empleados, 0, gm.maxEmpleados);
            SalesAnalytics.Deserialize(data.analyticsJson);

            // Reconstruir muebles comprados y ampliaciones ANTES de restaurar stocks,
            // porque las estanterías nuevas se buscan por nombre en la lista.
            if (gm.tienda != null)
                gm.tienda.AplicarGuardado(data.mueblesSpot, data.mueblesTipo,
                    data.mueblesProducto, data.ampliacionesCompradas);

            for (int i = 0; i < data.nombresEstanterias.Count && i < data.stocks.Count; i++)
            {
                Shelf shelf = gm.shelves.Find(s => s != null && s.name == data.nombresEstanterias[i]);
                if (shelf == null) continue;
                shelf.stock = Mathf.Clamp(data.stocks[i], 0, shelf.capacity);
                bool encendida = i < data.neverasEncendidas.Count && data.neverasEncendidas[i];
                bool estropeada = i < data.estanteriasEstropeadas.Count && data.estanteriasEstropeadas[i];
                shelf.ApplySavedState(encendida, estropeada);
            }

            for (int i = 0; i < data.nombresPedidos.Count && i < data.pedidosVacios.Count; i++)
            {
                DeliveryBox box = gm.deliveryBoxes.Find(b => b != null && b.name == data.nombresPedidos[i]);
                if (box == null) continue;
                box.empty = data.pedidosVacios[i];
                if (box.labelVisual != null) box.labelVisual.SetActive(!box.empty);
            }

            // Catálogo: si el guardado tiene datos, respeta los productos activados por el jugador.
            if (gm.catalogoPedidos != null && data.catalogoConfigurado)
            {
                foreach (OrderCatalog.Entry e in gm.catalogoPedidos.productos)
                {
                    if (e == null || e.producto == null) continue;
                    e.activo = data.catalogoProductosActivos.Contains(e.producto.nombre);
                }
            }

            if (gm.pedidosPendientes == null) gm.pedidosPendientes = new List<GameManager.PedidoPendiente>();
            else gm.pedidosPendientes.Clear();
            for (int i = 0; i < data.pedidosPCProductos.Count && i < data.pedidosPCCCantidad.Count; i++)
            {
                gm.pedidosPendientes.Add(new GameManager.PedidoPendiente
                {
                    producto = data.pedidosPCProductos[i],
                    cantidad = data.pedidosPCCCantidad[i],
                    lote = i < data.pedidosPCLotes.Count ? data.pedidosPCLotes[i] : 8,
                    totalPagado = i < data.pedidosPCTotal.Count ? data.pedidosPCTotal[i] : 0
                });
            }

            GameManager.Notify();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo cargar la partida: " + e.Message);
            return false;
        }
    }

    public static void Delete()
    {
        if (File.Exists(Ruta)) File.Delete(Ruta);
    }
}
