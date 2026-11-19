using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Registra cuánto se vende de cada producto, hoy y en días anteriores.
/// Es ficticio y solo sirve como herramienta de gestión dentro del juego.
/// </summary>
public static class SalesAnalytics
{
    [Serializable]
    public class DayReport
    {
        public int dia;
        public int ingresos;
        public int unidades;
        public string top = "";
    }

    [Serializable]
    private class AnalyticsSave
    {
        public List<DayReport> historial = new List<DayReport>();
        public List<string> nombres = new List<string>();
        public List<int> unidades = new List<int>();
    }

    private static readonly Dictionary<string, int> unidadesHoyPorProducto = new Dictionary<string, int>();
    private static readonly Dictionary<string, int> ingresosHoyPorProducto = new Dictionary<string, int>();
    private static readonly Dictionary<string, int> unidadesTotales = new Dictionary<string, int>();
    private static readonly List<DayReport> historial = new List<DayReport>();

    private static int ingresosHoy;
    private static int unidadesHoy;

    /// <summary>Llama la caja cuando termina un cobro.</summary>
    public static void RegisterSale(List<ProductData> productos, int importe)
    {
        ingresosHoy += importe;
        if (productos == null) return;

        foreach (ProductData p in productos)
        {
            if (p == null) continue;
            string nombre = string.IsNullOrEmpty(p.nombre) ? "Producto" : p.nombre;

            unidadesHoy++;
            Sumar(unidadesHoyPorProducto, nombre, 1);
            Sumar(ingresosHoyPorProducto, nombre, GameManager.PrecioDe(p));
            Sumar(unidadesTotales, nombre, 1);
        }
    }

    static void Sumar(Dictionary<string, int> datos, string clave, int valor)
    {
        int actual;
        datos[clave] = datos.TryGetValue(clave, out actual) ? actual + valor : valor;
    }

    /// <summary>Cierra el informe del día y empieza el contador siguiente.</summary>
    public static void NewDay()
    {
        int dia = GameManager.Instance != null ? GameManager.Instance.dia : 0;
        historial.Add(new DayReport
        {
            dia = dia,
            ingresos = ingresosHoy,
            unidades = unidadesHoy,
            top = TopDeHoy()
        });
        if (historial.Count > 10) historial.RemoveAt(0);

        ingresosHoy = 0;
        unidadesHoy = 0;
        unidadesHoyPorProducto.Clear();
        ingresosHoyPorProducto.Clear();
    }

    static string TopDeHoy()
    {
        if (unidadesHoyPorProducto.Count == 0) return "sin ventas";
        return string.Join(", ", unidadesHoyPorProducto
            .OrderByDescending(x => x.Value)
            .Take(2)
            .Select(x => $"{x.Key} ×{x.Value}"));
    }

    /// <summary>Texto listo para mostrar en el panel del jugador.</summary>
    public static string Report()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("ANALÍTICA DE VENTAS");
        sb.AppendLine($"{new string('─', 24)}");
        sb.AppendLine($"HOY  ·  {Currency.Format(ingresosHoy)} CUP  ·  {unidadesHoy} uds");
        sb.AppendLine("Por producto (hoy):");

        if (unidadesHoyPorProducto.Count == 0) sb.AppendLine("  sin ventas todavía");
        else
        {
            foreach (KeyValuePair<string, int> kv in unidadesHoyPorProducto.OrderByDescending(x => x.Value))
            {
                int ingreso;
                ingresosHoyPorProducto.TryGetValue(kv.Key, out ingreso);
                sb.AppendLine($"  {kv.Key,-10} {kv.Value,3} uds   {Currency.Format(ingreso),10} CUP");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Acumulado por producto:");
        if (unidadesTotales.Count == 0) sb.AppendLine("  sin datos");
        else
            foreach (KeyValuePair<string, int> kv in unidadesTotales.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kv.Key,-10} {kv.Value,3} uds");

        if (historial.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Últimos días:");
            foreach (DayReport d in historial.AsEnumerable().Reverse().Take(5))
                sb.AppendLine($"  Día {d.dia}: {Currency.Format(d.ingresos)} CUP · {d.unidades} uds · {d.top}");
        }

        return sb.ToString();
    }

    /// <summary>Guarda el historial en un único campo JSON.</summary>
    public static string Serialize()
    {
        AnalyticsSave data = new AnalyticsSave();
        data.historial.AddRange(historial);
        foreach (KeyValuePair<string, int> kv in unidadesTotales)
        {
            data.nombres.Add(kv.Key);
            data.unidades.Add(kv.Value);
        }
        return JsonUtility.ToJson(data);
    }

    public static void Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            AnalyticsSave data = JsonUtility.FromJson<AnalyticsSave>(json);
            if (data == null) return;

            historial.Clear();
            historial.AddRange(data.historial);
            unidadesTotales.Clear();
            for (int i = 0; i < data.nombres.Count && i < data.unidades.Count; i++)
                unidadesTotales[data.nombres[i]] = data.unidades[i];

            ingresosHoy = 0;
            unidadesHoy = 0;
            unidadesHoyPorProducto.Clear();
            ingresosHoyPorProducto.Clear();
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo cargar la analítica: " + e.Message);
        }
    }

    public static void Reset()
    {
        historial.Clear();
        unidadesTotales.Clear();
        unidadesHoyPorProducto.Clear();
        ingresosHoyPorProducto.Clear();
        ingresosHoy = 0;
        unidadesHoy = 0;
    }
}
