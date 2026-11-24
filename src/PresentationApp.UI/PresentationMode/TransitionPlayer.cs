using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.PresentationMode;

/// <summary>Joue la transition (fondu, glissement, zoom) entre deux calques de diapositive.</summary>
public static class TransitionPlayer
{
    public static Task PlayAsync(FrameworkElement outgoing, FrameworkElement incoming,
        TransitionType type, bool forward, int durationMs, double stageWidth)
    {
        var tcs = new TaskCompletionSource();
        var duration = TimeSpan.FromMilliseconds(Math.Max(durationMs, 50));
        var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

        void Finish()
        {
            Reset(outgoing);
            Reset(incoming);
            tcs.TrySetResult();
        }

        DoubleAnimation Anim(double from, double to) =>
            new(from, to, duration) { EasingFunction = ease };

        if (type == TransitionType.None) { Finish(); return tcs.Task; }

        outgoing.RenderTransformOrigin = new Point(0.5, 0.5);
        incoming.RenderTransformOrigin = new Point(0.5, 0.5);

        switch (type)
        {
            case TransitionType.Fade:
            {
                incoming.Opacity = 0;
                var a = Anim(0, 1);
                a.Completed += (_, _) => Finish();
                incoming.BeginAnimation(UIElement.OpacityProperty, a);
                break;
            }
            case TransitionType.Slide:
            {
                double dir = forward ? 1 : -1;
                var tIn = new TranslateTransform(stageWidth * dir, 0);
                var tOut = new TranslateTransform();
                incoming.RenderTransform = tIn;
                outgoing.RenderTransform = tOut;
                var aIn = Anim(stageWidth * dir, 0);
                aIn.Completed += (_, _) => Finish();
                tIn.BeginAnimation(TranslateTransform.XProperty, aIn);
                tOut.BeginAnimation(TranslateTransform.XProperty, Anim(0, -stageWidth * dir));
                break;
            }
            case TransitionType.Zoom:
            {
                var scale = new ScaleTransform(0.6, 0.6);
                incoming.RenderTransform = scale;
                incoming.Opacity = 0;
                var aS = Anim(0.6, 1);
                aS.Completed += (_, _) => Finish();
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, aS);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, Anim(0.6, 1));
                incoming.BeginAnimation(UIElement.OpacityProperty, Anim(0, 1));
                break;
            }
            default:
                Finish();
                break;
        }
        return tcs.Task;
    }

    private static void Reset(FrameworkElement e)
    {
        e.BeginAnimation(UIElement.OpacityProperty, null);
        e.Opacity = 1;
        e.RenderTransform = Transform.Identity;
    }
}
