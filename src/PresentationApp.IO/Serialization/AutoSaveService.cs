
using System;
using System.IO;
using PresentationApp.Core.Models;

namespace PresentationApp.IO.Serialization;

// Gestion des sauvegardes automatiques de secours.
public static class AutoSaveService
{
    // Dossier dédié aux sauvegardes automatiques.
    private static string DossierSauvegarde =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MiniPowerPoint",
            "AutoSave");

    // Chemin de la sauvegarde d'une présentation.
    public static string GetBackupPath(string? originalPath)
    {
        string nom;

        if (string.IsNullOrWhiteSpace(originalPath))
        {
            nom = "presentation-sans-titre";
        }
        else
        {
            nom = Path.GetFileNameWithoutExtension(originalPath);
        }

        string dossier = Path.Combine(
            DossierSauvegarde,
            nom);

        Directory.CreateDirectory(dossier);

        return Path.Combine(dossier, "autosave.json");
    }

    // Enregistre une copie de secours.
    public static string SaveBackup(
        Presentation presentation,
        string? originalPath = null)
    {
        string chemin = GetBackupPath(originalPath);

        ProjectFileService.Save(presentation, chemin);

        return chemin;
    }

    // Vérifie si une sauvegarde existe.
    public static bool BackupExists(string? originalPath = null)
    {
        string chemin = GetBackupPath(originalPath);

        return File.Exists(chemin);
    }

    // Récupère une présentation depuis sa sauvegarde.
    public static Presentation LoadBackup(string? originalPath = null)
    {
        string chemin = GetBackupPath(originalPath);

        if (!File.Exists(chemin))
        {
            throw new FileNotFoundException(
                "Aucune sauvegarde automatique n'a été trouvée.",
                chemin);
        }

        return ProjectFileService.Open(chemin);
    }

    // Supprime la sauvegarde après un enregistrement normal.
    public static void DeleteBackup(string? originalPath = null)
    {
        string chemin = GetBackupPath(originalPath);

        if (File.Exists(chemin))
            File.Delete(chemin);
    }
}
