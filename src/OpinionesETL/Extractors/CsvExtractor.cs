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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
        };

        using var reader = new StreamReader(rutaArchivo);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }
}
