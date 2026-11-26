using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PresentationApp.UI.Converters;

// Charge l'image d'un ImageElement pour sa miniature.
// Si le fichier n'existe plus ou n'est pas une image, renvoie null (la miniature
// affiche alors un cadre gris) au lieu de faire planter l'application.
public class ImagePathConverter : IValueConverter
{
    // Une miniature est petite : inutile de décoder l'image en pleine résolution
    private const int LargeurMiniature = 320;

    public object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string chemin || !File.Exists(chemin))
            return null;

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;   // ne garde pas le fichier verrouillé
            image.DecodePixelWidth = LargeurMiniature;
            image.UriSource = new Uri(chemin, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception ex) when (ex is IOException || ex is NotSupportedException
                                   || ex is UnauthorizedAccessException || ex is UriFormatException)
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
