using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ampliación del local: un ala nueva cerrada por una persiana metálica.
/// Al comprarla se retira la persiana, se activan sus huecos de muebles y
/// el local gana sitio para estanterías y algún cliente más.
/// Las ampliaciones son secuenciales, como en el juego original.
/// </summary>
public class ExpansionZone : MonoBehaviour, IInteractable
{
    [Tooltip("Índice de la ampliación (0 = la primera)")]
    public int id;
    public string nombre = "Ampliación";
    public string descripcion = "Más espacio para muebles";

    [Tooltip("Ampliación que debe comprarse antes (-1 = ninguna)")]
    public int indiceRequerido = -1;

    [Tooltip("Clientes simultáneos que añade")]
    public int clientesExtra = 1;

    [Header("Estado")]
    public bool comprada;
    public List<BuildSpot> spots = new List<BuildSpot>();
    [Tooltip("Persiana, cartel y cualquier cosa que se retira al comprar")]
    public List<GameObject> obstaculos = new List<GameObject>();

    public string GetPrompt(PlayerController player)
    {
        if (comprada) return "";

        StoreManager tienda = StoreManager.Instance;
        if (tienda == null) return nombre;

        if (!tienda.PuedeComprarAmpliacion(id))
            return $"{nombre}: primero compra {tienda.NombreAmpliacion(indiceRequerido)}";

        return $"{nombre} · {Currency.Format(tienda.PrecioAmpliacion(id))} CUP · abrir tienda";
    }

    public void Interact(PlayerController player)
    {
        if (comprada || StoreUI.Instance == null) return;
        StoreUI.Instance.OpenAmpliaciones(id);
    }

    /// <summary>Retira la persiana y habilita los huecos de esta ala.</summary>
    public void Abrir()
    {
        if (comprada) return;
        comprada = true;

        foreach (GameObject go in obstaculos)
            if (go != null) go.SetActive(false);

        foreach (BuildSpot spot in spots)
            if (spot != null) spot.gameObject.SetActive(true);
    }
}
