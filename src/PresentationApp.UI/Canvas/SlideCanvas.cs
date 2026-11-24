
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private SlideElement? _elementSelectionne;
        private FrameworkElement? _visuelSelectionne;

        private Rectangle? _cadreSelection;
        private readonly List<Thumb> _poignees = new();

        private Point _positionInitiale;
        private double _positionXInitiale;
        private double _positionYInitiale;

        private bool _deplacementEnCours;

        private const double TaillePoignee = 10;
        private const double TailleMinimum = 30;

        private const string TexteTitre =
            "Cliquez pour ajouter un titre";

        private const string TexteSousTitre =
            "Cliquez pour ajouter un sous-titre";

        public event Action<SlideElement?>? ElementSelectionChangee;

        public event Action? DiapositiveModifiee;

        public SlideCanvas()
        {
            Width = 960;
            Height = 540;
            Background = Brushes.White;
            ClipToBounds = true;

            MouseLeftButtonDown += SlideCanvas_MouseLeftButtonDown;

            CreerCadreSelection();
        }

        public void NotifierModification()
        {
            DiapositiveModifiee?.Invoke();
        }

        public void CreerCadreSelection()
        {
            _cadreSelection = new Rectangle
            {
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = Brushes.Transparent,
                IsHitTestVisible = false,
                Visibility = Visibility.Collapsed
            };

            Children.Add(_cadreSelection);
            SetZIndex(_cadreSelection, int.MaxValue - 1);

            CreerPoignee("HautGauche", Cursors.SizeNWSE);
            CreerPoignee("HautDroit", Cursors.SizeNESW);
            CreerPoignee("BasGauche", Cursors.SizeNESW);
            CreerPoignee("BasDroit", Cursors.SizeNWSE);
        }

        private void CreerPoignee(string nom, Cursor curseur)
        {
            var poignee = new Thumb
            {
                Width = TaillePoignee,
                Height = TaillePoignee,
                Background = Brushes.White,
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(1),
                Cursor = curseur,
                Tag = nom
            };

            poignee.DragDelta += Poignee_DragDelta;

            _poignees.Add(poignee);
            Children.Add(poignee);

            SetZIndex(poignee, int.MaxValue);
        }

        public void ActiverInsertionTexte(Slide slide)
        {
            _slidePourInsertion = slide;
            _modeInsertionTexte = true;

            Cursor = Cursors.Cross;

            DeselectionnerElement();
        }

        private void SlideCanvas_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_modeInsertionTexte && _slidePourInsertion != null)
            {
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

                TextBox zoneTexte = CreerEditeurTexte(nouvelElement);

                SetLeft(zoneTexte, nouvelElement.X);
                SetTop(zoneTexte, nouvelElement.Y);
                SetZIndex(zoneTexte, nouvelElement.ZIndex);

                AjouterEvenementsSelection(zoneTexte, nouvelElement);

                Children.Add(zoneTexte);

                _modeInsertionTexte = false;
                _slidePourInsertion = null;

                Cursor = Cursors.Arrow;

                SelectionnerElement(nouvelElement, zoneTexte);

                NotifierModification();

                zoneTexte.Focus();
                Keyboard.Focus(zoneTexte);

                e.Handled = true;
                return;
            }

            if (e.OriginalSource == this)
            {
                DeselectionnerElement();
            }
        }

        public void DisplaySlide(Slide? slide)
        {
            Children.Clear();

            _elementSelectionne = null;
            _visuelSelectionne = null;
            _cadreSelection = null;
            _poignees.Clear();

            ElementSelectionChangee?.Invoke(null);

            _modeInsertionTexte = false;
            _slidePourInsertion = null;

            Cursor = Cursors.Arrow;

            if (slide == null)
                return;

            var couleurConvertie =
                ColorConverter.ConvertFromString(slide.BackgroundColor);

            if (couleurConvertie is Color couleur)
                Background = new SolidColorBrush(couleur);
            else
                Background = Brushes.White;

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

                AjouterEvenementsSelection(visual, element);

                Children.Add(visual);
            }

            CreerCadreSelection();
        }

        private void AjouterEvenementsSelection(
            FrameworkElement visual,
            SlideElement element)
        {
            visual.PreviewMouseLeftButtonDown += (sender, e) =>
            {
                SelectionnerElement(element, visual);

                _positionInitiale = e.GetPosition(this);
                _positionXInitiale = element.X;
                _positionYInitiale = element.Y;

                _deplacementEnCours = false;

                Mouse.Capture(visual);
            };

            visual.PreviewMouseMove += (sender, e) =>
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return;

                if (_elementSelectionne != element)
                    return;

                Point positionActuelle = e.GetPosition(this);

                double differenceX =
                    positionActuelle.X - _positionInitiale.X;

                double differenceY =
                    positionActuelle.Y - _positionInitiale.Y;

                if (Math.Abs(differenceX) > 2 ||
                    Math.Abs(differenceY) > 2)
                {
                    _deplacementEnCours = true;
                }

                if (!_deplacementEnCours)
                    return;

                element.X = Math.Max(
                    0, _positionXInitiale + differenceX);

                element.Y = Math.Max(
                    0, _positionYInitiale + differenceY);

                SetLeft(visual, element.X);
                SetTop(visual, element.Y);

                ActualiserCadreSelection();

                ElementSelectionChangee?.Invoke(element);
                NotifierModification();
            };

            visual.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                if (Mouse.Captured == visual)
                    Mouse.Capture(null);

                _deplacementEnCours = false;
            };
        }

        private void SelectionnerElement(
            SlideElement element,
            FrameworkElement visual)
        {
            _elementSelectionne = element;
            _visuelSelectionne = visual;

            ActualiserCadreSelection();

            ElementSelectionChangee?.Invoke(element);
        }

        private void DeselectionnerElement()
        {
            _elementSelectionne = null;
            _visuelSelectionne = null;

            if (_cadreSelection != null)
                _cadreSelection.Visibility = Visibility.Collapsed;

            foreach (Thumb poignee in _poignees)
                poignee.Visibility = Visibility.Collapsed;

            ElementSelectionChangee?.Invoke(null);
        }

        public void ActualiserElementSelectionne()
        {
            if (_elementSelectionne == null ||
                _visuelSelectionne == null)
                return;

            _visuelSelectionne.Width = _elementSelectionne.Width;
            _visuelSelectionne.Height = _elementSelectionne.Height;

            SetLeft(_visuelSelectionne, _elementSelectionne.X);
            SetTop(_visuelSelectionne, _elementSelectionne.Y);

            _visuelSelectionne.RenderTransform =
                new RotateTransform(_elementSelectionne.Rotation);

            ActualiserCadreSelection();

            NotifierModification();
        }

        private void ActualiserCadreSelection()
        {
            if (_elementSelectionne == null ||
                _visuelSelectionne == null ||
                _cadreSelection == null)
                return;

            double x = _elementSelectionne.X;
            double y = _elementSelectionne.Y;
            double largeur = _elementSelectionne.Width;
            double hauteur = _elementSelectionne.Height;

            _cadreSelection.Width = largeur + 4;
            _cadreSelection.Height = hauteur + 4;

            SetLeft(_cadreSelection, x - 2);
            SetTop(_cadreSelection, y - 2);

            _cadreSelection.Visibility = Visibility.Visible;

            PositionnerPoignee(0, x - TaillePoignee / 2,
                y - TaillePoignee / 2);

            PositionnerPoignee(1, x + largeur - TaillePoignee / 2,
                y - TaillePoignee / 2);

            PositionnerPoignee(2, x - TaillePoignee / 2,
                y + hauteur - TaillePoignee / 2);

            PositionnerPoignee(3, x + largeur - TaillePoignee / 2,
                y + hauteur - TaillePoignee / 2);

            foreach (Thumb poignee in _poignees)
                poignee.Visibility = Visibility.Visible;
        }

        private void PositionnerPoignee(int index, double x, double y)
        {
            if (index >= _poignees.Count)
                return;

            SetLeft(_poignees[index], x);
            SetTop(_poignees[index], y);
        }

        private void Poignee_DragDelta(
            object sender,
            DragDeltaEventArgs e)
        {
            if (_elementSelectionne == null ||
                _visuelSelectionne == null ||
                sender is not Thumb poignee)
                return;

            double x = _elementSelectionne.X;
            double y = _elementSelectionne.Y;
            double largeur = _elementSelectionne.Width;
            double hauteur = _elementSelectionne.Height;

            double nouveauX = x;
            double nouveauY = y;
            double nouvelleLargeur = largeur;
            double nouvelleHauteur = hauteur;

            switch (poignee.Tag?.ToString())
            {
                case "HautGauche":
                    nouvelleLargeur = Math.Max(
                        TailleMinimum, largeur - e.HorizontalChange);

                    nouvelleHauteur = Math.Max(
                        TailleMinimum, hauteur - e.VerticalChange);

                    nouveauX = x + largeur - nouvelleLargeur;
                    nouveauY = y + hauteur - nouvelleHauteur;
                    break;

                case "HautDroit":
                    nouvelleLargeur = Math.Max(
                        TailleMinimum, largeur + e.HorizontalChange);

                    nouvelleHauteur = Math.Max(
                        TailleMinimum, hauteur - e.VerticalChange);

                    nouveauY = y + hauteur - nouvelleHauteur;
                    break;

                case "BasGauche":
                    nouvelleLargeur = Math.Max(
                        TailleMinimum, largeur - e.HorizontalChange);

                    nouvelleHauteur = Math.Max(
                        TailleMinimum, hauteur + e.VerticalChange);

                    nouveauX = x + largeur - nouvelleLargeur;
                    break;

                case "BasDroit":
                    nouvelleLargeur = Math.Max(
                        TailleMinimum, largeur + e.HorizontalChange);

                    nouvelleHauteur = Math.Max(
                        TailleMinimum, hauteur + e.VerticalChange);
                    break;
            }

            if (nouveauX < 0)
            {
                nouvelleLargeur += nouveauX;
                nouveauX = 0;
            }

            if (nouveauY < 0)
            {
                nouvelleHauteur += nouveauY;
                nouveauY = 0;
            }

            nouvelleLargeur = Math.Max(TailleMinimum, nouvelleLargeur);
            nouvelleHauteur = Math.Max(TailleMinimum, nouvelleHauteur);

            _elementSelectionne.X = nouveauX;
            _elementSelectionne.Y = nouveauY;
            _elementSelectionne.Width = nouvelleLargeur;
            _elementSelectionne.Height = nouvelleHauteur;

            _visuelSelectionne.Width = nouvelleLargeur;
            _visuelSelectionne.Height = nouvelleHauteur;

            SetLeft(_visuelSelectionne, nouveauX);
            SetTop(_visuelSelectionne, nouveauY);

            ActualiserCadreSelection();

            ElementSelectionChangee?.Invoke(_elementSelectionne);
            NotifierModification();
        }

        private FrameworkElement? CreateVisual(SlideElement element)
        {
            if (element is TextElement text)
                return CreerEditeurTexte(text);

            if (element is ShapeElement shape)
                return CreateShape(shape);

            if (element is ImageElement image)
                return CreateImage(image);

            return null;
        }

        private TextBox CreerEditeurTexte(TextElement element)
        {
            bool estTitre =
                element.Height >= 80 && element.Width >= 700;

            bool estSousTitre =
                element.Height < 80 && element.Width >= 700;

            bool estPlaceholder =
                element.Text == TexteTitre ||
                element.Text == TexteSousTitre;

            var zoneTexte = new TextBox
            {
                Width = element.Width,
                Height = element.Height,

                FontSize = estTitre ? 34 : 24,

                FontWeight = estTitre
                    ? FontWeights.Bold
                    : FontWeights.Normal,

                Foreground = estPlaceholder
                    ? Brushes.Gray
                    : Brushes.Black,

                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),

                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,

                VerticalContentAlignment = VerticalAlignment.Center,

                TextAlignment = estTitre || estSousTitre
                    ? TextAlignment.Center
                    : TextAlignment.Left,

                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,

                Text = element.Text
            };

            zoneTexte.TextChanged += (s, e) =>
            {
                element.Text = zoneTexte.Text;
                zoneTexte.Foreground = Brushes.Black;

                NotifierModification();
            };

            zoneTexte.GotKeyboardFocus += (s, e) =>
            {
                if (zoneTexte.Text == TexteTitre ||
                    zoneTexte.Text == TexteSousTitre)
                {
                    zoneTexte.Text = string.Empty;
                    zoneTexte.Foreground = Brushes.Black;
                }

                zoneTexte.BorderBrush = Brushes.DodgerBlue;
            };

            zoneTexte.LostKeyboardFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(zoneTexte.Text))
                {
                    if (estTitre)
                        zoneTexte.Text = TexteTitre;
                    else if (estSousTitre)
                        zoneTexte.Text = TexteSousTitre;
                }

                zoneTexte.Foreground =
                    zoneTexte.Text == TexteTitre ||
                    zoneTexte.Text == TexteSousTitre
                        ? Brushes.Gray
                        : Brushes.Black;

                zoneTexte.BorderBrush = Brushes.Transparent;
            };

            return zoneTexte;
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
                    string path =
                        System.IO.Path.GetFullPath(element.ImagePath);

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
