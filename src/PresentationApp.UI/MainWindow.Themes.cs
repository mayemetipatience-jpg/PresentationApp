using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;
using PresentationApp.UI.Services;
using PresentationApp.UI.Themes;
using PresentationApp.UI.ViewModels;

namespace PresentationApp.UI;

// Thèmes de présentation (onglet « Création »).
// Un thème change le fond des diapositives et la police / couleur des titres et sous-titres.
// Les fonds personnalisés et les textes déjà modifiés par l'utilisateur ne sont pas touchés.
public partial class MainWindow
{
    private void AppliquerTheme(string nomTheme)
    {
        Presentation presentation = _vm.CurrentPresentation;
        if (nomTheme == presentation.ThemeName)
            return;

        // On garde l'état actuel pour pouvoir annuler (Ctrl+Z)
        string ancienNom = presentation.ThemeName;
        var fonds = presentation.Slides.ToDictionary(s => s, s => s.BackgroundColor);
        var styles = presentation.Slides.SelectMany(s => s.Elements).OfType<TextElement>()
                                        .ToDictionary(t => t, t => ChangeStyleCommand.Copy(t.Style));

        _vm.History.Execute(new ActionAnnulable($"Thème « {nomTheme} »",
            faire: () => AppliquerThemeSurPresentation(presentation, nomTheme),
            defaire: () =>
            {
                presentation.ThemeName = ancienNom;
                foreach (var (diapositive, couleur) in fonds)
                    diapositive.BackgroundColor = couleur;
                foreach (var (texte, style) in styles)
                    texte.Style = ChangeStyleCommand.Copy(style);
            }));
    }

    private static void AppliquerThemeSurPresentation(Presentation presentation, string nomTheme)
    {
        PresentationTheme ancien = PresentationTheme.Trouver(presentation.ThemeName);
        PresentationTheme nouveau = PresentationTheme.Trouver(nomTheme);

        foreach (Slide diapositive in presentation.Slides)
        {
            // Seuls les fonds de l'ancien thème changent
            if (string.Equals(diapositive.BackgroundColor, ancien.BackgroundColor, StringComparison.OrdinalIgnoreCase))
                diapositive.BackgroundColor = nouveau.BackgroundColor;

            foreach (TextElement texte in diapositive.Elements.OfType<TextElement>())
            {
                if (texte.Text == SlideListViewModel.TexteTitre)
                {
                    texte.Style.FontFamily = nouveau.FontFamily;
                    texte.Style.TextColor = nouveau.TitleColor;
                    texte.Style.FontSize = 32;
                    texte.Style.IsBold = true;
                }
                else if (texte.Text == SlideListViewModel.TexteSousTitre)
                {
                    texte.Style.FontFamily = nouveau.FontFamily;
                    texte.Style.TextColor = nouveau.TextColor;
                    texte.Style.FontSize = 20;
                }
            }
        }

        presentation.ThemeName = nouveau.Name;
    }
}
