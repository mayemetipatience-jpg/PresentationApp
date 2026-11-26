namespace PresentationApp.Core.Models;

public enum AnimationType
{
    None = 0,
    Appear = 1,
    FlyIn = 2,
    Zoom = 3,
    Bounce = 4,
    Rotate = 5,
    Pulse = 6,
    Disappear = 7,
    FlyOut = 8
}

public class ElementAnimation
{
    public AnimationType Type { get; set; } = AnimationType.None;
    public int DurationMs { get; set; } = 1000;
    public string Direction { get; set; } = "Left";
    public int DelayMs { get; set; } = 0;
    public int RepeatCount { get; set; } = 1;
}