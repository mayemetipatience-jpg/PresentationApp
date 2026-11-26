using PresentationApp.Core.Models;

namespace PresentationApp.UI.ViewModels;

// Un choix dans une liste : le texte affiché (en français) et la valeur enregistrée
public record Choix<T>(string Nom, T Valeur);

// ViewModel des onglets « Transitions » et « Animations » du ruban.
//   - la transition s'applique à la diapositive sélectionnée
//   - l'animation s'applique à l'élément sélectionné (sauf s'il est verrouillé)
// Le diaporama (SlideshowWindow) lit ensuite ces réglages.
public class EffetsViewModel : ObservableObject
{
    private readonly MainViewModel _main;

    public EffetsViewModel(MainViewModel main)
    {
        _main = main;

        AppliquerATousCommand = new RelayCommand(AppliquerTransitionATous, () => Diapositive != null);

        _main.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainViewModel.SelectedSlide) or nameof(MainViewModel.SelectedElement))
                Recharger();
        };
        _main.History.Changed += (_, _) => Recharger();
    }

    // Prévient la fenêtre qu'un réglage a changé (titre avec « * »)
    public event Action? Modifie;

    private Slide? Diapositive => _main.SelectedSlide;
    private SlideElement? Element => _main.SelectedElement;

    // ---------- Listes ----------

    public IReadOnlyList<Choix<TransitionType>> Transitions { get; } = new Choix<TransitionType>[]
    {
        new("Aucune", TransitionType.None), new("Fondu", TransitionType.Fade),
        new("Balayage", TransitionType.Wipe), new("Poussée", TransitionType.Push),
        new("Fractionner", TransitionType.Split), new("Révéler", TransitionType.Reveal),
        new("Couvrir", TransitionType.Cover), new("Zoom", TransitionType.Zoom),
        new("Rotation", TransitionType.Rotate)
    };

    public IReadOnlyList<Choix<AnimationType>> Animations { get; } = new Choix<AnimationType>[]
    {
        new("Aucune", AnimationType.None), new("Apparaître", AnimationType.Appear),
        new("Entrée en vol", AnimationType.FlyIn), new("Zoom", AnimationType.Zoom),
        new("Rebond", AnimationType.Bounce), new("Rotation", AnimationType.Rotate),
        new("Pulsation", AnimationType.Pulse), new("Disparaître", AnimationType.Disappear),
        new("Sortie en vol", AnimationType.FlyOut)
    };

    // Les valeurs anglaises sont celles attendues par le diaporama
    public IReadOnlyList<Choix<string>> Directions { get; } = new Choix<string>[]
    {
        new("Depuis la gauche", "Left"), new("Depuis la droite", "Right"),
        new("Depuis le haut", "Top"), new("Depuis le bas", "Bottom")
    };

    public IReadOnlyList<Choix<int>> Repetitions { get; } = new Choix<int>[]
    {
        new("1 fois", 1), new("2 fois", 2), new("3 fois", 3), new("5 fois", 5), new("En continu", 0)
    };

    // ---------- Transition de la diapositive ----------

    public bool ADiapositive => Diapositive != null;

    public TransitionType TypeTransition
    {
        get => Diapositive?.Transition?.Type ?? TransitionType.None;
        set => ModifierTransition(t => t.Type = value);
    }

    public int DureeTransition
    {
        get => Diapositive?.Transition?.DurationMs ?? 500;
        set => ModifierTransition(t => t.DurationMs = Math.Clamp(value, 100, 5000));
    }

    public string DirectionTransition
    {
        get => Diapositive?.Transition?.Direction ?? "Left";
        set => ModifierTransition(t => t.Direction = value);
    }

    public RelayCommand AppliquerATousCommand { get; }

    // ---------- Animation de l'élément ----------

    public bool AnimationModifiable => Element is { IsLocked: false };

    public string MessageAnimation => Element switch
    {
        null => "Sélectionnez un élément sur la diapositive pour lui donner une animation.",
        { IsLocked: true } => "Cet élément est verrouillé : déverrouillez-le pour modifier son animation.",
        _ => "L'animation sera jouée pendant le diaporama, dans l'ordre des calques."
    };

    public AnimationType TypeAnimation
    {
        get => Element?.Animation?.Type ?? AnimationType.None;
        set => ModifierAnimation(a => a.Type = value);
    }

    public int DureeAnimation
    {
        get => Element?.Animation?.DurationMs ?? 1000;
        set => ModifierAnimation(a => a.DurationMs = Math.Clamp(value, 100, 5000));
    }

    // Contrôle de saisie : un délai entre 0 et 60 secondes
    public int DelaiAnimation
    {
        get => Element?.Animation?.DelayMs ?? 0;
        set => ModifierAnimation(a => a.DelayMs = Math.Clamp(value, 0, 60000));
    }

    public string DirectionAnimation
    {
        get => Element?.Animation?.Direction ?? "Left";
        set => ModifierAnimation(a => a.Direction = value);
    }

    public int RepetitionsAnimation
    {
        get => Element?.Animation?.RepeatCount ?? 1;
        set => ModifierAnimation(a => a.RepeatCount = Math.Max(0, value));
    }

    // ---------- Outils ----------

    public void Recharger()
    {
        OnPropertyChanged(string.Empty);
        AppliquerATousCommand.RaiseCanExecuteChanged();
    }

    private void ModifierTransition(Action<Transition> modification)
    {
        if (Diapositive is not { } diapositive)
            return;

        diapositive.Transition ??= new Transition();
        modification(diapositive.Transition);
        Modifie?.Invoke();
        OnPropertyChanged(string.Empty);
    }

    private void ModifierAnimation(Action<ElementAnimation> modification)
    {
        if (Element is not { IsLocked: false } element)
            return;

        element.Animation ??= new ElementAnimation();
        modification(element.Animation);
        Modifie?.Invoke();
        OnPropertyChanged(string.Empty);
    }

    // Copie la transition de la diapositive sélectionnée sur toutes les autres
    private void AppliquerTransitionATous()
    {
        if (Diapositive?.Transition is not { } modele)
            return;

        foreach (Slide diapositive in _main.Slides)
        {
            diapositive.Transition = new Transition
            {
                Type = modele.Type,
                DurationMs = modele.DurationMs,
                Direction = modele.Direction
            };
        }
        Modifie?.Invoke();
    }
}
