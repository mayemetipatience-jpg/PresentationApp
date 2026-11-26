using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PresentationApp.Core.Models;
using PresentationApp.UI.Themes;
using PresentationApp.UI.ViewModels;

namespace PresentationApp.UI.Views;

// Les actions du ruban qui ont besoin de la fenêtre ou du canevas
// (boîtes de dialogue, presse-papiers, insertion à la souris, diaporama...).
public enum ActionRuban
{
    Nouveau, Ouvrir, Enregistrer, EnregistrerSous,
    Coller, Couper, Copier,
    NouvelleDiapositive, DupliquerDiapositive, SupprimerDiapositive,
    SupprimerElement,
    ZoneTexte, Image,
    InsererTableau, AjouterLigne, SupprimerLigne, AjouterColonne, SupprimerColonne,
    PremierPlan, ArrierePlan, Avancer, Reculer, CouleurFond,
    DiaporamaDebut, DiaporamaSelection
}

// Ruban. Il ne fait aucun traitement lui-même : il signale à MainWindow
// l'action demandée, et MainWindow (qui assemble les panneaux) l'exécute.
public partial class ToolbarView : UserControl
{
    private bool _majTheme;     // évite de réagir quand c'est le programme qui change le thème affiché

    public event Action<ActionRuban>? ActionDemandee;
    public event Action<ShapeType>? FormeDemandee;
    public event Action<string>? ThemeDemande;

    public ToolbarView()
    {
        InitializeComponent();

        // Tableaux de 1 à 20 lignes / colonnes, 3 x 3 par défaut
        var nombres = Enumerable.Range(1, 20).ToList();
        cmbLignesTableau.ItemsSource = nombres;
        cmbColonnesTableau.ItemsSource = nombres;
        cmbLignesTableau.SelectedItem = 3;
        cmbColonnesTableau.SelectedItem = 3;

        cmbThemes.ItemsSource = PresentationTheme.GetThemes().Select(t => t.Name).ToList();
    }

    public int LignesTableau => cmbLignesTableau.SelectedItem as int? ?? 3;
    public int ColonnesTableau => cmbColonnesTableau.SelectedItem as int? ?? 3;

    // Les onglets Transitions et Animations travaillent avec leur propre ViewModel
    public void DefinirEffets(EffetsViewModel effets)
    {
        ongletTransitions.DataContext = effets;
        ongletAnimations.DataContext = effets;
    }

    // Affiche le thème actuel sans déclencher de changement
    public void AfficherTheme(string nom)
    {
        _majTheme = true;
        cmbThemes.SelectedItem = nom;
        _majTheme = false;
    }

    // Tous les boutons d'action passent ici : leur Tag contient le nom de l'action
    private void Action_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string nom } &&
            Enum.TryParse(nom, out ActionRuban action))
        {
            ActionDemandee?.Invoke(action);
        }
    }

    private void CmbFormes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbFormes.SelectedItem is not ComboBoxItem { Tag: string nom })
            return;

        if (Enum.TryParse(nom, out ShapeType type))
            FormeDemandee?.Invoke(type);

        // On remet la liste à vide pour pouvoir insérer deux fois la même forme
        cmbFormes.SelectedIndex = -1;
    }

    private void CmbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_majTheme && cmbThemes.SelectedItem is string nom)
            ThemeDemande?.Invoke(nom);
    }

    // Entrée dans un champ numérique = valider la valeur
    private void ChampNombre_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox champ)
            champ.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
