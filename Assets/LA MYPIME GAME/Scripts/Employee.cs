using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Empleado repositor: camina solo desde la zona de pedidos hasta la estantería
/// que necesita mercancía. El lote se paga al proveedor igual que lo pagaría el
/// jugador, así que el empleado ahorra trabajo, no dinero.
/// </summary>
public class Employee : MonoBehaviour
{
    enum State { Esperando, AlPedido, ALaEstanteria }

    [Tooltip("Velocidad al caminar")]
    public float speed = 2.1f;
    [Tooltip("Descanso entre reposiciones")]
    public float intervalo = 4f;
    [Tooltip("Orden de contratación: solo trabajan los primeros N empleados")]
    public int id;

    private State state = State.Esperando;
    private float espera;
    private DeliveryBox pedido;
    private Shelf destino;
    private ProductData producto;
    private int cantidad;
    private Vector3 destination;
    private readonly List<Vector3> path = new List<Vector3>();
    private int pathIndex;

    void Start()
    {
        espera = intervalo + id * 2f;
        if (GameManager.Instance != null && !GameManager.Instance.employeesNpc.Contains(this))
            GameManager.Instance.employeesNpc.Add(this);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.tiempoRestante <= 0f || gm.gameOver) return;

        switch (state)
        {
            case State.Esperando:
                espera -= Time.deltaTime;
                if (espera <= 0f) BuscarTarea();
                return; // mientras espera, se queda quieto
            case State.AlPedido:
                if (Llego()) RecogerPedido();
                break;
            case State.ALaEstanteria:
                if (Llego()) DejarMercancia();
                break;
        }

        Move();
    }

    /// <summary>Busca la estantería más vacía cuyo pedido esté disponible.</summary>
    void BuscarTarea()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) { espera = intervalo; return; }

        Shelf mejor = null;
        int menos = int.MaxValue;
        foreach (Shelf s in gm.shelves)
        {
            if (s == null || s.product == null || s.stock >= s.capacity) continue;
            if (s.stock < menos) { menos = s.stock; mejor = s; }
        }

        if (mejor == null) { espera = intervalo; return; }

        DeliveryBox caja = null;
        foreach (DeliveryBox b in gm.deliveryBoxes)
        {
            if (b == null || b.empty || b.product != mejor.product) continue;
            caja = b;
            break;
        }

        // Sin pedido: espera un poco y lo intenta de nuevo más tarde.
        if (caja == null) { espera = intervalo * 0.5f; return; }

        destino = mejor;
        pedido = caja;
        producto = mejor.product;
        cantidad = caja.amount;
        state = State.AlPedido;
        IrA(caja.transform.position + Vector3.forward * 1.2f);
    }

    void RecogerPedido()
    {
        bool yaPagado = pedido != null && pedido.pedidoPC;
        int coste = yaPagado ? 0 : GameManager.CosteDeCantidad(producto, cantidad);
        if (coste > 0 && GameManager.Instance != null && !GameManager.Instance.SpendMoney(coste))
        {
            if (HUD.Instance != null) HUD.Instance.ShowMessage("El empleado no tiene dinero para el pedido");
            state = State.Esperando;
            espera = intervalo;
            return;
        }

        if (pedido != null)
        {
            pedido.empty = true;
            pedido.pedidoPC = false;
            if (pedido.labelVisual != null) pedido.labelVisual.SetActive(false);
        }

        Vector3 punto = destino != null && destino.customerSpot != null
            ? destino.customerSpot.position
            : destino.transform.position + Vector3.back * 1.5f;
        state = State.ALaEstanteria;
        IrA(punto);
    }

    void DejarMercancia()
    {
        if (destino != null && producto != null)
        {
            destino.Restock(cantidad);
            if (HUD.Instance != null) HUD.Instance.ShowMessage($"Empleado repuso {cantidad}× {producto.nombre}");
        }

        state = State.Esperando;
        espera = intervalo;
        destino = null;
        pedido = null;
    }

    void IrA(Vector3 target)
    {
        destination = target;
        path.Clear();
        pathIndex = 0;

        if (NavigationGrid.Instance != null)
        {
            List<Vector3> ruta = NavigationGrid.Instance.FindPath(transform.position, target);
            if (ruta != null) path.AddRange(ruta);
        }
    }

    bool Llego()
    {
        Vector3 a = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 b = new Vector3(destination.x, 0f, destination.z);
        return pathIndex >= path.Count && Vector3.Distance(a, b) < 0.6f;
    }

    void Move()
    {
        Vector3 target = pathIndex < path.Count ? path[pathIndex] : destination;
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.04f)
        {
            if (pathIndex < path.Count) pathIndex++;
            return;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            8f * Time.deltaTime);
    }
}
