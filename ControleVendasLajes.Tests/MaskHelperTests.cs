using ControleVendasLajes.Helpers;
using Xunit;

namespace ControleVendasLajes.Tests;

public class MaskHelperTests
{
    [Theory]
    [InlineData("4", 0.04)]
    [InlineData("45", 0.45)]
    [InlineData("450", 4.50)]
    [InlineData("4500", 45.00)]
    [InlineData("125000", 1250.00)]
    [InlineData("R$ 45,00", 45.00)]
    [InlineData("R$ 1.500,50", 1500.50)]
    public void FormatCurrency_ValidInputs_ParsesAndFormatsCorrectly(string input, decimal expectedValue)
    {
        var formatted = MaskHelper.FormatCurrency(input, out var numericValue);

        Assert.Equal(expectedValue, numericValue);
        Assert.Contains(expectedValue.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")), formatted);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData(null, 0)]
    [InlineData("abc", 0)]
    public void FormatCurrency_EmptyOrInvalid_ReturnsZero(string? input, decimal expectedValue)
    {
        var formatted = MaskHelper.FormatCurrency(input, out var numericValue);

        Assert.Equal(expectedValue, numericValue);
        Assert.Equal(string.Empty, formatted);
    }

    [Theory]
    [InlineData("100", 100.0, "100")]
    [InlineData("100,5", 100.5, "100,5")]
    [InlineData("100,50", 100.50, "100,50")]
    [InlineData("100.50", 100.50, "100,50")]
    [InlineData("125,999", 125.99, "125,99")]
    public void FormatArea_ValidInputs_ParsesAndFormatsCorrectly(string input, double expectedValue, string expectedFormatted)
    {
        var formatted = MaskHelper.FormatArea(input, out var numericValue);

        Assert.Equal(expectedValue, numericValue);
        Assert.Equal(expectedFormatted, formatted);
    }
}
