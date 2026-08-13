using System.Diagnostics;
using OpinionesData.Interfaces;
using OpinionesProcess.Models;
using OpinionesProcess.Transformers;

namespace OpinionesProcess.Pipeline;

public sealed class FactLoadPipeline(
    IOpinionLoadReader loadReader,
    IOpinionFactWriter factWriter,
    OpinionTransformer transformer,
    ILogger<FactLoadPipeline> logger)
{
    private readonly IOpinionLoadReader _loadReader = loadReader;
    private readonly IOpinionFactWriter _factWriter = factWriter;
    private readonly OpinionTransformer _transformer = transformer;
    private readonly ILogger<FactLoadPipeline> _logger = logger;

    public async Task<ResultadoCargaFactOpiniones> ExecuteAsync(
        Guid loteId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando transformación y recarga de dbo.Opiniones para el lote {LoteId}.",
            loteId);

        var stagingRecords = await _loadReader.ReadStagingBatchAsync(
            loteId,
            cancellationToken);
        var references = await _loadReader.ReadReferencesAsync(cancellationToken);
        var transformation = _transformer.Transform(stagingRecords, references);

        var inserted = await _factWriter.ReplaceAllAsync(
            transformation.Opiniones,
            cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Fact Opiniones recargada: {Cargados} registros. " +
            "Leídos={Leidos}, Inválidos={Invalidos}, SinProducto={SinProducto}, " +
            "SinFuente={SinFuente}, ClientesNulificados={ClientesNulificados}, " +
            "Duplicados={Duplicados}, Duración={DuracionMs} ms. Lote {LoteId}.",
            inserted,
            transformation.Leidos,
            transformation.RechazadosDatosInvalidos,
            transformation.RechazadosSinProducto,
            transformation.RechazadosSinFuente,
            transformation.ClientesNulificados,
            transformation.DuplicadosOmitidos,
            stopwatch.ElapsedMilliseconds,
            loteId);

        return new ResultadoCargaFactOpiniones
        {
            LoteId = loteId,
            Leidos = transformation.Leidos,
            Cargados = inserted,
            RechazadosDatosInvalidos = transformation.RechazadosDatosInvalidos,
            RechazadosSinProducto = transformation.RechazadosSinProducto,
            RechazadosSinFuente = transformation.RechazadosSinFuente,
            ClientesNulificados = transformation.ClientesNulificados,
            DuplicadosOmitidos = transformation.DuplicadosOmitidos,
            DuracionMilisegundos = stopwatch.ElapsedMilliseconds
        };
    }
}
