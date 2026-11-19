using System.Globalization;
using UnityEngine;

/// <summary>
/// Conversión de referencia del juego: 1 USD = 930 CUP (valor publicado por
/// elTOQUE el 18/11/2026). La tasa cambia cada día de partida, como en el
/// mercado informal cubano, pero se simula localmente: el juego no consulta
/// internet ni usa datos reales en tiempo real.
/// </summary>
public static class Currency
{
    public const float TasaReferencia = 930f;
    public static float TasaUSD = TasaReferencia;

    /// <summary>Importe en USD convertido a CUP con la tasa del día.</summary>
    public static int CUP(float usd)
    {
        return Mathf.RoundToInt(usd * TasaUSD);
    }

    /// <summary>CUP con separador de miles para la interfaz.</summary>
    public static string Format(int cup)
    {
        return cup.ToString("N0", new CultureInfo("es-ES"));
    }

    /// <summary>Avanza un día: pequeña variación diaria de la tasa.</summary>
    public static float NuevoDia()
    {
        float delta = Random.Range(-0.008f, 0.008f);
        TasaUSD = Mathf.Clamp(TasaUSD * (1f + delta), 850f, 1050f);
        return TasaUSD;
    }

    /// <summary>Vuelve a la tasa de referencia al reiniciar la partida.</summary>
    public static void Reset()
    {
        TasaUSD = TasaReferencia;
    }
}
