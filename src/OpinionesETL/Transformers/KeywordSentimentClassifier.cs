using System.Globalization;
using System.Text;

namespace OpinionesETL.Transformers;

public static class KeywordSentimentClassifier
{
    private static readonly string[] PalabrasPositivas =
    [
        "excelente", "recomendable", "recomiendo", "encanta", "perfecto",
        "muy satisfecho", "contento", "buena calidad", "calidad superior",
        "gran relacion calidad", "funciona perfecto", "cumple su funcion",
        "entrega correcta", "rapido y funciona",
    ];

    private static readonly string[] PalabrasNegativas =
    [
        "mala calidad", "pesima", "rompio", "no cumple", "insatisfecho",
        "no volveria", "decepcionado", "danado", "no funciona como esperaba",
        "tardio", "no lo recomiendo", "no era lo que esperaba",
        "dejo de funcionar", "no resolvio",
    ];

    public static string Clasificar(string comentario)
    {
        var texto = QuitarAcentos(comentario.ToLowerInvariant());

        var positivos = PalabrasPositivas.Count(p => texto.Contains(QuitarAcentos(p)));
        var negativos = PalabrasNegativas.Count(p => texto.Contains(QuitarAcentos(p)));

        if (positivos > negativos) return "Positiva";
        if (negativos > positivos) return "Negativa";
        return "Neutra";
    }

    private static string QuitarAcentos(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
