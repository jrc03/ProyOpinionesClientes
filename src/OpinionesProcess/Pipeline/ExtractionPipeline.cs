using System.Diagnostics;
using OpinionesData.Interfaces;
using OpinionesProcess.Interfaces;
using OpinionesProcess.Models;

namespace OpinionesProcess.Pipeline;

public sealed class ExtractionPipeline(
    IEnumerable<IExtractor> extractors,
    IStagingWriter stagingWriter,
    ILogger<ExtractionPipeline> logger)
{
    private readonly IReadOnlyCollection<IExtractor> _extractors = extractors.ToList();
    private readonly IStagingWriter _stagingWriter = stagingWriter;
    private readonly ILogger<ExtractionPipeline> _logger = logger;

    public async Task<IReadOnlyCollection<ResultadoExtraccionFuente>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var loteId = Guid.NewGuid();

        _logger.LogInformation(
            "Iniciando lote de extracción {LoteId} con {CantidadFuentes} fuentes.",
            loteId,
            _extractors.Count);

        var tasks = _extractors
            .Select(extractor => ExecuteSourceAsync(
                extractor,
                loteId,
                cancellationToken));

        return await Task.WhenAll(tasks);
    }

    private async Task<ResultadoExtraccionFuente> ExecuteSourceAsync(
        IExtractor extractor,
        Guid loteId,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var opiniones = await extractor.ExtractAsync(loteId, cancellationToken);
            var insertados = await _stagingWriter.WriteBatchAsync(
                opiniones,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Fuente {Fuente}: {Extraidos} extraídos y {Insertados} guardados " +
                "en staging en {DuracionMs} ms. Lote {LoteId}.",
                extractor.NombreFuente,
                opiniones.Count,
                insertados,
                stopwatch.ElapsedMilliseconds,
                loteId);

            return new ResultadoExtraccionFuente
            {
                NombreFuente = extractor.NombreFuente,
                LoteId = loteId,
                Extraidos = opiniones.Count,
                GuardadosEnStaging = insertados,
                DuracionMilisegundos = stopwatch.ElapsedMilliseconds,
                Exitoso = true
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                exception,
                "Falló la extracción de la fuente {Fuente} para el lote {LoteId} " +
                "después de {DuracionMs} ms.",
                extractor.NombreFuente,
                loteId,
                stopwatch.ElapsedMilliseconds);

            return new ResultadoExtraccionFuente
            {
                NombreFuente = extractor.NombreFuente,
                LoteId = loteId,
                DuracionMilisegundos = stopwatch.ElapsedMilliseconds,
                Exitoso = false,
                Error = exception.Message
            };
        }
    }
}
