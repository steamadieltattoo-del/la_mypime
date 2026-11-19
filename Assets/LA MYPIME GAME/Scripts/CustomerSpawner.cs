using UnityEngine;

/// <summary>
/// Genera nuevos clientes en la entrada cada pocos segundos, con un límite simultáneo.
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    public Transform exitPoint;
    public float interval = 6f;
    public int maxCustomers = 4;

    private float timer;

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        if (GameManager.Instance.customers.Count >= maxCustomers) return;

        // Más reputación = más clientes (intervalo más corto).
        float factor = GameManager.Instance != null
            ? Mathf.Lerp(1.7f, 0.55f, GameManager.Instance.reputacion / 100f)
            : 1f;

        timer += Time.deltaTime;
        if (timer < interval * factor) return;

        timer = 0f;
        GameObject go = new GameObject("Cliente");
        go.transform.position = transform.position;

        Customer cliente = go.AddComponent<Customer>();
        cliente.exitPoint = exitPoint;
    }
}
