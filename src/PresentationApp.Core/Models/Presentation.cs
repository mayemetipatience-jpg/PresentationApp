
namespace PresentationApp.Core.Models;

public class Presentation
{
    public int FormatVersion { get; set; } = 1;

    public string Title { get; set; } = "Sans titre";

    // Nom du thème actuellement utilisé.
    public string ThemeName { get; set; } = "Moderne bleu";

    public List<Slide> Slides { get; set; } = new();
}
