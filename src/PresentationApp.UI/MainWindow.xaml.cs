
using System.Windows;
using System.Windows.Controls;
using PresentationApp.Core.Models;

namespace PresentationApp.UI;
public partial class MainWindow : Window
{
    // Présentation principale
    private readonly Presentation _presentation = new();

    public MainWindow()
    {
        InitializeComponent();

        InitialiserPresentation();
    }

    private void InitialiserPresentation()
    {
        // Créer la première diapositive
        var premiereDiapositive = new Slide
        {
            Order = 1
        };

        _presentation.Slides.Add(premiereDiapositive);

        // Connecter les diapositives à la liste de gauche
        lstDiapositives.ItemsSource = _presentation.Slides;
        lstDiapositives.DisplayMemberPath = nameof(Slide.Order);

        // Écouter les changements de sélection
        lstDiapositives.SelectionChanged +=
            LstDiapositives_SelectionChanged;

        // Sélectionner la première diapositive
        lstDiapositives.SelectedIndex = 0;
    }

    private void BtnNouvelleDiapositive_Click(
        object sender,
        RoutedEventArgs e)
    {
        // Calculer le numéro de la nouvelle diapositive
        int nouvelOrdre = _presentation.Slides.Count + 1;

        // Créer la nouvelle diapositive
        var nouvelleDiapositive = new Slide
        {
            Order = nouvelOrdre
        };

        // Ajouter la diapositive à la présentation
        _presentation.Slides.Add(nouvelleDiapositive);

        // Actualiser la liste de diapositives
        lstDiapositives.Items.Refresh();

        // Sélectionner automatiquement la nouvelle diapositive
        lstDiapositives.SelectedItem = nouvelleDiapositive;
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
}
