
using System.Windows;
using System.Windows.Controls;
using PresentationApp.Core.Models;

namespace PresentationApp.UI;

public partial class MainWindow : Window
{
    private readonly Presentation _presentation = new();

    public MainWindow()
    {
        InitializeComponent();
        InitialiserPresentation();
    }

    private void InitialiserPresentation()
    {
        var premiereDiapositive = new Slide
        {
            Order = 1
        };

        _presentation.Slides.Add(premiereDiapositive);

        lstDiapositives.ItemsSource = _presentation.Slides;
        lstDiapositives.DisplayMemberPath = nameof(Slide.Order);

        lstDiapositives.SelectionChanged +=
            LstDiapositives_SelectionChanged;

        lstDiapositives.SelectedIndex = 0;
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

        var nouvelleDiapositive = new Slide
        {
            Order = nouvelOrdre
        };

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

            MessageBox.Show(
                "Clique sur la diapositive pour commencer à écrire.",
                "Insertion de texte",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
