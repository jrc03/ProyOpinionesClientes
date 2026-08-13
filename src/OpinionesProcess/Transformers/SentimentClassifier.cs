using System.Globalization;
using System.Text;

namespace OpinionesProcess.Transformers;

public static class SentimentClassifier
{
    private static readonly string[] PositiveKeywords =
    [
        "excelente", "recomendable", "recomiendo", "encanta", "perfecto",
        "muy satisfecho", "contento", "buena calidad", "calidad superior",
        "gran relacion calidad", "funciona perfecto", "cumple su funcion",
        "entrega correcta", "rapido y funciona"
    ];

    private static readonly string[] NegativeKeywords =
    [
        "mala calidad", "pesima", "rompio", "no cumple", "insatisfecho",
        "no volveria", "decepcionado", "danado", "no funciona como esperaba",
        "tardio", "no lo recomiendo", "no era lo que esperaba",
        "dejo de funcionar", "no resolvio"
    ];

    public static string Classify(
        string comentario,
        string? clasificacionOrigen,
        int? puntajeOrigen)
    {
        var normalizedClassification = NormalizeClassification(clasificacionOrigen);
        if (normalizedClassification is not null)
            return normalizedClassification;

        if (puntajeOrigen is not null)
        {
            return puntajeOrigen.Value switch
            {
                >= 4 => "Positiva",
                3 => "Neutra",
                _ => "Negativa"
            };
        }

        var text = RemoveAccents(comentario.ToLowerInvariant());
        var positives = PositiveKeywords.Count(keyword => text.Contains(keyword));
        var negatives = NegativeKeywords.Count(keyword => text.Contains(keyword));

        if (positives > negatives)
            return "Positiva";

        if (negatives > positives)
            return "Negativa";

        return "Neutra";
    }

    private static string? NormalizeClassification(string? value)
    {
        var normalized = RemoveAccents(value?.Trim().ToLowerInvariant() ?? string.Empty);
        return normalized switch
        {
            "positiva" => "Positiva",
            "negativa" => "Negativa",
            "neutra" or "neutral" => "Neutra",
            _ => null
        };
    }

    private static string RemoveAccents(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            {
                result.Append(character);
            }
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
