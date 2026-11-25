namespace PresentationApp.IO.Export;

// Noms des fichiers PNG : une image par diapositive, numérotées sur 3 chiffres.
public static class ExportFileNames
{
    public static string SlidePng(int numero)
    {
        return $"diapositive-{numero:000}.png";
    }
}