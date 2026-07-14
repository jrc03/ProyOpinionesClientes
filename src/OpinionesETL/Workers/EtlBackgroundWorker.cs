using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpinionesETL.Pipeline;
using OpinionesETL.Reports;

namespace OpinionesETL.Workers;

public class EtlBackgroundWorker : BackgroundService
{
    private readonly ILogger<EtlBackgroundWorker> _logger;
    private readonly EtlOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public EtlBackgroundWorker(
        ILogger<EtlBackgroundWorker> logger,
        IOptions<EtlOptions> options,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando Worker Service de ETL de opiniones...");
        _logger.LogInformation("Ruta de datos: {Carpeta}", _options.CarpetaDatos);
        _logger.LogInformation("URL API Social: {Url}", _options.ApiComentariosSocialesUrl);
        _logger.LogInformation("El ETL se ejecutará periódicamente cada {Intervalo} segundos.", _options.EtlIntervaloSegundos);
        _logger.LogInformation("Esperando un retraso inicial de {Delay} segundos...", _options.EtlDelayInicialSegundos);

        // Retraso inicial configurable
        await Task.Delay(TimeSpan.FromSeconds(_options.EtlDelayInicialSegundos), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Ejecutando ciclo del pipeline ETL a las {Hora}...", DateTime.Now.ToString("T"));
                
                // Creación de scope para inyección de dependencias transient en cada ciclo
                using (var scope = _serviceProvider.CreateScope())
                {
                    var pipeline = scope.ServiceProvider.GetRequiredService<EtlPipeline>();
                    var resultados = await pipeline.EjecutarAsync();

                    _logger.LogInformation("ETL finalizado con éxito. Resumen de carga:");
                    foreach (var r in resultados)
                    {
                        _logger.LogInformation(
                            "- Fuente '{Fuente}': Leídos: {Leidos}, Insertados: {Insertados}, Duplicados: {Duplicados}, Inválidos/Rechazados: {Rechazados}",
                            r.NombreFuente, r.Leidos, r.Insertados, r.DuplicadosOmitidos, r.RechazadosSinProducto + r.RechazadosDatosInvalidos);
                    }

                    _logger.LogInformation("Consultando métricas en Base de Datos...");
                    var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();
                    await reportService.ImprimirResumenAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error crítico durante la ejecución del pipeline ETL.");
            }

            _logger.LogInformation("Ciclo completado. Esperando {Intervalo} segundos para el siguiente ciclo...", _options.EtlIntervaloSegundos);
            await Task.Delay(TimeSpan.FromSeconds(_options.EtlIntervaloSegundos), stoppingToken);
        }
    }
}
