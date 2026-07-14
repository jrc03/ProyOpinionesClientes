using System.Text.RegularExpressions;

namespace OpinionesETL.Transformers;

public static class IdNormalizer
{
    public static string? Normalizar(string? idCrudo)
    {
        if (string.IsNullOrWhiteSpace(idCrudo))
            return null;

        // Los CSV mezclan IDs como "7", "C007" y "P016"; la BD guarda solo el número.
        var soloDigitos = Regex.Replace(idCrudo.Trim(), "[^0-9]", "");
        if (soloDigitos.Length == 0)
            return null;

        var sinCeros = soloDigitos.TrimStart('0');
        return sinCeros.Length == 0 ? "0" : sinCeros;
    }
}
