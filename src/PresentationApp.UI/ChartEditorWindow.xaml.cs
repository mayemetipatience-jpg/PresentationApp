
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PresentationApp.Core.Models;

namespace PresentationApp.UI;

public partial class ChartEditorWindow : Window
{
    private readonly ChartElement _graphique;

    public ChartEditorWindow(ChartElement graphique)
    {
        InitializeComponent();
        _graphique = graphique;
        ChargerDonnees();
    }

    private void ChargerDonnees()
    {
        txtTitre.Text = _graphique.Title;
        txtLegende.Text = _graphique.LegendTitle;
        txtAxeHorizontal.Text = _graphique.HorizontalAxisTitle;
        txtAxeVertical.Text = _graphique.VerticalAxisTitle;

        txtCategories.Text =
            string.Join(Environment.NewLine, _graphique.Categories);

        txtValeurs.Text =
            string.Join(Environment.NewLine,
                _graphique.Values.Select(v =>
                    v.ToString(CultureInfo.InvariantCulture)));

        chkLegende.IsChecked = _graphique.ShowLegend;
        chkValeurs.IsChecked = _graphique.ShowValues;
        chkQuadrillage.IsChecked = _graphique.ShowGridLines;

        foreach (ComboBoxItem item in cmbType.Items)
        {
            if (item.Tag?.ToString() == _graphique.ChartType.ToString())
            {
                cmbType.SelectedItem = item;
                break;
            }
        }

        if (cmbType.SelectedItem == null)
            cmbType.SelectedIndex = 0;
    }

    private void BtnAppliquer_Click(object sender, RoutedEventArgs e)
    {
        if (cmbType.SelectedItem is not ComboBoxItem typeItem ||
            !Enum.TryParse(typeItem.Tag?.ToString(), out ChartType type))
        {
            MessageBox.Show("Sélectionnez un type de graphique valide.");
            return;
        }

        var categories = txtCategories.Text
            .Split(new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .ToList();

        var lignes = txtValeurs.Text
            .Split(new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

        var valeurs = new System.Collections.Generic.List<double>();

        foreach (string ligne in lignes)
        {
            if (!double.TryParse(
                    ligne.Trim().Replace(',', '.'),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double valeur)
                || !double.IsFinite(valeur))
            {
                MessageBox.Show($"Valeur incorrecte : {ligne}");
                return;
            }

            valeurs.Add(valeur);
        }

        if (categories.Count == 0 || valeurs.Count == 0)
        {
            MessageBox.Show("Ajoutez au moins une catégorie et une valeur.");
            return;
        }

        if (categories.Count != valeurs.Count)
        {
            MessageBox.Show(
                "Le nombre de catégories doit être égal au nombre de valeurs.");
            return;
        }

        _graphique.ChartType = type;
        _graphique.Title = txtTitre.Text;
        _graphique.LegendTitle = txtLegende.Text;
        _graphique.HorizontalAxisTitle = txtAxeHorizontal.Text;
        _graphique.VerticalAxisTitle = txtAxeVertical.Text;
        _graphique.Categories = categories;
        _graphique.Values = valeurs;

        _graphique.ShowLegend = chkLegende.IsChecked == true;
        _graphique.ShowValues = chkValeurs.IsChecked == true;
        _graphique.ShowGridLines = chkQuadrillage.IsChecked == true;

        DialogResult = true;
        Close();
    }
}
