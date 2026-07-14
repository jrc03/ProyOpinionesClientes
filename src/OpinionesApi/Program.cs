using Dapper;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://localhost:5000");

var connectionString = builder.Configuration.GetConnectionString("SistemaOpiniones");
var rutaComentariosSociales = builder.Configuration["RutaComentariosSociales"];
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"));
});

app.MapGet("/dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"));
});

app.MapGet("/api/social-comments", () =>
{
    var ruta = EncontrarArchivoComentarios(rutaComentariosSociales);
    return Results.File(ruta, "application/json");
});

app.MapGet("/api/dashboard/status", async () =>
{
    var (connected, error) = await CheckDbConnectionAsync(connectionString);
    return Results.Json(new { connected, error });
});

app.MapGet("/api/dashboard/kpis", async () =>
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        
        var row = await conn.QuerySingleAsync(
            """
            SELECT 
                COUNT(*) AS Total,
                COALESCE(SUM(CASE WHEN Clasificacion = 'Positiva' THEN 1 ELSE 0 END), 0) AS Positivas,
                COALESCE(SUM(CASE WHEN Clasificacion = 'Negativa' THEN 1 ELSE 0 END), 0) AS Negativas,
                COALESCE(SUM(CASE WHEN Clasificacion = 'Neutra' THEN 1 ELSE 0 END), 0) AS Neutras
            FROM Opiniones
            """
        );

        int total = row.Total;
        int positivas = row.Positivas;
        int negativas = row.Negativas;
        int neutras = row.Neutras;

        double satisfaccionGlobal = total > 0 
            ? (100.0 * positivas) / total 
            : 0.0;

        return Results.Json(new 
        {
            total,
            positivas,
            negativas,
            neutras,
            satisfaccionGlobal,
            dbConnected = true,
            errorMessage = (string?)null
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new 
        {
            total = 0,
            positivas = 0,
            negativas = 0,
            neutras = 0,
            satisfaccionGlobal = 0.0,
            dbConnected = false,
            errorMessage = ex.Message
        });
    }
});

app.MapGet("/api/dashboard/clasificacion", async () =>
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        var rows = await conn.QueryAsync(
            """
            SELECT Clasificacion AS clasificacion, COUNT(*) AS cantidad
            FROM Opiniones
            GROUP BY Clasificacion
            """
        );
        return Results.Json(rows);
    }
    catch (Exception)
    {
        return Results.Json(Array.Empty<object>());
    }
});

app.MapGet("/api/dashboard/productos", async () =>
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        var rows = await conn.QueryAsync(
            """
            SELECT TOP 10
                IdProducto AS idProducto,
                NombreProducto AS nombreProducto,
                Categoria AS categoria,
                TotalOpiniones AS totalOpiniones,
                Positivas AS positivas,
                Negativas AS negativas,
                Neutras AS neutras,
                COALESCE(PorcentajeSatisfaccion, 0.0) AS porcentajeSatisfaccion,
                COALESCE(PuntajePromedio, 0.0) AS puntajePromedio
            FROM vw_ResumenPorProducto
            ORDER BY TotalOpiniones DESC
            """
        );
        return Results.Json(rows);
    }
    catch (Exception)
    {
        return Results.Json(Array.Empty<object>());
    }
});

app.MapGet("/api/dashboard/tendencia", async () =>
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        var rows = await conn.QueryAsync(
            """
            SELECT
                Mes AS mes,
                SUM(TotalOpiniones) AS totalOpiniones,
                SUM(Positivas) AS positivas,
                SUM(Negativas) AS negativas,
                SUM(Neutras) AS neutras,
                COALESCE(CAST(100.0 * SUM(Positivas) / NULLIF(SUM(TotalOpiniones), 0) AS DECIMAL(5,2)), 0.0) AS porcentajeSatisfaccion,
                COALESCE(AVG(PuntajePromedio), 0.0) AS puntajePromedio
            FROM vw_TendenciaSatisfaccionMensual
            GROUP BY Mes
            ORDER BY Mes
            """
        );
        return Results.Json(rows);
    }
    catch (Exception)
    {
        return Results.Json(Array.Empty<object>());
    }
});

app.Run();

static async Task<(bool Connected, string? Error)> CheckDbConnectionAsync(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return (false, "No se ha configurado la cadena de conexión 'SistemaOpiniones'.");
    }

    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        return (true, null);
    }
    catch (Exception ex)
    {
        return (false, ex.Message);
    }
}

static string EncontrarArchivoComentarios(string? rutaConfigurada)
{
    if (!string.IsNullOrWhiteSpace(rutaConfigurada) && File.Exists(rutaConfigurada))
        return Path.GetFullPath(rutaConfigurada);

    var directorio = new DirectoryInfo(AppContext.BaseDirectory);
    while (directorio is not null)
    {
        var candidato = Path.Combine(directorio.FullName, "Data", "api", "social-comments.json");
        if (File.Exists(candidato))
            return candidato;

        directorio = directorio.Parent;
    }

    throw new FileNotFoundException(
        "No se encontró Data/api/social-comments.json. Genera o copia el archivo antes de iniciar la API.");
}
