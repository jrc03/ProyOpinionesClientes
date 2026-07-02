using Dapper;
using Microsoft.Data.SqlClient;
using OpinionesETL.Models;

namespace OpinionesETL.Loaders;

public class DimensionLoader
{
    private readonly string _connectionString;

    public DimensionLoader(string connectionString) => _connectionString = connectionString;

    public async Task CargarClientesAsync(IEnumerable<Cliente> clientes)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM Clientes WHERE IdCliente = @IdCliente)
                INSERT INTO Clientes (IdCliente, Nombre, Email) VALUES (@IdCliente, @Nombre, @Email);
            """, clientes);
    }

    public async Task CargarProductosAsync(IEnumerable<Producto> productos)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto)
                INSERT INTO Productos (IdProducto, Nombre, Categoria) VALUES (@IdProducto, @Nombre, @Categoria);
            """, productos);
    }

    public async Task<int> ObtenerOCrearFuenteAsync(string tipoFuente)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var idExistente = await conn.QuerySingleOrDefaultAsync<int?>(
            "SELECT IdFuente FROM FuenteDatos WHERE TipoFuente = @tipoFuente", new { tipoFuente });
        if (idExistente is not null)
            return idExistente.Value;

        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO FuenteDatos (TipoFuente, FechaCarga)
            OUTPUT INSERTED.IdFuente
            VALUES (@tipoFuente, GETDATE());
            """, new { tipoFuente });
    }
}
