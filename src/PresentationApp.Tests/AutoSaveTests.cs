using System.IO;
using PresentationApp.Core.Models;
using PresentationApp.IO.Serialization;
using Xunit;

namespace PresentationApp.Tests;

public class AutoSaveTests
{
    [Fact]
    public void Le_chemin_de_secours_ajoute_autosave()
    {
        string chemin = AutoSaveService.GetBackupPath(Path.Combine("dossier", "ma-presentation.json"));

        string attendu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniPowerPoint", "AutoSave", "ma-presentation", "autosave.json");
        Assert.Equal(attendu, chemin);
    }

    [Fact]
    public void La_sauvegarde_de_secours_ne_touche_pas_au_fichier_original()
    {
        string original = Path.Combine(Path.GetTempPath(), "test-original.json");
        File.WriteAllText(original, "contenu d'origine");

        string backup = AutoSaveService.SaveBackup(new Presentation { Title = "Secours" }, original);

        Assert.Equal("contenu d'origine", File.ReadAllText(original));
        Assert.True(File.Exists(backup));

        File.Delete(original);
        File.Delete(backup);
    }
}