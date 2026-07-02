using Dapper;
using Microsoft.Data.SqlClient;

namespace OpinionesETL.Reports;

public class ReportService(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public async Task ImprimirResumenAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var total = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Opiniones");

        Console.WriteLine();
        Console.WriteLine("========== RESUMEN DE OPINIONES EN BASE DE DATOS ==========");
        Console.WriteLine($"Total de opiniones almacenadas: {total}");

        Console.WriteLine();
        Console.WriteLine("-- Clasificación global --");
        var clasificacion = await conn.QueryAsync(
            "SELECT Clasificacion, COUNT(*) AS Cantidad FROM Opiniones GROUP BY Clasificacion ORDER BY Cantidad DESC");
        foreach (var fila in clasificacion)
            Console.WriteLine($"  {fila.Clasificacion,-10} {fila.Cantidad,6}");

        Console.WriteLine();
        Console.WriteLine("-- Top 10 productos por % de satisfacción (mínimo 3 opiniones) --");
        var topProductos = await conn.QueryAsync("""
            SELECT TOP 10 IdProducto, NombreProducto, TotalOpiniones, PorcentajeSatisfaccion, PuntajePromedio
            FROM vw_ResumenPorProducto
            WHERE TotalOpiniones >= 3
            ORDER BY PorcentajeSatisfaccion DESC;
            """);
        foreach (var fila in topProductos)
            Console.WriteLine(
                $"  [{fila.IdProducto,4}] {fila.NombreProducto,-20} " +
                $"Opiniones: {fila.TotalOpiniones,4}  Satisfacción: {fila.PorcentajeSatisfaccion,6}%  " +
                $"Promedio: {fila.PuntajePromedio}");

        Console.WriteLine("============================================================");
    }
}
