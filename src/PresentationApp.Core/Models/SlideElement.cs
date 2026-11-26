
using System;
using System.Text.Json.Serialization;

namespace PresentationApp.Core.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextElement), "text")]
[JsonDerivedType(typeof(ShapeElement), "shape")]
[JsonDerivedType(typeof(ImageElement), "image")]
[JsonDerivedType(typeof(TableElement), "table")]
[JsonDerivedType(typeof(ChartElement), "chart")]
public abstract class SlideElement
{
    // Identifiant unique de l'élément.
    public Guid Id { get; set; } = Guid.NewGuid();

    // Position de l'élément.
    public double X { get; set; }
    public double Y { get; set; }

    // Dimensions de l'élément.
    public double Width { get; set; } = 100;
    public double Height { get; set; } = 50;

    // Rotation de l'élément.
    public double Rotation { get; set; }

    // Ordre de superposition.
    public int ZIndex { get; set; }

    // Indique si l'élément est verrouillé.
    public bool IsLocked { get; set; } = false;

    // Style graphique.
    public ElementStyle Style { get; set; } = new();

    // Animation associée à cet élément.
    public ElementAnimation Animation { get; set; } = new();
}
