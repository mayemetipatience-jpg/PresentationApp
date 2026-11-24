using System.Windows;
using System.Windows.Shapes;

namespace PresentationApp.UI.PresentationMode;

/// <summary>Pointeur laser : un point rouge qui suit la souris (touche L).</summary>
public sealed class LaserPointer
{
    private readonly Ellipse _dot;

    public LaserPointer(Ellipse dot)
    {
        _dot = dot;
        _dot.Visibility = Visibility.Collapsed;
    }

    public bool IsOn { get; private set; }

    public void Toggle()
    {
        IsOn = !IsOn;
        _dot.Visibility = IsOn ? Visibility.Visible : Visibility.Collapsed;
    }

    public void MoveTo(Point p)
    {
        if (!IsOn) return;
        // Nom complet : le namespace PresentationApp.UI.Canvas masque la classe Canvas.
        System.Windows.Controls.Canvas.SetLeft(_dot, p.X - _dot.Width / 2);
        System.Windows.Controls.Canvas.SetTop(_dot, p.Y - _dot.Height / 2);
    }
}
