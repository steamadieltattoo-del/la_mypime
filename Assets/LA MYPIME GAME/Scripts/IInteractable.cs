/// <summary>
/// Contrato que cumplen todos los objetos con los que el jugador puede interactuar.
/// Así el jugador no necesita saber si mira una estantería, una caja o un pedido.
/// </summary>
public interface IInteractable
{
    /// <summary>Texto que aparece en pantalla al apuntar con la cámara.</summary>
    string GetPrompt(PlayerController player);

    /// <summary>Acción que se ejecuta al pulsar E o el botón táctil.</summary>
    void Interact(PlayerController player);
}
