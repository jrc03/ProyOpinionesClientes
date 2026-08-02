using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace OpinionesETL.Extractors;

public static class CsvExtractor
{
    public static List<T> Leer<T>(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
            throw new FileNotFoundException($"No se encontró el archivo de origen: {rutaArchivo}");

        // Configuración estricta para fallar rápidamente ante datos corruptos o campos faltantes
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        try
        {
            using var reader = new StreamReader(rutaArchivo);
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<T>().ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error de formato o datos inválidos al leer el archivo CSV: '{Path.GetFileName(rutaArchivo)}'. " +
                $"Asegúrate de que la estructura y los tipos coincidan.", ex);
        }
    }
}
