# PresentationApp

Logiciel de présentation type PowerPoint en **C# / .NET 8 / WPF** (MVVM), réalisé par une équipe de 5 étudiants.

## Fonctionnalités
- Diapositives : ajout, suppression, duplication, réordonnancement, miniatures
- Éléments : texte, formes, images (déplacer, redimensionner, pivoter)
- Mise en forme : police, couleurs, contour, fond
- Undo / Redo, grille et aimantation, copier/coller
- Sauvegarde JSON, export PDF et PNG, thèmes et modèles
- **Diaporama plein écran** avec transitions (fondu, glissement, zoom), mode présentateur et pointeur laser
- **Assistant de structure** : un plan texte devient des diapositives

## Captures d'écran
| Éditeur | Diaporama | Mode présentateur |
|---|---|---|
| ![Éditeur](docs/img/editeur.png) | ![Diaporama](docs/img/diaporama.png) | ![Présentateur](docs/img/presentateur.png) |

## Prérequis
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (charge de travail « Développement .NET Desktop ») ou Rider

## Installation et lancement
```bash
git clone https://github.com/VOTRE-PSEUDO/PresentationApp.git
cd PresentationApp
dotnet build
dotnet run --project src/PresentationApp.UI
```

## Tests
```bash
dotnet test
```

## Raccourcis
| Touche | Action |
|---|---|
| ← / → / Espace / clic | Diapositive précédente / suivante |
| Début / Fin | Première / dernière diapositive |
| L | Pointeur laser |
| P | Mode présentateur (minuterie, aperçu suivante) |
| Échap | Quitter le diaporama |
| Ctrl+Z / Ctrl+Y | Annuler / Rétablir |

## Architecture
Voir [docs/CONVENTIONS.md](docs/CONVENTIONS.md). Quatre projets : `Core` (modèle, sans WPF), `IO` (JSON, export), `UI` (WPF), `Tests` (xUnit).

## Équipe
| Étudiant | Module |
|---|---|
| E1 | Modèle, Undo/Redo, MainViewModel |
| E2 | Éditeur de canevas |
| E3 | Interface, panneaux, mise en forme |
| E4 | Sauvegarde, export, thèmes |
| E5 | Diaporama, transitions, CI, documentation |

## Intégration continue
Chaque Pull Request déclenche GitHub Actions (`.github/workflows/build.yml`) : compilation + tests.
