using PresentationApp.Core.Models;

namespace PresentationApp.Core.Templates;

// Applique un thème à une présentation.
// Pour l'instant : la couleur de fond de chaque diapositive.
public static class ThemeApplier
{
    public static void ApplyBackground(Presentation presentation, ThemeDefinition theme)
    {
        foreach (Slide slide in presentation.Slides)
        {
            slide.BackgroundColor = theme.BackgroundColor;
        }
    }
}