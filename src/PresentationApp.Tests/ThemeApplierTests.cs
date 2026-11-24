using PresentationApp.Core.Models;
using PresentationApp.Core.Templates;
using Xunit;

namespace PresentationApp.Tests;

public class ThemeApplierTests
{
    [Fact]
    public void Le_fond_du_theme_est_applique_a_toutes_les_diapositives()
    {
        var presentation = new Presentation();
        presentation.Slides.Add(new Slide { Order = 1 });
        presentation.Slides.Add(new Slide { Order = 2 });

        ThemeApplier.ApplyBackground(presentation, BuiltInThemes.Dark);

        Assert.All(presentation.Slides,
            slide => Assert.Equal(BuiltInThemes.Dark.BackgroundColor, slide.BackgroundColor));
    }

    [Fact]
    public void La_couleur_et_la_police_du_theme_sont_appliquees_au_texte()
    {
        var presentation = new Presentation();
        var slide = new Slide { Order = 1 };
        slide.Elements.Add(new TextElement { Text = "Bonjour" });
        presentation.Slides.Add(slide);

        ThemeApplier.ApplyTextStyle(presentation, BuiltInThemes.Warm);

        var texte = (TextElement)slide.Elements[0];
        Assert.Equal(BuiltInThemes.Warm.TextColor, texte.Style.TextColor);
        Assert.Equal(BuiltInThemes.Warm.FontFamily, texte.Style.FontFamily);
    }
}