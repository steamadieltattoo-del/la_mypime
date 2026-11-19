using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cerebro del juego: dinero, tiempo, ventas y referencias a los objetos de la escena.
/// Es un "Singleton": cualquier script puede acceder con GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economía")]
    public int dinero = Currency.CUP(50f);
    public int ventasHoy = 0;
    public int dia = 1;

    [Header("Índice de precios (ficticio, para el juego)")]
    [Tooltip("1.00 = precios base. Sube cada día.")]
    public float indicePrecios = 1f;
    [Tooltip("Aumento diario del índice: 0.08 = 8% por día")]
    public float inflacionDiaria = 0.08f;

    [Header("Objetivos diarios")]
    [Tooltip("Ventas mínimas del día 1 (se ajustan con el índice)")]
    public int metaBase = Currency.CUP(30f);
    [Tooltip("Alquiler del local en el día 1 (se ajusta con el índice)")]
    public int alquilerBase = Currency.CUP(20f);
    public int metaVentas = Currency.CUP(30f);
    public int alquiler = Currency.CUP(20f);
    [Tooltip("Gasto diario de las neveras encendidas")]
    public int electricidad;

    [Header("Empleados")]
    [Tooltip("Cuántos empleados hay contratados ahora mismo")]
    public int empleados;
    [Tooltip("Máximo de empleados simultáneos")]
    public int maxEmpleados = 2;
    [Tooltip("Salario por empleado y día")]
    public int salarioPorEmpleado = Currency.CUP(12f);
    [Tooltip("Pago único al contratar")]
    public int costeContratacion = Currency.CUP(15f);

    /// <summary>Sueldo total que se paga al cerrar el día.</summary>
    public int Salarios => empleados * salarioPorEmpleado;

    [Tooltip("0-100. Afecta a cuántos clientes entran")]
    public int reputacion = 70;

    [Header("Estadísticas del día")]
    public int clientesAtendidos;
    public int clientesSatisfechos;
    public int clientesPerdidos;
    [Tooltip("Se activa si no puedes pagar el alquiler")]
    public bool gameOver;

    private float gameOverTimer;

    [Header("Día")]
    [Tooltip("Duración de la jornada en segundos")]
    public float duracionDia = 180f;
    public float tiempoRestante;

    [Header("Referencias de la escena")]
    public List<Shelf> shelves = new List<Shelf>();
    public List<DeliveryBox> deliveryBoxes = new List<DeliveryBox>();
    public List<Customer> customers = new List<Customer>();
    public List<Employee> employeesNpc = new List<Employee>();
    public Checkout checkout;
    public DeliveryTricycle triciclo;

    [Header("Pedidos por computadora")]
    [Tooltip("Productos que se pueden pedir. Se edita en Assets/LA MYPIME GAME/Data/CatalogoPedidos.asset")]
    public OrderCatalog catalogoPedidos;
    [Tooltip("Pedidos ya pagados que el triciclo llevará al almacén (sale al confirmar)")]
    public List<PedidoPendiente> pedidosPendientes = new List<PedidoPendiente>();

    /// <summary>Un pedido pagado en la computadora que todavía no ha traído el triciclo.</summary>
    [Serializable]
    public class PedidoPendiente
    {
        public string producto;
        public int cantidad;
        public int lote;
        public int totalPagado;
    }

    public int PedidosPendientesCount => pedidosPendientes != null ? pedidosPendientes.Count : 0;

    /// <summary>Se dispara cuando cambia algo que la interfaz debe mostrar.</summary>
    public event Action OnStateChanged;

    void Awake()
    {
        Instance = this;
        tiempoRestante = duracionDia;
        CalcularObjetivos();
    }

    void CalcularObjetivos()
    {
        metaVentas = Mathf.Max(1, Mathf.RoundToInt(metaBase * indicePrecios));
        alquiler = Mathf.Max(1, Mathf.RoundToInt(alquilerBase * indicePrecios));
    }

    /// <summary>Suma el consumo de las neveras que estén encendidas.</summary>
    void CalcularElectricidad()
    {
        electricidad = 0;
        if (shelves == null) return;
        foreach (Shelf s in shelves)
            if (s != null && s.esNevera && s.encendida) electricidad += s.costeLuz;
    }

    /// <summary>Activa solo los NPC contratados; el resto no aparece en la escena.</summary>
    public void SyncEmployees()
    {
        if (employeesNpc == null) return;
        for (int i = 0; i < employeesNpc.Count; i++)
            if (employeesNpc[i] != null)
                employeesNpc[i].gameObject.SetActive(i < empleados);
    }

    /// <summary>Contrata a un repositor. Paga una entrada y luego un sueldo diario.</summary>
    public bool ContratarEmpleado()
    {
        if (gameOver) return false;
        if (empleados >= maxEmpleados)
        {
            if (HUD.Instance != null) HUD.Instance.ShowMessage("Ya tienes el máximo de empleados");
            return false;
        }

        if (!SpendMoney(costeContratacion))
        {
            if (HUD.Instance != null) HUD.Instance.ShowMessage("No te alcanza para contratar");
            return false;
        }

        empleados++;
        SyncEmployees();
        Notify();
        SaveSystem.Save(this);
        if (HUD.Instance != null)
            HUD.Instance.ShowMessage($"Empleado {empleados}/{maxEmpleados} · sueldos {Currency.Format(Salarios)} CUP/día");
        return true;
    }

    /// <summary>Despide al último empleado contratado (no devuelve la entrada).</summary>
    public bool DespedirEmpleado()
    {
        if (empleados <= 0) return false;
        empleados--;
        SyncEmployees();
        Notify();
        SaveSystem.Save(this);
        if (HUD.Instance != null) HUD.Instance.ShowMessage($"Empleado despedido · quedan {empleados}");
        return true;
    }

    void Start()
    {
        // Esperamos un par de frames a que estanterías y pedidos se registren en las listas.
        StartCoroutine(LoadAfterSceneReady());
    }

    IEnumerator LoadAfterSceneReady()
    {
        yield return null;
        yield return null;
        SaveSystem.Load(this);
        CalcularObjetivos();
        CalcularElectricidad();
        SyncEmployees();
        // Si quedó un pedido pagado sin caja libre, el triciclo lo recoge al reanudar.
        IntentarEnviarTriciclo();
    }

    void OnApplicationQuit()
    {
        SaveSystem.Save(this);
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save(this);
    }

    void Update()
    {
        // En quiebra, cualquier toque reinicia (funciona en PC y en móvil).
        if (gameOver)
        {
            gameOverTimer += Time.deltaTime;
            if (gameOverTimer > 2.5f && (Input.GetKeyDown(KeyCode.F6) || Input.GetMouseButtonDown(0)))
                Reiniciar();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F5)) SaveSystem.Save(this);

        if (tiempoRestante <= 0f) return;

        tiempoRestante = Mathf.Max(0f, tiempoRestante - Time.deltaTime);

        if (tiempoRestante == 0f)
        {
            bool cumplida = ventasHoy >= metaVentas;
            CalcularElectricidad();
            dinero -= alquiler + electricidad + Salarios;

            string resultado;
            if (cumplida)
            {
                int bonus = Mathf.RoundToInt(Currency.CUP(10f) * indicePrecios);
                dinero += bonus;
                reputacion = Mathf.Min(100, reputacion + 5);
                resultado = $"¡Meta cumplida! +{Currency.Format(bonus)} CUP de bonificación";
            }
            else
            {
                reputacion = Mathf.Max(0, reputacion - 10);
                resultado = $"No alcanzaste la meta ({Currency.Format(ventasHoy)}/{Currency.Format(metaVentas)} CUP)";
            }

            string resumen = $"Atendidos: {clientesAtendidos} · Satisfechos: {clientesSatisfechos} · Perdidos: {clientesPerdidos}";

            if (dinero < 0)
            {
                dinero = 0;
                gameOver = true;
                gameOverTimer = 0f;
                resultado += " · ¡QUIEBRA!";
            }

            Debug.Log($"Fin del día {dia}. {resultado} | {resumen} | Alquiler: -{Currency.Format(alquiler)} | Luz: -{Currency.Format(electricidad)} | Sueldos: -{Currency.Format(Salarios)} CUP | Dinero: {Currency.Format(dinero)} | Tasa: {Currency.TasaUSD:0.00} | Reputación: {reputacion}");
            if (HUD.Instance != null) HUD.Instance.ShowMessage($"Alquiler -{Currency.Format(alquiler)} · Luz -{Currency.Format(electricidad)} · Sueldos -{Currency.Format(Salarios)} CUP · {resultado}");
            Notify();
            if (!gameOver) Invoke(nameof(NextDay), 4f);
        }
    }

    /// <summary>Prepara el día siguiente y lanza el reparto de la mañana.</summary>
    public void NextDay()
    {
        SalesAnalytics.NewDay(); // cierra el informe del día que termina
        dia++;
        tiempoRestante = duracionDia;
        ventasHoy = 0;
        clientesAtendidos = 0;
        clientesSatisfechos = 0;
        clientesPerdidos = 0;

        // La tasa USD cambia cada día (referencia elTOQUE) y arrastra los precios.
        float tasaAnterior = Currency.TasaUSD;
        Currency.NuevoDia();
        indicePrecios *= Currency.TasaUSD / tasaAnterior;

        // Inflación ficticia adicional: los precios y pedidos suben cada jornada.
        indicePrecios *= 1f + Mathf.Max(0f, inflacionDiaria);
        CalcularObjetivos();

        // El triciclo no tiene horario: sale únicamente cuando hay pedidos pagados.
        // Si quedó alguno sin caja libre, se reintenta al empezar el día nuevo.
        IntentarEnviarTriciclo();

        SaveSystem.Save(this);
        Notify();
    }

    /// <summary>
    /// Lanza el triciclo en cuanto hay pedidos pagados. No hay horarios ni mercancía
    /// aleatoria: si no hay pedidos, se queda aparcado. Si ya está en ruta, los
    /// pedidos nuevos esperan a que descargue para salir en el viaje siguiente.
    /// </summary>
    public void IntentarEnviarTriciclo()
    {
        if (PedidosPendientesCount <= 0) return;
        if (triciclo != null && triciclo.enRuta) return; // se encolará al descargar
        EnviarTriciclo();
    }

    /// <summary>Prepara las cajas con los pedidos pagados y pone el triciclo en ruta.</summary>
    void EnviarTriciclo()
    {
        // Se materializan los pedidos pagados en las cajas del almacén.
        EntregarPedidosPC();

        List<bool> llega = new List<bool>();
        int pedidosEnCamion = 0;
        if (deliveryBoxes != null)
        {
            foreach (DeliveryBox box in deliveryBoxes)
            {
                bool hay = box != null && box.pedidoPC;
                llega.Add(hay);
                if (hay) pedidosEnCamion++;
            }
        }

        // No hay caja libre: se reintentará al descargar o al cambiar de día.
        if (pedidosEnCamion <= 0) return;

        if (triciclo != null && deliveryBoxes != null)
        {
            triciclo.Llegar(deliveryBoxes, llega);
        }
        else if (deliveryBoxes != null)
        {
            for (int i = 0; i < deliveryBoxes.Count && i < llega.Count; i++)
            {
                if (deliveryBoxes[i] == null) continue;
                if (llega[i]) deliveryBoxes[i].Refill();
                else deliveryBoxes[i].SinPedido();
            }
        }

        if (HUD.Instance != null)
            HUD.Instance.ShowMessage($"Pedido confirmado: el triciclo sale por la carretera hacia el almacén con {pedidosEnCamion} caja(s)");
    }

    /// <summary>Precio de venta ya ajustado por el índice del día.</summary>
    public static int PrecioDe(ProductData producto)
    {
        if (producto == null) return 0;
        float indice = Instance != null ? Instance.indicePrecios : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(producto.precio * indice));
    }

    /// <summary>Coste del pedido ya ajustado por el índice del día.</summary>
    public static int CosteDe(ProductData producto)
    {
        if (producto == null) return 0;
        float indice = Instance != null ? Instance.indicePrecios : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(producto.costePedido * indice));
    }

    /// <summary>Coste de una unidad según el tamaño de caja del catálogo.</summary>
    public static int CostePorUnidad(ProductData producto, int lote = 8)
    {
        if (producto == null) return 0;
        float indice = Instance != null ? Instance.indicePrecios : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(producto.costePedido * indice / Mathf.Max(1, lote)));
    }

    /// <summary>Coste de un lote de unidades ya ajustado por el índice del día.</summary>
    public static int CosteDeCantidad(ProductData producto, int unidades)
    {
        if (producto == null || unidades <= 0) return 0;
        float indice = Instance != null ? Instance.indicePrecios : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(producto.costePedido * indice * unidades / 8f));
    }

    /// <summary>
    /// Compra desde la computadora: cobra ahora y hace salir el triciclo de inmediato
    /// (o lo encola si ya está en ruta). Si el mismo producto ya estaba esperando,
    /// se suman las unidades.
    /// </summary>
    public bool HacerPedido(ProductData producto, int cantidad, int lote)
    {
        if (producto == null || cantidad <= 0) return false;
        lote = Mathf.Max(1, lote);
        int coste = CostePorUnidad(producto, lote) * cantidad;
        if (!SpendMoney(coste))
        {
            if (HUD.Instance != null) HUD.Instance.ShowMessage("No te alcanza para hacer este pedido");
            return false;
        }

        if (pedidosPendientes == null) pedidosPendientes = new List<PedidoPendiente>();
        PedidoPendiente pedido = pedidosPendientes.Find(p => p != null && p.producto == producto.nombre);
        if (pedido == null)
        {
            pedido = new PedidoPendiente { producto = producto.nombre, lote = lote };
            pedidosPendientes.Add(pedido);
        }
        pedido.cantidad += cantidad;
        pedido.totalPagado += coste;

        Notify();
        SaveSystem.Save(this);
        IntentarEnviarTriciclo();
        return true;
    }

    /// <summary>Pasa los pedidos pagados a las cajas del almacén cuando llega el triciclo.</summary>
    void EntregarPedidosPC()
    {
        if (pedidosPendientes == null || pedidosPendientes.Count == 0) return;
        if (deliveryBoxes == null) return;

        List<PedidoPendiente> sinEntregar = new List<PedidoPendiente>();
        foreach (PedidoPendiente pedido in pedidosPendientes)
        {
            if (pedido == null) continue;
            ProductData producto = catalogoPedidos != null ? catalogoPedidos.FindProduct(pedido.producto) : null;
            if (producto == null)
                producto = deliveryBoxes.Find(b => b != null && b.product != null && b.product.nombre == pedido.producto)?.product;
            if (producto == null) continue;

            // Se prefiere una caja vacía del mismo producto; nunca se sobrescribe
            // otro pedido PC que todavía no se haya recogido.
            DeliveryBox caja = deliveryBoxes.Find(b => b != null && b.product == producto && b.empty && !b.pedidoPC);
            if (caja == null) caja = deliveryBoxes.Find(b => b != null && b.product == producto && !b.pedidoPC);
            if (caja == null) caja = deliveryBoxes.Find(b => b != null && b.empty && !b.pedidoPC);
            if (caja == null) caja = deliveryBoxes.Find(b => b != null && b.product == null && !b.pedidoPC);
            if (caja == null)
            {
                sinEntregar.Add(pedido); // no hay caja libre: se intentará en el siguiente viaje
                continue;
            }
            caja.Asignar(producto, pedido.cantidad);
        }
        pedidosPendientes = sinEntregar;
    }

    public void RegisterSale(int amount, bool satisfecho = true)
    {
        dinero += amount;
        ventasHoy += amount;
        clientesAtendidos++;

        if (satisfecho)
        {
            clientesSatisfechos++;
            reputacion = Mathf.Min(100, reputacion + 1);
        }
        else
        {
            reputacion = Mathf.Max(0, reputacion - 2);
            if (HUD.Instance != null) HUD.Instance.ShowMessage("Cliente insatisfecho: no encontró su producto favorito");
        }
        Notify();
    }

    /// <summary>Un cliente se marcha porque le pareció caro.</summary>
    public void RegisterRejection()
    {
        clientesPerdidos++;
        reputacion = Mathf.Max(0, reputacion - 6);
        Notify();
    }

    /// <summary>Un cliente se marcha porque esperó demasiado en la cola.</summary>
    public void RegisterAbandon()
    {
        clientesPerdidos++;
        reputacion = Mathf.Max(0, reputacion - 4);
        Notify();
    }

    /// <summary>Borra la partida y vuelve a cargar la escena desde cero.</summary>
    public void Reiniciar()
    {
        SalesAnalytics.Reset();
        Currency.Reset();
        SaveSystem.Delete();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    /// <summary>Gasta dinero al pedir stock. Devuelve false si no hay fondos.</summary>
    public bool SpendMoney(int amount)
    {
        if (dinero < amount) return false;
        dinero -= amount;
        Notify();
        return true;
    }

    /// <summary>Devuelve el primer producto del que quede stock en alguna estantería.</summary>
    public ProductData FindProductWithStock()
    {
        if (shelves == null) return null;
        foreach (Shelf shelf in shelves)
        {
            if (shelf != null && shelf.stock > 0 && shelf.product != null)
                return shelf.product;
        }
        return null;
    }

    /// <summary>Avisa a la interfaz de que algo ha cambiado.</summary>
    public static void Notify()
    {
        if (Instance == null) return;
        Instance.CalcularElectricidad();
        Instance.OnStateChanged?.Invoke();
    }
}
