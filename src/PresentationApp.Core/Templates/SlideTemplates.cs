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

    // Modèle "Deux colonnes" : deux zones de texte côte à côte, séparées de 30 pixels.
    public static Slide DeuxColonnes()
    {
        var slide = new Slide();
        double largeur = (SlideWidth - 120 - 30) / 2;
        double xDroite = 60 + largeur + 30;

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
            Text = "Colonne de gauche",
            X = 60,
            Y = 160,
            Width = largeur,
            Height = 320
        });
        slide.Elements.Add(new TextElement
        {
            Text = "Colonne de droite",
            X = xDroite,
            Y = 160,
            Width = largeur,
            Height = 320
        });
        return slide;
    }

    // Modèle "Image + légende" : une image, avec une légende dessous.
    public static Slide ImageEtLegende(string cheminImage)
    {
        var slide = new Slide();
        slide.Elements.Add(new ImageElement
        {
            ImagePath = cheminImage,
            X = 130,
            Y = 40,
            Width = SlideWidth - 260,
            Height = 360
        });
        slide.Elements.Add(new TextElement
        {
            Text = "Légende",
            X = 130,
            Y = 420,
            Width = SlideWidth - 260,
            Height = 60
        });
        return slide;
    }
}