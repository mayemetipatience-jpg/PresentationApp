
using System.Collections.Generic;

namespace PresentationApp.Core.Models;

public enum ChartType
{
    Column = 0,
    Bar = 1,
    Line = 2,
    Pie = 3,
    Doughnut = 4
}

public class ChartElement : SlideElement
{
    public ChartType ChartType { get; set; } = ChartType.Column;

    public string Title { get; set; } = "Titre du graphique";

    public string HorizontalAxisTitle { get; set; } = "Catégories";

    public string VerticalAxisTitle { get; set; } = "Valeurs";

    public List<string> Categories { get; set; } = new()
    {
        "Janvier",
        "Février",
        "Mars",
        "Avril"
    };

    public List<double> Values { get; set; } = new()
    {
        10,
        20,
        15,
        30
    };

    public List<string> Colors { get; set; } = new()
    {
        "#4472C4",
        "#ED7D31",
        "#A5A5A5",
        "#70AD47"
    };

    public bool ShowLegend { get; set; } = true;

    public bool ShowValues { get; set; } = true;

    public bool ShowGridLines { get; set; } = true;

    public string LegendTitle { get; set; } = "Série 1";
}
