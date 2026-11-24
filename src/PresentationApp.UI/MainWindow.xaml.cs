
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using PresentationApp.Core.Models;

namespace PresentationApp.UI;

public partial class MainWindow : Window
{
    private readonly Presentation _presentation = new();

    private SlideElement? _elementActuel;

    private bool _actualisationEnCours;

    private const string TexteTitre =
        "Cliquez pour ajouter un titre";

    private const string TexteSousTitre =
        "Cliquez pour ajouter un sous-titre";

    public MainWindow()
    {
        InitializeComponent();

        slideCanvas.ElementSelectionChangee +=
            SlideCanvas_ElementSelectionChangee;

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
            Text = TexteTitre
        });

        diapositive.Elements.Add(new TextElement
        {
            X = 100,
            Y = 240,
            Width = 760,
            Height = 70,
            Text = TexteSousTitre
        });

        return diapositive;
    }

    private void LstDiapositives_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
        {
            slideCanvas.DisplaySlide(slide);
        }
    }

    private void BtnNouvelleDiapositive_Click(
        object sender,
        RoutedEventArgs e)
    {
        int nouvelOrdre = _presentation.Slides.Count + 1;

        var nouvelleDiapositive =
            CreerDiapositive(nouvelOrdre);

        _presentation.Slides.Add(nouvelleDiapositive);

        lstDiapositives.Items.Refresh();
        lstDiapositives.SelectedItem = nouvelleDiapositive;
    }

    private void BtnTexte_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (lstDiapositives.SelectedItem is Slide slide)
        {
            slideCanvas.ActiverInsertionTexte(slide);
        }
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

        _actualisationEnCours = false;
    }

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
