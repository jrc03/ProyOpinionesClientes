using Dapper;
using Microsoft.Data.SqlClient;
using OpinionesETL.Extractors;
using OpinionesETL.Loaders;
using OpinionesETL.Models;
using OpinionesETL.Transformers;
using OpinionesETL.Validation;

namespace OpinionesETL.Pipeline;

public class EtlPipeline(string connectionString, string carpetaDatos)
{
    private readonly string _connectionString = connectionString;
    private readonly string _carpetaDatos = carpetaDatos;
    private readonly DimensionLoader _dimensionLoader = new DimensionLoader(connectionString);

    public async Task<List<EtlSourceResult>> EjecutarAsync()
    {
        // Las dimensiones se cargan primero para validar las FK de Opiniones.
        var clientesCrudos = CsvExtractor.Leer<ClienteRecord>(Path.Combine(_carpetaDatos, "clients.csv"));
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
        await _dimensionLoader.CargarClientesAsync(clientes);

        var productosCrudos = CsvExtractor.Leer<ProductoRecord>(Path.Combine(_carpetaDatos, "products.csv"));
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
        await _dimensionLoader.CargarProductosAsync(productos);

        using var connValidacion = new SqlConnection(_connectionString);
        await connValidacion.OpenAsync();
        var idsClientes = await connValidacion.QueryAsync<string>("SELECT IdCliente FROM Clientes");
        var idsProductos = await connValidacion.QueryAsync<string>("SELECT IdProducto FROM Productos");
        var validador = new ReferentialValidator(idsClientes, idsProductos);

        var resultados = new List<EtlSourceResult>();

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        resultados.Add(await ProcesarEncuestasAsync(conn, validador));
        resultados.Add(await ProcesarResenasWebAsync(conn, validador));
        resultados.Add(await ProcesarComentariosSocialesAsync(conn, validador));

        return resultados;
    }

    private async Task<EtlSourceResult> ProcesarEncuestasAsync(SqlConnection conn, ReferentialValidator validador)
    {
        var resultado = new EtlSourceResult { NombreFuente = "Encuestas internas (CSV)" };
        var idFuente = await _dimensionLoader.ObtenerOCrearFuenteAsync("EncuestaInterna");

        var registros = CsvExtractor.Leer<SurveyRecord>(Path.Combine(_carpetaDatos, "surveys_part1.csv"));
        foreach (var r in registros)
        {
            var clasificacion = DataCleaner.NormalizarClasificacion(r.Clasificacion);
            int? puntaje = int.TryParse(r.PuntajeSatisfaccion, out var p) ? p : null;

            await ProcesarFilaAsync(conn, validador, idFuente,
                origenId: r.IdOpinion,
                idClienteCrudo: r.IdCliente,
                idProductoCrudo: r.IdProducto,
                fechaCruda: r.Fecha,
                comentarioCrudo: r.Comentario,
                clasificacion: clasificacion,
                puntaje: puntaje,
                resultado: resultado);
        }
        return resultado;
    }

    private async Task<EtlSourceResult> ProcesarResenasWebAsync(SqlConnection conn, ReferentialValidator validador)
    {
        var resultado = new EtlSourceResult { NombreFuente = "Reseñas web (CSV)" };
        var idFuente = await _dimensionLoader.ObtenerOCrearFuenteAsync("ReseñaWeb");

        var registros = CsvExtractor.Leer<WebReviewRecord>(Path.Combine(_carpetaDatos, "web_reviews.csv"));
        foreach (var r in registros)
        {
            int? rating = int.TryParse(r.Rating, out var rv) ? rv : null;
            var clasificacion = rating is not null ? RatingClassifier.Clasificar(rating.Value) : "Neutra";

            await ProcesarFilaAsync(conn, validador, idFuente,
                origenId: r.IdReview,
                idClienteCrudo: r.IdCliente,
                idProductoCrudo: r.IdProducto,
                fechaCruda: r.Fecha,
                comentarioCrudo: r.Comentario,
                clasificacion: clasificacion,
                puntaje: rating,
                resultado: resultado);
        }
        return resultado;
    }

    private async Task<EtlSourceResult> ProcesarComentariosSocialesAsync(SqlConnection conn, ReferentialValidator validador)
    {
        var resultado = new EtlSourceResult { NombreFuente = "Comentarios en redes sociales (CSV, simula API REST)" };
        var idFuente = await _dimensionLoader.ObtenerOCrearFuenteAsync("RedSocial");

        var registros = CsvExtractor.Leer<SocialCommentRecord>(Path.Combine(_carpetaDatos, "social_comments.csv"));
        foreach (var r in registros)
        {
            var comentarioLimpio = DataCleaner.Limpiar(r.Comentario);
            var clasificacion = KeywordSentimentClassifier.Clasificar(comentarioLimpio);

            await ProcesarFilaAsync(conn, validador, idFuente,
                origenId: r.IdComment,
                idClienteCrudo: r.IdCliente,
                idProductoCrudo: r.IdProducto,
                fechaCruda: r.Fecha,
                comentarioCrudo: r.Comentario,
                clasificacion: clasificacion,
                puntaje: null,
                resultado: resultado);
        }
        return resultado;
    }

    private static async Task ProcesarFilaAsync(
        SqlConnection conn,
        ReferentialValidator validador,
        int idFuente,
        string? origenId,
        string? idClienteCrudo,
        string idProductoCrudo,
        string? fechaCruda,
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

        var fecha = DataCleaner.ParsearFecha(fechaCruda);
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
        // Una opinión puede cargarse sin cliente, pero nunca sin producto.
        if (idClienteNormalizado is not null && validador.ClienteExiste(idClienteNormalizado))
            idClienteResuelto = idClienteNormalizado;
        else
            resultado.ClientesNulificados++;

        var insertado = await OpinionLoader.InsertarAsync(
            conn, origenId, idClienteResuelto, idProducto, idFuente,
            fecha.Value, comentario, clasificacion, puntaje);

        if (insertado) resultado.Insertados++;
        else resultado.DuplicadosOmitidos++;
    }
}
