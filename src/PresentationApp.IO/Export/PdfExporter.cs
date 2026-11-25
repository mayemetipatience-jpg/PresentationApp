using System.Linq;
using PresentationApp.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PresentationApp.IO.Export;

// Export PDF : une page par diapositive, dans l'ordre.
// Version simple : les éléments sont empilés, pas placés à leur position exacte.
public class PdfExporter : IExporter
{
    public PdfExporter()
    {
        // Licence gratuite pour un usage étudiant.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void Export(Presentation presentation, string outputPath)
    {
        Document.Create(container =>
        {
            foreach (Slide slide in presentation.Slides.OrderBy(s => s.Order))
            {
                container.Page(page =>
                {
                    page.Size(960, 540, Unit.Point);
                    page.Margin(40, Unit.Point);
                    page.PageColor(slide.BackgroundColor);

                    page.Content().Column(column =>
                    {
                        foreach (SlideElement element in slide.Elements)
                        {
                            if (element is TextElement texte)
                            {
                                column.Item().PaddingBottom(12)
                                    .Text(texte.Text)
                                    .FontSize(24)
                                    .FontColor(texte.Style.TextColor);
                            }
                        }
                    });
                });
            }
        }).GeneratePdf(outputPath);
    }
}