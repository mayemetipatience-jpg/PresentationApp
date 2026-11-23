using System.Text.Json.Serialization;

namespace PresentationApp.Core.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextElement), "text")]
[JsonDerivedType(typeof(ShapeElement), "shape")]
[JsonDerivedType(typeof(ImageElement), "image")]
public abstract class SlideElement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 100;
    public double Height { get; set; } = 50;
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public ElementStyle Style { get; set; } = new();
}