
using System;
using System.IO;
using System.Collections.Generic;
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

        private const string TexteTitre = "Cliquez pour ajouter un titre";
        private const string TexteSousTitre = "Cliquez pour ajouter un sous-titre";

        public event Action<SlideElement?>? ElementSelectionChangee;
        public event Action? DiapositiveModifiee;

        public SlideElement? ElementSelectionne => _elementSelectionne;

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
                DeselectionnerElement();
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

            try
            {
                var couleurConvertie =
                    ColorConverter.ConvertFromString(slide.BackgroundColor);

                Background = couleurConvertie is Color couleur
                    ? new SolidColorBrush(couleur)
                    : Brushes.White;
            }
            catch
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

            if (_elementSelectionne is TextElement texte &&
                _visuelSelectionne is TextBox zoneTexte)
            {
                ActualiserStyleTexte(texte, zoneTexte);
            }

            ActualiserCadreSelection();
            NotifierModification();
        }

        private TextAlignment ConvertirAlignement(string? alignement)
        {
            if (Enum.TryParse(
                    alignement,
                    true,
                    out TextAlignment resultat))
            {
                return resultat;
            }

            return TextAlignment.Left;
        }

        private void ActualiserStyleTexte(
            TextElement element,
            TextBox zoneTexte)
        {
            ElementStyle style = element.Style ?? new ElementStyle();

            bool estTitre =
                element.Height >= 80 && element.Width >= 700;

            bool estSousTitre =
                element.Height < 80 && element.Width >= 700;

            bool estPlaceholder =
                zoneTexte.Text == TexteTitre ||
                zoneTexte.Text == TexteSousTitre;

            double taillePolice = style.FontSize > 0
                ? style.FontSize
                : 18;

            if (taillePolice == 18 && estTitre)
                taillePolice = 34;
            else if (taillePolice == 18 && estSousTitre)
                taillePolice = 24;

            try
            {
                zoneTexte.FontFamily = new FontFamily(
                    string.IsNullOrWhiteSpace(style.FontFamily)
                        ? "Segoe UI"
                        : style.FontFamily);
            }
            catch
            {
                zoneTexte.FontFamily = new FontFamily("Segoe UI");
            }

            zoneTexte.FontSize = taillePolice;

            zoneTexte.FontWeight =
                style.IsBold || estTitre
                    ? FontWeights.Bold
                    : FontWeights.Normal;

            zoneTexte.FontStyle =
                style.IsItalic
                    ? FontStyles.Italic
                    : FontStyles.Normal;

            zoneTexte.TextDecorations =
                style.IsUnderline
                    ? TextDecorations.Underline
                    : null;

            zoneTexte.TextAlignment =
                ConvertirAlignement(style.TextAlignment);

            zoneTexte.Foreground = estPlaceholder
                ? Brushes.Gray
                : ConvertirCouleurTexte(style.TextColor);
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

        private Brush ConvertirCouleur(
            string? couleur,
            Brush couleurParDefaut)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(couleur))
                {
                    var resultat =
                        ColorConverter.ConvertFromString(couleur);

                    if (resultat is Color couleurConvertie)
                        return new SolidColorBrush(couleurConvertie);
                }
            }
            catch
            {
            }

            return couleurParDefaut;
        }

        private Brush ConvertirCouleurTexte(string? couleur)
        {
            return ConvertirCouleur(couleur, Brushes.Black);
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

            ElementStyle style = element.Style ?? new ElementStyle();

            double taillePolice = style.FontSize > 0
                ? style.FontSize
                : 18;

            if (taillePolice == 18 && estTitre)
                taillePolice = 34;
            else if (taillePolice == 18 && estSousTitre)
                taillePolice = 24;

            FontFamily famillePolice;

            try
            {
                famillePolice = new FontFamily(
                    string.IsNullOrWhiteSpace(style.FontFamily)
                        ? "Segoe UI"
                        : style.FontFamily);
            }
            catch
            {
                famillePolice = new FontFamily("Segoe UI");
            }

            var zoneTexte = new TextBox
            {
                Width = element.Width,
                Height = element.Height,

                FontFamily = famillePolice,
                FontSize = taillePolice,

                FontWeight = style.IsBold || estTitre
                    ? FontWeights.Bold
                    : FontWeights.Normal,

                FontStyle = style.IsItalic
                    ? FontStyles.Italic
                    : FontStyles.Normal,

                TextDecorations = style.IsUnderline
                    ? TextDecorations.Underline
                    : null,

                Foreground = estPlaceholder
                    ? Brushes.Gray
                    : ConvertirCouleurTexte(style.TextColor),

                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),

                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,

                VerticalContentAlignment = VerticalAlignment.Center,

                TextAlignment = ConvertirAlignement(style.TextAlignment),

                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,

                Text = element.Text
            };

            zoneTexte.TextChanged += (s, e) =>
            {
                element.Text = zoneTexte.Text;
                NotifierModification();
            };

            zoneTexte.GotKeyboardFocus += (s, e) =>
            {
                if (zoneTexte.Text == TexteTitre ||
                    zoneTexte.Text == TexteSousTitre)
                {
                    zoneTexte.Text = string.Empty;
                    zoneTexte.Foreground =
                        ConvertirCouleurTexte(element.Style?.TextColor);
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

                bool placeholderActuel =
                    zoneTexte.Text == TexteTitre ||
                    zoneTexte.Text == TexteSousTitre;

                zoneTexte.Foreground = placeholderActuel
                    ? Brushes.Gray
                    : ConvertirCouleurTexte(element.Style?.TextColor);

                zoneTexte.BorderBrush = Brushes.Transparent;
            };

            return zoneTexte;
        }

        private FrameworkElement CreateShape(ShapeElement element)
        {
            Brush remplissage =
                ConvertirCouleur(element.Style?.FillColor, Brushes.LightBlue);

            Brush contour =
                ConvertirCouleur(element.Style?.StrokeColor, Brushes.Black);

            double epaisseur =
                element.Style?.StrokeThickness > 0
                    ? element.Style.StrokeThickness
                    : 2;

            double largeur = Math.Max(1, element.Width);
            double hauteur = Math.Max(1, element.Height);

            Shape visual;

            switch (element.ShapeType)
            {
                case ShapeType.Rectangle:
                case ShapeType.Square:
                    visual = new Rectangle
                    {
                        Fill = remplissage,
                        Stroke = contour,
                        StrokeThickness = epaisseur
                    };
                    break;

                case ShapeType.RoundedRectangle:
                    visual = new Rectangle
                    {
                        RadiusX = 15,
                        RadiusY = 15,
                        Fill = remplissage,
                        Stroke = contour,
                        StrokeThickness = epaisseur
                    };
                    break;

                case ShapeType.Ellipse:
                    visual = new Ellipse
                    {
                        Fill = remplissage,
                        Stroke = contour,
                        StrokeThickness = epaisseur
                    };
                    break;

                case ShapeType.Line:
                    visual = new Line
                    {
                        X1 = 0,
                        Y1 = 0,
                        X2 = largeur,
                        Y2 = hauteur,
                        Stroke = contour,
                        StrokeThickness = epaisseur
                    };
                    break;

                default:
                    return CreerPolygone(
                        element.ShapeType,
                        largeur,
                        hauteur,
                        remplissage,
                        contour,
                        epaisseur);
            }

            visual.Width = largeur;
            visual.Height = hauteur;

            return visual;
        }

        private FrameworkElement CreerPolygone(
            ShapeType type,
            double largeur,
            double hauteur,
            Brush remplissage,
            Brush contour,
            double epaisseur)
        {
            PointCollection points = new();

            switch (type)
            {
                case ShapeType.Diamond:
                    points = Points(
                        (0.5, 0), (1, 0.5), (0.5, 1), (0, 0.5));
                    break;

                case ShapeType.Triangle:
                    points = Points(
                        (0.5, 0), (1, 1), (0, 1));
                    break;

                case ShapeType.InvertedTriangle:
                    points = Points(
                        (0, 0), (1, 0), (0.5, 1));
                    break;

                case ShapeType.Pentagon:
                    points = Points(
                        (0.5, 0), (1, 0.38), (0.81, 1),
                        (0.19, 1), (0, 0.38));
                    break;

                case ShapeType.Hexagon:
                    points = Points(
                        (0.25, 0), (0.75, 0), (1, 0.5),
                        (0.75, 1), (0.25, 1), (0, 0.5));
                    break;

                case ShapeType.SemiCircle:
                    points = CreerPointsDemiCercle();
                    break;

                case ShapeType.Cross:
                case ShapeType.Plus:
                    points = Points(
                        (0.35, 0), (0.65, 0),
                        (0.65, 0.35), (1, 0.35),
                        (1, 0.65), (0.65, 0.65),
                        (0.65, 1), (0.35, 1),
                        (0.35, 0.65), (0, 0.65),
                        (0, 0.35), (0.35, 0.35));
                    break;

                case ShapeType.Star5:
                    points = CreerEtoile(5);
                    break;

                case ShapeType.Star6:
                    points = CreerEtoile(6);
                    break;

                case ShapeType.Arrow:
                    points = Points(
                        (0, 0.35), (0.68, 0.35),
                        (0.68, 0.05), (1, 0.5),
                        (0.68, 0.95), (0.68, 0.65),
                        (0, 0.65));
                    break;

                case ShapeType.ArrowLeft:
                    points = Points(
                        (1, 0.35), (0.32, 0.35),
                        (0.32, 0.05), (0, 0.5),
                        (0.32, 0.95), (0.32, 0.65),
                        (1, 0.65));
                    break;

                case ShapeType.ArrowUp:
                    points = Points(
                        (0.35, 1), (0.35, 0.32),
                        (0.05, 0.32), (0.5, 0),
                        (0.95, 0.32), (0.65, 0.32),
                        (0.65, 1));
                    break;

                case ShapeType.ArrowDown:
                    points = Points(
                        (0.35, 0), (0.65, 0),
                        (0.65, 0.68), (0.95, 0.68),
                        (0.5, 1), (0.05, 0.68),
                        (0.35, 0.68));
                    break;

                case ShapeType.DoubleArrow:
                    points = Points(
                        (0, 0.5), (0.25, 0.05),
                        (0.25, 0.35), (0.75, 0.35),
                        (0.75, 0.05), (1, 0.5),
                        (0.75, 0.95), (0.75, 0.65),
                        (0.25, 0.65), (0.25, 0.95));
                    break;

                default:
                    points = Points(
                        (0, 0), (1, 0), (1, 1), (0, 1));
                    break;
            }

            return new Polygon
            {
                Width = largeur,
                Height = hauteur,
                Points = points,
                Fill = remplissage,
                Stroke = contour,
                StrokeThickness = epaisseur,
                Stretch = Stretch.Fill
            };
        }

        private PointCollection Points(
            params (double X, double Y)[] coordonnees)
        {
            var resultat = new PointCollection();

            foreach (var point in coordonnees)
                resultat.Add(new Point(point.X, point.Y));

            return resultat;
        }

        private PointCollection CreerEtoile(int branches)
        {
            var points = new PointCollection();

            double centreX = 0.5;
            double centreY = 0.5;

            double rayonExterieur = 0.5;
            double rayonInterieur = 0.22;

            for (int i = 0; i < branches * 2; i++)
            {
                double angle = -Math.PI / 2 +
                    i * Math.PI / branches;

                double rayon = i % 2 == 0
                    ? rayonExterieur
                    : rayonInterieur;

                double x = centreX + Math.Cos(angle) * rayon;
                double y = centreY + Math.Sin(angle) * rayon;

                points.Add(new Point(x, y));
            }

            return points;
        }

        private PointCollection CreerPointsDemiCercle()
        {
            var points = new PointCollection();

            points.Add(new Point(0, 1));

            for (int i = 0; i <= 20; i++)
            {
                double angle = Math.PI + i * Math.PI / 20;

                double x = 0.5 + 0.5 * Math.Cos(angle);
                double y = 1 - 0.5 * Math.Sin(angle);

                points.Add(new Point(x, y));
            }

            points.Add(new Point(1, 1));

            return points;
        }

        private FrameworkElement CreateImage(ImageElement element)
        {
            var image = new Image
            {
                Width = element.Width,
                Height = element.Height,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(element.ImagePath))
                {
                    string path = element.ImagePath;

                    if (File.Exists(path))
                    {
                        image.Source = new BitmapImage(
                            new Uri(path, UriKind.Absolute));
                    }
                }
            }
            catch
            {
                // L'image ne peut pas être chargée.
            }

            return new Border
            {
                Width = element.Width,
                Height = element.Height,
                Background = Brushes.Transparent,
                Child = image
            };
        }
    }
}
