using PresentationApp.IO.Export;
using Xunit;

namespace PresentationApp.Tests;

public class ExportFileNamesTests
{
    [Fact]
    public void Le_nom_PNG_est_numerote_sur_3_chiffres()
    {
        Assert.Equal("diapositive-001.png", ExportFileNames.SlidePng(1));
        Assert.Equal("diapositive-012.png", ExportFileNames.SlidePng(12));
    }
}