using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace PresentationApp.UI.Converters;

// Donne le numéro d'une diapositive d'après sa place dans la liste (1, 2, 3...).
// On n'utilise pas Slide.Order : les commandes le renumérotent à partir de 0.
//   values[0] = la diapositive, values[1] = la ListBox qui l'affiche
public class SlideNumberConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is not ItemsControl liste)
            return string.Empty;

        int index = liste.Items.IndexOf(values[0]);
        return index < 0 ? string.Empty : (index + 1).ToString(culture);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
