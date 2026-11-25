using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PresentationApp.Core.Models;
using PresentationApp.IO.Export;
using PresentationApp.UI.Canvas;

namespace PresentationApp.UI.Export;

// Export PNG : une image par diapositive, dessinée par le canevas.
// À appeler depuis le thread de l'interface, par exemple depuis un clic de bouton.
public class PngExporter : IExporter
{
    private const int Largeur = 960;
    private const int Hauteur = 540;

    public void Export(Presentation presentation, string dossier)
    {
        Directory.CreateDirectory(dossier);

        int numero = 1;
        foreach (Slide slide in presentation.Slides.OrderBy(s => s.Order))
        {
            string chemin = Path.Combine(dossier, ExportFileNames.SlidePng(numero));
            EnregistrerDiapositive(slide, chemin);
            numero++;
        }
    }

    private static void EnregistrerDiapositive(Slide slide, string chemin)
    {
        var canevas = new SlideCanvas { Width = Largeur, Height = Hauteur };
        canevas.DisplaySlide(slide);

        // Calcule la mise en page avant de dessiner.
        canevas.Measure(new Size(Largeur, Hauteur));
        canevas.Arrange(new Rect(0, 0, Largeur, Hauteur));
        canevas.UpdateLayout();

        var image = new RenderTargetBitmap(Largeur, Hauteur, 96, 96, PixelFormats.Pbgra32);
        image.Render(canevas);

        var encodeur = new PngBitmapEncoder();
        encodeur.Frames.Add(BitmapFrame.Create(image));

        using var fichier = File.Create(chemin);
        encodeur.Save(fichier);
    }
}