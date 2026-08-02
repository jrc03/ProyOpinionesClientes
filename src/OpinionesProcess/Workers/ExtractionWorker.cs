using Microsoft.Extensions.Options;
using OpinionesProcess.Configuration;
using OpinionesProcess.Pipeline;

namespace OpinionesProcess.Workers;

public sealed class ExtractionWorker(
    ExtractionPipeline pipeline,
    IOptions<ExtractionOptions> options,
    ILogger<ExtractionWorker> logger) : BackgroundService
{
    private readonly ExtractionPipeline _pipeline = pipeline;
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

            await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
        }
    }
}
