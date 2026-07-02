using System.Globalization;

namespace OpinionesETL.Transformers;

public static class DataCleaner
{
    public static string Limpiar(string? texto) => (texto ?? "").Trim();

    public static bool EsComentarioValido(string comentario) =>
        !string.IsNullOrWhiteSpace(comentario);

    public static DateTime? ParsearFecha(string? fecha)
    {
        if (DateTime.TryParse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var resultado))
            return resultado;
        return null;
    }

    public static string NormalizarClasificacion(string? clasificacionCruda)
    {
        var valor = Limpiar(clasificacionCruda);
        return valor.ToLowerInvariant() switch
        {
            "positiva" => "Positiva",
            "negativa" => "Negativa",
            "neutra" or "neutral" => "Neutra",
            _ => "Neutra",
        };
    }
}
