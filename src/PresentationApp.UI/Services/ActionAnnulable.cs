using PresentationApp.Core.Commands;

namespace PresentationApp.UI.Services;

// Commande annulable "sur mesure" : on lui donne ce qu'il faut faire et comment le défaire.
// Elle permet de passer par l'historique (Ctrl+Z / Ctrl+Y) pour des actions qui n'ont pas
// de commande dans le Core : couleur de fond, thème, lignes / colonnes d'un tableau.
public class ActionAnnulable : IUndoableCommand
{
    private readonly Action _faire;
    private readonly Action _defaire;

    public ActionAnnulable(string description, Action faire, Action defaire)
    {
        Description = description;
        _faire = faire;
        _defaire = defaire;
    }

    public string Description { get; }

    public void Execute() => _faire();

    public void Undo() => _defaire();
}
