using UnityEngine;

/// <summary>
/// Joystick táctil dibujado con UI. No usa EventSystem: lee los dedos directamente,
/// así funciona igual en Android, iOS y con ratón en el editor.
/// Zona: mitad inferior-izquierda de la pantalla.
/// </summary>
public class Joystick : MonoBehaviour
{
    public RectTransform knob;

    [Range(0.2f, 0.8f)] public float zonaAncho = 0.42f;
    [Range(0.2f, 0.9f)] public float zonaAlto = 0.55f;
    public float radio = 70f;

    /// <summary>Vector normalizado: X = derecha, Y = adelante.</summary>
    public Vector2 Value { get; private set; }

    private int touchId = -1;
    private Vector2 origin;

    void Update()
    {
        Vector2 pos = Vector2.zero;
        bool pressed = false;
        bool ended = false;

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (touchId == -1 && t.phase == TouchPhase.Began && EnZona(t.position))
                {
                    touchId = t.fingerId;
                    origin = t.position;
                }

                if (t.fingerId == touchId)
                {
                    pos = t.position;
                    pressed = true;
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                        ended = true;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && EnZona(Input.mousePosition))
        {
            touchId = -2; // -2 = ratón
            origin = Input.mousePosition;
        }

        if (touchId == -2)
        {
            pos = Input.mousePosition;
            pressed = true;
            if (Input.GetMouseButtonUp(0)) ended = true;
        }

        if (!pressed || ended)
        {
            Value = Vector2.zero;
            touchId = -1;
            UpdateKnob();
            return;
        }

        Vector2 delta = pos - origin;
        if (delta.magnitude > radio) delta = delta.normalized * radio;
        Value = delta / radio;
        UpdateKnob();
    }

    bool EnZona(Vector2 p)
    {
        return p.x < Screen.width * zonaAncho && p.y < Screen.height * zonaAlto;
    }

    void UpdateKnob()
    {
        if (knob != null) knob.anchoredPosition = Value * radio;
    }
}
