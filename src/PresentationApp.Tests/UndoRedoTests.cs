using PresentationApp.Core.Commands;
using PresentationApp.Core.Models;
using PresentationApp.Core.Services;
using Xunit;

namespace PresentationApp.Tests;

public class UndoRedoTests
{
    private static PresentationService CreateService() =>
        new(new Presentation(), new CommandManager());

    [Fact]
    public void AddSlide_ThenUndo_RemovesSlide()
    {
        var service = CreateService();
        service.AddSlide();
        Assert.Single(service.Current.Slides);

        service.Commands.Undo();
        Assert.Empty(service.Current.Slides);
    }

    [Fact]
    public void Undo_ThenRedo_RestoresSlide()
    {
        var service = CreateService();
        service.AddSlide();
        service.Commands.Undo();
        service.Commands.Redo();
        Assert.Single(service.Current.Slides);
    }

    [Fact]
    public void RemoveSlide_ThenUndo_RestoresAtSamePosition()
    {
        var service = CreateService();
        var a = service.AddSlide();
        var b = service.AddSlide();
        var c = service.AddSlide();

        service.RemoveSlide(b);
        Assert.Equal(2, service.Current.Slides.Count);
        Assert.Same(a, service.Current.Slides[0]);
        Assert.Same(c, service.Current.Slides[1]);

        service.Commands.Undo();
        Assert.Equal(3, service.Current.Slides.Count);
        Assert.Same(b, service.Current.Slides[1]);
    }

    [Fact]
    public void DuplicateSlide_CreatesIndependentCopy()
    {
        var service = CreateService();
        var original = service.AddSlide();
        original.Elements.Add(new TextElement { Text = "Bonjour" });

        var copy = service.DuplicateSlide(original);

        Assert.NotNull(copy);
        Assert.Equal(2, service.Current.Slides.Count);
        Assert.Same(copy, service.Current.Slides[1]);
        Assert.NotEqual(original.Id, copy!.Id);
        Assert.Single(copy.Elements);
        Assert.NotSame(original.Elements[0], copy.Elements[0]);
        Assert.NotEqual(original.Elements[0].Id, copy.Elements[0].Id);
    }

    [Fact]
    public void MoveSlide_Reorders_AndUndoRestores()
    {
        var service = CreateService();
        var a = service.AddSlide();
        var b = service.AddSlide();
        var c = service.AddSlide();

        service.MoveSlide(0, 2);
        Assert.Same(b, service.Current.Slides[0]);
        Assert.Same(c, service.Current.Slides[1]);
        Assert.Same(a, service.Current.Slides[2]);
        Assert.Equal(2, a.Order);

        service.Commands.Undo();
        Assert.Same(a, service.Current.Slides[0]);
        Assert.Equal(0, a.Order);
    }

    [Fact]
    public void NewAction_ClearsRedoStack()
    {
        var service = CreateService();
        service.AddSlide();
        service.Commands.Undo();
        Assert.True(service.Commands.CanRedo);

        service.AddSlide();
        Assert.False(service.Commands.CanRedo);
    }
}
