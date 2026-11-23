using System.Text.Json;
using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;

namespace PresentationApp.Core.Services;

public class PresentationService
{
    public PresentationService(Presentation presentation, CommandManager commands)
    {
        Current = presentation;
        Commands = commands;
    }

    public Presentation Current { get; }
    public CommandManager Commands { get; }

    public Slide AddSlide(int? index = null)
    {
        var slide = new Slide();
        var position = index ?? Current.Slides.Count;
        Commands.Execute(new AddSlideCommand(Current, slide, position));
        return slide;
    }

    public void RemoveSlide(Slide slide)
    {
        if (!Current.Slides.Contains(slide)) return;
        Commands.Execute(new RemoveSlideCommand(Current, slide));
    }

    public Slide? DuplicateSlide(Slide slide)
    {
        var index = Current.Slides.IndexOf(slide);
        if (index < 0) return null;
        var copy = Clone(slide);
        Commands.Execute(new AddSlideCommand(Current, copy, index + 1));
        return copy;
    }

    public void MoveSlide(int fromIndex, int toIndex)
    {
        var count = Current.Slides.Count;
        if (fromIndex < 0 || fromIndex >= count) return;
        if (toIndex < 0 || toIndex >= count) return;
        if (fromIndex == toIndex) return;
        Commands.Execute(new MoveSlideCommand(Current, fromIndex, toIndex));
    }

    private static Slide Clone(Slide slide)
    {
        var json = JsonSerializer.Serialize(slide);
        var copy = JsonSerializer.Deserialize<Slide>(json)
            ?? throw new InvalidOperationException("Duplication impossible.");
        copy.Id = Guid.NewGuid();
        foreach (var element in copy.Elements)
            element.Id = Guid.NewGuid();
        return copy;
    }
}
