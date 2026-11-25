using System.Windows.Controls;
using System.Windows.Input;

namespace PresentationApp.UI.Views;

// Panneau des propriétés : tout passe par les liaisons (Binding) vers PropertiesViewModel.
// Le seul code ici valide un champ numérique quand on appuie sur Entrée.
public partial class PropertiesPanelView : UserControl
{
    public PropertiesPanelView()
    {
        InitializeComponent();
    }

    private void ChampNombre_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox champ)
            return;

        // Envoie la valeur au ViewModel sans attendre que le champ perde le focus
        champ.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        champ.SelectAll();
        e.Handled = true;
    }
}
