using System.Windows;
using System.Windows.Controls;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.Views;

// Les actions du ruban qui ont besoin de la fenêtre ou du canevas
// (boîtes de dialogue, presse-papiers, insertion à la souris...).
public enum ActionRuban
{
    Nouveau, Ouvrir, Enregistrer, EnregistrerSous,
    Coller, Couper, Copier,
    NouvelleDiapositive, DupliquerDiapositive, SupprimerDiapositive,
    ZoneTexte, Image,
    PremierPlan, ArrierePlan, Avancer, Reculer, CouleurFond
}

// Ruban. Il ne fait aucun traitement lui-même : il signale à MainWindow
// l'action demandée, et MainWindow (qui assemble les panneaux) l'exécute.
public partial class ToolbarView : UserControl
{
    public event Action<ActionRuban>? ActionDemandee;
    public event Action<ShapeType>? FormeDemandee;

    public ToolbarView()
    {
        InitializeComponent();
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
}
