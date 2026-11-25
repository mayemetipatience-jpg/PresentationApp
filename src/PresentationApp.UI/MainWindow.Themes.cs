
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PresentationApp.Core.Models;
using PresentationApp.UI.Themes;

namespace PresentationApp.UI;

public partial class MainWindow
{
    private void CmbThemes_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (cmbThemes.SelectedItem is not ComboBoxItem item)
            return;

        string? nomTheme = item.Content?.ToString();

        if (string.IsNullOrWhiteSpace(nomTheme))
            return;

        AppliquerTheme(nomTheme);
    }

    private void AppliquerTheme(string nomTheme)
    {
        PresentationTheme? nouveauTheme = PresentationTheme
            .GetThemes()
            .FirstOrDefault(t => t.Name == nomTheme);

        if (nouveauTheme == null)
        {
            MessageBox.Show(
                "Le thème sélectionné est introuvable.",
                "Thèmes",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        PresentationTheme? ancienTheme = PresentationTheme
            .GetThemes()
            .FirstOrDefault(t => t.Name == _presentation.ThemeName);

        foreach (Slide slide in _presentation.Slides)
        {
            // Ne remplacer que les fonds correspondant
            // à l'ancien thème. Les fonds personnalisés restent intacts.
            if (ancienTheme != null &&
                string.Equals(
                    slide.BackgroundColor,
                    ancienTheme.BackgroundColor,
                    StringComparison.OrdinalIgnoreCase))
            {
                slide.BackgroundColor = nouveauTheme.BackgroundColor;
            }

            foreach (SlideElement element in slide.Elements)
            {
                if (element is not TextElement texte)
                    continue;

                if (texte.Text == "Cliquez pour ajouter un titre")
                {
                    texte.Style.FontFamily = nouveauTheme.FontFamily;
                    texte.Style.TextColor = nouveauTheme.TitleColor;
                    texte.Style.FontSize = 32;
                    texte.Style.IsBold = true;
                }
                else if (texte.Text == "Cliquez pour ajouter un sous-titre")
                {
                    texte.Style.FontFamily = nouveauTheme.FontFamily;
                    texte.Style.TextColor = nouveauTheme.TextColor;
                    texte.Style.FontSize = 20;
                }
            }
        }

        _presentation.ThemeName = nouveauTheme.Name;

        if (lstDiapositives.SelectedItem is Slide diapositive)
        {
            slideCanvas.DisplaySlide(diapositive);
        }

        lstDiapositives.Items.Refresh();
        ActualiserListeCalques();
    }
}
