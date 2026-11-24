using PresentationApp.Core.Models;

namespace PresentationApp.Core.Services;

/// <summary>
/// Assistant de structure : transforme un plan texte en diapositives.
/// Une ligne sans retrait = titre d'une nouvelle diapositive.
/// Une ligne indentée (espaces, tabulation) ou commençant par - * • = puce de la diapositive courante.
/// Aucune dépendance WPF.
/// </summary>
public static class OutlineAssistant
{
    public const double SlideWidth = 960;
    public const double SlideHeight = 540;

    public static IReadOnlyList<Slide> Parse(string? outline)
    {
        var result = new List<Slide>();
        if (string.IsNullOrWhiteSpace(outline)) return result;

        var entries = new List<(string Title, List<string> Bullets)>();

        foreach (var raw in outline.Replace("\r", "").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;

            bool isBullet = char.IsWhiteSpace(raw[0]) || "-*•".Contains(raw.TrimStart()[0]);
            string text = raw.Trim().TrimStart('-', '*', '•').Trim();
            if (text.Length == 0) continue;

            if (isBullet)
            {
                if (entries.Count == 0) entries.Add(("Sans titre", new List<string>()));
                entries[^1].Bullets.Add(text);
            }
            else
            {
                entries.Add((text, new List<string>()));
            }
        }

        foreach (var (title, bullets) in entries)
            result.Add(BuildSlide(title, bullets));

        return result;
    }

    private static Slide BuildSlide(string title, List<string> bullets)
    {
        var slide = new Slide();

        if (bullets.Count == 0)
        {
            // Diapositive de titre seul : texte centré et grand
            slide.Elements.Add(new TextElement
            {
                Text = title, FontSize = 54, IsBold = true,
                X = 60, Y = 200, Width = SlideWidth - 120, Height = 140
            });
        }
        else
        {
            slide.Elements.Add(new TextElement
            {
                Text = title, FontSize = 40, IsBold = true,
                X = 60, Y = 30, Width = SlideWidth - 120, Height = 90
            });
            slide.Elements.Add(new TextElement
            {
                Text = string.Join(Environment.NewLine, bullets.Select(b => "• " + b)),
                FontSize = 28,
                X = 60, Y = 140, Width = SlideWidth - 120, Height = SlideHeight - 180
            });
        }
        return slide;
    }
}
