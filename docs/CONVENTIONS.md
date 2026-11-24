# Conventions du projet

## Architecture
- `PresentationApp.Core` : modèle et logique, **aucune référence à WPF**.
- `PresentationApp.IO` : sérialisation JSON, export PDF/PNG.
- `PresentationApp.UI` : WPF + MVVM (CommunityToolkit.Mvvm).
- `PresentationApp.Tests` : xUnit.

## Règles anti-conflits
- Un fichier = un propriétaire (voir le cahier de répartition).
- Prévenir l'équipe avant de toucher à un fichier partagé (.csproj, .sln, modèle du Core).
- MainWindow.xaml assemble seulement les panneaux (UserControls).

## Git
- Jamais de code directement sur `main`.
- Une branche par fonctionnalité : `feat/...`, `fix/...`, `docs/...`.
- Commits : `type(portée): message` (ex. `feat(presentation): transitions`).
- Pull Request : base `main`, description courte, au moins un relecteur, puis « Squash and merge ».
- Ne jamais commiter `bin/` et `obj/`.

## Code
- C# 12, nullable activé, noms en PascalCase, champs privés en `_camelCase`.
- ViewModels : `[ObservableProperty]` et `[RelayCommand]`.
- Toute modification du document passe par une commande Undo/Redo.

## Dépendances attendues par le mode présentation (E5)
- `Slide.Transition` (nullable) avec `Type` (`TransitionType` : None, Fade, Slide, Zoom) et `DurationMs`.
- `SlideCanvas.Slide` (propriété) et `SlideCanvas.IsReadOnly` (E2).
- `TextElement` : `Text`, `FontSize`, `IsBold`, `X`, `Y`, `Width`, `Height`.
