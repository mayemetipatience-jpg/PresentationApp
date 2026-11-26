using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;
using PresentationApp.UI.Services;
using PresentationApp.UI.ViewModels;
using PresentationApp.UI.Views;

namespace PresentationApp.UI;

// Fenêtre principale : elle assemble les panneaux et fait le lien entre eux.
//   - les données et l'historique Undo/Redo sont dans MainViewModel (Étudiant 1)
//   - chaque panneau a son ViewModel (SlideListViewModel, PropertiesViewModel, EffetsViewModel)
//   - le canevas (Étudiant 2) affiche et manipule la diapositive à la souris
// Les thèmes et le diaporama sont dans MainWindow.Themes.cs et MainWindow.Slideshow.cs.
public partial class MainWindow : Window
{
    private const string FiltreFichiers = "Présentation (*.json)|*.json|Tous les fichiers (*.*)|*.*";

    private readonly MainViewModel _vm;
    private readonly SlideListViewModel _diapositives;
    private readonly PropertiesViewModel _proprietes;
    private readonly EffetsViewModel _effets;

    private SlideElement? _elementCopie;   // presse-papiers interne
    private bool _modifie;                 // modifications non enregistrées ?

    private readonly string[] _couleursFond =
    {
        "#FFFFFF", "#F2F2F2", "#DDEBF7", "#E2F0D9", "#FFF2CC", "#FCE4D6", "#E4DFEC"
    };
    private int _indexCouleurFond;

    public MainWindow()
    {
        InitializeComponent();

        // Une présentation neuve commence avec une diapositive titre + sous-titre, au thème par défaut
        var presentation = new Presentation();
        presentation.Slides.Add(SlideListViewModel.CreerDiapositive(presentation.ThemeName));

        _vm = new MainViewModel(presentation);
        _diapositives = new SlideListViewModel(_vm) { Confirmer = Confirmer };
        _proprietes = new PropertiesViewModel(_vm);
        _effets = new EffetsViewModel(_vm);

        // Chaque panneau reçoit son ViewModel
        panneauDiapositives.DataContext = _diapositives;
        panneauProprietes.DataContext = _proprietes;
        ruban.DataContext = _proprietes;
        ruban.DefinirEffets(_effets);
        ruban.AfficherTheme(presentation.ThemeName);

        // Le ruban signale les actions, la fenêtre les exécute
        ruban.ActionDemandee += Ruban_ActionDemandee;
        ruban.FormeDemandee += AjouterForme;
        ruban.ThemeDemande += AppliquerTheme;

        // Réglages faits hors historique (animations, transitions, verrouillage)
        _effets.Modifie += MarquerModifie;
        _proprietes.VerrouillageModifie += () =>
        {
            slideCanvas.ActualiserEtatVerrouillage();
            _effets.Recharger();
            MarquerModifie();
        };

        // Canevas -> ViewModel
        slideCanvas.ElementSelectionChangee += element => _vm.SelectedElement = element;
        slideCanvas.DiapositiveModifiee += SlideCanvas_DiapositiveModifiee;

        // ViewModel -> canevas
        _vm.PropertyChanged += Vm_PropertyChanged;
        _vm.History.Changed += History_Changed;

        PreviewKeyDown += MainWindow_PreviewKeyDown;
        Closing += MainWindow_Closing;

        slideCanvas.DisplaySlide(_vm.SelectedSlide);
        MettreAJourTitre();
        MettreAJourInfo();
    }

    // =====================================================
    // SYNCHRONISATION ENTRE LES PANNEAUX
    // =====================================================

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.SelectedSlide):
                slideCanvas.DisplaySlide(_vm.SelectedSlide);
                MettreAJourInfo();
                break;

            // Sélection faite ailleurs que sur le canevas (liste des calques, collage...)
            case nameof(MainViewModel.SelectedElement):
                if (_vm.SelectedElement is { } element &&
                    !ReferenceEquals(slideCanvas.ElementSelectionne, element))
                    slideCanvas.SelectionnerElementDepuisListe(element);
                break;

            case nameof(MainViewModel.CheminFichier):
                MettreAJourTitre();
                break;
        }
    }

    // Après chaque action ou Annuler/Refaire : on redessine la diapositive
    // et on garde l'élément sélectionné s'il existe toujours.
    private void History_Changed(object? sender, EventArgs e)
    {
        SlideElement? element = _vm.SelectedElement;
        Slide? diapositive = _vm.SelectedSlide;

        slideCanvas.DisplaySlide(diapositive);

        if (element != null && diapositive != null && diapositive.Elements.Contains(element))
            slideCanvas.SelectionnerElementDepuisListe(element);

        ruban.AfficherTheme(_vm.CurrentPresentation.ThemeName);
        MarquerModifie();
        MettreAJourInfo();
    }

    // Modification faite directement à la souris dans le canevas (déplacement, texte, tableau...)
    private void SlideCanvas_DiapositiveModifiee()
    {
        panneauDiapositives.Rafraichir();
        _proprietes.RechargerCalques();
        _proprietes.Recharger();
        MarquerModifie();
        MettreAJourInfo();
    }

    private void MarquerModifie()
    {
        if (_modifie) return;
        _modifie = true;
        MettreAJourTitre();
    }

    private void MettreAJourTitre()
    {
        string nom = string.IsNullOrEmpty(_vm.CheminFichier)
            ? "Présentation sans titre"
            : Path.GetFileNameWithoutExtension(_vm.CheminFichier);

        string etoile = _modifie ? " *" : string.Empty;
        txtTitre.Text = $"Mini PowerPoint | {nom}{etoile}";
        Title = $"{nom}{etoile} - Mini PowerPoint";
    }

    private void MettreAJourInfo()
    {
        int numero = _vm.SelectedSlide != null ? _vm.Slides.IndexOf(_vm.SelectedSlide) + 1 : 0;
        txtInfo.Text = $"Diapositive {numero} sur {_vm.SlideCount}   |   Thème : {_vm.CurrentPresentation.ThemeName}";
    }

    // =====================================================
    // ACTIONS DU RUBAN
    // =====================================================

    private void Ruban_ActionDemandee(ActionRuban action)
    {
        switch (action)
        {
            case ActionRuban.Nouveau: Nouveau(); break;
            case ActionRuban.Ouvrir: Ouvrir(); break;
            case ActionRuban.Enregistrer: Enregistrer(demanderChemin: false); break;
            case ActionRuban.EnregistrerSous: Enregistrer(demanderChemin: true); break;

            case ActionRuban.Copier: Copier(); break;
            case ActionRuban.Couper: Couper(); break;
            case ActionRuban.Coller: Coller(); break;

            case ActionRuban.NouvelleDiapositive: _diapositives.NouvelleCommand.Execute(null); break;
            case ActionRuban.DupliquerDiapositive: _diapositives.DupliquerCommand.Execute(null); break;
            case ActionRuban.SupprimerDiapositive:
                if (_diapositives.SupprimerCommand.CanExecute(null))
                    _diapositives.SupprimerCommand.Execute(null);
                else
                    Informer("La présentation doit conserver au moins une diapositive.", "Suppression");
                break;

            case ActionRuban.SupprimerElement: SupprimerElement(); break;

            case ActionRuban.ZoneTexte:
                if (_vm.SelectedSlide != null)
                {
                    slideCanvas.ActiverInsertionTexte(_vm.SelectedSlide);
                    txtInfo.Text = "Cliquez sur la diapositive pour placer la zone de texte.";
                }
                break;
            case ActionRuban.Image: InsererImage(); break;

            case ActionRuban.InsererTableau: InsererTableau(); break;
            case ActionRuban.AjouterLigne: ModifierTableau("Ajouter une ligne", t => t.AjouterLigne()); break;
            case ActionRuban.SupprimerLigne: ModifierTableau("Supprimer une ligne", t => t.SupprimerLigne()); break;
            case ActionRuban.AjouterColonne: ModifierTableau("Ajouter une colonne", t => t.AjouterColonne()); break;
            case ActionRuban.SupprimerColonne: ModifierTableau("Supprimer une colonne", t => t.SupprimerColonne()); break;

            case ActionRuban.PremierPlan: ModifierCalque(slideCanvas.MettreAuPremierPlan); break;
            case ActionRuban.ArrierePlan: ModifierCalque(slideCanvas.MettreEnArrierePlan); break;
            case ActionRuban.Avancer: ModifierCalque(slideCanvas.AvancerDUnCalque); break;
            case ActionRuban.Reculer: ModifierCalque(slideCanvas.ReculerDUnCalque); break;

            case ActionRuban.CouleurFond: ChangerCouleurFond(); break;

            case ActionRuban.DiaporamaDebut: DemarrerDiaporama(depuisSelection: false); break;
            case ActionRuban.DiaporamaSelection: DemarrerDiaporama(depuisSelection: true); break;
        }
    }

    // ---------- Insertion (passe par l'historique : Ctrl+Z annule l'ajout) ----------

    private void AjouterElement(SlideElement element)
    {
        if (_vm.SelectedSlide is not { } diapositive)
            return;

        _vm.History.Execute(new AddElementCommand(diapositive, element));
        _vm.SelectedElement = element;
    }

    private void AjouterForme(ShapeType type)
    {
        AjouterElement(new ShapeElement
        {
            X = 300, Y = 180, Width = 180, Height = 120,
            ShapeType = type,
            Style = new ElementStyle { FillColor = "#ADD8E6", StrokeColor = "#000000", StrokeThickness = 2 }
        });
    }

    private void InsererImage()
    {
        var dialogue = new OpenFileDialog
        {
            Title = "Choisir une image",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
        };

        if (dialogue.ShowDialog(this) != true)
            return;

        AjouterElement(new ImageElement
        {
            ImagePath = dialogue.FileName,
            X = 250, Y = 150, Width = 300, Height = 200
        });
    }

    // ---------- Tableaux ----------

    // Le canevas attend un clic pour placer le tableau (comme pour une zone de texte)
    private void InsererTableau()
    {
        if (_vm.SelectedSlide is not { } diapositive)
            return;

        slideCanvas.ActiverInsertionTableau(diapositive, ruban.LignesTableau, ruban.ColonnesTableau);
        txtInfo.Text = $"Cliquez sur la diapositive pour placer le tableau " +
                       $"({ruban.LignesTableau} x {ruban.ColonnesTableau}).";
    }

    private sealed record EtatTableau(int Lignes, int Colonnes, List<List<string>> Cellules);

    private static EtatTableau Photographier(TableElement t) =>
        new(t.Rows, t.Columns, t.Cells.Select(ligne => ligne.ToList()).ToList());

    private static void Restaurer(TableElement t, EtatTableau etat)
    {
        t.Rows = etat.Lignes;
        t.Columns = etat.Colonnes;
        t.Cells = etat.Cellules.Select(ligne => ligne.ToList()).ToList();
    }

    // Ajout / suppression de lignes et de colonnes, annulable avec Ctrl+Z
    private void ModifierTableau(string description, Action<TableElement> modification)
    {
        if (_vm.SelectedElement is not TableElement tableau)
        {
            Informer("Sélectionnez d'abord un tableau sur la diapositive.", "Tableau");
            return;
        }
        if (tableau.IsLocked)
        {
            InformerVerrouille();
            return;
        }

        EtatTableau avant = Photographier(tableau);
        EtatTableau? apres = null;

        _vm.History.Execute(new ActionAnnulable(description,
            faire: () =>
            {
                if (apres == null)
                {
                    modification(tableau);
                    apres = Photographier(tableau);
                }
                else
                {
                    Restaurer(tableau, apres);       // Refaire
                }
            },
            defaire: () => Restaurer(tableau, avant)));
    }

    // ---------- Presse-papiers ----------

    private void Copier()
    {
        if (_vm.SelectedElement is not { } element)
        {
            Informer("Sélectionnez un élément à copier.", "Copier");
            return;
        }
        _elementCopie = ClonerElement(element);
    }

    private void Couper()
    {
        if (_vm.SelectedElement is not { } element)
        {
            Informer("Sélectionnez un élément à couper.", "Couper");
            return;
        }
        if (element.IsLocked)
        {
            InformerVerrouille();
            return;
        }
        _elementCopie = ClonerElement(element);
        _vm.DeleteElementCommand.Execute(null);
    }

    private void Coller()
    {
        if (_elementCopie == null)
        {
            Informer("Aucun élément à coller. Copiez d'abord un élément.", "Coller");
            return;
        }

        // Une nouvelle copie à chaque collage, décalée pour qu'on la voie
        SlideElement copie = ClonerElement(_elementCopie);
        copie.X += 30;
        copie.Y += 30;
        AjouterElement(copie);
    }

    // Copie indépendante : style, animation et cellules ne sont pas partagés avec l'original
    private static SlideElement ClonerElement(SlideElement element)
    {
        SlideElement copie = element switch
        {
            TextElement texte => new TextElement { Text = texte.Text },
            ShapeElement forme => new ShapeElement { ShapeType = forme.ShapeType },
            ImageElement image => new ImageElement { ImagePath = image.ImagePath },
            TableElement tableau => new TableElement
            {
                Rows = tableau.Rows,
                Columns = tableau.Columns,
                Cells = tableau.Cells.Select(ligne => ligne.ToList()).ToList(),
                CellBackgroundColor = tableau.CellBackgroundColor,
                HeaderBackgroundColor = tableau.HeaderBackgroundColor,
                BorderColor = tableau.BorderColor,
                BorderThickness = tableau.BorderThickness,
                CellFontSize = tableau.CellFontSize
            },
            _ => throw new NotSupportedException("Type d'élément non pris en charge.")
        };

        copie.X = element.X;
        copie.Y = element.Y;
        copie.Width = element.Width;
        copie.Height = element.Height;
        copie.Rotation = element.Rotation;
        copie.Style = ChangeStyleCommand.Copy(element.Style);
        copie.IsLocked = false;
        copie.Animation = new ElementAnimation
        {
            Type = element.Animation?.Type ?? AnimationType.None,
            DurationMs = element.Animation?.DurationMs ?? 1000,
            Direction = element.Animation?.Direction ?? "Left",
            DelayMs = element.Animation?.DelayMs ?? 0,
            RepeatCount = element.Animation?.RepeatCount ?? 1
        };
        return copie;
    }

    // ---------- Calques et fond ----------

    private void ModifierCalque(Action action)
    {
        if (slideCanvas.ElementSelectionne == null)
        {
            Informer("Sélectionnez d'abord un élément sur la diapositive.", "Ordre des calques");
            return;
        }
        action();
        SlideCanvas_DiapositiveModifiee();
    }

    // Change la couleur de fond (annulable avec Ctrl+Z)
    private void ChangerCouleurFond()
    {
        if (_vm.SelectedSlide is not { } diapositive)
            return;

        _indexCouleurFond = (_indexCouleurFond + 1) % _couleursFond.Length;
        string ancienne = diapositive.BackgroundColor;
        string nouvelle = _couleursFond[_indexCouleurFond];

        _vm.History.Execute(new ActionAnnulable("Couleur de fond",
            faire: () => diapositive.BackgroundColor = nouvelle,
            defaire: () => diapositive.BackgroundColor = ancienne));
    }

    private void SupprimerElement()
    {
        if (_vm.SelectedElement is not { } element)
        {
            Informer("Sélectionnez d'abord un élément à supprimer.", "Suppression");
            return;
        }
        if (element.IsLocked)
        {
            InformerVerrouille();
            return;
        }
        _vm.DeleteElementCommand.Execute(null);
    }

    // =====================================================
    // FICHIER : NOUVEAU, OUVRIR, ENREGISTRER
    // =====================================================

    // Comme dans PowerPoint, « Nouveau » ouvre une nouvelle fenêtre
    private void Nouveau()
    {
        new MainWindow().Show();
    }

    private void Ouvrir()
    {
        if (!ConfirmerAbandonModifications())
            return;

        var dialogue = new OpenFileDialog { Title = "Ouvrir une présentation", Filter = FiltreFichiers };
        if (dialogue.ShowDialog(this) != true)
            return;

        bool ouvert;
        try
        {
            ouvert = _vm.Ouvrir(dialogue.FileName);
        }
        catch (UnauthorizedAccessException)
        {
            Erreur("Vous n'avez pas le droit de lire ce fichier.");
            return;
        }

        if (!ouvert)
        {
            Erreur(_vm.ErrorMessage ?? "Impossible d'ouvrir ce fichier.");
            return;
        }

        ruban.AfficherTheme(_vm.CurrentPresentation.ThemeName);
        _modifie = false;
        MettreAJourTitre();
        MettreAJourInfo();
    }

    // Renvoie false si l'utilisateur a annulé ou si l'enregistrement a échoué
    private bool Enregistrer(bool demanderChemin)
    {
        string? chemin = _vm.CheminFichier;

        if (demanderChemin || string.IsNullOrEmpty(chemin))
        {
            var dialogue = new SaveFileDialog
            {
                Title = "Enregistrer la présentation",
                Filter = FiltreFichiers,
                DefaultExt = ".json",
                FileName = string.IsNullOrEmpty(chemin) ? "Présentation" : Path.GetFileName(chemin)
            };

            if (dialogue.ShowDialog(this) != true)
                return false;

            chemin = dialogue.FileName;
        }

        _vm.Enregistrer(chemin);

        if (_vm.ErrorMessage != null)
        {
            Erreur(_vm.ErrorMessage);
            return false;
        }

        _modifie = false;
        MettreAJourTitre();
        return true;
    }

    // Avant de perdre des modifications : Oui = enregistrer, Non = abandonner, Annuler = rester
    private bool ConfirmerAbandonModifications()
    {
        if (!_modifie)
            return true;

        MessageBoxResult reponse = MessageBox.Show(this,
            "La présentation a été modifiée. Voulez-vous enregistrer les modifications ?",
            "Mini PowerPoint", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

        return reponse switch
        {
            MessageBoxResult.Yes => Enregistrer(demanderChemin: false),
            MessageBoxResult.No => true,
            _ => false
        };
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!ConfirmerAbandonModifications())
            e.Cancel = true;
    }

    // =====================================================
    // RACCOURCIS CLAVIER
    // =====================================================

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        bool maj = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        // Diaporama : F5 depuis le début, Maj+F5 depuis la diapositive actuelle
        if (e.Key == Key.F5)
        {
            DemarrerDiaporama(depuisSelection: maj);
            e.Handled = true;
            return;
        }

        // Raccourcis valables partout, même pendant la saisie d'un texte
        if (ctrl)
        {
            switch (e.Key)
            {
                case Key.S: Enregistrer(demanderChemin: maj); e.Handled = true; return;
                case Key.O: Ouvrir(); e.Handled = true; return;
                case Key.N: Nouveau(); e.Handled = true; return;
                case Key.M: _diapositives.NouvelleCommand.Execute(null); e.Handled = true; return;
                case Key.Delete: SupprimerElement(); e.Handled = true; return;
            }
        }

        // Pendant la saisie d'un texte, les autres touches servent au texte
        if (Keyboard.FocusedElement is TextBox)
            return;

        if (ctrl)
        {
            switch (e.Key)
            {
                case Key.Z: Executer(_vm.UndoCommand); e.Handled = true; return;
                case Key.Y: Executer(_vm.RedoCommand); e.Handled = true; return;
                case Key.C: Copier(); e.Handled = true; return;
                case Key.X: Couper(); e.Handled = true; return;
                case Key.V: Coller(); e.Handled = true; return;
            }
        }

        if (e.Key == Key.Delete && _vm.SelectedElement != null)
        {
            SupprimerElement();
            e.Handled = true;
        }
    }

    private static void Executer(ICommand commande)
    {
        if (commande.CanExecute(null))
            commande.Execute(null);
    }

    // =====================================================
    // ZOOM
    // =====================================================

    private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
            return;

        // Zoom fixe : la diapositive garde sa taille et on peut faire défiler
        double facteur = e.NewValue / 100;
        vueDiapositive.Width = 960 * facteur;
        vueDiapositive.Height = 540 * facteur;
        zoneDiapositive.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        zoneDiapositive.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        txtZoom.Text = $"{e.NewValue:0} %";
    }

    // Mode « Ajuster » : la diapositive prend toute la place disponible
    private void BtnAjuster_Click(object sender, RoutedEventArgs e)
    {
        vueDiapositive.Width = double.NaN;
        vueDiapositive.Height = double.NaN;
        zoneDiapositive.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        zoneDiapositive.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        txtZoom.Text = "Ajusté";
    }

    // =====================================================
    // BOÎTES DE DIALOGUE
    // =====================================================

    private bool Confirmer(string question)
        => MessageBox.Show(this, question, "Confirmation",
                           MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private void Informer(string message, string titre)
        => MessageBox.Show(this, message, titre, MessageBoxButton.OK, MessageBoxImage.Information);

    private void InformerVerrouille()
        => Informer("Cet élément est verrouillé. Déverrouillez-le pour le modifier.", "Élément verrouillé");

    private void Erreur(string message)
        => MessageBox.Show(this, message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
}
