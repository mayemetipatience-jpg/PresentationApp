using System.IO;
using PresentationApp.Core.Models;
using PresentationApp.IO.Export;
using Xunit;

namespace PresentationApp.Tests;

public class PdfExporterTests
{
    [Fact]
    public void Le_PDF_est_cree_et_commence_par_PDF()
    {
        string chemin = Path.Combine(Path.GetTempPath(), "test-export.pdf");

        var presentation = new Presentation();
        var slide = new Slide { Order = 1 };
        slide.Elements.Add(new TextElement { Text = "Bonjour" });
        presentation.Slides.Add(slide);
        presentation.Slides.Add(new Slide { Order = 2 });

        new PdfExporter().Export(presentation, chemin);

        byte[] octets = File.ReadAllBytes(chemin);
        Assert.True(octets.Length > 0);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(octets, 0, 4));
        File.Delete(chemin);
    }
}