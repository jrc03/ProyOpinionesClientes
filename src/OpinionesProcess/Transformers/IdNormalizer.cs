using System.Text;

namespace OpinionesProcess.Transformers;

public static class IdNormalizer
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new StringBuilder();
        foreach (var character in value.Trim())
        {
            if (char.IsDigit(character))
                digits.Append(character);
        }

        if (digits.Length == 0)
            return null;

        var normalized = digits.ToString().TrimStart('0');
        return normalized.Length == 0 ? "0" : normalized;
    }
}
