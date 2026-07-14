using Dapper;
using Microsoft.Data.SqlClient;

namespace OpinionesETL.Loaders;

public static class OpinionLoader
{
    public static async Task<bool> InsertarAsync(
        SqlConnection conn,
        SqlTransaction? transaction,
        string? origenId,
        string? idCliente,
        string idProducto,
        int idFuente,
        DateTime fecha,
        string comentario,
        string clasificacion,
        int? puntaje)
    {
        var filas = await conn.ExecuteAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM Opiniones
                WHERE OrigenId = @origenId AND IdFuente = @idFuente AND OrigenId IS NOT NULL
            )
            INSERT INTO Opiniones
                (IdCliente, IdProducto, IdFuente, Fecha, Comentario, Clasificacion, PuntajeSatisfaccion, OrigenId)
            VALUES
                (@idCliente, @idProducto, @idFuente, @fecha, @comentario, @clasificacion, @puntaje, @origenId);
            """, new { origenId, idCliente, idProducto, idFuente, fecha, comentario, clasificacion, puntaje }, transaction);

        return filas > 0;
    }
}
