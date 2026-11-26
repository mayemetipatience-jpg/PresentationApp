using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PresentationApp.Core.Models;
using PresentationApp.UI.Canvas;

namespace PresentationApp.UI.Slideshow;

public partial class SlideshowWindow : Window
{
    private readonly IReadOnlyList<Slide> _slides;
    private int _index;
    private CancellationTokenSource? _animationCts;
    private bool _navigationEnCours;

    public SlideshowWindow(IReadOnlyList<Slide> slides, int indexDepart = 0)
    {
        InitializeComponent();

        _slides = slides ?? throw new ArgumentNullException(nameof(slides));
        if (_slides.Count == 0)
            throw new ArgumentException("La présentation ne contient aucune diapositive.", nameof(slides));

        _index = Math.Clamp(indexDepart, 0, _slides.Count - 1);
        Loaded += async (_, _) =>
        {
            Focus();
            await AfficherDiapositiveAsync(-1);
        };
    }

    private async Task AfficherDiapositiveAsync(int ancienIndex)
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = new CancellationTokenSource();

        Slide slide = _slides[_index];
        canvasDiaporama.DisplaySlide(slide);

        // En mode présentation, les éléments ne doivent pas être sélectionnables.
        foreach (UIElement enfant in canvasDiaporama.Children)
            enfant.IsHitTestVisible = false;

        txtCompteur.Text = $"Diapositive {_index + 1} / {_slides.Count}";

        try
        {
            await SlideTransitionEngine.ExecuterAsync(
                canvasDiaporama, slide.Transition, _animationCts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!_animationCts.IsCancellationRequested)
            _ = ElementAnimationEngine.ExecuterAsync(
                canvasDiaporama, _animationCts.Token);
    }

    private async Task NaviguerAsync(int delta)
    {
        if (_navigationEnCours)
            return;

        int nouvelIndex = _index + delta;
        if (nouvelIndex < 0 || nouvelIndex >= _slides.Count)
        {
            if (delta > 0)
                Close();
            return;
        }

        _navigationEnCours = true;
        try
        {
            int ancien = _index;
            _index = nouvelIndex;
            await AfficherDiapositiveAsync(ancien);
        }
        finally
        {
            _navigationEnCours = false;
        }
    }

    private async void BtnPrecedente_Click(object sender, RoutedEventArgs e)
        => await NaviguerAsync(-1);

    private async void BtnSuivante_Click(object sender, RoutedEventArgs e)
        => await NaviguerAsync(1);

    private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        => Close();

    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Right or Key.Down or Key.Space or Key.Enter or Key.PageDown)
        {
            e.Handled = true;
            await NaviguerAsync(1);
        }
        else if (e.Key is Key.Left or Key.Up or Key.Back or Key.PageUp)
        {
            e.Handled = true;
            await NaviguerAsync(-1);
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private async void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => await NaviguerAsync(1);

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        canvasDiaporama.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
    }

    protected override void OnClosed(EventArgs e)
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        base.OnClosed(e);
    }
}

/// <summary>
/// Moteur des transitions entre diapositives.
/// La transition configurée sur une diapositive est exécutée à son arrivée.
/// </summary>
internal static class SlideTransitionEngine
{
    public static async Task ExecuterAsync(
        FrameworkElement cible, Transition? transition, CancellationToken token)
    {
        if (transition == null || transition.Type == TransitionType.None)
            return;

        double secondes = Math.Clamp(transition.DurationMs, 100, 10000) / 1000d;
        var duree = TimeSpan.FromSeconds(secondes);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var groupe = new TransformGroup();
        var translation = new TranslateTransform();
        var scale = new ScaleTransform(1, 1);
        var rotation = new RotateTransform();
        groupe.Children.Add(scale);
        groupe.Children.Add(rotation);
        groupe.Children.Add(translation);

        cible.RenderTransformOrigin = new Point(.5, .5);
        cible.RenderTransform = groupe;

        string direction = transition.Direction ?? "Left";
        double dx = direction.Equals("Right", StringComparison.OrdinalIgnoreCase) ? 960 :
                    direction.Equals("Left", StringComparison.OrdinalIgnoreCase) ? -960 : 0;
        double dy = direction.Equals("Bottom", StringComparison.OrdinalIgnoreCase) ? 540 :
                    direction.Equals("Top", StringComparison.OrdinalIgnoreCase) ? -540 : 0;

        switch (transition.Type)
        {
            case TransitionType.Fade:
            case TransitionType.Reveal:
                cible.Opacity = 0;
                cible.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation(0, 1, duree) { EasingFunction = ease });
                break;

            case TransitionType.Wipe:
                cible.Opacity = 1;
                scale.ScaleX = 0;
                cible.RenderTransformOrigin = new Point(
                    direction.Equals("Right", StringComparison.OrdinalIgnoreCase) ? 1 : 0, .5);
                scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                    new DoubleAnimation(0, 1, duree) { EasingFunction = ease });
                break;

            case TransitionType.Split:
                scale.ScaleY = 0;
                scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                    new DoubleAnimation(0, 1, duree) { EasingFunction = ease });
                break;

            case TransitionType.Push:
            case TransitionType.Cover:
                translation.X = dx;
                translation.Y = dy;
                translation.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation(dx, 0, duree) { EasingFunction = ease });
                translation.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(dy, 0, duree) { EasingFunction = ease });
                break;

            case TransitionType.Zoom:
                scale.ScaleX = scale.ScaleY = .2;
                scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                    new DoubleAnimation(.2, 1, duree) { EasingFunction = ease });
                scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                    new DoubleAnimation(.2, 1, duree) { EasingFunction = ease });
                break;

            case TransitionType.Rotate:
                rotation.Angle = direction.Equals("Right", StringComparison.OrdinalIgnoreCase) ? 90 : -90;
                rotation.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation(rotation.Angle, 0, duree) { EasingFunction = ease });
                break;
        }

        await Task.Delay(duree, token);
        token.ThrowIfCancellationRequested();

        cible.BeginAnimation(UIElement.OpacityProperty, null);
        cible.RenderTransform = Transform.Identity;
        cible.Opacity = 1;
    }
}

/// <summary>
/// Moteur des animations appliquées aux éléments d'une diapositive.
/// </summary>
internal static class ElementAnimationEngine
{
    public static async Task ExecuterAsync(SlideCanvas canvas, CancellationToken token)
    {
        var elements = canvas.Children
            .OfType<FrameworkElement>()
            .Where(v => v.Tag is SlideElement)
            .OrderBy(v => ((SlideElement)v.Tag).ZIndex)
            .ToList();

        foreach (FrameworkElement visuel in elements)
        {
            if (token.IsCancellationRequested)
                return;

            var element = (SlideElement)visuel.Tag;
            ElementAnimation animation = element.Animation ?? new ElementAnimation();
            if (animation.Type == AnimationType.None)
                continue;

            // Chaque animation respecte son délai sans bloquer la navigation.
            _ = AnimerElementAsync(visuel, element, animation, token);
        }

        await Task.CompletedTask;
    }

    private static async Task AnimerElementAsync(
        FrameworkElement visuel, SlideElement element,
        ElementAnimation animation, CancellationToken token)
    {
        try
        {
            int delai = Math.Clamp(animation.DelayMs, 0, 60000);
            if (delai > 0)
                await Task.Delay(delai, token);

            token.ThrowIfCancellationRequested();

            double secondes = Math.Clamp(animation.DurationMs, 100, 30000) / 1000d;
            var duree = TimeSpan.FromSeconds(secondes);
            int repetitions = Math.Clamp(animation.RepeatCount, 0, 100);
            RepeatBehavior repetition = repetitions == 0
                ? RepeatBehavior.Forever
                : new RepeatBehavior(Math.Max(1, repetitions));

            var groupe = new TransformGroup();
            var scale = new ScaleTransform(1, 1);
            var rotation = new RotateTransform(element.Rotation);
            var translation = new TranslateTransform();
            groupe.Children.Add(scale);
            groupe.Children.Add(rotation);
            groupe.Children.Add(translation);
            visuel.RenderTransformOrigin = new Point(.5, .5);
            visuel.RenderTransform = groupe;

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            double dx = animation.Direction switch
            {
                "Right" => Math.Max(100, 960 + element.Width),
                "Top" => 0,
                "Bottom" => 0,
                _ => -Math.Max(100, 960 + element.Width)
            };
            double dy = animation.Direction switch
            {
                "Top" => -Math.Max(100, 540 + element.Height),
                "Bottom" => Math.Max(100, 540 + element.Height),
                _ => 0
            };

            switch (animation.Type)
            {
                case AnimationType.Appear:
                    visuel.Opacity = 0;
                    visuel.BeginAnimation(UIElement.OpacityProperty,
                        new DoubleAnimation(0, 1, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    break;

                case AnimationType.FlyIn:
                    visuel.Opacity = 0;
                    translation.X = dx;
                    translation.Y = dy;
                    translation.BeginAnimation(TranslateTransform.XProperty,
                        new DoubleAnimation(dx, 0, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    translation.BeginAnimation(TranslateTransform.YProperty,
                        new DoubleAnimation(dy, 0, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    visuel.BeginAnimation(UIElement.OpacityProperty,
                        new DoubleAnimation(0, 1, duree) { RepeatBehavior = repetition });
                    break;

                case AnimationType.Zoom:
                    scale.ScaleX = scale.ScaleY = .15;
                    visuel.Opacity = 0;
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                        new DoubleAnimation(.15, 1, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(.15, 1, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    visuel.BeginAnimation(UIElement.OpacityProperty,
                        new DoubleAnimation(0, 1, duree) { RepeatBehavior = repetition });
                    break;

                case AnimationType.Bounce:
                    scale.ScaleX = scale.ScaleY = .35;
                    var bounce = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = .45 };
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                        new DoubleAnimation(.35, 1, duree)
                        { EasingFunction = bounce, RepeatBehavior = repetition });
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(.35, 1, duree)
                        { EasingFunction = bounce, RepeatBehavior = repetition });
                    break;

                case AnimationType.Rotate:
                    rotation.Angle = element.Rotation - 180;
                    rotation.BeginAnimation(RotateTransform.AngleProperty,
                        new DoubleAnimation(element.Rotation - 180, element.Rotation + 180, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    break;

                case AnimationType.Pulse:
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                        new DoubleAnimation(1, 1.12, duree)
                        { AutoReverse = true, RepeatBehavior = repetition, EasingFunction = ease });
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(1, 1.12, duree)
                        { AutoReverse = true, RepeatBehavior = repetition, EasingFunction = ease });
                    break;

                case AnimationType.Disappear:
                    visuel.BeginAnimation(UIElement.OpacityProperty,
                        new DoubleAnimation(1, 0, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    break;

                case AnimationType.FlyOut:
                    translation.BeginAnimation(TranslateTransform.XProperty,
                        new DoubleAnimation(0, dx, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    translation.BeginAnimation(TranslateTransform.YProperty,
                        new DoubleAnimation(0, dy, duree)
                        { EasingFunction = ease, RepeatBehavior = repetition });
                    visuel.BeginAnimation(UIElement.OpacityProperty,
                        new DoubleAnimation(1, 0, duree) { RepeatBehavior = repetition });
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Le changement de diapositive annule les animations de l'ancienne.
        }
    }
}
