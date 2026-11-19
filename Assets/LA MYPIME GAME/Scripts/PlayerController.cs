using UnityEngine;

/// <summary>
/// Jugador en primera persona. Controles:
/// - Móvil: joystick izquierdo para andar, arrastrar la derecha para mirar, botón inferior derecho para interactuar.
/// - PC: WASD / flechas, arrastrar con el ratón, E para interactuar.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Local { get; private set; }

    [Header("Referencias")]
    public Camera playerCamera;
    public Joystick joystick;
    public GameObject carriedVisual;

    [Header("Ajustes")]
    public float speed = 4f;
    public float lookSensitivity = 0.12f;
    public float interactRange = 3.5f;
    public float interactButtonRadius = 95f;

    public ProductData CarriedProduct { get; private set; }
    public int CarriedCount { get; private set; }
    public string Prompt { get; private set; } = "";

    private CharacterController controller;
    private float yaw;
    private float pitch;
    private int lookTouchId = -1;
    private Vector2 lastLookPos;
    private bool lookMouseActive;
    private IInteractable target;
    private Vector2 buttonCenter;

    void Awake()
    {
        Local = this;
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;
    }

    void Start()
    {
        buttonCenter = new Vector2(Screen.width - 105f, 105f);
        RefreshCarried();
    }

    void Update()
    {
        // Con la computadora o la tienda de muebles abiertas, el jugador no se mueve ni interactúa.
        if (OrderComputerUI.Abierto || OrderComputerUI.CerroAhora ||
            StoreUI.Abierto || StoreUI.CerroAhora) return;

        HandleLook();
        HandleMove();
        UpdateTarget();
        HandleInteract();
    }

    void HandleLook()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (lookTouchId == -1 && t.phase == TouchPhase.Began &&
                    t.position.x > Screen.width * 0.45f && !IsInteractButton(t.position))
                {
                    lookTouchId = t.fingerId;
                    lastLookPos = t.position;
                }

                if (t.fingerId == lookTouchId)
                {
                    ApplyLook(t.position - lastLookPos);
                    lastLookPos = t.position;
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                        lookTouchId = -1;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.x > Screen.width * 0.45f &&
                !IsInteractButton(Input.mousePosition))
            {
                lookMouseActive = true;
                lastLookPos = Input.mousePosition;
            }

            if (lookMouseActive && Input.GetMouseButton(0))
            {
                ApplyLook((Vector2)Input.mousePosition - lastLookPos);
                lastLookPos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0)) lookMouseActive = false;
        }
    }

    void ApplyLook(Vector2 delta)
    {
        yaw += delta.x * lookSensitivity;
        pitch = Mathf.Clamp(pitch - delta.y * lookSensitivity, -80f, 80f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMove()
    {
        Vector2 input = joystick != null ? joystick.Value : Vector2.zero;
        input.x += Input.GetAxisRaw("Horizontal");
        input.y += Input.GetAxisRaw("Vertical");
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 move = (transform.forward * input.y + transform.right * input.x) * speed;
        move.y = -2f; // gravedad simple para pegarnos al suelo
        controller.Move(move * Time.deltaTime);
    }

    void UpdateTarget()
    {
        target = null;
        Prompt = "";
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, interactRange)) return;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable == null) return;

        target = interactable;
        Prompt = interactable.GetPrompt(this);
    }

    void HandleInteract()
    {
        bool pressed = Input.GetKeyDown(KeyCode.E) || InteractPointerDown();
        if (!pressed || target == null) return;
        target.Interact(this);
    }

    bool InteractPointerDown()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
                if (t.phase == TouchPhase.Began && IsInteractButton(t.position)) return true;
        }
        else if (Input.GetMouseButtonDown(0) && IsInteractButton(Input.mousePosition))
        {
            return true;
        }
        return false;
    }

    bool IsInteractButton(Vector2 screenPos)
    {
        return Vector2.Distance(screenPos, buttonCenter) < interactButtonRadius;
    }

    public void PickUp(ProductData product, int amount)
    {
        if (product == null || amount <= 0) return;
        CarriedProduct = product;
        CarriedCount += amount;
        RefreshCarried();
    }

    /// <summary>Quita unidades de la mano (por ejemplo al reponer una estantería).</summary>
    public void RemoveCarried(int amount)
    {
        if (amount <= 0) return;
        CarriedCount = Mathf.Max(0, CarriedCount - amount);
        RefreshCarried();
    }

    /// <summary>Actualiza el objeto que se ve en la mano y el texto del HUD.</summary>
    public void RefreshCarried()
    {
        if (CarriedCount <= 0)
        {
            CarriedProduct = null;
            CarriedCount = 0;
        }

        bool show = CarriedProduct != null && CarriedCount > 0;
        if (carriedVisual != null)
        {
            carriedVisual.SetActive(show);
            if (show)
            {
                Material mat = CarriedProduct.material != null
                    ? CarriedProduct.material
                    : MaterialFactory.Get(CarriedProduct.color);

                Renderer renderer = carriedVisual.GetComponent<Renderer>();
                if (mat != null && renderer != null) renderer.sharedMaterial = mat;
            }
        }

        if (HUD.Instance != null) HUD.Instance.Refresh();
    }
}
