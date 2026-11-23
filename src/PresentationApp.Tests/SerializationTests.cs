using System.IO;
using PresentationApp.Core.Models;
using PresentationApp.IO.Serialization;
using Xunit;

namespace PresentationApp.Tests;

public class SerializationTests
{
    [Fact]
    public void Aller_retour_redonne_la_meme_presentation()
    {
        var original = new Presentation { Title = "Mon projet" };
        original.Slides.Add(new Slide { Order = 1, BackgroundColor = "#123456" });
        original.Slides.Add(new Slide { Order = 2, BackgroundColor = "#ABCDEF" });

        string json = ProjectSerializer.ToJson(original);
        Presentation relue = ProjectSerializer.FromJson(json);

        Assert.Equal(original.Title, relue.Title);
        Assert.Equal(original.FormatVersion, relue.FormatVersion);
        Assert.Equal(2, relue.Slides.Count);
        Assert.Equal(1, relue.Slides[0].Order);
        Assert.Equal("#123456", relue.Slides[0].BackgroundColor);
        Assert.Equal(2, relue.Slides[1].Order);
        Assert.Equal("#ABCDEF", relue.Slides[1].BackgroundColor);
    }

    [Fact]
    public void Fichier_sur_disque_se_relit_correctement()
    {
        string chemin = Path.Combine(Path.GetTempPath(), "test-presentation.json");

        ProjectFileService.Save(new Presentation { Title = "Fichier" }, chemin);
        Presentation relue = ProjectFileService.Open(chemin);

        Assert.Equal("Fichier", relue.Title);
        File.Delete(chemin);
    }

    [Fact]
    public void JSON_invalide_leve_CorruptFileException()
    {
        Assert.Throws<CorruptFileException>(() => ProjectSerializer.FromJson("ceci n'est pas du json {"));
    }

    [Fact]
    public void Fichier_corrompu_sur_disque_leve_CorruptFileException()
    {
        string chemin = Path.Combine(Path.GetTempPath(), "test-corrompu.json");
        File.WriteAllText(chemin, "{ fichier cassé");

        Assert.Throws<CorruptFileException>(() => ProjectFileService.Open(chemin));
        File.Delete(chemin);
    }
}