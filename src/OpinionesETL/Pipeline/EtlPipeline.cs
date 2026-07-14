using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OpinionesETL.Extractors;
using OpinionesETL.Loaders;
using OpinionesETL.Models;
using OpinionesETL.Transformers;
using OpinionesETL.Validation;

namespace OpinionesETL.Pipeline;

public class EtlPipeline
{
    private readonly EtlOptions _options;
    private readonly SocialCommentsApiExtractor _apiExtractor;
    private readonly DimensionLoader _dimensionLoader;

    public EtlPipeline(
        IOptions<EtlOptions> options,
        SocialCommentsApiExtractor apiExtractor,
        DimensionLoader dimensionLoader)
    {
        _options = options.Value;
        _apiExtractor = apiExtractor;
        _dimensionLoader = dimensionLoader;
    }

    public async Task<List<EtlSourceResult>> EjecutarAsync()
    {
        var clientesCrudos = CsvExtractor.Leer<ClienteRecord>(Path.Combine(_options.CarpetaDatos, _options.ClientesCsv));
        var clientes = clientesCrudos
            .Where(c => !string.IsNullOrWhiteSpace(c.IdCliente) && !string.IsNullOrWhiteSpace(c.Nombre))
            .GroupBy(c => c.IdCliente.Trim())
            .Select(g => new Cliente
            {
                IdCliente = g.Key,
                Nombre = DataCleaner.Limpiar(g.First().Nombre),
                Email = string.IsNullOrWhiteSpace(g.First().Email) ? null : g.First().Email!.Trim(),
            })
            .ToList();

        var productosCrudos = CsvExtractor.Leer<ProductoRecord>(Path.Combine(_options.CarpetaDatos, _options.ProductosCsv));
        var productos = productosCrudos
            .Where(p => !string.IsNullOrWhiteSpace(p.IdProducto) && !string.IsNullOrWhiteSpace(p.Nombre))
            .GroupBy(p => p.IdProducto.Trim())
            .Select(g => new Producto
            {
                IdProducto = g.Key,
                Nombre = DataCleaner.Limpiar(g.First().Nombre),
                Categoria = string.IsNullOrWhiteSpace(g.First().Categoria) ? null : g.First().Categoria!.Trim(),
            })
            .ToList();

        using var conn = new SqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        // 1. Cargar dimensiones dentro de una transacción compartida
        using (var transDimensiones = conn.BeginTransaction())
        {
            try
            {
                await _dimensionLoader.CargarClientesAsync(conn, transDimensiones, clientes);
                await _dimensionLoader.CargarProductosAsync(conn, transDimensiones, productos);
                await transDimensiones.CommitAsync();
            }
            catch (Exception)
            {
                await transDimensiones.RollbackAsync();
                throw;
            }
        }

        // 2. Obtener datos para la validación referencial en la misma conexión
        var idsClientes = await conn.QueryAsync<string>("SELECT IdCliente FROM Clientes");
        var idsProductos = await conn.QueryAsync<string>("SELECT IdProducto FROM Productos");
        var validador = new ReferentialValidator(idsClientes, idsProductos);

        var resultados = new List<EtlSourceResult>();

        // 3. Procesar las fuentes con transacciones individuales
        resultados.Add(await ProcesarEncuestasAsync(conn, validador));
        resultados.Add(await ProcesarResenasWebAsync(conn, validador));
        resultados.Add(await ProcesarComentariosSocialesAsync(conn, validador));

        return resultados;
    }

    private async Task<EtlSourceResult> ProcesarEncuestasAsync(SqlConnection conn, ReferentialValidator validador)
    {
        var resultado = new EtlSourceResult { NombreFuente = "Encuestas internas" };
        
        using var trans = conn.BeginTransaction();
        try
        {
            var idFuente = await _dimensionLoader.ObtenerOCrearFuenteAsync(conn, trans, "EncuestaInterna");
            var registros = CsvExtractor.Leer<SurveyRecord>(Path.Combine(_options.CarpetaDatos, _options.EncuestasCsv));
            foreach (var r in registros)
            {
                var clasificacion = DataCleaner.NormalizarClasificacion(r.Clasificacion);
                int? puntaje = int.TryParse(r.PuntajeSatisfaccion, out var p) ? p : null;
                var fecha = DataCleaner.ParsearFecha(r.Fecha);

                await ProcesarFilaAsync(conn, trans, validador, idFuente,
                    origenId: r.IdOpinion,
                    idClienteCrudo: r.IdCliente,
                    idProductoCrudo: r.IdProducto,
                    fecha: fecha,
                    comentarioCrudo: r.Comentario,
                    clasificacion: clasificacion,
                    puntaje: puntaje,
                    resultado: resultado);
            }
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }

        return resultado;
    }

    private async Task<EtlSourceResult> ProcesarResenasWebAsync(SqlConnection conn, ReferentialValidator validador)
    {
        var resultado = new EtlSourceResult { NombreFuente = "Reseñas web (BD relacional)" };
        
        using var trans = conn.BeginTransaction();
        try
        {
            var idFuente = await _dimensionLoader.ObtenerOCrearFuenteAsync(conn, trans, "ReseñaWeb");
            var registros = await conn.QueryAsync<WebReviewSourceRecord>("EXEC sp_ObtenerResenasWebOrigen", null, trans);
            foreach (var r in registros)
            {
                var clasificacion = RatingClassifier.Clasificar(r.Rating);

                // Se pasa directamente r.Fecha (DateTime), evitando el string round-trip
                await ProcesarFilaAsync(conn, trans, validador, idFuente,
                    origenId: r.IdReview,
                    idClienteCrudo: r.IdCliente,
                    idProductoCrudo: r.IdProducto,
                    fecha: r.Fecha,
                    comentarioCrudo: r.Comentario,
                    clasificacion: clasificacion,
                    puntaje: r.Rating,
                    resultado: resultado);
            }
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }

        return resultado;
    }

    private async Task<EtlSourceResult> ProcesarComentariosSocialesAsync(SqlConnection conn, ReferentialValidator validador)
    {
        var resultado = new EtlSourceResult { NombreFuente = "Comentarios en redes sociales (API REST)" };
        
        using var trans = conn.BeginTransaction();
        try
        {
            var idFuente = await _dimensionLoader.ObtenerOCrearFuenteAsync(conn, trans, "RedSocial");
            var registros = await _apiExtractor.LeerAsync();
            foreach (var r in registros)
            {
                var comentarioLimpio = DataCleaner.Limpiar(r.Comentario);
                var clasificacion = KeywordSentimentClassifier.Clasificar(comentarioLimpio);
                var fecha = DataCleaner.ParsearFecha(r.Fecha);

                await ProcesarFilaAsync(conn, trans, validador, idFuente,
                    origenId: r.IdComment,
                    idClienteCrudo: r.IdCliente,
                    idProductoCrudo: r.IdProducto,
                    fecha: fecha,
                    comentarioCrudo: r.Comentario,
                    clasificacion: clasificacion,
                    puntaje: null,
                    resultado: resultado);
            }
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }

        return resultado;
    }

    private static async Task ProcesarFilaAsync(
        SqlConnection conn,
        SqlTransaction? transaction,
        ReferentialValidator validador,
        int idFuente,
        string? origenId,
        string? idClienteCrudo,
        string idProductoCrudo,
        DateTime? fecha,
        string? comentarioCrudo,
        string clasificacion,
        int? puntaje,
        EtlSourceResult resultado)
    {
        resultado.Leidos++;

        var comentario = DataCleaner.Limpiar(comentarioCrudo);
        if (!DataCleaner.EsComentarioValido(comentario))
        {
            resultado.RechazadosDatosInvalidos++;
            return;
        }

        if (fecha is null)
        {
            resultado.RechazadosDatosInvalidos++;
            return;
        }

        var idProducto = IdNormalizer.Normalizar(idProductoCrudo);
        if (idProducto is null || !validador.ProductoExiste(idProducto))
        {
            resultado.RechazadosSinProducto++;
            return;
        }

        var idClienteNormalizado = IdNormalizer.Normalizar(idClienteCrudo);
        string? idClienteResuelto = null;
        if (idClienteNormalizado is not null && validador.ClienteExiste(idClienteNormalizado))
            idClienteResuelto = idClienteNormalizado;
        else
            resultado.ClientesNulificados++;

        var insertado = await OpinionLoader.InsertarAsync(
            conn, transaction, origenId, idClienteResuelto, idProducto, idFuente,
            fecha.Value, comentario, clasificacion, puntaje);

        if (insertado) resultado.Insertados++;
        else resultado.DuplicadosOmitidos++;
    }
}
