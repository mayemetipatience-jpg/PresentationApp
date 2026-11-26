
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PresentationApp.Core.Models;
using PresentationApp.UI.Themes;

namespace PresentationApp.UI;

public partial class MainWindow : Window
{
    private readonly Presentation _presentation = new();

    private SlideElement? _elementActuel;
    private SlideElement? _elementCopie;

    private bool _actualisationEnCours;
    private bool _actualisationCalquesEnCours;

    private readonly string[] _couleursFond =
    {
        "#FFFFFF",
        "#F2F2F2",
        "#DDEBF7",
        "#E2F0D9",
        "#FFF2CC",
        "#FCE4D6",
        "#E4DFEC"
    };

    private sealed class CouleurOption
    {
        public string Nom { get; set; }
        public string Code { get; set; }

        public CouleurOption(string nom, string code)
        {
            Nom = nom;
            Code = code;
        }
    }

    private readonly CouleurOption[] _couleursRemplissage =
    {
        new("Blanc", "#FFFFFF"),
        new("Noir", "#000000"),
        new("Rouge", "#FF0000"),
        new("Vert", "#00FF00"),
        new("Bleu", "#0000FF"),
        new("Jaune", "#FFFF00"),
        new("Orange", "#FFA500"),
        new("Violet", "#800080"),
        new("Bleu clair", "#ADD8E6"),
        new("Rose", "#FFC0CB"),
        new("Marron", "#A52A2A"),
        new("Gris", "#808080"),
        new("Turquoise", "#008080"),
        new("Or", "#FFD700"),
        new("Vert clair", "#90EE90"),
        new("Rose vif", "#FF69B4")
    };

    private int _indexCouleurFond;

    private const string TexteTitre = "Cliquez pour ajouter un titre";
    private const string TexteSousTitre = "Cliquez pour ajouter un sous-titre";

    private sealed class CalqueItem
    {
        public SlideElement Element { get; set; }
        public string Nom { get; set; }

        public CalqueItem(SlideElement element, string nom)
        {
            Element = element;
            Nom = nom;
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        slideCanvas.ElementSelectionChangee +=
            SlideCanvas_ElementSelectionChangee;

        slideCanvas.DiapositiveModifiee +=
            SlideCanvas_DiapositiveModifiee;

        PreviewKeyDown += MainWindow_PreviewKeyDown;

        InitialiserPresentation();
    }

    // INITIALISATION

    private void InitialiserPresentation()
    {
        var premiereDiapositive = CreerDiapositive(1);

        _presentation.Slides.Add(premiereDiapositive);

        lstDiapositives.ItemsSource = _presentation.Slides;

        lstDiapositives.SelectionChanged +=
            LstDiapositives_SelectionChanged;

        lstDiapositives.SelectedIndex = 0;
    }

    // CREATION D'UNE DIAPOSITIVE AVEC LE THEME ACTIF

    private Slide CreerDiapositive(int ordre)
    {
        PresentationTheme? theme = PresentationTheme
            .GetThemes()
            .FirstOrDefault(t => t.Name == _presentation.ThemeName);

        theme ??= PresentationTheme
            .GetThemes()
            .First(t => t.Name == "Moderne bleu");

        var diapositive = new Slide
        {
            Order = ordre,
            BackgroundColor = theme.BackgroundColor
        };

        // Zone de titre.
        diapositive.Elements.Add(new TextElement
        {
            X = 100,
            Y = 130,
            Width = 760,
            Height = 90,
            Text = TexteTitre,
            Style = new ElementStyle
            {
                TextAlignment = "Center",
                FontFamily = theme.FontFamily,
                FontSize = 32,
                TextColor = theme.TitleColor,
                IsBold = true
            }
        });

        // Zone de sous-titre.
        diapositive.Elements.Add(new TextElement
        {
            X = 100,
            Y = 240,
            Width = 760,
            Height = 70,
            Text = TexteSousTitre,
            Style = new ElementStyle
            {
                TextAlignment = "Center",
                FontFamily = theme.FontFamily,
                FontSize = 20,
                TextColor = theme.TextColor
            }
        });

        return diapositive;
    }

    // SELECTION DES DIAPOSITIVES

    private void LstDiapositives_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
        {
            _elementActuel = null;

            slideCanvas.DisplaySlide(slide);

            ActualiserListeCalques();
        }
    }

    private void BtnNouvelleDiapositive_Click(
        object sender,
        RoutedEventArgs e)
    {
        int nouvelOrdre = _presentation.Slides.Count + 1;

        var nouvelleDiapositive = CreerDiapositive(nouvelOrdre);

        _presentation.Slides.Add(nouvelleDiapositive);

        lstDiapositives.Items.Refresh();
        lstDiapositives.SelectedItem = nouvelleDiapositive;
    }

    // INSERTION DE TEXTE

    private void BtnTexte_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
            slideCanvas.ActiverInsertionTexte(slide);
    }

    // INSERTION D'UN TABLEAU

    private void BtnInsererTableau_Click(
        object sender,
        RoutedEventArgs e)
    {
        // Vérifier qu'une diapositive est sélectionnée.
        if (lstDiapositives.SelectedItem is not Slide slide)
        {
            MessageBox.Show(
                "Sélectionnez d'abord une diapositive.",
                "Insertion d'un tableau",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        // Récupérer le nombre de lignes.
        if (cmbLignesTableau.SelectedItem is not ComboBoxItem itemLignes ||
            !int.TryParse(
                itemLignes.Content?.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int lignes))
        {
            MessageBox.Show(
                "Veuillez choisir un nombre de lignes valide.",
                "Insertion d'un tableau",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // Récupérer le nombre de colonnes.
        if (cmbColonnesTableau.SelectedItem is not ComboBoxItem itemColonnes ||
            !int.TryParse(
                itemColonnes.Content?.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int colonnes))
        {
            MessageBox.Show(
                "Veuillez choisir un nombre de colonnes valide.",
                "Insertion d'un tableau",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // Vérifier les dimensions.
        if (lignes < 1 || lignes > 20 ||
            colonnes < 1 || colonnes > 20)
        {
            MessageBox.Show(
                "Le tableau doit contenir entre 1 et 20 lignes et colonnes.",
                "Insertion d'un tableau",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // Activer le mode d'insertion du tableau.
        slideCanvas.ActiverInsertionTableau(
            slide,
            lignes,
            colonnes);

        MessageBox.Show(
            $"Tableau de {lignes} ligne(s) et {colonnes} colonne(s) prêt.\n\nCliquez sur la diapositive pour le placer.",
            "Insertion d'un tableau",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // INSERTION DES FORMES

    private void CmbFormes_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (cmbFormes.SelectedItem is not ComboBoxItem item)
            return;

        if (item.Tag == null)
            return;

        if (!Enum.TryParse(item.Tag.ToString(), out ShapeType type))
            return;

        AjouterForme(type);

        cmbFormes.SelectedIndex = -1;
    }

    private void AjouterForme(ShapeType type)
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        var forme = new ShapeElement
        {
            X = 300,
            Y = 180,
            Width = 180,
            Height = 120,
            ShapeType = type,
            Style = new ElementStyle
            {
                FillColor = "#ADD8E6",
                StrokeColor = "#000000",
                StrokeThickness = 2
            }
        };

        slide.Elements.Add(forme);

        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // VERROUILLAGE / DEVERROUILLAGE

    private void BtnVerrouiller_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show(
                "Sélectionnez d'abord un élément à verrouiller.",
                "Verrouillage",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        element.IsLocked = !element.IsLocked;

        slideCanvas.ActualiserEtatVerrouillage();

        SlideCanvas_ElementSelectionChangee(element);

        ActualiserListeCalques();
    }

    private bool ElementEstVerrouille()
    {
        return _elementActuel?.IsLocked == true;
    }

    private void AfficherMessageElementVerrouille()
    {
        MessageBox.Show(
            "Cet élément est verrouillé. Déverrouillez-le pour le modifier.",
            "Élément verrouillé",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // CHOIX DE LA COULEUR DE REMPLISSAGE

    private void BtnCouleurRemplissage_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show(
                "Sélectionnez d'abord une forme.",
                "Couleur de remplissage",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        if (element is not ShapeElement)
        {
            MessageBox.Show(
                "La couleur de remplissage s'applique aux formes.",
                "Couleur de remplissage",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        Window fenetre = new()
        {
            Title = "Choisir une couleur de remplissage",
            Width = 320,
            Height = 260,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        StackPanel panneau = new()
        {
            Margin = new Thickness(15)
        };

        panneau.Children.Add(new TextBlock
        {
            Text = "Sélectionnez une couleur :",
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 12)
        });

        ComboBox listeCouleurs = new()
        {
            Height = 34
        };

        foreach (CouleurOption couleur in _couleursRemplissage)
        {
            listeCouleurs.Items.Add(new ComboBoxItem
            {
                Content = couleur.Nom,
                Tag = couleur.Code
            });
        }

        int indexActuel = Array.FindIndex(
            _couleursRemplissage,
            couleur => string.Equals(
                couleur.Code,
                element.Style.FillColor,
                StringComparison.OrdinalIgnoreCase));

        listeCouleurs.SelectedIndex =
            indexActuel >= 0 ? indexActuel : 0;

        panneau.Children.Add(listeCouleurs);

        Button boutonAppliquer = new()
        {
            Content = "Appliquer la couleur",
            Height = 35,
            Margin = new Thickness(0, 20, 0, 0),
            IsDefault = true
        };

        boutonAppliquer.Click += (_, _) =>
        {
            if (listeCouleurs.SelectedItem is not ComboBoxItem item)
                return;

            if (item.Tag is not string couleur)
                return;

            element.Style.FillColor = couleur;

            slideCanvas.ActualiserElementSelectionne();

            lstDiapositives.Items.Refresh();

            fenetre.DialogResult = true;
            fenetre.Close();
        };

        panneau.Children.Add(boutonAppliquer);

        fenetre.Content = panneau;

        fenetre.ShowDialog();
    }

    // COPIER

    private void BtnCopier_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show(
                "Sélectionnez un élément à copier.",
                "Copier",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        _elementCopie = ClonerElement(element);
    }

    // COUPER

    private void BtnCouper_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show(
                "Sélectionnez un élément à couper.",
                "Couper",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        _elementCopie = ClonerElement(element);

        SupprimerElementSelectionne();
    }

    // COLLER

    private void BtnColler_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_elementCopie == null)
        {
            MessageBox.Show(
                "Aucun élément à coller. Copiez d'abord un élément.",
                "Coller",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        SlideElement nouvelElement = ClonerElement(_elementCopie);

        nouvelElement.X += 30;
        nouvelElement.Y += 30;

        slide.Elements.Add(nouvelElement);

        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // CLONAGE

    private SlideElement ClonerElement(SlideElement element)
    {
        ElementStyle style = new()
        {
            FillColor = element.Style.FillColor,
            StrokeColor = element.Style.StrokeColor,
            StrokeThickness = element.Style.StrokeThickness,
            TextColor = element.Style.TextColor,
            FontFamily = element.Style.FontFamily,
            FontSize = element.Style.FontSize,
            IsBold = element.Style.IsBold,
            IsItalic = element.Style.IsItalic,
            IsUnderline = element.Style.IsUnderline,
            TextAlignment = element.Style.TextAlignment
        };

        SlideElement copie;

        if (element is TextElement texte)
        {
            copie = new TextElement
            {
                Text = texte.Text
            };
        }
        else if (element is ShapeElement forme)
        {
            copie = new ShapeElement
            {
                ShapeType = forme.ShapeType
            };
        }
        else if (element is ImageElement image)
        {
            copie = new ImageElement
            {
                ImagePath = image.ImagePath
            };
        }
        else if (element is TableElement tableau)
        {
            TableElement copieTableau = new()
            {
                Rows = tableau.Rows,
                Columns = tableau.Columns,
                CellBackgroundColor = tableau.CellBackgroundColor,
                HeaderBackgroundColor = tableau.HeaderBackgroundColor,
                BorderColor = tableau.BorderColor,
                BorderThickness = tableau.BorderThickness,
                CellFontSize = tableau.CellFontSize,
                Cells = tableau.Cells
                    .Select(ligne => ligne.ToList())
                    .ToList()
            };

            copie = copieTableau;
        }
        else
        {
            throw new NotSupportedException(
                "Type d'élément non pris en charge.");
        }

        copie.X = element.X;
        copie.Y = element.Y;
        copie.Width = element.Width;
        copie.Height = element.Height;
        copie.Rotation = element.Rotation;
        copie.ZIndex = element.ZIndex;
        copie.Style = style;

        copie.IsLocked = false;

        return copie;
    }

    // INSERTION D'UNE IMAGE

    private void BtnImage_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        OpenFileDialog dialogue = new()
        {
            Title = "Choisir une image",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
        };

        if (dialogue.ShowDialog() != true)
            return;

        var nouvelImage = new ImageElement
        {
            ImagePath = dialogue.FileName,
            X = 250,
            Y = 150,
            Width = 300,
            Height = 200
        };

        slide.Elements.Add(nouvelImage);

        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // ARRIERE-PLAN DE LA DIAPOSITIVE

    private void BtnArrierePlan_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        _indexCouleurFond =
            (_indexCouleurFond + 1) % _couleursFond.Length;

        slide.BackgroundColor = _couleursFond[_indexCouleurFond];

        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // GESTION DES CALQUES

    private void BtnPremierPlan_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (slideCanvas.ElementSelectionne == null)
        {
            AfficherMessageSelectionCalque();
            return;
        }

        slideCanvas.MettreAuPremierPlan();

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    private void BtnArrierePlanCalque_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (slideCanvas.ElementSelectionne == null)
        {
            AfficherMessageSelectionCalque();
            return;
        }

        slideCanvas.MettreEnArrierePlan();

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    private void BtnAvancerCalque_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (slideCanvas.ElementSelectionne == null)
        {
            AfficherMessageSelectionCalque();
            return;
        }

        slideCanvas.AvancerDUnCalque();

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    private void BtnReculerCalque_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (slideCanvas.ElementSelectionne == null)
        {
            AfficherMessageSelectionCalque();
            return;
        }

        slideCanvas.ReculerDUnCalque();

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    private void AfficherMessageSelectionCalque()
    {
        MessageBox.Show(
            "Sélectionnez d'abord un élément sur la diapositive.",
            "Gestion des calques",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // ACTUALISATION DE LA LISTE DES CALQUES

    private void ActualiserListeCalques()
    {
        if (lstCalques == null)
            return;

        if (lstDiapositives.SelectedItem is not Slide slide)
        {
            _actualisationCalquesEnCours = true;

            try
            {
                lstCalques.ItemsSource = null;
                lstCalques.SelectedItem = null;
            }
            finally
            {
                _actualisationCalquesEnCours = false;
            }

            return;
        }

        _actualisationCalquesEnCours = true;

        try
        {
            SlideElement? elementSelectionne =
                slideCanvas.ElementSelectionne ?? _elementActuel;

            var elements = slide.Elements
                .OrderByDescending(element => element.ZIndex)
                .ToList();

            List<CalqueItem> calques = new();

            foreach (SlideElement element in elements)
            {
                string nom;

                if (element is TextElement texte)
                {
                    string contenu = texte.Text ?? "";

                    contenu = contenu
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Trim();

                    if (string.IsNullOrWhiteSpace(contenu))
                        contenu = "Texte vide";

                    if (contenu.Length > 25)
                        contenu = contenu.Substring(0, 25) + "...";

                    nom = "Texte : " + contenu;
                }
                else if (element is ShapeElement forme)
                {
                    nom = "Forme : " + forme.ShapeType;
                }
                else if (element is ImageElement)
                {
                    nom = "Image";
                }
                else if (element is TableElement tableau)
                {
                    nom = $"Tableau : {tableau.Rows} x {tableau.Columns}";
                }
                else
                {
                    nom = "Élément";
                }

                if (element.IsLocked)
                    nom = "🔒 " + nom;

                calques.Add(new CalqueItem(element, nom));
            }

            lstCalques.ItemsSource = calques;

            lstCalques.SelectedItem = elementSelectionne == null
                ? null
                : calques.FirstOrDefault(
                    calque => ReferenceEquals(
                        calque.Element,
                        elementSelectionne));
        }
        finally
        {
            _actualisationCalquesEnCours = false;
        }
    }

    // SELECTION DEPUIS LA LISTE DES CALQUES

    private void LstCalques_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationCalquesEnCours)
            return;

        if (lstCalques.SelectedItem is not CalqueItem calque)
            return;

        slideCanvas.SelectionnerElementDepuisListe(
            calque.Element);
    }

    // ROTATION PAR BOUTON

    private void BtnRotation_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show(
                "Sélectionnez un élément à faire pivoter.",
                "Rotation",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        element.Rotation =
            (element.Rotation + 90) % 360;

        slideCanvas.ActualiserElementSelectionne();

        SlideCanvas_ElementSelectionChangee(element);
    }

    // SUPPRESSION D'UN ELEMENT

    private void BtnSupprimerElement_Click(
        object sender,
        RoutedEventArgs e)
    {
        SupprimerElementSelectionne();
    }

    private void SupprimerElementSelectionne()
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        SlideElement? elementASupprimer =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (elementASupprimer == null)
        {
            MessageBox.Show(
                "Sélectionnez d'abord un élément à supprimer.",
                "Suppression",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (elementASupprimer.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        bool elementSupprime =
            slide.Elements.Remove(elementASupprimer);

        if (!elementSupprime)
        {
            MessageBox.Show(
                "Impossible de trouver cet élément dans la diapositive.",
                "Suppression",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        _elementActuel = null;

        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // SUPPRESSION D'UNE DIAPOSITIVE

    private void BtnSupprimerDiapositive_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        if (_presentation.Slides.Count <= 1)
        {
            MessageBox.Show(
                "La présentation doit conserver au moins une diapositive.",
                "Suppression",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        MessageBoxResult resultat = MessageBox.Show(
            "Voulez-vous vraiment supprimer cette diapositive ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (resultat != MessageBoxResult.Yes)
            return;

        int index = lstDiapositives.SelectedIndex;

        _presentation.Slides.Remove(slide);

        for (int i = 0; i < _presentation.Slides.Count; i++)
            _presentation.Slides[i].Order = i + 1;

        lstDiapositives.Items.Refresh();

        lstDiapositives.SelectedIndex =
            Math.Min(index, _presentation.Slides.Count - 1);
    }

    // FORMATAGE DU TEXTE

    private bool VerifierSelectionTexte()
    {
        if (_elementActuel is TextElement && !ElementEstVerrouille())
            return true;

        if (ElementEstVerrouille())
        {
            AfficherMessageElementVerrouille();
            return false;
        }

        MessageBox.Show(
            "Sélectionnez d'abord une zone de texte.",
            "Formatage du texte",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        return false;
    }

    private void ActualiserFormatageTexte()
    {
        if (_elementActuel is not TextElement ||
            ElementEstVerrouille())
            return;

        slideCanvas.ActualiserElementSelectionne();

        lstDiapositives.Items.Refresh();
    }

    private void CmbPolice_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationEnCours ||
            _elementActuel is not TextElement texte ||
            ElementEstVerrouille())
            return;

        if (cmbPolice.SelectedItem is not ComboBoxItem item)
            return;

        string? police = item.Content?.ToString();

        if (string.IsNullOrWhiteSpace(police))
            return;

        texte.Style.FontFamily = police;

        ActualiserFormatageTexte();
    }

    private void CmbTaille_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationEnCours ||
            _elementActuel is not TextElement texte ||
            ElementEstVerrouille())
            return;

        if (cmbTaille.SelectedItem is not ComboBoxItem item)
            return;

        if (!double.TryParse(
            item.Content?.ToString(),
            CultureInfo.InvariantCulture,
            out double taille))
            return;

        if (taille <= 0)
            return;

        texte.Style.FontSize = taille;

        ActualiserFormatageTexte();
    }

    private void CmbCouleurTexte_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationEnCours ||
            _elementActuel is not TextElement texte ||
            ElementEstVerrouille())
            return;

        if (cmbCouleurTexte.SelectedItem is not ComboBoxItem item)
            return;

        if (item.Tag == null)
            return;

        texte.Style.TextColor = item.Tag.ToString()!;

        ActualiserFormatageTexte();
    }

    private void BtnGras_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;

        texte.Style.IsBold = !texte.Style.IsBold;

        ActualiserFormatageTexte();
    }

    private void BtnItalique_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;

        texte.Style.IsItalic = !texte.Style.IsItalic;

        ActualiserFormatageTexte();
    }

    private void BtnSouligne_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;

        texte.Style.IsUnderline = !texte.Style.IsUnderline;

        ActualiserFormatageTexte();
    }

    // ALIGNEMENT DU TEXTE

    private void AppliquerAlignementTexte(string alignement)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;

        texte.Style.TextAlignment = alignement;

        ActualiserFormatageTexte();
    }

    private void BtnAlignerGauche_Click(
        object sender,
        RoutedEventArgs e)
    {
        AppliquerAlignementTexte("Left");
    }

    private void BtnCentrerTexte_Click(
        object sender,
        RoutedEventArgs e)
    {
        AppliquerAlignementTexte("Center");
    }

    private void BtnAlignerDroite_Click(
        object sender,
        RoutedEventArgs e)
    {
        AppliquerAlignementTexte("Right");
    }

    // RACCOURCIS CLAVIER

    private void MainWindow_PreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        bool toucheCtrl =
            Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        if (e.Key == Key.Delete && toucheCtrl)
        {
            SupprimerElementSelectionne();

            e.Handled = true;
            return;
        }

        if (Keyboard.FocusedElement is TextBox)
            return;

        if (toucheCtrl)
        {
            switch (e.Key)
            {
                case Key.C:
                    BtnCopier_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;

                case Key.X:
                    BtnCouper_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;

                case Key.V:
                    BtnColler_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;
            }
        }

        if (e.Key == Key.Delete)
        {
            SupprimerElementSelectionne();
            e.Handled = true;
        }
    }

    // ACTUALISATION DU CANVAS

    private void SlideCanvas_DiapositiveModifiee()
    {
        lstDiapositives.Items.Refresh();

        ActualiserListeCalques();
    }

    private void SlideCanvas_ElementSelectionChangee(
        SlideElement? element)
    {
        _elementActuel = element;

        _actualisationEnCours = true;

        try
        {
            bool selectionExiste = element != null;
            bool elementVerrouille = element?.IsLocked == true;

            txtPositionX.IsEnabled =
                selectionExiste && !elementVerrouille;

            txtPositionY.IsEnabled =
                selectionExiste && !elementVerrouille;

            txtLargeur.IsEnabled =
                selectionExiste && !elementVerrouille;

            txtHauteur.IsEnabled =
                selectionExiste && !elementVerrouille;

            sliderRotation.IsEnabled =
                selectionExiste && !elementVerrouille;

            bool texteModifiable =
                element is TextElement && !elementVerrouille;

            cmbPolice.IsEnabled = texteModifiable;
            cmbTaille.IsEnabled = texteModifiable;
            cmbCouleurTexte.IsEnabled = texteModifiable;

            if (element == null)
            {
                txtPositionX.Text = "0";
                txtPositionY.Text = "0";
                txtLargeur.Text = "100";
                txtHauteur.Text = "50";

                sliderRotation.Value = 0;
                txtValeurRotation.Text = "0°";
            }
            else
            {
                txtPositionX.Text = FormaterValeur(element.X);
                txtPositionY.Text = FormaterValeur(element.Y);
                txtLargeur.Text = FormaterValeur(element.Width);
                txtHauteur.Text = FormaterValeur(element.Height);

                sliderRotation.Value =
                    Math.Clamp(
                        element.Rotation,
                        sliderRotation.Minimum,
                        sliderRotation.Maximum);

                txtValeurRotation.Text =
                    FormaterValeur(element.Rotation) + "°";
            }

            if (element is TextElement texte)
                SelectionnerValeursFormatage(texte);
            else
            {
                cmbPolice.SelectedIndex = -1;
                cmbTaille.SelectedIndex = -1;
                cmbCouleurTexte.SelectedIndex = -1;
            }
        }
        finally
        {
            _actualisationEnCours = false;
        }

        ActualiserListeCalques();
    }

    private void SelectionnerValeursFormatage(TextElement texte)
    {
        foreach (ComboBoxItem item in cmbPolice.Items)
        {
            if (item.Content?.ToString() == texte.Style.FontFamily)
            {
                cmbPolice.SelectedItem = item;
                break;
            }
        }

        foreach (ComboBoxItem item in cmbTaille.Items)
        {
            if (double.TryParse(
                item.Content?.ToString(),
                CultureInfo.InvariantCulture,
                out double taille) &&
                Math.Abs(taille - texte.Style.FontSize) < 0.01)
            {
                cmbTaille.SelectedItem = item;
                break;
            }
        }

        foreach (ComboBoxItem item in cmbCouleurTexte.Items)
        {
            if (string.Equals(
                item.Tag?.ToString(),
                texte.Style.TextColor,
                StringComparison.OrdinalIgnoreCase))
            {
                cmbCouleurTexte.SelectedItem = item;
                break;
            }
        }
    }

    // MODIFICATION DES PROPRIETES

    private void Propriete_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_actualisationEnCours ||
            _elementActuel == null ||
            ElementEstVerrouille())
            return;

        if (sender is not TextBox champ)
            return;

        if (!double.TryParse(
            champ.Text,
            NumberStyles.Float,
            CultureInfo.CurrentCulture,
            out double valeur))
            return;

        if (double.IsNaN(valeur) || double.IsInfinity(valeur))
            return;

        switch (champ.Name)
        {
            case "txtPositionX":
                _elementActuel.X = Math.Max(0, valeur);
                break;

            case "txtPositionY":
                _elementActuel.Y = Math.Max(0, valeur);
                break;

            case "txtLargeur":
                _elementActuel.Width = Math.Max(30, valeur);
                break;

            case "txtHauteur":
                _elementActuel.Height = Math.Max(30, valeur);
                break;
        }

        slideCanvas.ActualiserElementSelectionne();
    }

    // ROTATION PAR CURSEUR

    private void SliderRotation_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (_actualisationEnCours ||
            _elementActuel == null ||
            ElementEstVerrouille())
            return;

        _elementActuel.Rotation = e.NewValue;

        txtValeurRotation.Text =
            FormaterValeur(e.NewValue) + "°";

        slideCanvas.ActualiserElementSelectionne();
    }

    // FORMATAGE DES VALEURS

    private string FormaterValeur(double valeur)
    {
        return valeur.ToString(
            "0.##",
            CultureInfo.CurrentCulture);
    }
}
