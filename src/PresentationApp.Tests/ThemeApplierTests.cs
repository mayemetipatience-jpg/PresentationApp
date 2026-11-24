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
}