using System.Text.RegularExpressions;

namespace OpinionesETL.Transformers;

/// <summary>
/// Normaliza IDs de cliente/producto provenientes de distintas fuentes a la misma
/// forma usada por las tablas Clientes/Productos ("1", "2", ... sin prefijo ni ceros
/// a la izquierda). Ejemplos: "C007" -> "7", "P016" -> "16", "045" -> "45".
/// </summary>
public static class IdNormalizer
{
    public static string? Normalizar(string? idCrudo)
    {
        if (string.IsNullOrWhiteSpace(idCrudo))
            return null;

        var soloDigitos = Regex.Replace(idCrudo.Trim(), "[^0-9]", "");
        if (soloDigitos.Length == 0)
            return null;

        var sinCeros = soloDigitos.TrimStart('0');
        return sinCeros.Length == 0 ? "0" : sinCeros;
    }
}
