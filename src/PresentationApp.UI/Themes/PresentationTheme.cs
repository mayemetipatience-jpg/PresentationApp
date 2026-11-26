
namespace PresentationApp.UI.Themes;

public class PresentationTheme
{
    public string Name { get; set; } = "";
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string TitleColor { get; set; } = "#000000";
    public string TextColor { get; set; } = "#333333";
    public string PrimaryColor { get; set; } = "#0000FF";
    public string SecondaryColor { get; set; } = "#DDEBF7";
    public string FontFamily { get; set; } = "Segoe UI";

    // Le thème qui porte ce nom, ou "Moderne bleu" s'il n'existe pas
    public static PresentationTheme Trouver(string? nom)
    {
        List<PresentationTheme> themes = GetThemes();
        return themes.FirstOrDefault(t => t.Name == nom) ?? themes[0];
    }

    public static List<PresentationTheme> GetThemes()
    {
        return new List<PresentationTheme>
        {
            new()
            {
                Name = "Moderne bleu",
                BackgroundColor = "#FFFFFF",
                TitleColor = "#17365D",
                TextColor = "#333333",
                PrimaryColor = "#2878B5",
                SecondaryColor = "#DDEBF7",
                FontFamily = "Segoe UI"
            },

            new()
            {
                Name = "Élégant violet",
                BackgroundColor = "#F8F5FC",
                TitleColor = "#512D6D",
                TextColor = "#403747",
                PrimaryColor = "#8055A6",
                SecondaryColor = "#E8DDF2",
                FontFamily = "Georgia"
            },

            new()
            {
                Name = "Sombre",
                BackgroundColor = "#202124",
                TitleColor = "#FFD966",
                TextColor = "#F1F1F1",
                PrimaryColor = "#D6A84B",
                SecondaryColor = "#3C4043",
                FontFamily = "Segoe UI"
            },

            new()
            {
                Name = "Minimaliste",
                BackgroundColor = "#FAF9F6",
                TitleColor = "#333333",
                TextColor = "#555555",
                PrimaryColor = "#9B8065",
                SecondaryColor = "#EAE4DC",
                FontFamily = "Arial"
            },

            new()
            {
                Name = "Nature",
                BackgroundColor = "#F5F9F3",
                TitleColor = "#245C38",
                TextColor = "#354B3B",
                PrimaryColor = "#43865A",
                SecondaryColor = "#DCEBDD",
                FontFamily = "Segoe UI"
            }
        };
    }
}
