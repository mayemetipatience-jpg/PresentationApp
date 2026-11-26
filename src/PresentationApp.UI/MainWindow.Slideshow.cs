using System.Windows;
using PresentationApp.Core.Models;
using PresentationApp.UI.Slideshow;

namespace PresentationApp.UI;

// Lancement du diaporama en plein écran (fenêtre SlideshowWindow de l'Étudiant 2).
// Les transitions et animations réglées dans le ruban sont jouées automatiquement.
public partial class MainWindow
{
    private void DemarrerDiaporama(bool depuisSelection)
    {
        List<Slide> diapositives = _vm.Slides.ToList();

        if (diapositives.Count == 0)
        {
            Informer("La présentation ne contient aucune diapositive.", "Diaporama");
            return;
        }

        int depart = depuisSelection && _vm.SelectedSlide != null
            ? Math.Max(0, diapositives.IndexOf(_vm.SelectedSlide))
            : 0;

        var fenetre = new SlideshowWindow(diapositives, depart) { Owner = this };
        fenetre.ShowDialog();
    }
}
