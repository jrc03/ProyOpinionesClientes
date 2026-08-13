using Microsoft.Extensions.Options;
using OpinionesProcess.Configuration;
using OpinionesProcess.Pipeline;

namespace OpinionesProcess.Workers;

public sealed class ExtractionWorker(
    ExtractionPipeline pipeline,
    FactLoadPipeline factLoadPipeline,
    IOptions<ExtractionOptions> options,
    ILogger<ExtractionWorker> logger) : BackgroundService
{
    private readonly ExtractionPipeline _pipeline = pipeline;
    private readonly FactLoadPipeline _factLoadPipeline = factLoadPipeline;
    private readonly ExtractionOptions _options = options.Value;
    private readonly ILogger<ExtractionWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var initialDelay = Math.Max(0, _options.InitialDelaySeconds);
        var interval = Math.Max(1, _options.IntervalSeconds);

        _logger.LogInformation(
            "Worker de extracción iniciado. Intervalo: {IntervaloSegundos} segundos.",
            interval);

        if (initialDelay > 0)
            await Task.Delay(TimeSpan.FromSeconds(initialDelay), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var results = await _pipeline.ExecuteAsync(stoppingToken);

                foreach (var result in results)
                {
                    _logger.LogInformation(
                        "Resumen {Fuente}: Exitoso={Exitoso}, Extraídos={Extraidos}, " +
                        "Staging={Staging}, Duración={DuracionMs} ms.",
                        result.NombreFuente,
                        result.Exitoso,
                        result.Extraidos,
                        result.GuardadosEnStaging,
                        result.DuracionMilisegundos);
                }

                if (results.Count > 0 && results.All(result => result.Exitoso))
                {
                    var loteIds = results
                        .Select(result => result.LoteId)
                        .Distinct()
                        .ToList();

                    if (loteIds.Count != 1)
                    {
                        throw new InvalidOperationException(
                            "Las fuentes extraídas no pertenecen al mismo lote.");
                    }

                    await _factLoadPipeline.ExecuteAsync(
                        loteIds[0],
                        stoppingToken);
                }
                else
                {
                    _logger.LogWarning(
                        "La fact Opiniones no se recargará porque una o más fuentes fallaron.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falló el ciclo ETL. La carga anterior de dbo.Opiniones se conserva.");
            }

            await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
        }
    }
}
