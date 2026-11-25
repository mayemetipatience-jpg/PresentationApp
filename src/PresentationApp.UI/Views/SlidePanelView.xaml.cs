using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PresentationApp.Core.Models;
using PresentationApp.UI.ViewModels;

namespace PresentationApp.UI.Views;

// Panneau des miniatures. Le code ici ne gère que la souris
// (glisser-déposer, clic droit) : les actions sont faites par SlideListViewModel.
public partial class SlidePanelView : UserControl
{
    private Point _pointDepart;
    private Slide? _diapositiveCliquee;

    public SlidePanelView()
    {
        InitializeComponent();
    }

    private SlideListViewModel? ViewModel => DataContext as SlideListViewModel;

    // Redessine les miniatures (après une modification faite dans le canevas)
    public void Rafraichir()
    {
        lstDiapositives.Items.Refresh();
    }

    // ---------- Glisser-déposer pour réordonner ----------

    private void Liste_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _pointDepart = e.GetPosition(null);
        _diapositiveCliquee = TrouverDiapositive(e.OriginalSource as DependencyObject);
    }

    private void Liste_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _diapositiveCliquee == null)
            return;

        // On ne commence le glisser qu'après un petit déplacement (sinon c'est un simple clic)
        Vector ecart = _pointDepart - e.GetPosition(null);
        if (Math.Abs(ecart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(ecart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        Slide glissee = _diapositiveCliquee;
        _diapositiveCliquee = null;
        DragDrop.DoDragDrop(lstDiapositives, glissee, DragDropEffects.Move);
    }

    private void Liste_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(Slide)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void Liste_Drop(object sender, DragEventArgs e)
    {
        if (ViewModel == null || e.Data.GetData(typeof(Slide)) is not Slide glissee)
            return;

        int depart = ViewModel.Slides.IndexOf(glissee);

        // Lâchée sur une miniature : elle prend sa place. Lâchée dans le vide : elle va à la fin.
        Slide? cible = TrouverDiapositive(e.OriginalSource as DependencyObject);
        int arrivee = cible != null ? ViewModel.Slides.IndexOf(cible) : ViewModel.Slides.Count - 1;

        ViewModel.DeplacerDiapositive(depart, arrivee);
    }

    // ---------- Clic droit : sélectionne la miniature avant d'ouvrir le menu ----------

    private void Liste_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Slide? diapositive = TrouverDiapositive(e.OriginalSource as DependencyObject);
        if (diapositive != null && ViewModel != null)
            ViewModel.SelectedSlide = diapositive;
    }

    // Remonte depuis l'élément cliqué jusqu'à la ligne de la liste (ListBoxItem)
    private static Slide? TrouverDiapositive(DependencyObject? element)
    {
        while (element != null && element is not ListBoxItem)
        {
            element = element is Visual
                ? VisualTreeHelper.GetParent(element)
                : LogicalTreeHelper.GetParent(element);
        }

        return (element as ListBoxItem)?.DataContext as Slide;
    }
}
