using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;
using PresentationApp.Core.Services;
using Xunit;

namespace PresentationApp.Tests;

public class ElementCommandsTests
{
    private static (Slide slide, ElementService service, CommandManager commands) Create()
    {
        var commands = new CommandManager();
        return (new Slide(), new ElementService(commands), commands);
    }

    [Fact]
    public void AddText_ThenUndo_RemovesElement()
    {
        var (slide, service, commands) = Create();
        service.AddText(slide, "Bonjour", 10, 20);
        Assert.Single(slide.Elements);

        commands.Undo();
        Assert.Empty(slide.Elements);
    }

    [Fact]
    public void RemoveElement_ThenUndo_RestoresAtSameIndex()
    {
        var (slide, service, commands) = Create();
        var a = service.AddShape(slide, ShapeType.Rectangle, 0, 0);
        var b = service.AddShape(slide, ShapeType.Ellipse, 0, 0);
        var c = service.AddShape(slide, ShapeType.Line, 0, 0);

        service.Remove(slide, b);
        Assert.Equal(2, slide.Elements.Count);

        commands.Undo();
        Assert.Equal(3, slide.Elements.Count);
        Assert.Same(b, slide.Elements[1]);
        Assert.Same(a, slide.Elements[0]);
        Assert.Same(c, slide.Elements[2]);
    }

    [Fact]
    public void Move_ThenUndo_RestoresPosition()
    {
        var (slide, service, commands) = Create();
        var element = service.AddText(slide, "A", 10, 20);

        service.Move(element, 100, 200);
        Assert.Equal(100, element.X);
        Assert.Equal(200, element.Y);

        commands.Undo();
        Assert.Equal(10, element.X);
        Assert.Equal(20, element.Y);
    }

    [Fact]
    public void Resize_ThenUndo_RestoresBounds()
    {
        var (slide, service, commands) = Create();
        var element = service.AddShape(slide, ShapeType.Rectangle, 5, 5);

        service.Resize(element, 0, 0, 300, 150);
        Assert.Equal(300, element.Width);
        Assert.Equal(150, element.Height);

        commands.Undo();
        Assert.Equal(100, element.Width);
        Assert.Equal(50, element.Height);
        Assert.Equal(5, element.X);
    }

    [Fact]
    public void Rotate_Undo_Redo_Works()
    {
        var (slide, service, commands) = Create();
        var element = service.AddShape(slide, ShapeType.Arrow, 0, 0);

        service.Rotate(element, 45);
        Assert.Equal(45, element.Rotation);

        commands.Undo();
        Assert.Equal(0, element.Rotation);

        commands.Redo();
        Assert.Equal(45, element.Rotation);
    }

    [Fact]
    public void ChangeStyle_ThenUndo_RestoresStyle()
    {
        var (slide, service, commands) = Create();
        var element = service.AddShape(slide, ShapeType.Rectangle, 0, 0);
        var before = element.Style.FillColor;

        service.ChangeStyle(element, new ElementStyle { FillColor = "#FF0000" });
        Assert.Equal("#FF0000", element.Style.FillColor);

        commands.Undo();
        Assert.Equal(before, element.Style.FillColor);
    }

    [Fact]
    public void ChangeText_ThenUndo_RestoresText()
    {
        var (slide, service, commands) = Create();
        var element = service.AddText(slide, "Avant", 0, 0);

        service.ChangeText(element, "Apres");
        Assert.Equal("Apres", element.Text);

        commands.Undo();
        Assert.Equal("Avant", element.Text);
    }

    [Fact]
    public void BringToFront_SetsHighestZIndex_AndUndoRestores()
    {
        var (slide, service, commands) = Create();
        var a = service.AddShape(slide, ShapeType.Rectangle, 0, 0);
        var b = service.AddShape(slide, ShapeType.Ellipse, 0, 0);
        var c = service.AddShape(slide, ShapeType.Line, 0, 0);

        service.BringToFront(slide, a);
        Assert.True(a.ZIndex > b.ZIndex);
        Assert.True(a.ZIndex > c.ZIndex);

        commands.Undo();
        Assert.Equal(0, a.ZIndex);
    }
}
