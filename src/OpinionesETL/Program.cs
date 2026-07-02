using Microsoft.Extensions.Configuration;
using OpinionesETL.Pipeline;
using OpinionesETL.Reports;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("SistemaOpiniones")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SistemaOpiniones' en appsettings.json");

var carpetaDatos = args.Length > 0 ? args[0] : EncontrarCarpetaDatos(config["RutaCarpetaDatos"]);

Console.WriteLine("=== Sistema de Análisis de Opiniones de Clientes - Proceso ETL ===");
Console.WriteLine($"Carpeta de datos: {carpetaDatos}");
Console.WriteLine();

var pipeline = new EtlPipeline(connectionString, carpetaDatos);
var resultados = await pipeline.EjecutarAsync();

Console.WriteLine();
Console.WriteLine("========== RESULTADO DEL PROCESO ETL POR FUENTE ==========");
var totalLeidos = 0;
var totalInsertados = 0;
var totalDuplicados = 0;
var totalRechazadosProducto = 0;
var totalRechazadosInvalidos = 0;

foreach (var r in resultados)
{
    Console.WriteLine($"- {r.NombreFuente}");
    Console.WriteLine($"    Leídos:                          {r.Leidos}");
    Console.WriteLine($"    Insertados:                      {r.Insertados}");
    Console.WriteLine($"    Duplicados omitidos:             {r.DuplicadosOmitidos}");
    Console.WriteLine($"    Rechazados (producto inválido):  {r.RechazadosSinProducto}");
    Console.WriteLine($"    Rechazados (datos inválidos):    {r.RechazadosDatosInvalidos}");
    Console.WriteLine($"    Clientes con Id inválido (-> NULL): {r.ClientesNulificados}");

    totalLeidos += r.Leidos;
    totalInsertados += r.Insertados;
    totalDuplicados += r.DuplicadosOmitidos;
    totalRechazadosProducto += r.RechazadosSinProducto;
    totalRechazadosInvalidos += r.RechazadosDatosInvalidos;
}

Console.WriteLine("------------------------------------------------------------");
Console.WriteLine(
    $"TOTAL leídos: {totalLeidos} | insertados: {totalInsertados} | duplicados: {totalDuplicados} | " +
    $"rechazados por producto: {totalRechazadosProducto} | rechazados por datos: {totalRechazadosInvalidos}");
Console.WriteLine("============================================================");

var reportService = new ReportService(connectionString);
await reportService.ImprimirResumenAsync();

static string EncontrarCarpetaDatos(string? rutaConfigurada)
{
    if (!string.IsNullOrWhiteSpace(rutaConfigurada) && Directory.Exists(rutaConfigurada))
        return Path.GetFullPath(rutaConfigurada);

    var directorio = new DirectoryInfo(AppContext.BaseDirectory);
    while (directorio is not null)
    {
        var candidato = Path.Combine(directorio.FullName, "Data");
        if (Directory.Exists(candidato) && File.Exists(Path.Combine(candidato, "clients.csv")))
            return candidato;
        directorio = directorio.Parent;
    }

    throw new DirectoryNotFoundException(
        "No se encontró la carpeta 'Data' con los archivos CSV de origen. " +
        "Pásala como argumento: dotnet run -- \"ruta/a/Data\"");
}
