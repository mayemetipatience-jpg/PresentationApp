using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PresentationApp.Core.Models;
using PresentationApp.UI.PresentationMode;
using PresentationApp.UI.ViewModels;

namespace PresentationApp.UI.Views;

public partial class PresentationWindow : Window
{
    private readonly PresentationModeViewModel _vm;
    private readonly LaserPointer _laser;
    private FrameworkElement _frontBox, _backBox;
    private PresentationApp.UI.Canvas.SlideCanvas _frontCanvas, _backCanvas;
    private bool _busy;

    public PresentationWindow(Presentation presentation, int startIndex = 0)
    {
        // Convertisseur local (évite de dépendre des ressources de MainWindow)
        Resources.Add("BoolToVis", new BooleanToVisibilityConverter());
        InitializeComponent();

        _vm = new PresentationModeViewModel(presentation, startIndex);
        DataContext = _vm;
        _laser = new LaserPointer(Laser);

        _frontBox = BoxA; _frontCanvas = LayerA;
        _backBox = BoxB; _backCanvas = LayerB;

        _vm.SlideChanged += OnSlideChanged;
        _vm.ExitRequested += (_, _) => Close();

        Loaded += (_, _) =>
        {
            _frontCanvas.Slide = _vm.CurrentSlide;
            NextPreview.Slide = _vm.NextSlide;
            Focus();
            _vm.Start();
        };
        Closed += (_, _) => _vm.Stop();
    }

    private async void OnSlideChanged(object? sender, SlideChangedEventArgs e)
    {
        _busy = true;
        _backCanvas.Slide = _vm.CurrentSlide;
        NextPreview.Slide = _vm.NextSlide;

        System.Windows.Controls.Panel.SetZIndex(_backBox, 1);
        System.Windows.Controls.Panel.SetZIndex(_frontBox, 0);
        _backBox.Visibility = Visibility.Visible;

        var t = _vm.CurrentSlide?.Transition;
        await TransitionPlayer.PlayAsync(_frontBox, _backBox,
            t?.Type ?? TransitionType.None,
            e.NewIndex >= e.OldIndex,
            t?.DurationMs ?? 500,
            Stage.ActualWidth);

        _frontBox.Visibility = Visibility.Hidden;
        (_frontBox, _backBox) = (_backBox, _frontBox);
        (_frontCanvas, _backCanvas) = (_backCanvas, _frontCanvas);
        _busy = false;
    }

    private void Navigate(Action action)
    {
        if (_busy) return;
        action();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right: case Key.Down: case Key.Space: case Key.PageDown: case Key.Enter:
                Navigate(() => _vm.NextCommand.Execute(null)); break;
            case Key.Left: case Key.Up: case Key.PageUp: case Key.Back:
                Navigate(() => _vm.PreviousCommand.Execute(null)); break;
            case Key.Home: Navigate(() => _vm.FirstCommand.Execute(null)); break;
            case Key.End: Navigate(() => _vm.LastCommand.Execute(null)); break;
            case Key.L:
                _laser.Toggle();
                Cursor = _laser.IsOn ? Cursors.None : Cursors.Arrow;
                break;
            case Key.P: _vm.TogglePresenterCommand.Execute(null); break;
            case Key.Escape: _vm.ExitCommand.Execute(null); break;
        }
    }

    private void OnLeftClick(object sender, MouseButtonEventArgs e) => Navigate(() => _vm.NextCommand.Execute(null));
    private void OnRightClick(object sender, MouseButtonEventArgs e) => Navigate(() => _vm.PreviousCommand.Execute(null));
    private void OnMouseMove(object sender, MouseEventArgs e) => _laser.MoveTo(e.GetPosition(LaserLayer));

    private void OnMouseWheel(object sender, MouseWheelEventArgs e) =>
        Navigate(() => { if (e.Delta < 0) _vm.NextCommand.Execute(null); else _vm.PreviousCommand.Execute(null); });
}
