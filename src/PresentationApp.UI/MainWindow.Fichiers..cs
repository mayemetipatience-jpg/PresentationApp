
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using PresentationApp.Core.Models;
using PresentationApp.IO.Serialization;

namespace PresentationApp.UI;

public partial class MainWindow
{
    private string? _cheminFichierActuel;

    private bool _modificationsNonEnregistrees;

    private bool _chargementPresentation;

    private readonly DispatcherTimer _minuterieSauvegarde = new()
    {
        Interval = TimeSpan.FromMinutes(2)
    };

    // INITIALISATION DE LA SAUVEGARDE AUTOMATIQUE

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        slideCanvas.DiapositiveModifiee += Fichiers_DiapositiveModifiee;

        PreviewKeyDown += Fichiers_PreviewKeyDown;

        Closing += Fichiers_Closing;

        AddHandler(
            Button.ClickEvent,
            new RoutedEventHandler(Fichiers_BoutonClique),
            true);

        slideCanvas.PreviewMouseLeftButtonUp +=
            (_, _) => MarquerPresentationModifiee();

        _minuterieSauvegarde.Tick += MinuterieSauvegarde_Tick;
        _minuterieSauvegarde.Start();
    }

    // MENU FICHIER

    private void BoutonFichier_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button bouton)
            return;

        ContextMenu menu = new();

        menu.Items.Add(CreerCommandeMenu(
            "Nouveau",
            "Ctrl + N",
            Nouveau_Click));

        menu.Items.Add(CreerCommandeMenu(
            "Ouvrir",
            "Ctrl + O",
            Ouvrir_Click));

        menu.Items.Add(new Separator());

        menu.Items.Add(CreerCommandeMenu(
            "Enregistrer",
            "Ctrl + S",
            Enregistrer_Click));

        menu.Items.Add(CreerCommandeMenu(
            "Enregistrer sous",
            null,
            EnregistrerSous_Click));

        menu.PlacementTarget = bouton;
        menu.IsOpen = true;
    }

    private MenuItem CreerCommandeMenu(
        string titre,
        string? raccourci,
        RoutedEventHandler action)
    {
        MenuItem item = new()
        {
            Header = titre
        };

        if (!string.IsNullOrWhiteSpace(raccourci))
            item.InputGestureText = raccourci;

        item.Click += action;

        return item;
    }

    // NOUVELLE PRESENTATION

    private void Nouveau_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!ConfirmerAbandonModifications())
            return;

        _chargementPresentation = true;

        try
        {
            _presentation.Title = "Sans titre";
            _presentation.FormatVersion = FileFormat.CurrentVersion;

            _presentation.Slides = new()
            {
                CreerDiapositive(1)
            };

            _cheminFichierActuel = null;
            _elementActuel = null;
            _elementCopie = null;

            lstDiapositives.ItemsSource = null;
            lstDiapositives.ItemsSource = _presentation.Slides;
            lstDiapositives.SelectedIndex = 0;

            _modificationsNonEnregistrees = false;

            ActualiserTitreFenetre();
        }
        finally
        {
            _chargementPresentation = false;
        }

        if (_presentation.Slides.Count > 0)
        {
            lstDiapositives.SelectedIndex = -1;
            lstDiapositives.SelectedIndex = 0;
        }
    }

    // OUVERTURE D'UNE PRESENTATION

    private void Ouvrir_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBox.Show(
            "La commande Ouvrir a bien été déclenchée.",
            "Test de la commande",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        if (!ConfirmerAbandonModifications())
            return;

        OpenFileDialog dialogue = new()
        {
            Title = "Ouvrir une présentation",
            Filter = "Présentation Mini PowerPoint (*.json)|*.json|Tous les fichiers (*.*)|*.*",
            DefaultExt = ".json",
            CheckFileExists = true
        };

        if (dialogue.ShowDialog() != true)
            return;

        try
        {
            Presentation presentationChargee =
                ProjectFileService.Open(dialogue.FileName);

            if (presentationChargee.Slides == null ||
                presentationChargee.Slides.Count == 0)
            {
                MessageBox.Show(
                    "Cette présentation ne contient aucune diapositive.",
                    "Ouverture impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (presentationChargee.FormatVersion > FileFormat.CurrentVersion)
            {
                MessageBox.Show(
                    "Ce fichier utilise une version plus récente du format de présentation.",
                    "Version incompatible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _chargementPresentation = true;

            try
            {
                _presentation.Title = presentationChargee.Title;
                _presentation.FormatVersion = presentationChargee.FormatVersion;
                _presentation.Slides = presentationChargee.Slides;

                _cheminFichierActuel = dialogue.FileName;

                _elementActuel = null;
                _elementCopie = null;

                lstDiapositives.ItemsSource = null;
                lstDiapositives.ItemsSource = _presentation.Slides;
                lstDiapositives.SelectedIndex = 0;

                _modificationsNonEnregistrees = false;

                ActualiserTitreFenetre();
            }
            finally
            {
                _chargementPresentation = false;
            }

            lstDiapositives.SelectedIndex = -1;
            lstDiapositives.SelectedIndex = 0;

            MessageBox.Show(
                "La présentation a été ouverte avec succès.",
                "Ouverture",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Impossible d'ouvrir cette présentation.\n\n" + ex.Message,
                "Erreur d'ouverture",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // ENREGISTREMENT

    private void Enregistrer_Click(
        object sender,
        RoutedEventArgs e)
    {
        EnregistrerPresentation();
    }

    private void EnregistrerSous_Click(
        object sender,
        RoutedEventArgs e)
    {
        EnregistrerPresentationSous();
    }

    private bool EnregistrerPresentation()
    {
        if (string.IsNullOrWhiteSpace(_cheminFichierActuel))
            return EnregistrerPresentationSous();

        return SauvegarderDansFichier(_cheminFichierActuel);
    }

    private bool EnregistrerPresentationSous()
    {
        SaveFileDialog dialogue = new()
        {
            Title = "Enregistrer la présentation",
            Filter = "Présentation Mini PowerPoint (*.json)|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            FileName = _presentation.Title
        };

        if (dialogue.ShowDialog() != true)
            return false;

        if (!SauvegarderDansFichier(dialogue.FileName))
            return false;

        _cheminFichierActuel = dialogue.FileName;

        ActualiserTitreFenetre();

        return true;
    }

    private bool SauvegarderDansFichier(string chemin)
    {
        try
        {
            _presentation.FormatVersion = FileFormat.CurrentVersion;

            ProjectFileService.Save(_presentation, chemin);

            _cheminFichierActuel = chemin;

            _modificationsNonEnregistrees = false;

            ActualiserTitreFenetre();

            MessageBox.Show(
                "La présentation a été enregistrée avec succès.",
                "Enregistrement",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Impossible d'enregistrer la présentation.\n\n" + ex.Message,
                "Erreur d'enregistrement",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return false;
        }
    }

    // MODIFICATIONS NON ENREGISTREES

    private void MarquerPresentationModifiee()
    {
        if (_chargementPresentation)
            return;

        _modificationsNonEnregistrees = true;

        ActualiserTitreFenetre();
    }

    private void Fichiers_DiapositiveModifiee()
    {
        MarquerPresentationModifiee();
    }

    private void Fichiers_BoutonClique(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button bouton)
            return;

        string contenu = bouton.Content?.ToString() ?? "";

        if (contenu == "Fichier" ||
            contenu == "Partager" ||
            contenu == "Copier")
        {
            return;
        }

        MarquerPresentationModifiee();
    }

    private void ActualiserTitreFenetre()
    {
        string titre = string.IsNullOrWhiteSpace(_cheminFichierActuel)
            ? _presentation.Title
            : Path.GetFileNameWithoutExtension(_cheminFichierActuel);

        string marqueur = _modificationsNonEnregistrees ? " *" : "";

        Title = "Mini PowerPoint - " + titre + marqueur;
    }

    // CONFIRMATION AVANT DE PERDRE LES MODIFICATIONS

    private bool ConfirmerAbandonModifications()
    {
        if (!_modificationsNonEnregistrees)
            return true;

        MessageBoxResult resultat = MessageBox.Show(
            "Cette présentation contient des modifications non enregistrées.\n\nVoulez-vous les enregistrer avant de continuer ?",
            "Modifications non enregistrées",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (resultat == MessageBoxResult.Cancel)
            return false;

        if (resultat == MessageBoxResult.No)
            return true;

        return EnregistrerPresentation();
    }

    // SAUVEGARDE AUTOMATIQUE DE SECOURS

    private void MinuterieSauvegarde_Tick(
        object? sender,
        EventArgs e)
    {
        if (!_modificationsNonEnregistrees)
            return;

        if (string.IsNullOrWhiteSpace(_cheminFichierActuel))
            return;

        try
        {
            AutoSaveService.SaveBackup(
                _presentation,
                _cheminFichierActuel);
        }
        catch (Exception ex)
        {
            _minuterieSauvegarde.Stop();

            MessageBox.Show(
                "La sauvegarde automatique de secours a échoué.\n\n" +
                ex.Message,
                "Sauvegarde automatique",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            _minuterieSauvegarde.Start();
        }
    }

    // RACCOURCIS CLAVIER DES FICHIERS

    private void Fichiers_PreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            return;

        if (Keyboard.FocusedElement is TextBox)
            return;

        switch (e.Key)
        {
            case Key.N:
                Nouveau_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;

            case Key.O:
                Ouvrir_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;

            case Key.S:
                Enregistrer_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
        }
    }

    // FERMETURE DE L'APPLICATION

    private void Fichiers_Closing(
        object? sender,
        System.ComponentModel.CancelEventArgs e)
    {
        if (!ConfirmerAbandonModifications())
        {
            e.Cancel = true;
            return;
        }

        _minuterieSauvegarde.Stop();
    }
}
