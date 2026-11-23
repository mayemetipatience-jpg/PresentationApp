using System.Text.Json;
using PresentationApp.Core.Models;

namespace PresentationApp.IO.Serialization;

// Transforme une présentation en texte JSON, et inversement.
public static class ProjectSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    // Présentation -> texte JSON
    public static string ToJson(Presentation presentation)
    {
        return JsonSerializer.Serialize(presentation, Options);
    }

    // Texte JSON -> présentation
    // Si le texte n'est pas un JSON valide, on lève CorruptFileException.
    public static Presentation FromJson(string json)
    {
        try
        {
            var presentation = JsonSerializer.Deserialize<Presentation>(json, Options);

            if (presentation is null)
            {
                throw new CorruptFileException("Le fichier est vide.");
            }

            return presentation;
        }
        catch (JsonException ex)
        {
            throw new CorruptFileException("Le fichier n'est pas un JSON valide : " + ex.Message);
        }
    }
}