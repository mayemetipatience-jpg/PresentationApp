using System.Collections.Generic;

namespace PresentationApp.Core.Templates;

// Thèmes prêts à l'emploi. Les couleurs sont des textes "#RRGGBB".
public static class BuiltInThemes
{
    public static readonly ThemeDefinition Light = new()
    {
        Name = "Clair",
        BackgroundColor = "#FFFFFF",
        TextColor = "#222222",
        AccentColor = "#2B579A",
        FontFamily = "Segoe UI"
    };

    public static readonly ThemeDefinition Dark = new()
    {
        Name = "Sombre",
        BackgroundColor = "#1E1E1E",
        TextColor = "#F0F0F0",
        AccentColor = "#4CC2FF",
        FontFamily = "Segoe UI"
    };

    public static readonly ThemeDefinition Ocean = new()
    {
        Name = "Océan",
        BackgroundColor = "#E8F4F8",
        TextColor = "#0B3C5D",
        AccentColor = "#1F8A9E",
        FontFamily = "Segoe UI"
    };

    public static readonly ThemeDefinition Warm = new()
    {
        Name = "Chaud",
        BackgroundColor = "#FFF8EE",
        TextColor = "#3B2F2F",
        AccentColor = "#D9822B",
        FontFamily = "Georgia"
    };

    public static IReadOnlyList<ThemeDefinition> All { get; } =
        new[] { Light, Dark, Ocean, Warm };
}