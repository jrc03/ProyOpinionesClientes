using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using OpinionesETL;
using OpinionesETL.Extractors;
using OpinionesETL.Loaders;
using OpinionesETL.Pipeline;
using OpinionesETL.Reports;
using OpinionesETL.Workers;

var options = new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};

var builder = Host.CreateApplicationBuilder(options);

var connectionString = builder.Configuration.GetConnectionString("SistemaOpiniones")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SistemaOpiniones' en appsettings.json");

var rutaConfigurada = builder.Configuration["RutaCarpetaDatos"];
var carpetaDatos = EncontrarCarpetaDatos(rutaConfigurada);

var apiComentariosSocialesUrl = builder.Configuration["ApiComentariosSocialesUrl"]
    ?? throw new InvalidOperationException("Falta configurar ApiComentariosSocialesUrl en appsettings.json");

int.TryParse(builder.Configuration["EtlIntervaloSegundos"], out var intervaloSegundos);
if (intervaloSegundos <= 0) intervaloSegundos = 120;

int.TryParse(builder.Configuration["EtlDelayInicialSegundos"], out var delayInicialSegundos);
if (delayInicialSegundos < 0) delayInicialSegundos = 3;

builder.Services.Configure<EtlOptions>(opt =>
{
    opt.ConnectionString = connectionString;
    opt.CarpetaDatos = carpetaDatos;
    opt.ApiComentariosSocialesUrl = apiComentariosSocialesUrl;
    opt.EtlIntervaloSegundos = intervaloSegundos;
    opt.EtlDelayInicialSegundos = delayInicialSegundos;
    
    // Binding de nombres de CSV configurables con fallbacks
    opt.ClientesCsv = builder.Configuration["ClientesCsv"] ?? "clients.csv";
    opt.ProductosCsv = builder.Configuration["ProductosCsv"] ?? "products.csv";
    opt.EncuestasCsv = builder.Configuration["EncuestasCsv"] ?? "surveys_part1.csv";
});

builder.Services.AddHttpClient<SocialCommentsApiExtractor>(client =>
{
    client.BaseAddress = new Uri(apiComentariosSocialesUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddTransient<DimensionLoader>();
builder.Services.AddTransient<EtlPipeline>();
builder.Services.AddTransient<ReportService>();
builder.Services.AddHostedService<EtlBackgroundWorker>();

var host = builder.Build();
await host.RunAsync();

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
        "No se encontró la carpeta Data. Indica la ruta al ejecutar o en appsettings.json");
}
