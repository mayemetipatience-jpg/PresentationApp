using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.ViewModels;

// Une couleur proposée dans les listes (nom affiché + code hexadécimal)
public record CouleurChoix(string Nom, string Hex);

// Une ligne de la liste des calques
public record CalqueItem(SlideElement Element, string Nom);

// ViewModel du panneau de propriétés (à droite) et de la mise en forme du ruban.
// Il affiche les valeurs de l'élément sélectionné. Quand l'utilisateur les change,
// il passe par ElementService : chaque modification peut donc être annulée (Ctrl+Z).
public class PropertiesViewModel : ObservableObject
{
    private const double TailleMinimum = 10;
    private readonly MainViewModel _main;

    public PropertiesViewModel(MainViewModel main)
    {
        _main = main;

        AlignerGaucheCommand = new RelayCommand(() => TextAlignment = "Left", () => EstTexte);
        AlignerCentreCommand = new RelayCommand(() => TextAlignment = "Center", () => EstTexte);
        AlignerDroiteCommand = new RelayCommand(() => TextAlignment = "Right", () => EstTexte);
        PivoterCommand = new RelayCommand(() => Rotation = Rotation + 90, () => AElement);

        _main.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedSlide))
                RechargerCalques();
            if (e.PropertyName is nameof(MainViewModel.SelectedElement) or nameof(MainViewModel.SelectedSlide))
                Recharger();
        };

        // Après chaque action ou Annuler/Refaire, les valeurs ont pu changer
        _main.History.Changed += (_, _) =>
        {
            RechargerCalques();
            Recharger();
        };

        RechargerCalques();
    }

    // Accès aux commandes générales (Annuler, Refaire, Supprimer) pour le ruban
    public MainViewModel Main => _main;

    private SlideElement? Element => _main.SelectedElement;

    // ---------- Ce qui est sélectionné ----------

    public bool AElement => Element != null;
    public bool AucunElement => Element == null;
    public bool EstTexte => Element is TextElement;
    public bool EstForme => Element is ShapeElement;

    public string Titre => Element switch
    {
        TextElement => "Zone de texte",
        ShapeElement forme => "Forme : " + forme.ShapeType,
        ImageElement => "Image",
        _ => "Aucun élément sélectionné"
    };

    // ---------- Listes proposées à l'utilisateur ----------

    public IReadOnlyList<string> Polices { get; } = new[]
    {
        "Segoe UI", "Arial", "Calibri", "Times New Roman", "Verdana", "Georgia", "Comic Sans MS"
    };

    public IReadOnlyList<double> Tailles { get; } = new double[]
    {
        8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 60, 72
    };

    public IReadOnlyList<double> Epaisseurs { get; } = new double[] { 0, 1, 2, 3, 4, 6, 8 };

    public IReadOnlyList<CouleurChoix> Couleurs { get; } = new CouleurChoix[]
    {
        new("Noir", "#000000"), new("Blanc", "#FFFFFF"), new("Gris", "#808080"),
        new("Gris clair", "#D9D9D9"), new("Rouge", "#FF0000"), new("Orange", "#FFA500"),
        new("Jaune", "#FFD700"), new("Vert", "#008000"), new("Bleu clair", "#ADD8E6"),
        new("Bleu", "#0000FF"), new("Bleu nuit", "#1F3864"), new("Violet", "#800080"),
        new("Rose", "#FF69B4"), new("Marron", "#8B4513"), new("Transparent", "#00FFFFFF")
    };

    // ---------- Position, taille, rotation ----------

    public double X
    {
        get => Element?.X ?? 0;
        set
        {
            if (Element is { } el && Different(el.X, value))
                _main.ElementOperations.Move(el, Math.Max(0, value), el.Y);
            Resynchroniser();
        }
    }

    public double Y
    {
        get => Element?.Y ?? 0;
        set
        {
            if (Element is { } el && Different(el.Y, value))
                _main.ElementOperations.Move(el, el.X, Math.Max(0, value));
            Resynchroniser();
        }
    }

    public double Width
    {
        get => Element?.Width ?? 0;
        set
        {
            if (Element is { } el && Different(el.Width, value))
                _main.ElementOperations.Resize(el, el.X, el.Y, Math.Max(TailleMinimum, value), el.Height);
            Resynchroniser();
        }
    }

    public double Height
    {
        get => Element?.Height ?? 0;
        set
        {
            if (Element is { } el && Different(el.Height, value))
                _main.ElementOperations.Resize(el, el.X, el.Y, el.Width, Math.Max(TailleMinimum, value));
            Resynchroniser();
        }
    }

    // Toujours ramenée entre 0 et 360 degrés
    public double Rotation
    {
        get => Element?.Rotation ?? 0;
        set
        {
            double angle = ((value % 360) + 360) % 360;
            if (Element is { } el && Different(el.Rotation, angle))
                _main.ElementOperations.Rotate(el, angle);
            Resynchroniser();
        }
    }

    // ---------- Texte ----------

    public string FontFamily
    {
        get => Element?.Style.FontFamily ?? string.Empty;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && Element?.Style.FontFamily != value)
                ModifierStyle(s => s.FontFamily = value);
        }
    }

    public double FontSize
    {
        get => Element?.Style.FontSize ?? 0;
        set
        {
            // Contrôle de saisie : une taille de police raisonnable
            if (value >= 1 && value <= 400 && Element is { } el && Different(el.Style.FontSize, value))
                ModifierStyle(s => s.FontSize = value);
            Resynchroniser();
        }
    }

    public bool IsBold
    {
        get => Element?.Style.IsBold ?? false;
        set => ModifierStyle(s => s.IsBold = value);
    }

    public bool IsItalic
    {
        get => Element?.Style.IsItalic ?? false;
        set => ModifierStyle(s => s.IsItalic = value);
    }

    public bool IsUnderline
    {
        get => Element?.Style.IsUnderline ?? false;
        set => ModifierStyle(s => s.IsUnderline = value);
    }

    public string TextAlignment
    {
        get => Element?.Style.TextAlignment ?? "Left";
        set
        {
            if (Element?.Style.TextAlignment != value)
                ModifierStyle(s => s.TextAlignment = value);
        }
    }

    // ---------- Couleurs et contour ----------
    // Les codes sont mis en majuscules pour correspondre à la liste Couleurs

    public string TextColor
    {
        get => Majuscules(Element?.Style.TextColor);
        set => ChangerCouleur(value, TextColor, s => s.TextColor = value);
    }

    public string FillColor
    {
        get => Majuscules(Element?.Style.FillColor);
        set => ChangerCouleur(value, FillColor, s => s.FillColor = value);
    }

    public string StrokeColor
    {
        get => Majuscules(Element?.Style.StrokeColor);
        set => ChangerCouleur(value, StrokeColor, s => s.StrokeColor = value);
    }

    public double StrokeThickness
    {
        get => Element?.Style.StrokeThickness ?? 0;
        set
        {
            if (value >= 0 && Element is { } el && Different(el.Style.StrokeThickness, value))
                ModifierStyle(s => s.StrokeThickness = value);
        }
    }

    // ---------- Commandes du ruban ----------

    public RelayCommand AlignerGaucheCommand { get; }
    public RelayCommand AlignerCentreCommand { get; }
    public RelayCommand AlignerDroiteCommand { get; }
    public RelayCommand PivoterCommand { get; }

    // ---------- Liste des calques (du premier plan vers l'arrière-plan) ----------

    public ObservableCollection<CalqueItem> Calques { get; } = new();

    public CalqueItem? CalqueSelectionne
    {
        get => Calques.FirstOrDefault(c => ReferenceEquals(c.Element, Element));
        set
        {
            if (value != null && !ReferenceEquals(value.Element, Element))
                _main.SelectedElement = value.Element;
        }
    }

    public void RechargerCalques()
    {
        Calques.Clear();

        if (_main.SelectedSlide is { } diapositive)
        {
            foreach (SlideElement element in diapositive.Elements.OrderByDescending(e => e.ZIndex))
                Calques.Add(new CalqueItem(element, NommerCalque(element)));
        }

        OnPropertyChanged(nameof(CalqueSelectionne));
    }

    // Relit toutes les valeurs de l'élément (après une sélection ou une modification)
    public void Recharger()
    {
        OnPropertyChanged(string.Empty);   // chaîne vide = « toutes les propriétés ont changé »
        AlignerGaucheCommand.RaiseCanExecuteChanged();
        AlignerCentreCommand.RaiseCanExecuteChanged();
        AlignerDroiteCommand.RaiseCanExecuteChanged();
        PivoterCommand.RaiseCanExecuteChanged();
    }

    // ---------- Outils internes ----------

    // Copie le style, applique la modification, puis l'envoie à l'historique Undo/Redo
    private void ModifierStyle(Action<ElementStyle> modification)
    {
        if (Element is not { } el)
            return;

        ElementStyle nouveau = ChangeStyleCommand.Copy(el.Style);
        modification(nouveau);
        _main.ElementOperations.ChangeStyle(el, nouveau);
    }

    private void ChangerCouleur(string? nouvelle, string actuelle, Action<ElementStyle> modification)
    {
        if (!string.IsNullOrWhiteSpace(nouvelle) &&
            !string.Equals(nouvelle, actuelle, StringComparison.OrdinalIgnoreCase))
            ModifierStyle(modification);
    }

    // Si la valeur saisie a été refusée ou corrigée (ex : largeur négative),
    // on réaffiche la vraie valeur dans le champ.
    private void Resynchroniser([CallerMemberName] string? propriete = null)
        => OnPropertyChanged(propriete);

    private static bool Different(double a, double b) => Math.Abs(a - b) > 0.001;

    private static string Majuscules(string? couleur) => couleur?.ToUpperInvariant() ?? string.Empty;

    private static string NommerCalque(SlideElement element)
    {
        switch (element)
        {
            case TextElement texte:
                string contenu = string.IsNullOrWhiteSpace(texte.Text) ? "Texte vide" : texte.Text;
                return "Texte : " + (contenu.Length > 25 ? contenu[..25] + "..." : contenu);
            case ShapeElement forme:
                return "Forme : " + forme.ShapeType;
            case ImageElement:
                return "Image";
            default:
                return "Élément";
        }
    }
}
