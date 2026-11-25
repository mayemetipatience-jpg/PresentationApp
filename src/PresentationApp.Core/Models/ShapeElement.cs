
namespace PresentationApp.Core.Models;

public enum ShapeType
{
    Rectangle,
    RoundedRectangle,
    Ellipse,
    Diamond,
    Triangle,
    InvertedTriangle,
    Pentagon,
    Hexagon,
    Square,
    SemiCircle,
    Cross,
    Plus,
    Star5,
    Star6,
    Arrow,
    ArrowLeft,
    ArrowUp,
    ArrowDown,
    DoubleArrow,
    Line
}

public class ShapeElement : SlideElement
{
    public ShapeType ShapeType { get; set; } = ShapeType.Rectangle;
}
