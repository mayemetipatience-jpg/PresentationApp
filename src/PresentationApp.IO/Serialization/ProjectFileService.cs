using System.IO;
using PresentationApp.Core.Models;

namespace PresentationApp.IO.Serialization;

// Enregistre et ouvre les fichiers de présentation sur le disque.
public static class ProjectFileService
{
    // Enregistre la présentation dans le fichier indiqué.
    public static void Save(Presentation presentation, string path)
    {
        string json = ProjectSerializer.ToJson(presentation);
        File.WriteAllText(path, json);
    }

    // Ouvre une présentation depuis le fichier indiqué.
    public static Presentation Open(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Fichier introuvable.", path);
        }

        string json = File.ReadAllText(path);
        return ProjectSerializer.FromJson(json);
    }
}