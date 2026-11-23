namespace PresentationApp.Core.Models;

public enum TransitionType
{
    None,
    Fade,
    Slide,
    Zoom
}

public class Transition
{
    public TransitionType Type { get; set; } = TransitionType.None;
    public int DurationMs { get; set; } = 500;
}
