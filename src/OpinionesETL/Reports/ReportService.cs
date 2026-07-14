using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpinionesETL.Reports;

public class ReportService
{
    private readonly string _connectionString;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IOptions<EtlOptions> options, ILogger<ReportService> logger)
    {
        _connectionString = options.Value.ConnectionString;
        _logger = logger;
    }

    public async Task ImprimirResumenAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var total = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Opiniones");

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("Resumen en base de datos");
        sb.AppendLine($"Opiniones guardadas: {total}");
        sb.AppendLine();
        sb.AppendLine("Clasificación");

        var clasificacion = await conn.QueryAsync(
            "SELECT Clasificacion, COUNT(*) AS Cantidad FROM Opiniones GROUP BY Clasificacion ORDER BY Cantidad DESC");
        foreach (var fila in clasificacion)
            sb.AppendLine($"  {fila.Clasificacion,-10} {fila.Cantidad,6}");

        sb.AppendLine();
        sb.AppendLine("Productos con mejor satisfacción");
        var topProductos = await conn.QueryAsync("""
            SELECT TOP 10 IdProducto, NombreProducto, TotalOpiniones, PorcentajeSatisfaccion, PuntajePromedio
            FROM vw_ResumenPorProducto
            WHERE TotalOpiniones >= 3
            ORDER BY PorcentajeSatisfaccion DESC;
            """);
        foreach (var fila in topProductos)
            sb.AppendLine(
                $"  [{fila.IdProducto,4}] {fila.NombreProducto,-20} " +
                $"Opiniones: {fila.TotalOpiniones,4}  Satisfacción: {fila.PorcentajeSatisfaccion,6}%  " +
                $"Promedio: {fila.PuntajePromedio}");

        _logger.LogInformation(sb.ToString());
    }
}
