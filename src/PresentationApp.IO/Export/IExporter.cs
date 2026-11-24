using PresentationApp.Core.Models;

namespace PresentationApp.IO.Export;

// Contrat commun pour les exports (PDF, PNG).
// Pour un PDF, outputPath est le nom du fichier.
// Pour les PNG, outputPath est un dossier : une image par diapositive.
public interface IExporter
{
    void Export(Presentation presentation, string outputPath);
}