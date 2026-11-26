
using System;
using System.Collections.Generic;

namespace PresentationApp.Core.Models;

public class Slide
{
    // Identifiant unique de la diapositive.
    public Guid Id { get; set; } = Guid.NewGuid();

    // Position de la diapositive dans la présentation.
    public int Order { get; set; }

    // Couleur d'arrière-plan.
    public string BackgroundColor { get; set; } = "#FFFFFF";

    // Transition utilisée lors du passage à cette diapositive.
    public Transition Transition { get; set; } = new();

    // Éléments contenus dans la diapositive.
    public List<SlideElement> Elements { get; set; } = new();
}
