
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    private bool _actualisationTransitionEnCours;
    private bool _actualisationAnimationEnCours;

    private readonly string[] _couleursFond =
    {
        "#FFFFFF", "#F2F2F2", "#DDEBF7",
        "#E2F0D9", "#FFF2CC", "#FCE4D6", "#E4DFEC"
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
        slideCanvas.MouseLeftButtonDown +=
    SlideCanvas_MouseLeftButtonDown;

        slideCanvas.ElementSelectionChangee +=
            SlideCanvas_ElementSelectionChangee;

        slideCanvas.DiapositiveModifiee +=
            SlideCanvas_DiapositiveModifiee;

        PreviewKeyDown += MainWindow_PreviewKeyDown;

        cmbTransitions.SelectionChanged +=
            CmbTransitions_SelectionChanged;

        sliderDureeTransition.ValueChanged +=
            SliderDureeTransition_ValueChanged;

        cmbDirectionTransition.SelectionChanged +=
            CmbDirectionTransition_SelectionChanged;

        cmbAnimations.SelectionChanged +=
            CmbAnimations_SelectionChanged;

        sliderDureeAnimation.ValueChanged +=
            SliderDureeAnimation_ValueChanged;

        cmbDirectionAnimation.SelectionChanged +=
            CmbDirectionAnimation_SelectionChanged;

        txtDelaiAnimation.TextChanged +=
            TxtDelaiAnimation_TextChanged;

        cmbRepetitionsAnimation.SelectionChanged +=
            CmbRepetitionsAnimation_SelectionChanged;

        InitialiserPresentation();
    }

    private static string? LireValeurCombo(ComboBoxItem item)
    {
        return item.Tag?.ToString() ?? item.Content?.ToString();
    }

    private static bool LireEntierCombo(
        ComboBoxItem item,
        out int valeur)
    {
        return int.TryParse(
            LireValeurCombo(item),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out valeur);
    }

    // INITIALISATION

    private void InitialiserPresentation()
    {
        var premiereDiapositive = CreerDiapositive(1);

        _presentation.Slides.Add(premiereDiapositive);

        lstDiapositives.ItemsSource = _presentation.Slides;

        lstDiapositives.SelectionChanged +=
            LstDiapositives_SelectionChanged;

        lstCalques.SelectionChanged +=
            LstCalques_SelectionChanged;

        lstDiapositives.SelectedIndex = 0;
    }

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

    // OUVERTURE D'UNE PRESENTATION


    // THEMES


    // DIAPOSITIVES

    private void LstDiapositives_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        _elementActuel = null;

        slideCanvas.DisplaySlide(slide);

        ActualiserListeCalques();
        ActualiserParametresTransition(slide);
        ActualiserParametresAnimation(null);
    }

    private void BtnNouvelleDiapositive_Click(
        object sender,
        RoutedEventArgs e)
    {
        int ordre = _presentation.Slides.Count + 1;
        Slide nouvelle = CreerDiapositive(ordre);

        _presentation.Slides.Add(nouvelle);

        lstDiapositives.Items.Refresh();
        lstDiapositives.SelectedItem = nouvelle;
    }

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

    // TEXTE

    private void BtnTexte_Click(object sender, RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
            slideCanvas.ActiverInsertionTexte(slide);
    }

    // TABLEAUX

    private void BtnInsererTableau_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
        {
            MessageBox.Show(
                "Sélectionnez d'abord une diapositive.",
                "Insertion d'un tableau",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (cmbLignesTableau.SelectedItem is not ComboBoxItem itemLignes ||
            !int.TryParse(itemLignes.Content?.ToString(), out int lignes))
        {
            MessageBox.Show("Choisissez un nombre de lignes valide.");
            return;
        }

        if (cmbColonnesTableau.SelectedItem is not ComboBoxItem itemColonnes ||
            !int.TryParse(itemColonnes.Content?.ToString(), out int colonnes))
        {
            MessageBox.Show("Choisissez un nombre de colonnes valide.");
            return;
        }

        if (lignes < 1 || lignes > 20 ||
            colonnes < 1 || colonnes > 20)
        {
            MessageBox.Show(
                "Le tableau doit contenir entre 1 et 20 lignes et colonnes.");
            return;
        }

        slideCanvas.ActiverInsertionTableau(slide, lignes, colonnes);

        MessageBox.Show(
            $"Tableau de {lignes} ligne(s) et {colonnes} colonne(s) prêt.\nCliquez sur la diapositive pour le placer.",
            "Insertion d'un tableau",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ModifierTableauSelectionne(Action<TableElement> modification)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez d'abord un tableau.");
            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        if (element is not TableElement tableau)
        {
            MessageBox.Show("L'élément sélectionné n'est pas un tableau.");
            return;
        }

        modification(tableau);

        slideCanvas.ActualiserElementSelectionne();
        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
        SlideCanvas_ElementSelectionChangee(tableau);
    }

    private void BtnAjouterLigneTableau_Click(object sender, RoutedEventArgs e)
    {
        ModifierTableauSelectionne(tableau =>
        {
            tableau.InitialiserCellules();
            tableau.Cells.Add(
                Enumerable.Range(0, tableau.Columns)
                    .Select(_ => string.Empty).ToList());
            tableau.Rows++;
        });
    }

    private void BtnSupprimerLigneTableau_Click(object sender, RoutedEventArgs e)
    {
        ModifierTableauSelectionne(tableau =>
        {
            tableau.InitialiserCellules();

            if (tableau.Rows <= 1)
                return;

            tableau.Cells.RemoveAt(tableau.Rows - 1);
            tableau.Rows--;
        });
    }

    private void BtnAjouterColonneTableau_Click(object sender, RoutedEventArgs e)
    {
        ModifierTableauSelectionne(tableau =>
        {
            tableau.InitialiserCellules();

            foreach (List<string> ligne in tableau.Cells)
                ligne.Add(string.Empty);

            tableau.Columns++;
        });
    }

    private void BtnSupprimerColonneTableau_Click(object sender, RoutedEventArgs e)
    {
        ModifierTableauSelectionne(tableau =>
        {
            tableau.InitialiserCellules();

            if (tableau.Columns <= 1)
                return;

            foreach (List<string> ligne in tableau.Cells)
                ligne.RemoveAt(tableau.Columns - 1);

            tableau.Columns--;
        });
    }

    // FORMES

    private void CmbFormes_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (cmbFormes.SelectedItem is not ComboBoxItem item ||
            item.Tag == null)
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

        ShapeElement forme = new()
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

    // VERROUILLAGE

    private void BtnVerrouiller_Click(object sender, RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez d'abord un élément à verrouiller.");
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

    // COULEUR DE REMPLISSAGE

    private void BtnCouleurRemplissage_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez d'abord une forme.");
            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        if (element is not ShapeElement)
        {
            MessageBox.Show("La couleur de remplissage s'applique aux formes.");
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

        ComboBox listeCouleurs = new() { Height = 34 };

        foreach (CouleurOption couleur in _couleursRemplissage)
        {
            listeCouleurs.Items.Add(new ComboBoxItem
            {
                Content = couleur.Nom,
                Tag = couleur.Code
            });
        }

        int index = Array.FindIndex(
            _couleursRemplissage,
            couleur => string.Equals(
                couleur.Code,
                element.Style.FillColor,
                StringComparison.OrdinalIgnoreCase));

        listeCouleurs.SelectedIndex = index >= 0 ? index : 0;
        panneau.Children.Add(listeCouleurs);

        Button bouton = new()
        {
            Content = "Appliquer la couleur",
            Height = 35,
            Margin = new Thickness(0, 20, 0, 0),
            IsDefault = true
        };

        bouton.Click += (_, _) =>
        {
            if (listeCouleurs.SelectedItem is not ComboBoxItem item ||
                item.Tag is not string couleur)
                return;

            element.Style.FillColor = couleur;

            slideCanvas.ActualiserElementSelectionne();
            lstDiapositives.Items.Refresh();

            fenetre.DialogResult = true;
            fenetre.Close();
        };

        panneau.Children.Add(bouton);
        fenetre.Content = panneau;
        fenetre.ShowDialog();
    }

    // COPIER / COUPER / COLLER

    private void BtnCopier_Click(object sender, RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez un élément à copier.");
            return;
        }

        _elementCopie = ClonerElement(element);
    }

    private void BtnCouper_Click(object sender, RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez un élément à couper.");
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

    private void BtnColler_Click(object sender, RoutedEventArgs e)
    {
        if (_elementCopie == null)
        {
            MessageBox.Show("Aucun élément à coller. Copiez d'abord un élément.");
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
            copie = new TextElement { Text = texte.Text };
        }
        else if (element is ShapeElement forme)
        {
            copie = new ShapeElement { ShapeType = forme.ShapeType };
        }
        else if (element is ImageElement image)
        {
            copie = new ImageElement { ImagePath = image.ImagePath };
        }
        else if (element is TableElement tableau)
        {
            copie = new TableElement
            {
                Rows = tableau.Rows,
                Columns = tableau.Columns,
                CellBackgroundColor = tableau.CellBackgroundColor,
                HeaderBackgroundColor = tableau.HeaderBackgroundColor,
                BorderColor = tableau.BorderColor,
                BorderThickness = tableau.BorderThickness,
                CellFontSize = tableau.CellFontSize,
                Cells = tableau.Cells
                    .Select(ligne => ligne.ToList()).ToList()
            };
        }
        else if (element is ChartElement graphique)
        {
            copie = new ChartElement
            {
                ChartType = graphique.ChartType,
                Title = graphique.Title,
                HorizontalAxisTitle = graphique.HorizontalAxisTitle,
                VerticalAxisTitle = graphique.VerticalAxisTitle,
                Categories = graphique.Categories.ToList(),
                Values = graphique.Values.ToList(),
                Colors = graphique.Colors.ToList(),
                ShowLegend = graphique.ShowLegend,
                ShowValues = graphique.ShowValues,
                ShowGridLines = graphique.ShowGridLines,
                LegendTitle = graphique.LegendTitle
            };
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

        copie.Animation = new ElementAnimation
        {
            Type = element.Animation?.Type ?? AnimationType.None,
            DurationMs = element.Animation?.DurationMs ?? 1000,
            Direction = element.Animation?.Direction ?? "Left",
            DelayMs = element.Animation?.DelayMs ?? 0,
            RepeatCount = element.Animation?.RepeatCount ?? 1
        };

        return copie;
    }

    // IMAGES

    private void BtnImage_Click(object sender, RoutedEventArgs e)
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

        ImageElement image = new()
        {
            ImagePath = dialogue.FileName,
            X = 250,
            Y = 150,
            Width = 300,
            Height = 200
        };

        slide.Elements.Add(image);
        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // ARRIERE-PLAN

    private void BtnArrierePlan_Click(object sender, RoutedEventArgs e)
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

    // CALQUES

    private void BtnPremierPlan_Click(object sender, RoutedEventArgs e)
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

    private void BtnArrierePlanCalque_Click(object sender, RoutedEventArgs e)
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

    private void BtnAvancerCalque_Click(object sender, RoutedEventArgs e)
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

    private void BtnReculerCalque_Click(object sender, RoutedEventArgs e)
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
            SlideElement? selection =
                slideCanvas.ElementSelectionne ?? _elementActuel;

            List<CalqueItem> calques = new();

            foreach (SlideElement element in slide.Elements
                .OrderByDescending(x => x.ZIndex))
            {
                string nom;

                if (element is TextElement texte)
                {
                    string contenu = (texte.Text ?? "")
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
                    nom = "Forme : " + forme.ShapeType;
                else if (element is ImageElement)
                    nom = "Image";
                else if (element is TableElement tableau)
                    nom = $"Tableau : {tableau.Rows} x {tableau.Columns}";
                else if (element is ChartElement graphique)
                    nom = "Graphique : " + graphique.ChartType;
                else
                    nom = "Élément";

                if (element.IsLocked)
                    nom = "🔒 " + nom;

                calques.Add(new CalqueItem(element, nom));
            }

            lstCalques.ItemsSource = calques;

            lstCalques.SelectedItem = selection == null
                ? null
                : calques.FirstOrDefault(
                    x => ReferenceEquals(x.Element, selection));
        }
        finally
        {
            _actualisationCalquesEnCours = false;
        }
    }

    private void LstCalques_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationCalquesEnCours)
            return;

        if (lstCalques.SelectedItem is not CalqueItem calque)
            return;

        slideCanvas.SelectionnerElementDepuisListe(calque.Element);
    }

    // ROTATION

    private void BtnRotation_Click(object sender, RoutedEventArgs e)
    {
        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez un élément à faire pivoter.");
            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        element.Rotation = (element.Rotation + 90) % 360;

        slideCanvas.ActualiserElementSelectionne();
        SlideCanvas_ElementSelectionChangee(element);
    }

    // SUPPRESSION D'ELEMENT

    private void BtnSupprimerElement_Click(object sender, RoutedEventArgs e)
    {
        SupprimerElementSelectionne();
    }

    private void SupprimerElementSelectionne()
    {
        if (lstDiapositives.SelectedItem is not Slide slide)
            return;

        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show("Sélectionnez d'abord un élément à supprimer.");
            return;
        }

        if (element.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        if (!slide.Elements.Remove(element))
        {
            MessageBox.Show("Impossible de trouver cet élément.");
            return;
        }

        _elementActuel = null;

        slideCanvas.DisplaySlide(slide);
        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
        ActualiserParametresAnimation(null);
    }

    // FORMATAGE TEXTE

    private bool VerifierSelectionTexte()
    {
        if (_elementActuel is TextElement && !ElementEstVerrouille())
            return true;

        if (ElementEstVerrouille())
        {
            AfficherMessageElementVerrouille();
            return false;
        }

        MessageBox.Show("Sélectionnez d'abord une zone de texte.");
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
            out double taille) || taille <= 0)
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

        if (cmbCouleurTexte.SelectedItem is not ComboBoxItem item ||
            item.Tag == null)
            return;

        texte.Style.TextColor = item.Tag.ToString()!;
        ActualiserFormatageTexte();
    }

    private void BtnGras_Click(object sender, RoutedEventArgs e)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;
        texte.Style.IsBold = !texte.Style.IsBold;
        ActualiserFormatageTexte();
    }

    private void BtnItalique_Click(object sender, RoutedEventArgs e)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;
        texte.Style.IsItalic = !texte.Style.IsItalic;
        ActualiserFormatageTexte();
    }

    private void BtnSouligne_Click(object sender, RoutedEventArgs e)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;
        texte.Style.IsUnderline = !texte.Style.IsUnderline;
        ActualiserFormatageTexte();
    }

    private void AppliquerAlignementTexte(string alignement)
    {
        if (!VerifierSelectionTexte())
            return;

        TextElement texte = (TextElement)_elementActuel!;
        texte.Style.TextAlignment = alignement;
        ActualiserFormatageTexte();
    }

    private void BtnAlignerGauche_Click(object sender, RoutedEventArgs e)
    {
        AppliquerAlignementTexte("Left");
    }

    private void BtnCentrerTexte_Click(object sender, RoutedEventArgs e)
    {
        AppliquerAlignementTexte("Center");
    }

    private void BtnAlignerDroite_Click(object sender, RoutedEventArgs e)
    {
        AppliquerAlignementTexte("Right");
    }

    // RACCOURCIS CLAVIER

    private void MainWindow_PreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        if (e.Key == Key.Delete && ctrl)
        {
            SupprimerElementSelectionne();
            e.Handled = true;
            return;
        }

        if (Keyboard.FocusedElement is TextBox)
            return;

        if (ctrl)
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

    private void SlideCanvas_ElementSelectionChangee(SlideElement? element)
    {
        _elementActuel = element;

        _actualisationEnCours = true;

        try
        {
            bool selectionExiste = element != null;
            bool verrouille = element?.IsLocked == true;

            txtPositionX.IsEnabled = selectionExiste && !verrouille;
            txtPositionY.IsEnabled = selectionExiste && !verrouille;
            txtLargeur.IsEnabled = selectionExiste && !verrouille;
            txtHauteur.IsEnabled = selectionExiste && !verrouille;
            sliderRotation.IsEnabled = selectionExiste && !verrouille;

            bool texteModifiable =
                element is TextElement && !verrouille;

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

                sliderRotation.Value = Math.Clamp(
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
        ActualiserParametresAnimation(element);
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

    // PROPRIETES

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
            out double valeur) ||
            double.IsNaN(valeur) ||
            double.IsInfinity(valeur))
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

    private void SliderRotation_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (_actualisationEnCours ||
            _elementActuel == null ||
            ElementEstVerrouille())
            return;

        _elementActuel.Rotation = e.NewValue;
        txtValeurRotation.Text = FormaterValeur(e.NewValue) + "°";

        slideCanvas.ActualiserElementSelectionne();
    }

    // TRANSITIONS

    private void CmbTransitions_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationTransitionEnCours ||
            lstDiapositives.SelectedItem is not Slide slide ||
            cmbTransitions.SelectedItem is not ComboBoxItem item)
            return;

        string? valeur = LireValeurCombo(item);

        if (!Enum.TryParse(valeur, true, out TransitionType type))
            return;

        slide.Transition ??= new Transition();
        slide.Transition.Type = type;

        AppliquerParametresTransition(slide);
    }

    private void SliderDureeTransition_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (_actualisationTransitionEnCours ||
            lstDiapositives.SelectedItem is not Slide slide)
            return;

        slide.Transition ??= new Transition();

        slide.Transition.DurationMs =
            (int)Math.Round(sliderDureeTransition.Value);

        if (txtDureeTransition != null)
            txtDureeTransition.Text = slide.Transition.DurationMs + " ms";
    }

    private void CmbDirectionTransition_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationTransitionEnCours ||
            lstDiapositives.SelectedItem is not Slide slide ||
            cmbDirectionTransition.SelectedItem is not ComboBoxItem item)
            return;

        string? direction = LireValeurCombo(item);

        if (string.IsNullOrWhiteSpace(direction))
            return;

        slide.Transition ??= new Transition();
        slide.Transition.Direction = direction;
    }

    private void AppliquerParametresTransition(Slide slide)
    {
        slide.Transition ??= new Transition();

        if (cmbTransitions.SelectedItem is ComboBoxItem item &&
            Enum.TryParse(
                LireValeurCombo(item),
                true,
                out TransitionType type))
        {
            slide.Transition.Type = type;
        }

        slide.Transition.DurationMs =
            (int)Math.Round(sliderDureeTransition.Value);

        if (cmbDirectionTransition.SelectedItem is ComboBoxItem directionItem)
        {
            string? direction = LireValeurCombo(directionItem);

            if (!string.IsNullOrWhiteSpace(direction))
                slide.Transition.Direction = direction;
        }

        if (txtDureeTransition != null)
            txtDureeTransition.Text = slide.Transition.DurationMs + " ms";
    }

    private void ActualiserParametresTransition(Slide slide)
    {
        slide.Transition ??= new Transition();

        _actualisationTransitionEnCours = true;

        try
        {
            string type = slide.Transition.Type.ToString();

            foreach (ComboBoxItem item in cmbTransitions.Items)
            {
                if (string.Equals(
                    LireValeurCombo(item),
                    type,
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbTransitions.SelectedItem = item;
                    break;
                }
            }

            sliderDureeTransition.Value = Math.Clamp(
                slide.Transition.DurationMs,
                sliderDureeTransition.Minimum,
                sliderDureeTransition.Maximum);

            if (txtDureeTransition != null)
                txtDureeTransition.Text = slide.Transition.DurationMs + " ms";

            foreach (ComboBoxItem item in cmbDirectionTransition.Items)
            {
                if (string.Equals(
                    LireValeurCombo(item),
                    slide.Transition.Direction,
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbDirectionTransition.SelectedItem = item;
                    break;
                }
            }
        }
        finally
        {
            _actualisationTransitionEnCours = false;
        }
    }

    // ANIMATIONS

    private void CmbAnimations_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationAnimationEnCours || _elementActuel == null)
            return;

        if (_elementActuel.IsLocked)
        {
            AfficherMessageElementVerrouille();
            ActualiserParametresAnimation(_elementActuel);
            return;
        }

        if (cmbAnimations.SelectedItem is not ComboBoxItem item)
            return;

        string? valeur = LireValeurCombo(item);

        if (!Enum.TryParse(
            valeur?.Replace(" ", ""),
            true,
            out AnimationType type))
            return;

        _elementActuel.Animation ??= new ElementAnimation();
        _elementActuel.Animation.Type = type;

        AppliquerParametresAnimation(_elementActuel);
    }

    private void SliderDureeAnimation_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (_actualisationAnimationEnCours ||
            _elementActuel == null ||
            _elementActuel.IsLocked)
            return;

        _elementActuel.Animation ??= new ElementAnimation();

        _elementActuel.Animation.DurationMs =
            (int)Math.Round(sliderDureeAnimation.Value);

        if (txtDureeAnimation != null)
            txtDureeAnimation.Text =
                _elementActuel.Animation.DurationMs + " ms";
    }

    private void CmbDirectionAnimation_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationAnimationEnCours ||
            _elementActuel == null ||
            _elementActuel.IsLocked)
            return;

        if (cmbDirectionAnimation.SelectedItem is not ComboBoxItem item)
            return;

        string? direction = LireValeurCombo(item);

        if (string.IsNullOrWhiteSpace(direction))
            return;

        _elementActuel.Animation ??= new ElementAnimation();
        _elementActuel.Animation.Direction = direction;
    }

    private void TxtDelaiAnimation_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_actualisationAnimationEnCours ||
            _elementActuel == null ||
            _elementActuel.IsLocked)
            return;

        if (!int.TryParse(
            txtDelaiAnimation.Text,
            NumberStyles.Integer,
            CultureInfo.CurrentCulture,
            out int delai))
            return;

        _elementActuel.Animation ??= new ElementAnimation();
        _elementActuel.Animation.DelayMs = Math.Max(0, delai);
    }

    private void CmbRepetitionsAnimation_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationAnimationEnCours ||
            _elementActuel == null ||
            _elementActuel.IsLocked)
            return;

        if (cmbRepetitionsAnimation.SelectedItem is not ComboBoxItem item)
            return;

        if (!LireEntierCombo(item, out int repetitions))
            return;

        _elementActuel.Animation ??= new ElementAnimation();
        _elementActuel.Animation.RepeatCount = repetitions;
    }

    private void AppliquerParametresAnimation(SlideElement element)
    {
        element.Animation ??= new ElementAnimation();

        if (cmbAnimations.SelectedItem is ComboBoxItem animationItem &&
            Enum.TryParse(
                LireValeurCombo(animationItem)?.Replace(" ", ""),
                true,
                out AnimationType type))
        {
            element.Animation.Type = type;
        }

        element.Animation.DurationMs =
            (int)Math.Round(sliderDureeAnimation.Value);

        element.Animation.DelayMs =
            int.TryParse(
                txtDelaiAnimation.Text,
                NumberStyles.Integer,
                CultureInfo.CurrentCulture,
                out int delai)
                ? Math.Max(0, delai)
                : 0;

        if (cmbDirectionAnimation.SelectedItem is ComboBoxItem directionItem)
        {
            string? direction = LireValeurCombo(directionItem);

            if (!string.IsNullOrWhiteSpace(direction))
                element.Animation.Direction = direction;
        }

        if (cmbRepetitionsAnimation.SelectedItem is ComboBoxItem repetitionItem &&
            LireEntierCombo(repetitionItem, out int repetitions))
        {
            element.Animation.RepeatCount = repetitions;
        }

        if (txtDureeAnimation != null)
            txtDureeAnimation.Text = element.Animation.DurationMs + " ms";
    }

    private void ActualiserParametresAnimation(SlideElement? element)
    {
        _actualisationAnimationEnCours = true;

        try
        {
            bool disponible = element != null;
            bool modifiable = disponible && !element!.IsLocked;

            cmbAnimations.IsEnabled = modifiable;
            sliderDureeAnimation.IsEnabled = modifiable;
            txtDelaiAnimation.IsEnabled = modifiable;
            cmbDirectionAnimation.IsEnabled = modifiable;
            cmbRepetitionsAnimation.IsEnabled = modifiable;

            if (element == null)
            {
                cmbAnimations.SelectedIndex = 0;
                sliderDureeAnimation.Value = 1000;

                if (txtDureeAnimation != null)
                    txtDureeAnimation.Text = "1000 ms";

                txtDelaiAnimation.Text = "0";
                cmbDirectionAnimation.SelectedIndex = 0;
                cmbRepetitionsAnimation.SelectedIndex = 0;
                return;
            }

            element.Animation ??= new ElementAnimation();

            string type = element.Animation.Type.ToString();
            bool trouve = false;

            foreach (ComboBoxItem item in cmbAnimations.Items)
            {
                if (string.Equals(
                    LireValeurCombo(item)?.Replace(" ", ""),
                    type,
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbAnimations.SelectedItem = item;
                    trouve = true;
                    break;
                }
            }

            if (!trouve)
                cmbAnimations.SelectedIndex = 0;

            sliderDureeAnimation.Value = Math.Clamp(
                element.Animation.DurationMs,
                sliderDureeAnimation.Minimum,
                sliderDureeAnimation.Maximum);

            if (txtDureeAnimation != null)
                txtDureeAnimation.Text = element.Animation.DurationMs + " ms";

            txtDelaiAnimation.Text =
                element.Animation.DelayMs.ToString(CultureInfo.CurrentCulture);

            trouve = false;

            foreach (ComboBoxItem item in cmbDirectionAnimation.Items)
            {
                if (string.Equals(
                    LireValeurCombo(item),
                    element.Animation.Direction,
                    StringComparison.OrdinalIgnoreCase))
                {
                    cmbDirectionAnimation.SelectedItem = item;
                    trouve = true;
                    break;
                }
            }

            if (!trouve)
                cmbDirectionAnimation.SelectedIndex = 0;

            trouve = false;

            foreach (ComboBoxItem item in cmbRepetitionsAnimation.Items)
            {
                if (LireEntierCombo(item, out int repetitions) &&
                    repetitions == element.Animation.RepeatCount)
                {
                    cmbRepetitionsAnimation.SelectedItem = item;
                    trouve = true;
                    break;
                }
            }

            if (!trouve)
                cmbRepetitionsAnimation.SelectedIndex = 0;
        }
        finally
        {
            _actualisationAnimationEnCours = false;
        }
    }

    // GRAPHIQUES

    private void BtnGraphique_Click(object sender, RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is not Slide diapositive)
        {
            MessageBox.Show("Sélectionnez d'abord une diapositive.");
            return;
        }

        ChartElement graphique = new()
        {
            X = 250,
            Y = 120,
            Width = 460,
            Height = 300,
            ChartType = ChartType.Column,
            Title = "Mon graphique",
            HorizontalAxisTitle = "Catégories",
            VerticalAxisTitle = "Valeurs"
        };

        diapositive.Elements.Add(graphique);

        slideCanvas.DisplaySlide(diapositive);
        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }

    // OUTILS

    private string FormaterValeur(double valeur)
    {
        return valeur.ToString("0.##", CultureInfo.CurrentCulture);
    }
    private void SlideCanvas_MouseLeftButtonDown(
    object sender,
    MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2)
            return;

        SlideElement? element =
            slideCanvas.ElementSelectionne ?? _elementActuel;

        if (element is not ChartElement graphique)
            return;

        if (graphique.IsLocked)
        {
            AfficherMessageElementVerrouille();
            return;
        }

        ChartEditorWindow fenetre = new(graphique)
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (fenetre.ShowDialog() == true)
        {
            slideCanvas.ActualiserElementSelectionne();

            lstDiapositives.Items.Refresh();

            ActualiserListeCalques();
        }

        e.Handled = true;
    }
}
