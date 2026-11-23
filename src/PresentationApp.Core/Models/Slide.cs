namespace PresentationApp.Core.Models;

public class Slide
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Order { get; set; }
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public Transition Transition { get; set; } = new();
    public List<SlideElement> Elements { get; set; } = new();
}
