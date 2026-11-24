using System.Windows;

namespace PresentationApp.UI.Views;

public partial class OutlineAssistantWindow : Window
{
    public OutlineAssistantWindow() => InitializeComponent();

    public string OutlineText => OutlineBox.Text;

    private void OnGenerate(object sender, RoutedEventArgs e) => DialogResult = true;
}
