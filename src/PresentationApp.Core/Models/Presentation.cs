namespace PresentationApp.Core.Models;

public class Presentation
{
    public int FormatVersion { get; set; } = 1;
    public string Title { get; set; } = "Sans titre";
    public List<Slide> Slides { get; set; } = new();
}
