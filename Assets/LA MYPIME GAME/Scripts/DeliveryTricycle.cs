using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triciclo del proveedor: entra por la carretera exterior, para en el muelle
/// y deja SOLO los pedidos pagados en las cajas del almacén. NUNCA repone
/// estanterías: eso lo hacen el jugador o los empleados. No tiene horario:
/// sale cuando se confirma un pedido en la computadora.
/// </summary>
public class DeliveryTricycle : MonoBehaviour
{
    [Tooltip("Puntos de la ruta: carretera → centro → muelle. Descarga en almacén, nunca en estanterías")]
    public Transform[] waypoints;
    [Tooltip("Velocidad del triciclo")]
    public float speed = 3.5f;
    [Tooltip("Ruedas que giran mientras avanza")]
    public Transform[] ruedas;

    [Header("Estado")]
    public bool enRuta;

    private int waypointIndex;
    private List<DeliveryBox> cajas;
    private List<bool> pedidosQueLlegan;
    private bool descargado;

    /// <summary>Inicia la ruta con la lista de pedidos que debe dejar.</summary>
    public void Llegar(List<DeliveryBox> boxes, List<bool> llega)
    {
        cajas = boxes != null ? new List<DeliveryBox>(boxes) : new List<DeliveryBox>();
        pedidosQueLlegan = llega != null ? new List<bool>(llega) : new List<bool>();

        waypointIndex = 0;
        descargado = false;
        enRuta = true;
        gameObject.SetActive(true);

        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            if (waypoints.Length > 1 && waypoints[1] != null)
            {
                Vector3 inicial = waypoints[1].position - waypoints[0].position;
                inicial.y = 0f;
                if (inicial.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(-inicial.normalized);
            }
        }
    }

    void Update()
    {
        if (!enRuta) return;

        if (waypoints == null || waypoints.Length == 0 || waypointIndex >= waypoints.Length)
        {
            Descargar();
            return;
        }

        Transform destino = waypoints[waypointIndex];
        if (destino == null) { waypointIndex++; return; }

        Vector3 dir = destino.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.09f)
        {
            waypointIndex++;
            return;
        }

        Vector3 paso = dir.normalized * speed * Time.deltaTime;
        if (paso.magnitude > dir.magnitude) paso = dir;
        transform.position += paso;
        // El modelo tiene el morro en -Z; usamos -dir para que avance hacia delante.
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(-dir),
            6f * Time.deltaTime);

        if (ruedas != null)
            foreach (Transform r in ruedas)
                if (r != null) r.Rotate(Vector3.left, speed * Time.deltaTime * 22f, Space.Self);
    }

    /// <summary>Deja los pedidos disponibles en el almacén y avisa al jugador.</summary>
    void Descargar()
    {
        if (descargado) return;
        descargado = true;
        enRuta = false;

        int entregados = 0;
        for (int i = 0; i < cajas.Count && i < pedidosQueLlegan.Count; i++)
        {
            DeliveryBox caja = cajas[i];
            if (caja == null) continue;

            if (pedidosQueLlegan[i])
            {
                caja.Refill();
                entregados++;
            }
            else caja.SinPedido();
        }

        if (HUD.Instance != null)
            HUD.Instance.ShowMessage($"El triciclo dejó {entregados} pedido(s) en el almacén — llévalos a las estanterías");

        if (GameManager.Instance != null)
        {
            GameManager.Notify();
            SaveSystem.Save(GameManager.Instance);
            // Si se hicieron pedidos mientras veníamos en camino, sale el siguiente viaje.
            GameManager.Instance.IntentarEnviarTriciclo();
        }
    }
}
