namespace PresentationApp.Core.Models;

public enum ShapeType
{
    Rectangle,
    Ellipse,
    Line,
    Arrow
}

public class ShapeElement : SlideElement
{
    public ShapeType ShapeType { get; set; } = ShapeType.Rectangle;
}
