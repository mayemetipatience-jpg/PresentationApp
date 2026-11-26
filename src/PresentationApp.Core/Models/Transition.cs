
namespace PresentationApp.Core.Models;

public enum TransitionType
{
    None = 0,

    // Transitions simples
    Fade = 1,
    Wipe = 2,
    Push = 3,
    Split = 4,
    Reveal = 5,
    Cover = 6,
    Zoom = 7,
    Rotate = 8
}

public class Transition
{
    public TransitionType Type { get; set; } = TransitionType.None;

    // Durée de la transition en millisecondes.
    public int DurationMs { get; set; } = 500;

    // Direction de la transition.
    public string Direction { get; set; } = "Left";
}
