using PresentationApp.Core.Models;

namespace PresentationApp.Core.Commands;

internal static class SlideOrder
{
    public static void Renumber(Presentation presentation)
    {
        for (var i = 0; i < presentation.Slides.Count; i++)
            presentation.Slides[i].Order = i;
    }
}

public class AddSlideCommand : IUndoableCommand
{
    private readonly Presentation _presentation;
    private readonly Slide _slide;
    private readonly int _index;

    public AddSlideCommand(Presentation presentation, Slide slide, int index)
    {
        _presentation = presentation;
        _slide = slide;
        _index = Math.Clamp(index, 0, presentation.Slides.Count);
    }

    public string Description => "Ajouter une diapositive";

    public void Execute()
    {
        _presentation.Slides.Insert(_index, _slide);
        SlideOrder.Renumber(_presentation);
    }

    public void Undo()
    {
        _presentation.Slides.Remove(_slide);
        SlideOrder.Renumber(_presentation);
    }
}

public class RemoveSlideCommand : IUndoableCommand
{
    private readonly Presentation _presentation;
    private readonly Slide _slide;
    private int _index = -1;

    public RemoveSlideCommand(Presentation presentation, Slide slide)
    {
        _presentation = presentation;
        _slide = slide;
    }

    public string Description => "Supprimer une diapositive";

    public void Execute()
    {
        _index = _presentation.Slides.IndexOf(_slide);
        if (_index < 0) return;
        _presentation.Slides.RemoveAt(_index);
        SlideOrder.Renumber(_presentation);
    }

    public void Undo()
    {
        if (_index < 0) return;
        _presentation.Slides.Insert(_index, _slide);
        SlideOrder.Renumber(_presentation);
    }
}

public class MoveSlideCommand : IUndoableCommand
{
    private readonly Presentation _presentation;
    private readonly int _fromIndex;
    private readonly int _toIndex;

    public MoveSlideCommand(Presentation presentation, int fromIndex, int toIndex)
    {
        _presentation = presentation;
        _fromIndex = fromIndex;
        _toIndex = toIndex;
    }

    public string Description => "Deplacer une diapositive";

    public void Execute() => Move(_fromIndex, _toIndex);

    public void Undo() => Move(_toIndex, _fromIndex);

    private void Move(int from, int to)
    {
        var slide = _presentation.Slides[from];
        _presentation.Slides.RemoveAt(from);
        _presentation.Slides.Insert(to, slide);
        SlideOrder.Renumber(_presentation);
    }
}
