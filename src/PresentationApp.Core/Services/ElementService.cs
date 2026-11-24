using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;

namespace PresentationApp.Core.Services;

public class ElementService
{
    private readonly CommandManager _commands;

    public ElementService(CommandManager commands)
    {
        _commands = commands;
    }

    public TextElement AddText(Slide slide, string text, double x, double y)
    {
        var element = new TextElement { Text = text, X = x, Y = y };
        _commands.Execute(new AddElementCommand(slide, element));
        return element;
    }

    public ShapeElement AddShape(Slide slide, ShapeType shapeType, double x, double y)
    {
        var element = new ShapeElement { ShapeType = shapeType, X = x, Y = y };
        _commands.Execute(new AddElementCommand(slide, element));
        return element;
    }

    public ImageElement AddImage(Slide slide, string imagePath, double x, double y)
    {
        var element = new ImageElement { ImagePath = imagePath, X = x, Y = y };
        _commands.Execute(new AddElementCommand(slide, element));
        return element;
    }

    public void Remove(Slide slide, SlideElement element)
    {
        if (!slide.Elements.Contains(element)) return;
        _commands.Execute(new RemoveElementCommand(slide, element));
    }

    public void Move(SlideElement element, double x, double y)
    {
        if (element.X == x && element.Y == y) return;
        _commands.Execute(new MoveElementCommand(element, x, y));
    }

    public void Resize(SlideElement element, double x, double y, double width, double height)
        => _commands.Execute(new ResizeElementCommand(element, x, y, width, height));

    public void Rotate(SlideElement element, double rotation)
        => _commands.Execute(new RotateElementCommand(element, rotation));

    public void ChangeStyle(SlideElement element, ElementStyle newStyle)
        => _commands.Execute(new ChangeStyleCommand(element, newStyle));

    public void ChangeText(TextElement element, string newText)
        => _commands.Execute(new ChangeTextCommand(element, newText));

    public void BringToFront(Slide slide, SlideElement element)
        => _commands.Execute(new ReorderElementCommand(slide, element, true));

    public void SendToBack(Slide slide, SlideElement element)
        => _commands.Execute(new ReorderElementCommand(slide, element, false));
}
