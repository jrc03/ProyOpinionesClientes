using System.Globalization;

namespace OpinionesProcess.Extractors;

internal static class ExtractorValueParser
{
    public static DateTime? ParseDate(string? value)
    {
        return DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? date
            : null;
    }

    public static int? ParseInteger(string? value)
    {
        return int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var number)
            ? number
            : null;
    }
}
