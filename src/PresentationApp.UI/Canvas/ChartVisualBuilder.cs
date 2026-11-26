
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.Canvas;

public static class ChartVisualBuilder
{
    public static FrameworkElement Creer(ChartElement graphique)
    {
        double largeur = Math.Max(200, graphique.Width);
        double hauteur = Math.Max(150, graphique.Height);

        var conteneur = new Border
        {
            Width = largeur,
            Height = hauteur,
            Background = Brushes.White,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            ClipToBounds = true
        };

        var grille = new Grid();

        grille.RowDefinitions.Add(new RowDefinition
        {
            Height = new GridLength(35)
        });

        grille.RowDefinitions.Add(new RowDefinition
        {
            Height = new GridLength(1, GridUnitType.Star)
        });

        grille.RowDefinitions.Add(new RowDefinition
        {
            Height = new GridLength(
                graphique.ShowLegend ? 35 : 5)
        });

        var titre = new TextBlock
        {
            Text = graphique.Title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkSlateGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetRow(titre, 0);
        grille.Children.Add(titre);

        var zoneGraphique =
            new System.Windows.Controls.Canvas
            {
                ClipToBounds = true,
                Background = Brushes.Transparent
            };

        Grid.SetRow(zoneGraphique, 1);
        grille.Children.Add(zoneGraphique);

        var legende = CreerLegende(graphique);

        Grid.SetRow(legende, 2);
        grille.Children.Add(legende);

        conteneur.Child = grille;

        conteneur.Loaded += (s, e) =>
        {
            Dessiner(graphique, zoneGraphique);
        };

        return conteneur;
    }

    // =====================================================
    // LEGENDE
    // =====================================================

    private static StackPanel CreerLegende(
        ChartElement graphique)
    {
        var legende = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (!graphique.ShowLegend)
            return legende;

        var carre = new Rectangle
        {
            Width = 10,
            Height = 10,
            Fill = ObtenirCouleur(graphique, 0),
            Margin = new Thickness(3, 0, 4, 0)
        };

        legende.Children.Add(carre);

        legende.Children.Add(new TextBlock
        {
            Text = graphique.LegendTitle,
            FontSize = 11,
            Foreground = Brushes.DimGray,
            VerticalAlignment = VerticalAlignment.Center
        });

        return legende;
    }

    // =====================================================
    // DESSIN GENERAL
    // =====================================================

    private static void Dessiner(
        ChartElement graphique,
        System.Windows.Controls.Canvas zone)
    {
        zone.Children.Clear();

        double largeur = zone.ActualWidth;
        double hauteur = zone.ActualHeight;

        if (largeur <= 0 || hauteur <= 0)
            return;

        var valeurs = graphique.Values
            .Select(v => double.IsFinite(v) ? v : 0)
            .ToList();

        if (valeurs.Count == 0)
            return;

        switch (graphique.ChartType)
        {
            case ChartType.Column:
                DessinerColonnes(
                    graphique, zone, valeurs, largeur, hauteur);
                break;

            case ChartType.Bar:
                DessinerBarres(
                    graphique, zone, valeurs, largeur, hauteur);
                break;

            case ChartType.Line:
                DessinerCourbe(
                    graphique, zone, valeurs, largeur, hauteur);
                break;

            case ChartType.Pie:
                DessinerSecteurs(
                    graphique, zone, valeurs, largeur, hauteur, false);
                break;

            case ChartType.Doughnut:
                DessinerSecteurs(
                    graphique, zone, valeurs, largeur, hauteur, true);
                break;
        }
    }

    // =====================================================
    // GRAPHIQUE EN COLONNES
    // =====================================================

    private static void DessinerColonnes(
        ChartElement graphique,
        System.Windows.Controls.Canvas zone,
        System.Collections.Generic.List<double> valeurs,
        double largeur,
        double hauteur)
    {
        double maximum = Math.Max(1, valeurs.Max());

        double margeGauche = 35;
        double margeBas = 25;

        double zoneLargeur = largeur - margeGauche - 10;
        double zoneHauteur = hauteur - margeBas - 10;

        AjouterGrille(
            graphique,
            zone,
            margeGauche,
            10,
            zoneLargeur,
            zoneHauteur);

        double espace = zoneLargeur / valeurs.Count;
        double largeurBarre = espace * 0.6;

        for (int i = 0; i < valeurs.Count; i++)
        {
            double valeur = Math.Max(0, valeurs[i]);

            double hauteurBarre =
                valeur / maximum * zoneHauteur;

            var barre = new Rectangle
            {
                Width = Math.Max(2, largeurBarre),
                Height = Math.Max(0, hauteurBarre),
                Fill = ObtenirCouleur(graphique, i),
                ToolTip =
                    $"{NomCategorie(graphique, i)} : {valeurs[i]}"
            };

            System.Windows.Controls.Canvas.SetLeft(
                barre,
                margeGauche + i * espace +
                (espace - largeurBarre) / 2);

            System.Windows.Controls.Canvas.SetTop(
                barre,
                10 + zoneHauteur - hauteurBarre);

            zone.Children.Add(barre);

            AjouterTexte(
                zone,
                NomCategorie(graphique, i),
                margeGauche + i * espace,
                hauteur - 22,
                espace,
                10);

            if (graphique.ShowValues)
            {
                AjouterTexte(
                    zone,
                    valeurs[i].ToString("0.##"),
                    margeGauche + i * espace,
                    10 + zoneHauteur - hauteurBarre - 18,
                    espace,
                    10);
            }
        }
    }

    // =====================================================
    // GRAPHIQUE EN BARRES
    // =====================================================

    private static void DessinerBarres(
        ChartElement graphique,
        System.Windows.Controls.Canvas zone,
        System.Collections.Generic.List<double> valeurs,
        double largeur,
        double hauteur)
    {
        double maximum = Math.Max(1, valeurs.Max());

        double margeGauche = 75;
        double zoneLargeur = largeur - margeGauche - 10;

        double espace = (hauteur - 15) / valeurs.Count;
        double hauteurBarre = espace * 0.6;

        AjouterGrille(
            graphique,
            zone,
            margeGauche,
            5,
            zoneLargeur,
            hauteur - 15);

        for (int i = 0; i < valeurs.Count; i++)
        {
            double valeur = Math.Max(0, valeurs[i]);

            double longueur =
                valeur / maximum * zoneLargeur;

            AjouterTexte(
                zone,
                NomCategorie(graphique, i),
                0,
                5 + i * espace,
                margeGauche - 5,
                10);

            var barre = new Rectangle
            {
                Width = Math.Max(0, longueur),
                Height = Math.Max(2, hauteurBarre),
                Fill = ObtenirCouleur(graphique, i),
                ToolTip =
                    $"{NomCategorie(graphique, i)} : {valeurs[i]}"
            };

            System.Windows.Controls.Canvas.SetLeft(
                barre,
                margeGauche);

            System.Windows.Controls.Canvas.SetTop(
                barre,
                5 + i * espace +
                (espace - hauteurBarre) / 2);

            zone.Children.Add(barre);

            if (graphique.ShowValues)
            {
                AjouterTexte(
                    zone,
                    valeurs[i].ToString("0.##"),
                    margeGauche + longueur,
                    5 + i * espace,
                    45,
                    10);
            }
        }
    }

    // =====================================================
    // GRAPHIQUE LINEAIRE
    // =====================================================

    private static void DessinerCourbe(
        ChartElement graphique,
        System.Windows.Controls.Canvas zone,
        System.Collections.Generic.List<double> valeurs,
        double largeur,
        double hauteur)
    {
        double maximum = Math.Max(1, valeurs.Max());

        double marge = 25;

        double zoneLargeur = largeur - marge * 2;
        double zoneHauteur = hauteur - marge * 2;

        AjouterGrille(
            graphique,
            zone,
            marge,
            marge,
            zoneLargeur,
            zoneHauteur);

        var ligne = new Polyline
        {
            Stroke = ObtenirCouleur(graphique, 0),
            StrokeThickness = 2
        };

        for (int i = 0; i < valeurs.Count; i++)
        {
            double x = valeurs.Count == 1
                ? marge + zoneLargeur / 2
                : marge + i * zoneLargeur /
                  (valeurs.Count - 1);

            double y = marge + zoneHauteur -
                Math.Max(0, valeurs[i]) /
                maximum * zoneHauteur;

            ligne.Points.Add(new Point(x, y));

            var point = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = ObtenirCouleur(graphique, 0),
                ToolTip =
                    $"{NomCategorie(graphique, i)} : {valeurs[i]}"
            };

            System.Windows.Controls.Canvas.SetLeft(
                point,
                x - 4);

            System.Windows.Controls.Canvas.SetTop(
                point,
                y - 4);

            zone.Children.Add(point);

            if (graphique.ShowValues)
            {
                AjouterTexte(
                    zone,
                    valeurs[i].ToString("0.##"),
                    x - 25,
                    y - 20,
                    50,
                    10);
            }

            AjouterTexte(
                zone,
                NomCategorie(graphique, i),
                x - 35,
                hauteur - 15,
                70,
                10);
        }

        zone.Children.Add(ligne);
    }

    // =====================================================
    // GRAPHIQUES CIRCULAIRES ET ANNEAUX
    // =====================================================

    private static void DessinerSecteurs(
        ChartElement graphique,
        System.Windows.Controls.Canvas zone,
        System.Collections.Generic.List<double> valeurs,
        double largeur,
        double hauteur,
        bool anneau)
    {
        var positives = valeurs
            .Select(v => Math.Max(0, v))
            .ToList();

        double total = positives.Sum();

        if (total <= 0)
        {
            AjouterTexte(
                zone,
                "Aucune donnée positive",
                0,
                hauteur / 2,
                largeur,
                12);

            return;
        }

        double rayon = Math.Max(
            10,
            Math.Min(largeur, hauteur) * 0.38);

        double centreX = largeur / 2;
        double centreY = hauteur / 2;

        double angleDepart = -Math.PI / 2;

        for (int i = 0; i < positives.Count; i++)
        {
            if (positives[i] <= 0)
                continue;

            double angle =
                positives[i] / total * Math.PI * 2;

            var points = new PointCollection
            {
                new Point(centreX, centreY)
            };

            int segments = Math.Max(
                2,
                (int)Math.Ceiling(angle * 20));

            for (int j = 0; j <= segments; j++)
            {
                double a =
                    angleDepart + angle * j / segments;

                points.Add(new Point(
                    centreX + Math.Cos(a) * rayon,
                    centreY + Math.Sin(a) * rayon));
            }

            var secteur = new Polygon
            {
                Points = points,
                Fill = ObtenirCouleur(graphique, i),
                Stroke = Brushes.White,
                StrokeThickness = 1,
                ToolTip =
                    $"{NomCategorie(graphique, i)} : {valeurs[i]}"
            };

            zone.Children.Add(secteur);

            angleDepart += angle;
        }

        if (anneau)
        {
            var trou = new Ellipse
            {
                Width = rayon,
                Height = rayon,
                Fill = Brushes.White,
                Stroke = Brushes.White,
                IsHitTestVisible = false
            };

            System.Windows.Controls.Canvas.SetLeft(
                trou,
                centreX - rayon / 2);

            System.Windows.Controls.Canvas.SetTop(
                trou,
                centreY - rayon / 2);

            zone.Children.Add(trou);
        }
    }

    // =====================================================
    // LIGNES DE GRILLE
    // =====================================================

    private static void AjouterGrille(
        ChartElement graphique,
        System.Windows.Controls.Canvas zone,
        double x,
        double y,
        double largeur,
        double hauteur)
    {
        if (!graphique.ShowGridLines)
            return;

        for (int i = 0; i <= 4; i++)
        {
            double positionY =
                y + hauteur * i / 4;

            var ligne = new Line
            {
                X1 = x,
                X2 = x + largeur,
                Y1 = positionY,
                Y2 = positionY,

                Stroke = Brushes.LightGray,
                StrokeThickness = 0.5,

                StrokeDashArray =
                    new DoubleCollection { 2, 3 },

                IsHitTestVisible = false
            };

            zone.Children.Add(ligne);
        }
    }

    // =====================================================
    // TEXTES
    // =====================================================

    private static void AjouterTexte(
        System.Windows.Controls.Canvas zone,
        string texte,
        double x,
        double y,
        double largeur,
        double taille)
    {
        var bloc = new TextBlock
        {
            Text = texte,

            Width = Math.Max(1, largeur),

            FontSize = taille,

            Foreground = Brushes.DimGray,

            TextAlignment = TextAlignment.Center,

            TextTrimming =
                TextTrimming.CharacterEllipsis,

            IsHitTestVisible = false
        };

        System.Windows.Controls.Canvas.SetLeft(bloc, x);
        System.Windows.Controls.Canvas.SetTop(bloc, y);

        zone.Children.Add(bloc);
    }

    // =====================================================
    // CATEGORIES
    // =====================================================

    private static string NomCategorie(
        ChartElement graphique,
        int index)
    {
        return index < graphique.Categories.Count
            ? graphique.Categories[index]
            : $"Catégorie {index + 1}";
    }

    // =====================================================
    // COULEURS
    // =====================================================

    private static Brush ObtenirCouleur(
        ChartElement graphique,
        int index)
    {
        string couleur = graphique.Colors.Count > 0
            ? graphique.Colors[
                index % graphique.Colors.Count]
            : "#4472C4";

        try
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    couleur));
        }
        catch
        {
            return Brushes.SteelBlue;
        }
    }
}
