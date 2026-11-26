using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PresentationApp.Core.Models;

namespace PresentationApp.UI.Converters;

// Transforme un type de forme en dessin (Geometry) pour les miniatures.
// Les points sont donnés entre 0 et 1 : la forme est ensuite étirée à la taille de l'élément.
// Ce sont les mêmes contours que ceux dessinés par le canevas, pour que la miniature
// ressemble à la vraie diapositive.
public class ShapeGeometryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ShapeType type)
            return Geometry.Empty;

        return type switch
        {
            ShapeType.Rectangle or ShapeType.Square => new RectangleGeometry(new Rect(0, 0, 1, 1)),
            ShapeType.RoundedRectangle => new RectangleGeometry(new Rect(0, 0, 1, 1), 0.1, 0.1),
            ShapeType.Ellipse => new EllipseGeometry(new Rect(0, 0, 1, 1)),
            ShapeType.Line => new LineGeometry(new Point(0, 0), new Point(1, 1)),
            ShapeType.Diamond => Polygone((0.5, 0), (1, 0.5), (0.5, 1), (0, 0.5)),
            ShapeType.Triangle => Polygone((0.5, 0), (1, 1), (0, 1)),
            ShapeType.InvertedTriangle => Polygone((0, 0), (1, 0), (0.5, 1)),
            ShapeType.Pentagon => Polygone((0.5, 0), (1, 0.38), (0.81, 1), (0.19, 1), (0, 0.38)),
            ShapeType.Hexagon => Polygone((0.25, 0), (0.75, 0), (1, 0.5), (0.75, 1), (0.25, 1), (0, 0.5)),
            ShapeType.SemiCircle => DemiCercle(),
            ShapeType.Cross or ShapeType.Plus => Polygone(
                (0.35, 0), (0.65, 0), (0.65, 0.35), (1, 0.35), (1, 0.65), (0.65, 0.65),
                (0.65, 1), (0.35, 1), (0.35, 0.65), (0, 0.65), (0, 0.35), (0.35, 0.35)),
            ShapeType.Star5 => Etoile(5),
            ShapeType.Star6 => Etoile(6),
            ShapeType.Arrow => Polygone(
                (0, 0.35), (0.68, 0.35), (0.68, 0.05), (1, 0.5), (0.68, 0.95), (0.68, 0.65), (0, 0.65)),
            ShapeType.ArrowLeft => Polygone(
                (1, 0.35), (0.32, 0.35), (0.32, 0.05), (0, 0.5), (0.32, 0.95), (0.32, 0.65), (1, 0.65)),
            ShapeType.ArrowUp => Polygone(
                (0.35, 1), (0.35, 0.32), (0.05, 0.32), (0.5, 0), (0.95, 0.32), (0.65, 0.32), (0.65, 1)),
            ShapeType.ArrowDown => Polygone(
                (0.35, 0), (0.65, 0), (0.65, 0.68), (0.95, 0.68), (0.5, 1), (0.05, 0.68), (0.35, 0.68)),
            ShapeType.DoubleArrow => Polygone(
                (0, 0.5), (0.25, 0.05), (0.25, 0.35), (0.75, 0.35), (0.75, 0.05),
                (1, 0.5), (0.75, 0.95), (0.75, 0.65), (0.25, 0.65), (0.25, 0.95)),
            _ => new RectangleGeometry(new Rect(0, 0, 1, 1))
        };
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    // Construit un polygone fermé à partir d'une liste de points
    private static Geometry Polygone(params (double X, double Y)[] coordonnees)
    {
        var figure = new PathFigure
        {
            StartPoint = new Point(coordonnees[0].X, coordonnees[0].Y),
            IsClosed = true,
            IsFilled = true
        };

        for (int i = 1; i < coordonnees.Length; i++)
            figure.Segments.Add(new LineSegment(new Point(coordonnees[i].X, coordonnees[i].Y), true));

        var geometrie = new PathGeometry();
        geometrie.Figures.Add(figure);
        geometrie.Freeze();
        return geometrie;
    }

    private static Geometry Etoile(int branches)
    {
        var points = new (double X, double Y)[branches * 2];

        for (int i = 0; i < points.Length; i++)
        {
            double angle = -Math.PI / 2 + i * Math.PI / branches;
            double rayon = i % 2 == 0 ? 0.5 : 0.22;
            points[i] = (0.5 + Math.Cos(angle) * rayon, 0.5 + Math.Sin(angle) * rayon);
        }

        return Polygone(points);
    }

    private static Geometry DemiCercle()
    {
        var points = new List<(double X, double Y)> { (0, 1) };

        for (int i = 0; i <= 20; i++)
        {
            double angle = Math.PI + i * Math.PI / 20;
            points.Add((0.5 + 0.5 * Math.Cos(angle), 1 - 0.5 * Math.Sin(angle)));
        }

        points.Add((1, 1));
        return Polygone(points.ToArray());
    }
}
