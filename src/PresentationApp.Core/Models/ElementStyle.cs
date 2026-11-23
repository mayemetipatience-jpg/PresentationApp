
namespace PresentationApp.Core.Models;

public class ElementStyle
{
    public string FillColor { get; set; } = "#FFFFFF";
    public string StrokeColor { get; set; } = "#000000";
    public double StrokeThickness { get; set; } = 1;
    public string TextColor { get; set; } = "#000000";
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 18;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
}