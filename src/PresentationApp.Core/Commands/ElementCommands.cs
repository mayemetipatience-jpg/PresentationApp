using PresentationApp.Core.Models;

namespace PresentationApp.Core.Commands;

public class AddElementCommand : IUndoableCommand
{
    private readonly Slide _slide;
    private readonly SlideElement _element;

    public AddElementCommand(Slide slide, SlideElement element)
    {
        _slide = slide;
        _element = element;
    }

    public string Description => "Ajouter un element";

    public void Execute()
    {
        _element.ZIndex = _slide.Elements.Count == 0 ? 0 : _slide.Elements.Max(e => e.ZIndex) + 1;
        _slide.Elements.Add(_element);
    }

    public void Undo() => _slide.Elements.Remove(_element);
}

public class RemoveElementCommand : IUndoableCommand
{
    private readonly Slide _slide;
    private readonly SlideElement _element;
    private int _index = -1;

    public RemoveElementCommand(Slide slide, SlideElement element)
    {
        _slide = slide;
        _element = element;
    }

    public string Description => "Supprimer un element";

    public void Execute()
    {
        _index = _slide.Elements.IndexOf(_element);
        if (_index < 0) return;
        _slide.Elements.RemoveAt(_index);
    }

    public void Undo()
    {
        if (_index < 0) return;
        _slide.Elements.Insert(_index, _element);
    }
}
