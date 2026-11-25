using System;
using System.IO;
using System.Windows.Threading;
using PresentationApp.Core.Models;
using PresentationApp.IO.Serialization;

namespace PresentationApp.UI.Services;

// Sauvegarde de secours toutes les 2 minutes, seulement si la présentation a déjà un fichier.
public class AutoSaveTimer
{
    private readonly DispatcherTimer _timer;
    private readonly Func<Presentation> _presentation;
    private readonly Func<string?> _cheminFichier;

    public AutoSaveTimer(Func<Presentation> presentation, Func<string?> cheminFichier)
    {
        _presentation = presentation;
        _cheminFichier = cheminFichier;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
        _timer.Tick += Timer_Tick;
    }

    public void Demarrer()
    {
        _timer.Start();
    }

    public void Arreter()
    {
        _timer.Stop();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        string? chemin = _cheminFichier();
        if (string.IsNullOrEmpty(chemin))
        {
            // Pas encore de fichier enregistré : rien à sauvegarder.
            return;
        }

        try
        {
            AutoSaveService.SaveBackup(_presentation(), chemin);
        }
        catch (IOException)
        {
            // Une sauvegarde de secours qui échoue ne doit pas bloquer l'utilisateur.
        }
    }
}