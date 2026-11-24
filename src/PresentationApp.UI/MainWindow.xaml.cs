
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PresentationApp.Core.Models;

namespace PresentationApp.UI;

public partial class MainWindow : Window
{
    private readonly Presentation _presentation = new();

    private SlideElement? _elementActuel;
    private SlideElement? _elementCopie;

    private bool _actualisationEnCours;

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

    private int _indexCouleurFond;

    private const string TexteTitre =
        "Cliquez pour ajouter un titre";

    private const string TexteSousTitre =
        "Cliquez pour ajouter un sous-titre";

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

    private void InitialiserPresentation()
    {
        var premiereDiapositive = CreerDiapositive(1);

        _presentation.Slides.Add(premiereDiapositive);

        lstDiapositives.ItemsSource = _presentation.Slides;

        lstDiapositives.SelectionChanged +=
            LstDiapositives_SelectionChanged;

        lstDiapositives.SelectedIndex = 0;
    }

    private Slide CreerDiapositive(int ordre)
    {
        var diapositive = new Slide
        {
            Order = ordre
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
                FontSize = 18
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
                FontSize = 18
            }
        });

        return diapositive;
    }

    private void LstDiapositives_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
        {
            _elementActuel = null;
            slideCanvas.DisplaySlide(slide);
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

    private void BtnTexte_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
            slideCanvas.ActiverInsertionTexte(slide);
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

        if (!Enum.TryParse(
                item.Tag.ToString(),
                out ShapeType type))
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
    }

    // COPIER

    private void BtnCopier_Click(
        object sender,
        RoutedEventArgs e)
    {
        SlideElement? element = slideCanvas.ElementSelectionne
            ?? _elementActuel;

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
        SlideElement? element = slideCanvas.ElementSelectionne
            ?? _elementActuel;

        if (element == null)
        {
            MessageBox.Show(
                "Sélectionnez un élément à couper.",
                "Couper",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
    }

    // ARRIERE-PLAN

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
    }

    // ROTATION

    private void BtnRotation_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_elementActuel == null)
        {
            MessageBox.Show(
                "Sélectionnez un élément à faire pivoter.",
                "Rotation",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        _elementActuel.Rotation =
            (_elementActuel.Rotation + 90) % 360;

        slideCanvas.ActualiserElementSelectionne();
        SlideCanvas_ElementSelectionChangee(_elementActuel);
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

        // Récupérer l'élément réellement sélectionné sur le canvas.
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

        // Vérifier que l'élément appartient bien à la diapositive.
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

        // Réinitialiser la sélection.
        _elementActuel = null;

        // Reconstruire le canvas sans l'élément supprimé.
        slideCanvas.DisplaySlide(slide);

        lstDiapositives.Items.Refresh();
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
        if (_elementActuel is TextElement)
            return true;

        MessageBox.Show(
            "Sélectionnez d'abord une zone de texte.",
            "Formatage du texte",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        return false;
    }

    private void ActualiserFormatageTexte()
    {
        if (_elementActuel is not TextElement)
            return;

        slideCanvas.ActualiserElementSelectionne();
        lstDiapositives.Items.Refresh();
    }

    private void CmbPolice_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_actualisationEnCours ||
            _elementActuel is not TextElement texte)
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
            _elementActuel is not TextElement texte)
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
            _elementActuel is not TextElement texte)
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

        // Ctrl + Suppr : supprimer toute la zone sélectionnée,
        // même lorsque le curseur est dans un TextBox.
        if (e.Key == Key.Delete && toucheCtrl)
        {
            SupprimerElementSelectionne();
            e.Handled = true;
            return;
        }

        // Lorsqu'on écrit dans une zone de texte,
        // Suppr doit continuer à supprimer les caractères.
        if (Keyboard.FocusedElement is TextBox)
        {
            return;
        }

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
    }

    private void SlideCanvas_ElementSelectionChangee(
        SlideElement? element)
    {
        _elementActuel = element;

        _actualisationEnCours = true;

        bool selectionExiste = element != null;

        txtPositionX.IsEnabled = selectionExiste;
        txtPositionY.IsEnabled = selectionExiste;
        txtLargeur.IsEnabled = selectionExiste;
        txtHauteur.IsEnabled = selectionExiste;
        sliderRotation.IsEnabled = selectionExiste;

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
                Math.Clamp(element.Rotation, 0, 360);

            txtValeurRotation.Text =
                FormaterValeur(element.Rotation) + "°";
        }

        if (element is TextElement texte)
            SelectionnerValeursFormatage(texte);

        _actualisationEnCours = false;
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
        if (_actualisationEnCours || _elementActuel == null)
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

    // ROTATION

    private void SliderRotation_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (_actualisationEnCours || _elementActuel == null)
            return;

        _elementActuel.Rotation = e.NewValue;

        txtValeurRotation.Text =
            FormaterValeur(e.NewValue) + "°";

        slideCanvas.ActualiserElementSelectionne();
    }

    private string FormaterValeur(double valeur)
    {
        return valeur.ToString(
            "0.##",
            CultureInfo.CurrentCulture);
    }
}
