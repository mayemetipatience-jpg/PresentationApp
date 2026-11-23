using System;

namespace PresentationApp.IO.Serialization;

// Erreur levée quand un fichier de présentation est illisible ou corrompu.
public class CorruptFileException : Exception
{
    public CorruptFileException(string message) : base(message)
    {
    }
}