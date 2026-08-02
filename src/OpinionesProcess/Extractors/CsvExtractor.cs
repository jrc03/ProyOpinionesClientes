using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using OpinionesData.Models;
using OpinionesProcess.Configuration;
using OpinionesProcess.Interfaces;
using OpinionesProcess.Models;

namespace OpinionesProcess.Extractors;

public sealed class CsvExtractor(IOptions<ExtractionOptions> options) : IExtractor
{
    private readonly ExtractionOptions _options = options.Value;

    public string NombreFuente => "EncuestaInterna";

    public async Task<IReadOnlyCollection<OpinionStaging>> ExtractAsync(
        Guid loteId,
        CancellationToken cancellationToken = default)
    {
        var filePath = FindDataFile(_options.DataFolder, _options.SurveyFile);
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        var opiniones = new List<OpinionStaging>();
        var extractedAtUtc = DateTime.UtcNow;

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, configuration);

        await foreach (var registro in csv
            .GetRecordsAsync<RegistroEncuesta>(cancellationToken)
            .ConfigureAwait(false))
        {
            opiniones.Add(new OpinionStaging
            {
                LoteId = loteId,
                Fuente = NombreFuente,
                OrigenId = registro.IdOpinion,
                IdCliente = registro.IdCliente,
                IdProducto = registro.IdProducto,
                Fecha = ExtractorValueParser.ParseDate(registro.Fecha),
                Comentario = registro.Comentario,
                ClasificacionOrigen = registro.Clasificacion,
                PuntajeOrigen = ExtractorValueParser.ParseInteger(registro.PuntajeSatisfaccion),
                FechaExtraccionUtc = extractedAtUtc
            });
        }

        return opiniones;
    }

    private static string FindDataFile(string configuredFolder, string fileName)
    {
        var folder = string.IsNullOrWhiteSpace(configuredFolder)
            ? "Data"
            : configuredFolder;

        var configuredPath = Path.IsPathRooted(folder)
            ? Path.Combine(folder, fileName)
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), folder, fileName));

        if (File.Exists(configuredPath))
            return configuredPath;

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, folder, fileName);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"No se encontró el archivo CSV de encuestas: {fileName}");
    }
}
