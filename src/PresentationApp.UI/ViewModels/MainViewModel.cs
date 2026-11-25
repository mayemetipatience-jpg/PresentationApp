using PresentationApp.Core.Models;
using PresentationApp.Core.Services;
using PresentationApp.IO.Serialization;
using PresentationApp.UI.Services;
using System.Collections.ObjectModel;
using System.IO;
using CoreCommandManager = PresentationApp.Core.Commands.CommandManager;

namespace PresentationApp.UI.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly PresentationService _slideService;
    private readonly ElementService _elementService;
    private readonly AutoSaveTimer _autoSave;
    private Slide? _selectedSlide;
    private SlideElement? _selectedElement;
    private string? _cheminFichier;
    private string? _errorMessage;

    public MainViewModel() : this(new Presentation())
    {
    }

    public MainViewModel(Presentation presentation)
    {
        History = new CoreCommandManager();
        _slideService = new PresentationService(presentation, History);
        _elementService = new ElementService(History);

        AddSlideCommand = new RelayCommand(AddSlide);
        RemoveSlideCommand = new RelayCommand(RemoveSelectedSlide, () => SelectedSlide != null);
        DuplicateSlideCommand = new RelayCommand(DuplicateSelectedSlide, () => SelectedSlide != null);
        MoveSlideUpCommand = new RelayCommand(() => MoveSelectedSlide(-1), () => CanMove(-1));
        MoveSlideDownCommand = new RelayCommand(() => MoveSelectedSlide(1), () => CanMove(1));
        AddTextCommand = new RelayCommand(AddText, () => SelectedSlide != null);
        DeleteElementCommand = new RelayCommand(DeleteSelectedElement, () => SelectedSlide != null && SelectedElement != null);
        UndoCommand = new RelayCommand(History.Undo, () => History.CanUndo);
        RedoCommand = new RelayCommand(History.Redo, () => History.CanRedo);

        History.Changed += (_, _) => RefreshSlides();

        if (presentation.Slides.Count == 0)
            presentation.Slides.Add(new Slide());

        RefreshSlides();

        // Sauvegarde de secours toutes les 2 minutes, si la présentation a déjà un fichier.
        _autoSave = new AutoSaveTimer(() => CurrentPresentation, () => CheminFichier);
        _autoSave.Demarrer();
    }

    // Historique Undo/Redo. Son evenement Changed se declenche apres chaque action,
    // y compris un deplacement d'element : le canevas peut s'y abonner pour se redessiner.
    public CoreCommandManager History { get; }

    public Presentation CurrentPresentation => _slideService.Current;
    public PresentationService SlideOperations => _slideService;
    public ElementService ElementOperations => _elementService;

    public ObservableCollection<Slide> Slides { get; } = new();

    public int SlideCount => Slides.Count;

    // Fichier actuellement ouvert ou enregistré. Vide tant que la présentation n'a jamais été enregistrée.
    public string? CheminFichier
    {
        get => _cheminFichier;
        private set => SetProperty(ref _cheminFichier, value);
    }

    // Message à afficher quand une opération échoue. Vide si tout va bien.
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public Slide? SelectedSlide
    {
        get => _selectedSlide;
        set
        {
            if (SetProperty(ref _selectedSlide, value))
            {
                SelectedElement = null;
                RaiseCommandStates();
            }
        }
    }

    public SlideElement? SelectedElement
    {
        get => _selectedElement;
        set
        {
            if (SetProperty(ref _selectedElement, value))
                RaiseCommandStates();
        }
    }

    public RelayCommand AddSlideCommand { get; }
    public RelayCommand RemoveSlideCommand { get; }
    public RelayCommand DuplicateSlideCommand { get; }
    public RelayCommand MoveSlideUpCommand { get; }
    public RelayCommand MoveSlideDownCommand { get; }
    public RelayCommand AddTextCommand { get; }
    public RelayCommand DeleteElementCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand RedoCommand { get; }

    // Enregistre la présentation dans le fichier indiqué.
    public void Enregistrer(string chemin)
    {
        try
        {
            ProjectFileService.Save(CurrentPresentation, chemin);
            CheminFichier = chemin;
            ErrorMessage = null;
        }
        catch (IOException ex)
        {
            ErrorMessage = "Impossible d'enregistrer le fichier : " + ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = "Impossible d'enregistrer le fichier : " + ex.Message;
        }
    }

    // Ouvre un fichier et remplace la présentation actuelle.
    // Renvoie false si le fichier est illisible : ErrorMessage contient alors l'explication.
    public bool Ouvrir(string chemin)
    {
        try
        {
            Presentation chargee = ProjectFileService.Open(chemin);

            CurrentPresentation.Title = chargee.Title;
            CurrentPresentation.Slides.Clear();
            foreach (Slide slide in chargee.Slides)
                CurrentPresentation.Slides.Add(slide);

            // L'historique Undo/Redo ne doit pas annuler des actions sur l'ancien fichier.
            History.Clear();

            CheminFichier = chemin;
            ErrorMessage = null;
            RefreshSlides();
            return true;
        }
        catch (CorruptFileException ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        catch (IOException ex)
        {
            ErrorMessage = "Impossible d'ouvrir le fichier : " + ex.Message;
            return false;
        }
    }

    private void AddSlide()
    {
        var slide = _slideService.AddSlide();
        SelectedSlide = slide;
    }

    private void RemoveSelectedSlide()
    {
        if (SelectedSlide == null) return;
        var index = Slides.IndexOf(SelectedSlide);
        _slideService.RemoveSlide(SelectedSlide);
        if (Slides.Count > 0)
            SelectedSlide = Slides[Math.Clamp(index, 0, Slides.Count - 1)];
    }

    private void DuplicateSelectedSlide()
    {
        if (SelectedSlide == null) return;
        var copy = _slideService.DuplicateSlide(SelectedSlide);
        if (copy != null) SelectedSlide = copy;
    }

    private bool CanMove(int delta)
    {
        if (SelectedSlide == null) return false;
        var target = Slides.IndexOf(SelectedSlide) + delta;
        return target >= 0 && target < Slides.Count;
    }

    private void MoveSelectedSlide(int delta)
    {
        if (SelectedSlide == null || !CanMove(delta)) return;
        var from = Slides.IndexOf(SelectedSlide);
        _slideService.MoveSlide(from, from + delta);
    }

    private void AddText()
    {
        if (SelectedSlide == null) return;
        SelectedElement = _elementService.AddText(SelectedSlide, "Nouveau texte", 50, 50);
    }

    private void DeleteSelectedElement()
    {
        if (SelectedSlide == null || SelectedElement == null) return;
        _elementService.Remove(SelectedSlide, SelectedElement);
        SelectedElement = null;
    }

    private void RefreshSlides()
    {
        var keepSlide = SelectedSlide;
        var keepElement = SelectedElement;

        Slides.Clear();
        foreach (var slide in _slideService.Current.Slides)
            Slides.Add(slide);

        SelectedSlide = keepSlide != null && Slides.Contains(keepSlide)
            ? keepSlide
            : Slides.FirstOrDefault();

        SelectedElement = keepElement != null && SelectedSlide != null && SelectedSlide.Elements.Contains(keepElement)
            ? keepElement
            : null;

        OnPropertyChanged(nameof(SlideCount));
        RaiseCommandStates();
    }

    private void RaiseCommandStates()
    {
        RemoveSlideCommand.RaiseCanExecuteChanged();
        DuplicateSlideCommand.RaiseCanExecuteChanged();
        MoveSlideUpCommand.RaiseCanExecuteChanged();
        MoveSlideDownCommand.RaiseCanExecuteChanged();
        AddTextCommand.RaiseCanExecuteChanged();
        DeleteElementCommand.RaiseCanExecuteChanged();
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }
}