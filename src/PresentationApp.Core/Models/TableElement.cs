
using System.Collections.Generic;
using System.Linq;

namespace PresentationApp.Core.Models;

public class TableElement : SlideElement
{
    public int Rows { get; set; } = 3;

    public int Columns { get; set; } = 3;

    public List<List<string>> Cells { get; set; } = new();

    public string CellBackgroundColor { get; set; } = "#FFFFFF";

    public string HeaderBackgroundColor { get; set; } = "#DDEBF7";

    public string BorderColor { get; set; } = "#808080";

    public double BorderThickness { get; set; } = 1;

    public double CellFontSize { get; set; } = 16;

    public TableElement()
    {
        InitialiserCellules();
    }

    public void InitialiserCellules()
    {
        Rows = System.Math.Max(1, Rows);
        Columns = System.Math.Max(1, Columns);

        while (Cells.Count < Rows)
        {
            Cells.Add(
                Enumerable.Range(0, Columns)
                    .Select(_ => string.Empty)
                    .ToList());
        }

        while (Cells.Count > Rows)
            Cells.RemoveAt(Cells.Count - 1);

        foreach (List<string> ligne in Cells)
        {
            while (ligne.Count < Columns)
                ligne.Add(string.Empty);

            while (ligne.Count > Columns)
                ligne.RemoveAt(ligne.Count - 1);
        }
    }

    // Ajouter une ligne
    public void AjouterLigne(int index = -1)
    {
        InitialiserCellules();

        if (index < 0 || index > Rows)
            index = Rows;

        List<string> nouvelleLigne =
            Enumerable.Range(0, Columns)
                .Select(_ => string.Empty)
                .ToList();

        Cells.Insert(index, nouvelleLigne);
        Rows++;
    }

    // Supprimer une ligne
    public void SupprimerLigne(int index = -1)
    {
        InitialiserCellules();

        if (Rows <= 1)
            return;

        if (index < 0 || index >= Rows)
            index = Rows - 1;

        Cells.RemoveAt(index);
        Rows--;
    }

    // Ajouter une colonne
    public void AjouterColonne(int index = -1)
    {
        InitialiserCellules();

        if (index < 0 || index > Columns)
            index = Columns;

        foreach (List<string> ligne in Cells)
            ligne.Insert(index, string.Empty);

        Columns++;
    }

    // Supprimer une colonne
    public void SupprimerColonne(int index = -1)
    {
        InitialiserCellules();

        if (Columns <= 1)
            return;

        if (index < 0 || index >= Columns)
            index = Columns - 1;

        foreach (List<string> ligne in Cells)
            ligne.RemoveAt(index);

        Columns--;
    }
}
