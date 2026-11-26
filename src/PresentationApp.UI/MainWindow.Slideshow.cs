using System;
using System.Linq;
using System.Windows;
using PresentationApp.Core.Models;
using PresentationApp.UI.Slideshow;

namespace PresentationApp.UI;

public partial class MainWindow
{
    private void BtnDemarrerDiaporama_Click(object sender, RoutedEventArgs e)
    {
        DemarrerDiaporama(0);
    }

    private void BtnDemarrerDepuisSelection_Click(object sender, RoutedEventArgs e)
    {
        int index = lstDiapositives.SelectedIndex;
        if (index < 0)
        {
            MessageBox.Show(
                "Sélectionnez d'abord une diapositive.",
                "Diaporama",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        DemarrerDiaporama(index);
    }

    private void DemarrerDiaporama(int indexDepart)
    {
        if (_presentation.Slides.Count == 0)
        {
            MessageBox.Show(
                "La présentation ne contient aucune diapositive.",
                "Diaporama",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        // Réorganise les diapositives suivant leur ordre dans la liste de l'éditeur.
        var slides = _presentation.Slides.ToList();
        var fenetre = new SlideshowWindow(slides, indexDepart)
        {
            Owner = this
        };

        fenetre.ShowDialog();
    }
}
