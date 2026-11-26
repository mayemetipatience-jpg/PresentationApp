using PresentationApp.Core.Models;

namespace PresentationApp.Core.Commands;

public class ChangeStyleCommand : IUndoableCommand
{
    private readonly SlideElement _element;
    private readonly ElementStyle _oldStyle;
    private readonly ElementStyle _newStyle;

    public ChangeStyleCommand(SlideElement element, ElementStyle newStyle)
    {
        _element = element;
        _oldStyle = Copy(element.Style);
        _newStyle = Copy(newStyle);
    }

    public string Description => "Changer le style";

    public void Execute() => _element.Style = Copy(_newStyle);

    public void Undo() => _element.Style = Copy(_oldStyle);

    public static ElementStyle Copy(ElementStyle s) => new()
    {
        FillColor = s.FillColor,
        StrokeColor = s.StrokeColor,
        StrokeThickness = s.StrokeThickness,
        TextColor = s.TextColor,
        FontFamily = s.FontFamily,
        FontSize = s.FontSize,
        IsBold = s.IsBold,
        IsItalic = s.IsItalic,
        IsUnderline = s.IsUnderline,
        TextAlignment = s.TextAlignment
    };
}

public class ChangeTextCommand : IUndoableCommand
{
    private readonly TextElement _element;
    private readonly string _oldText;
    private readonly string _newText;

    public ChangeTextCommand(TextElement element, string newText)
    {
        _element = element;
        _oldText = element.Text;
        _newText = newText;
    }

    public string Description => "Modifier le texte";

    public void Execute() => _element.Text = _newText;

    public void Undo() => _element.Text = _oldText;
}

public class ReorderElementCommand : IUndoableCommand
{
    private readonly SlideElement _element;
    private readonly int _oldZIndex;
    private readonly int _newZIndex;

    public ReorderElementCommand(Slide slide, SlideElement element, bool toFront)
    {
        _element = element;
        _oldZIndex = element.ZIndex;

        var others = slide.Elements.Where(e => e != element).ToList();
        if (others.Count == 0)
            _newZIndex = _oldZIndex;
        else if (toFront)
            _newZIndex = others.Max(e => e.ZIndex) + 1;
        else
            _newZIndex = others.Min(e => e.ZIndex) - 1;
    }

    public string Description => "Changer l'ordre des calques";

    public void Execute() => _element.ZIndex = _newZIndex;

    public void Undo() => _element.ZIndex = _oldZIndex;
}
