using System.Collections.ObjectModel;
using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.ViewModels;

// ViewModel du panneau des miniatures (à gauche).
// Il ne modifie jamais la présentation directement : tout passe par
// l'historique du MainViewModel, donc chaque action peut être annulée (Ctrl+Z).
public class SlideListViewModel : ObservableObject
{
    // Textes d'exemple d'une nouvelle diapositive (les mêmes que dans le canevas)
    public const string TexteTitre = "Cliquez pour ajouter un titre";
    public const string TexteSousTitre = "Cliquez pour ajouter un sous-titre";

    private readonly MainViewModel _main;

    public SlideListViewModel(MainViewModel main)
    {
        _main = main;

        NouvelleCommand = new RelayCommand(AjouterDiapositive);
        DupliquerCommand = new RelayCommand(() => _main.DuplicateSlideCommand.Execute(null),
                                            () => SelectedSlide != null);
        SupprimerCommand = new RelayCommand(SupprimerDiapositive,
                                            () => SelectedSlide != null && Slides.Count > 1);

        _main.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedSlide))
                OnPropertyChanged(nameof(SelectedSlide));
        };

        // Après chaque action (et chaque Annuler/Refaire), la liste a été reconstruite :
        // on redonne la sélection à la ListBox et on met à jour les boutons.
        _main.History.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(SelectedSlide));
            DupliquerCommand.RaiseCanExecuteChanged();
            SupprimerCommand.RaiseCanExecuteChanged();
        };
    }

    public ObservableCollection<Slide> Slides => _main.Slides;

    public Slide? SelectedSlide
    {
        get => _main.SelectedSlide;
        set
        {
            // Quand la liste est vidée puis reconstruite, la ListBox envoie null :
            // on l'ignore pour ne pas perdre la diapositive affichée.
            if (value == null) return;
            _main.SelectedSlide = value;
            DupliquerCommand.RaiseCanExecuteChanged();
            SupprimerCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand NouvelleCommand { get; }
    public RelayCommand DupliquerCommand { get; }
    public RelayCommand SupprimerCommand { get; }
    public RelayCommand MonterCommand => _main.MoveSlideUpCommand;
    public RelayCommand DescendreCommand => _main.MoveSlideDownCommand;

    // Fournie par la vue pour demander une confirmation (boîte de dialogue).
    // Le ViewModel ne connaît pas MessageBox : il reste testable sans fenêtre.
    public Func<string, bool>? Confirmer { get; set; }

    // Crée une diapositive avec une zone de titre et une zone de sous-titre
    public static Slide CreerDiapositive()
    {
        var diapositive = new Slide();

        diapositive.Elements.Add(new TextElement
        {
            X = 100, Y = 130, Width = 760, Height = 90,
            Text = TexteTitre,
            Style = new ElementStyle { TextAlignment = "Center", FontSize = 18 }
        });

        diapositive.Elements.Add(new TextElement
        {
            X = 100, Y = 240, Width = 760, Height = 70,
            ZIndex = 1,
            Text = TexteSousTitre,
            Style = new ElementStyle { TextAlignment = "Center", FontSize = 18 }
        });

        return diapositive;
    }

    // Ajoute une diapositive juste après celle qui est sélectionnée
    private void AjouterDiapositive()
    {
        int position = SelectedSlide != null ? Slides.IndexOf(SelectedSlide) + 1 : Slides.Count;
        Slide nouvelle = CreerDiapositive();

        _main.History.Execute(new AddSlideCommand(_main.CurrentPresentation, nouvelle, position));
        _main.SelectedSlide = nouvelle;
    }

    private void SupprimerDiapositive()
    {
        if (SelectedSlide == null || Slides.Count <= 1)
            return;

        if (Confirmer != null && !Confirmer("Voulez-vous vraiment supprimer cette diapositive ?"))
            return;

        _main.RemoveSlideCommand.Execute(null);
    }

    // Appelée par le glisser-déposer des miniatures
    public void DeplacerDiapositive(int depart, int arrivee)
    {
        if (depart < 0 || arrivee < 0 || depart == arrivee)
            return;

        Slide deplacee = Slides[depart];
        _main.SlideOperations.MoveSlide(depart, arrivee);
        _main.SelectedSlide = deplacee;
    }
}
