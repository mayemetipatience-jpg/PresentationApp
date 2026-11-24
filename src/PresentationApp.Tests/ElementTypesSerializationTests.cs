using PresentationApp.Core.Models;
using PresentationApp.IO.Serialization;
using Xunit;

namespace PresentationApp.Tests;

public class ElementTypesSerializationTests
{
    [Fact]
    public void Les_types_d_elements_sont_retrouves_apres_relecture()
    {
        var original = new Presentation();
        var slide = new Slide { Order = 1 };
        slide.Elements.Add(new TextElement { Text = "Bonjour", X = 10, Y = 20 });
        slide.Elements.Add(new ImageElement { ImagePath = "photo.png", X = 30, Y = 40 });
        original.Slides.Add(slide);

        string json = ProjectSerializer.ToJson(original);
        Presentation relue = ProjectSerializer.FromJson(json);

        var elements = relue.Slides[0].Elements;
        Assert.Equal(2, elements.Count);
        Assert.IsType<TextElement>(elements[0]);
        Assert.IsType<ImageElement>(elements[1]);
        Assert.Equal("Bonjour", ((TextElement)elements[0]).Text);
        Assert.Equal("photo.png", ((ImageElement)elements[1]).ImagePath);
    }
}