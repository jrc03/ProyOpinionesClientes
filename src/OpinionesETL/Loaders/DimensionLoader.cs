using Dapper;
using Microsoft.Data.SqlClient;
using OpinionesETL.Models;

namespace OpinionesETL.Loaders;

public class DimensionLoader
{
    public async Task CargarClientesAsync(SqlConnection conn, SqlTransaction? transaction, IEnumerable<Cliente> clientes)
    {
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM Clientes WHERE IdCliente = @IdCliente)
                INSERT INTO Clientes (IdCliente, Nombre, Email) VALUES (@IdCliente, @Nombre, @Email);
            """, clientes, transaction);
    }

    public async Task CargarProductosAsync(SqlConnection conn, SqlTransaction? transaction, IEnumerable<Producto> productos)
    {
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto)
                INSERT INTO Productos (IdProducto, Nombre, Categoria) VALUES (@IdProducto, @Nombre, @Categoria);
            """, productos, transaction);
    }

    public async Task<int> ObtenerOCrearFuenteAsync(SqlConnection conn, SqlTransaction? transaction, string tipoFuente)
    {
        var idExistente = await conn.QuerySingleOrDefaultAsync<int?>(
            "SELECT IdFuente FROM FuenteDatos WHERE TipoFuente = @tipoFuente", new { tipoFuente }, transaction);
        if (idExistente is not null)
            return idExistente.Value;

        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO FuenteDatos (TipoFuente, FechaCarga)
            OUTPUT INSERTED.IdFuente
            VALUES (@tipoFuente, GETDATE());
            """, new { tipoFuente }, transaction);
    }
}
