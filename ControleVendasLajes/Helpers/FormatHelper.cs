using System.Globalization;

namespace ControleVendasLajes.Helpers;

public static class FormatHelper
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string ToMoney(this decimal value)
    {
        return $"R$ {value.ToString("N2", PtBr)}";
    }

    public static string ToMoney(this double value)
    {
        return $"R$ {value.ToString("N2", PtBr)}";
    }
}
