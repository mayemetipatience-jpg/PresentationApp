using System.IO;
using PresentationApp.Core.Models;

namespace PresentationApp.IO.Serialization;

// Sauvegarde de secours : écrit une copie à côté du fichier de l'utilisateur,
// sans jamais écraser le fichier original.
public static class AutoSaveService
{
    // Exemple : "ma-presentation.json" devient "ma-presentation.autosave.json"
    public static string GetBackupPath(string originalPath)
    {
        string dossier = Path.GetDirectoryName(originalPath) ?? string.Empty;
        string nom = Path.GetFileNameWithoutExtension(originalPath);
        return Path.Combine(dossier, nom + ".autosave.json");
    }

    // Écrit la sauvegarde de secours et renvoie son chemin.
    public static string SaveBackup(Presentation presentation, string originalPath)
    {
        string chemin = GetBackupPath(originalPath);
        ProjectFileService.Save(presentation, chemin);
        return chemin;
    }
}