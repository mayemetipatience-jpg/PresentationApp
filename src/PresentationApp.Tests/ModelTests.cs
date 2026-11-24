using PresentationApp.Core.Models;
using Xunit;

namespace PresentationApp.Tests;

public class ModelTests
{
    [Fact]
    public void Presentation_HasDefaults()
    {
        var presentation = new Presentation();

        Assert.Equal(1, presentation.FormatVersion);
        Assert.Equal("Sans titre", presentation.Title);
        Assert.Empty(presentation.Slides);
    }

    [Fact]
    public void Slide_HasDefaults()
    {
        var slide = new Slide();

        Assert.NotEqual(Guid.Empty, slide.Id);
        Assert.Equal(0, slide.Order);
        Assert.Equal("#FFFFFF", slide.BackgroundColor);
        Assert.Equal(TransitionType.None, slide.Transition.Type);
        Assert.Empty(slide.Elements);
    }

    [Fact]
    public void TwoSlides_HaveDifferentIds()
    {
        Assert.NotEqual(new Slide().Id, new Slide().Id);
    }

    [Fact]
    public void SlideElement_HasDefaultBounds()
    {
        var element = new TextElement();

        Assert.Equal(100, element.Width);
        Assert.Equal(50, element.Height);
        Assert.Equal(0, element.Rotation);
        Assert.Equal(0, element.ZIndex);
    }

    [Fact]
    public void Elements_HaveUniqueIds()
    {
        Assert.NotEqual(new TextElement().Id, new TextElement().Id);
    }

    [Fact]
    public void ElementStyle_HasDefaults()
    {
        var style = new ElementStyle();

        Assert.Equal("#FFFFFF", style.FillColor);
        Assert.Equal("#000000", style.TextColor);
        Assert.Equal("Segoe UI", style.FontFamily);
        Assert.Equal(18, style.FontSize);
        Assert.False(style.IsBold);
    }

    [Fact]
    public void Transition_HasDefaults()
    {
        var transition = new Transition();

        Assert.Equal(TransitionType.None, transition.Type);
        Assert.Equal(500, transition.DurationMs);
    }

    [Fact]
    public void ShapeElement_DefaultsToRectangle()
    {
        Assert.Equal(ShapeType.Rectangle, new ShapeElement().ShapeType);
    }

    [Fact]
    public void Slide_CanHoldDifferentElementTypes()
    {
        var slide = new Slide();
        slide.Elements.Add(new TextElement { Text = "Titre" });
        slide.Elements.Add(new ShapeElement());
        slide.Elements.Add(new ImageElement { ImagePath = "logo.png" });

        Assert.Equal(3, slide.Elements.Count);
        Assert.IsType<TextElement>(slide.Elements[0]);
        Assert.IsType<ShapeElement>(slide.Elements[1]);
        Assert.IsType<ImageElement>(slide.Elements[2]);
    }

    [Fact]
    public void Elements_DoNotShareStyle()
    {
        var a = new TextElement();
        var b = new TextElement();

        Assert.NotSame(a.Style, b.Style);
    }
}
