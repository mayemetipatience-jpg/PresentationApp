using PresentationApp.Core.Models;

namespace PresentationApp.Core.Templates;

// Applique un thème à une présentation.
public static class ThemeApplier
{
    // Couleur de fond de chaque diapositive.
    public static void ApplyBackground(Presentation presentation, ThemeDefinition theme)
    {
        foreach (Slide slide in presentation.Slides)
        {
            slide.BackgroundColor = theme.BackgroundColor;
        }
    }

    // Couleur et police de chaque élément texte.
    public static void ApplyTextStyle(Presentation presentation, ThemeDefinition theme)
    {
        foreach (Slide slide in presentation.Slides)
        {
            foreach (SlideElement element in slide.Elements)
            {
                if (element is TextElement)
                {
                    element.Style.TextColor = theme.TextColor;
                    element.Style.FontFamily = theme.FontFamily;
                }
            }
        }
    }
}