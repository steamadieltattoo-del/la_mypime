using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interfaz en pantalla: dinero, tiempo, producto en mano, texto de interacción y avisos.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    public Text moneyText;
    public Text goalText;
    public Text timeText;
    public Text carriedText;
    public Text promptText;
    public Text messageText;
    public GameObject gameOverPanel;
    public GameObject analyticsPanel;
    public Text analyticsText;
    public RectTransform analyticsButton;
    public Text empleadosText;
    public RectTransform hireButton;
    public RectTransform fireButton;
    [Tooltip("Botón para abrir la tienda de muebles y ampliaciones")]
    public RectTransform tiendaButton;

    private float messageTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= Refresh;
    }

    void Update()
    {
        if ((GameManager.Instance == null || !GameManager.Instance.gameOver)
            && !OrderComputerUI.Abierto && !StoreUI.Abierto)
        {
            if (Input.GetKeyDown(KeyCode.Tab)) ToggleAnalytics();

            if (Input.GetKeyDown(KeyCode.B) && StoreUI.Instance != null)
                StoreUI.Instance.OpenMuebles(null);

            if (GameManager.Instance != null && Input.GetKeyDown(KeyCode.H))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    GameManager.Instance.DespedirEmpleado();
                else
                    GameManager.Instance.ContratarEmpleado();
            }

            if (GameManager.Instance != null && Input.GetMouseButtonDown(0))
            {
                if (UiClicked(analyticsButton)) ToggleAnalytics();
                if (UiClicked(hireButton)) GameManager.Instance.ContratarEmpleado();
                if (UiClicked(fireButton)) GameManager.Instance.DespedirEmpleado();
                if (UiClicked(tiendaButton) && StoreUI.Instance != null)
                    StoreUI.Instance.OpenMuebles(null);
            }
        }

        if (timeText != null && GameManager.Instance != null)
        {
            int t = Mathf.CeilToInt(GameManager.Instance.tiempoRestante);
            timeText.text = $"Tiempo: {t / 60:00}:{t % 60:00}";
        }

        if (promptText != null)
            promptText.text = PlayerController.Local != null ? PlayerController.Local.Prompt : "";

        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && messageText != null) messageText.text = "";
        }
    }

    public void Refresh()
    {
        if (moneyText != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            moneyText.text = $"Día {gm.dia}   {Currency.Format(gm.dinero)} CUP   Ventas: {Currency.Format(gm.ventasHoy)}   USD: {Currency.TasaUSD:0.00} CUP   Precios ×{gm.indicePrecios:0.00}";
        }

        if (goalText != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            goalText.text = $"Meta: {Currency.Format(gm.ventasHoy)}/{Currency.Format(gm.metaVentas)} CUP   Alquiler: {Currency.Format(gm.alquiler)}   Luz: {Currency.Format(gm.electricidad)}   Sueldos: {Currency.Format(gm.Salarios)}   Rep: {gm.reputacion}%   Atendidos: {gm.clientesAtendidos}   Satisfechos: {gm.clientesSatisfechos}   Perdidos: {gm.clientesPerdidos}";
        }

        if (empleadosText != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            empleadosText.text = $"Empleados: {gm.empleados}/{gm.maxEmpleados}   Sueldos: {Currency.Format(gm.Salarios)} CUP/día   Contratar: {Currency.Format(gm.costeContratacion)} CUP";
        }

        if (gameOverPanel != null && GameManager.Instance != null)
            gameOverPanel.SetActive(GameManager.Instance.gameOver);

        if (carriedText != null)
        {
            PlayerController p = PlayerController.Local;
            carriedText.text = p != null && p.CarriedProduct != null
                ? $"Llevas: {p.CarriedCount}× {p.CarriedProduct.nombre}"
                : "Manos vacías";
        }
    }

    /// <summary>Detecta un toque de ratón/táctil dentro de un botón de la UI.</summary>
    static bool UiClicked(RectTransform rt)
    {
        if (rt == null) return false;
        Vector2 local;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                   rt, Input.mousePosition, null, out local)
               && rt.rect.Contains(local);
    }

    public void ToggleAnalytics()
    {
        if (analyticsPanel == null) return;
        bool show = !analyticsPanel.activeSelf;
        analyticsPanel.SetActive(show);
        if (show) RefreshAnalytics();
    }

    void RefreshAnalytics()
    {
        if (analyticsText != null) analyticsText.text = SalesAnalytics.Report();
    }

    public void ShowMessage(string text)
    {
        if (messageText != null) messageText.text = text;
        messageTimer = 2.5f;
    }
}
