
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PresentationApp.Core.Models;


namespace PresentationApp.UI.Canvas
{
    public class SlideCanvas : System.Windows.Controls.Canvas
    {
        private Slide? _slidePourInsertion;
        private bool _modeInsertionTexte;

        public SlideCanvas()
        {
            Width = 960;
            Height = 540;
            Background = Brushes.White;
            ClipToBounds = true;

            MouseLeftButtonDown += SlideCanvas_MouseLeftButtonDown;
        }

        public void ActiverInsertionTexte(Slide slide)
        {
            _slidePourInsertion = slide;
            _modeInsertionTexte = true;
        }

        private void SlideCanvas_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
        {
            if (!_modeInsertionTexte || _slidePourInsertion == null)
                return;

            // Ne pas créer un nouveau texte lorsqu'on clique sur un élément existant.
            if (e.OriginalSource != this)
                return;

            Point position = e.GetPosition(this);

            var nouvelElement = new TextElement
            {
                X = position.X,
                Y = position.Y,
                Width = 250,
                Height = 80,
                Text = string.Empty
            };

            _slidePourInsertion.Elements.Add(nouvelElement);

            var zoneTexte = new TextBox
            {
                Width = nouvelElement.Width,
                Height = nouvelElement.Height,
                FontSize = 24,
                Foreground = Brushes.Black,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(1),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Text = string.Empty
            };

            zoneTexte.TextChanged += (s, args) =>
            {
                nouvelElement.Text = zoneTexte.Text;
            };

            SetLeft(zoneTexte, nouvelElement.X);
            SetTop(zoneTexte, nouvelElement.Y);
            SetZIndex(zoneTexte, nouvelElement.ZIndex);

            Children.Add(zoneTexte);

            _modeInsertionTexte = false;
            _slidePourInsertion = null;

            zoneTexte.Focus();
            Keyboard.Focus(zoneTexte);

            e.Handled = true;
        }

        public void DisplaySlide(Slide? slide)
        {
            Children.Clear();

            if (slide == null)
                return;

            var couleurConvertie =
            ColorConverter.ConvertFromString(slide.BackgroundColor);

            if (couleurConvertie is Color couleur)
            {
                Background = new SolidColorBrush(couleur);
            }
            else
            {
                Background = Brushes.White;
            }

            foreach (SlideElement element in slide.Elements)
            {
                FrameworkElement? visual = CreateVisual(element);

                if (visual == null)
                    continue;

                SetLeft(visual, element.X);
                SetTop(visual, element.Y);
                SetZIndex(visual, element.ZIndex);

                visual.RenderTransformOrigin = new Point(0.5, 0.5);
                visual.RenderTransform =
                new RotateTransform(element.Rotation);

                Children.Add(visual);
            }
        }

        private FrameworkElement? CreateVisual(SlideElement element)
        {
            if (element is TextElement text)
                return CreateText(text);

            if (element is ShapeElement shape)
                return CreateShape(shape);

            if (element is ImageElement image)
                return CreateImage(image);

            return null;
        }

        private FrameworkElement CreateText(TextElement element)
        {
            return new TextBlock
            {
                Text = element.Text,
                Width = element.Width,
                Height = element.Height,
                FontSize = 24,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private FrameworkElement CreateShape(ShapeElement element)
        {
            Shape visual;

            switch (element.ShapeType)
            {
                case ShapeType.Ellipse:
                    visual = new Ellipse
                    {
                        Fill = Brushes.LightBlue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    break;

                case ShapeType.Line:
                    visual = new Line
                    {
                        X1 = 0,
                        Y1 = 0,
                        X2 = element.Width,
                        Y2 = element.Height,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    break;

                case ShapeType.Arrow:
                    return CreateArrow(element);

                default:
                    visual = new Rectangle
                    {
                        Fill = Brushes.LightBlue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    break;
            }

            visual.Width = element.Width;
            visual.Height = element.Height;

            return visual;
        }

        private FrameworkElement CreateArrow(ShapeElement element)
        {
            var arrow = new Grid
            {
                Width = element.Width,
                Height = element.Height
            };

            var line = new Line
            {
                X1 = 0,
                Y1 = element.Height / 2,
                X2 = element.Width - 12,
                Y2 = element.Height / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            var head = new Polygon
            {
                Points = new PointCollection
{
new Point(element.Width, element.Height / 2),
new Point(element.Width - 14, element.Height / 2 - 8),
new Point(element.Width - 14, element.Height / 2 + 8)
},
                Fill = Brushes.Black
            };

            arrow.Children.Add(line);
            arrow.Children.Add(head);

            return arrow;
        }

        private FrameworkElement CreateImage(ImageElement element)
        {
            var visual = new Image
            {
                Width = element.Width,
                Height = element.Height,
                Stretch = Stretch.Fill
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(element.ImagePath))
                {
                    string path = System.IO.Path.GetFullPath(element.ImagePath);

                    if (File.Exists(path))
                    {
                        visual.Source = new BitmapImage(
                        new Uri(path, UriKind.Absolute));
                    }
                }
            }
            catch
            {
                // L'image ne peut pas être chargée.
            }

            return visual;
        }
    }
}
