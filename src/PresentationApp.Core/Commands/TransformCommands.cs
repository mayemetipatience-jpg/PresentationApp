using PresentationApp.Core.Models;

namespace PresentationApp.Core.Commands;

public class MoveElementCommand : IUndoableCommand
{
    private readonly SlideElement _element;
    private readonly double _oldX, _oldY, _newX, _newY;

    public MoveElementCommand(SlideElement element, double newX, double newY)
    {
        _element = element;
        _oldX = element.X;
        _oldY = element.Y;
        _newX = newX;
        _newY = newY;
    }

    public string Description => "Deplacer un element";

    public void Execute()
    {
        _element.X = _newX;
        _element.Y = _newY;
    }

    public void Undo()
    {
        _element.X = _oldX;
        _element.Y = _oldY;
    }
}

public class ResizeElementCommand : IUndoableCommand
{
    private readonly SlideElement _element;
    private readonly double _oldX, _oldY, _oldWidth, _oldHeight;
    private readonly double _newX, _newY, _newWidth, _newHeight;

    public ResizeElementCommand(SlideElement element, double newX, double newY, double newWidth, double newHeight)
    {
        _element = element;
        _oldX = element.X;
        _oldY = element.Y;
        _oldWidth = element.Width;
        _oldHeight = element.Height;
        _newX = newX;
        _newY = newY;
        _newWidth = Math.Max(1, newWidth);
        _newHeight = Math.Max(1, newHeight);
    }

    public string Description => "Redimensionner un element";

    public void Execute()
    {
        _element.X = _newX;
        _element.Y = _newY;
        _element.Width = _newWidth;
        _element.Height = _newHeight;
    }

    public void Undo()
    {
        _element.X = _oldX;
        _element.Y = _oldY;
        _element.Width = _oldWidth;
        _element.Height = _oldHeight;
    }
}

public class RotateElementCommand : IUndoableCommand
{
    private readonly SlideElement _element;
    private readonly double _oldRotation;
    private readonly double _newRotation;

    public RotateElementCommand(SlideElement element, double newRotation)
    {
        _element = element;
        _oldRotation = element.Rotation;
        _newRotation = newRotation;
    }

    public string Description => "Pivoter un element";

    public void Execute() => _element.Rotation = _newRotation;

    public void Undo() => _element.Rotation = _oldRotation;
}
