using PresentationApp.Core.Models;

namespace PresentationApp.Core.Templates;

// Modèles de diapositive : chaque méthode renvoie une diapositive prête à remplir.
// Positions en pixels, sur une diapositive de 960 x 540.
public static class SlideTemplates
{
    public const double SlideWidth = 960;
    public const double SlideHeight = 540;

    // Modèle "Titre" : un grand titre au centre.
    public static Slide Titre()
    {
        var slide = new Slide();
        slide.Elements.Add(new TextElement
        {
            Text = "Titre de la présentation",
            X = 60,
            Y = 200,
            Width = SlideWidth - 120,
            Height = 120
        });
        return slide;
    }

    // Modèle "Titre + contenu" : un titre en haut, un texte en dessous.
    public static Slide TitreEtContenu()
    {
        var slide = new Slide();
        slide.Elements.Add(new TextElement
        {
            Text = "Titre",
            X = 60,
            Y = 40,
            Width = SlideWidth - 120,
            Height = 100
        });
        slide.Elements.Add(new TextElement
        {
            Text = "Votre contenu ici",
            X = 60,
            Y = 160,
            Width = SlideWidth - 120,
            Height = 320
        });
        return slide;
    }
}