namespace PresentationApp.Core.Templates;

// Un thème décrit l'apparence d'une présentation.
// Les couleurs sont de simples textes "#RRGGBB" : aucun type WPF dans le Core.
public class ThemeDefinition
{
    public string Name { get; init; } = string.Empty;
    public string BackgroundColor { get; init; } = "#FFFFFF";
    public string TextColor { get; init; } = "#222222";
    public string AccentColor { get; init; } = "#2B579A";
    public string FontFamily { get; init; } = "Segoe UI";
}