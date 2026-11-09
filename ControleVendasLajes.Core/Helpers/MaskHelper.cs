using System.Globalization;
using System.Text.RegularExpressions;

namespace ControleVendasLajes.Helpers;

public static class MaskHelper
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>
    /// Formata uma string de entrada contendo números para o formato monetário brasileiro: R$ 0,00
    /// </summary>
    public static string FormatCurrency(string? input, out decimal numericValue)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            numericValue = 0m;
            return string.Empty;
        }

        // Extrai apenas os dígitos
        var digits = Regex.Replace(input, @"\D", "");
        if (string.IsNullOrEmpty(digits))
        {
            numericValue = 0m;
            return string.Empty;
        }

        // Limita o tamanho para evitar overflow (até 12 dígitos = 999 milhões)
        if (digits.Length > 12)
        {
            digits = digits[..12];
        }

        if (decimal.TryParse(digits, out var cents))
        {
            numericValue = cents / 100m;
            return numericValue.ToString("C2", PtBr);
        }

        numericValue = 0m;
        return string.Empty;
    }

    /// <summary>
    /// Converte um decimal existente diretamente para string no padrão R$ 0,00
    /// </summary>
    public static string FormatCurrency(decimal value)
    {
        if (value <= 0)
            return string.Empty;

        return value.ToString("C2", PtBr);
    }

    /// <summary>
    /// Formata metragem / área em m² permitindo números e vírgula decimal (ex: 120 ou 120,50)
    /// </summary>
    public static string FormatArea(string? input, out double numericValue)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            numericValue = 0.0;
            return string.Empty;
        }

        // Converte ponto para vírgula
        var cleaned = input.Replace(".", ",");

        // Remove caracteres inválidos (permite apenas dígitos e vírgula)
        cleaned = Regex.Replace(cleaned, @"[^\d,]", "");

        // Permite apenas uma vírgula
        var parts = cleaned.Split(',');
        if (parts.Length > 2)
        {
            cleaned = parts[0] + "," + string.Concat(parts.Skip(1));
        }

        // Limita a 2 casas decimais após a vírgula
        var commaIndex = cleaned.IndexOf(',');
        if (commaIndex >= 0 && cleaned.Length > commaIndex + 3)
        {
            cleaned = cleaned[..(commaIndex + 3)];
        }

        // Limita a parte inteira a 7 dígitos
        if (commaIndex < 0 && cleaned.Length > 7)
        {
            cleaned = cleaned[..7];
        }
        else if (commaIndex > 7)
        {
            cleaned = cleaned[..7] + cleaned[commaIndex..];
        }

        // Tenta converter para double
        if (double.TryParse(cleaned, NumberStyles.Any, PtBr, out var parsed))
        {
            numericValue = parsed;
            return cleaned;
        }

        numericValue = 0.0;
        return cleaned;
    }

    /// <summary>
    /// Formata número de telefone brasileiro no padrão (XX) XXXXX-XXXX ou (XX) XXXX-XXXX
    /// </summary>
    public static string ApplyPhoneMask(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var digits = Regex.Replace(input, @"\D", "");
        if (digits.Length > 11) digits = digits[..11];

        return digits.Length switch
        {
            <= 2 => digits.Length > 0 ? $"({digits}" : string.Empty,
            <= 6 => $"({digits[..2]}) {digits[2..]}",
            <= 10 => $"({digits[..2]}) {digits[2..6]}-{digits[6..]}",
            _ => $"({digits[..2]}) {digits[2..7]}-{digits[7..]}"
        };
    }

    /// <summary>
    /// Converte um double existente de área diretamente para string (ex: 125,50)
    /// </summary>
    public static string FormatArea(double value)
    {
        if (value <= 0)
            return string.Empty;

        return value.ToString("N2", PtBr);
    }
}
