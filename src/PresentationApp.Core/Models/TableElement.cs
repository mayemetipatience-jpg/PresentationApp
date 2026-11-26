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
}