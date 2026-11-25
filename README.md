# PresentationApp

Application de présentation de type PowerPoint, développée en C# (.NET 10, WPF) par une équipe de 5 étudiants.

## Fonctionnalités

- Diapositives : ajouter, supprimer, dupliquer, réordonner
- Éléments : textes, formes, images (position, taille, rotation, couleurs, polices)
- Annuler / Refaire (Undo/Redo) sur toutes les actions d'édition
- Sauvegarde et ouverture de présentations au format JSON, sauvegarde de secours
- Thèmes et modèles de diapositives
- Export PDF et images, mode présentation plein écran avec transitions (en cours)

## Technologies

- C# 12, .NET 10
- WPF (interface) avec le motif MVVM
- System.Text.Json (sauvegarde)
- xUnit (tests)
- Git et GitHub (branches, Pull Requests)

## Structure du projet

    src/
      PresentationApp.Core/    Modèle de données, Undo/Redo, services (sans WPF)
      PresentationApp.IO/      Sauvegarde, ouverture, export
      PresentationApp.UI/      Interface WPF (fenêtres, canevas, mode présentation)
      PresentationApp.Tests/   Tests automatiques
    docs/                      Documentation

## Installation

Prérequis : Visual Studio avec la charge de travail « Développement .NET Desktop » et le SDK .NET 10 ou plus récent.

    git clone https://github.com/mayemetipatience-jpg/PresentationApp.git
    cd PresentationApp
    dotnet build
    dotnet test
    dotnet run --project src/PresentationApp.UI

## L'équipe

- Étudiant 1 : [nom], cheffe de projet, modèle de données, Undo/Redo, MainViewModel
- Étudiant 2 : [nom], éditeur de canevas
- Étudiant 3 : [nom], interface principale et panneaux
- Étudiant 4 : [nom], sauvegarde, export, thèmes et modèles
- Étudiant 5 : [nom], mode présentation, transitions, documentation

## Travailler sur le projet

- Ne jamais coder directement sur `main`.
- Une branche par fonctionnalité (par exemple `feat/canvas-selection`).
- Une Pull Request relue par un autre membre avant la fusion.
- `dotnet build` et `dotnet test` doivent rester verts.
