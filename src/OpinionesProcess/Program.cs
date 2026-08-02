using OpinionesData;
using OpinionesProcess.Configuration;
using OpinionesProcess.Extractors;
using OpinionesProcess.Interfaces;
using OpinionesProcess.Pipeline;
using OpinionesProcess.Workers;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SistemaOpiniones")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'SistemaOpiniones'.");

var apiUrl = builder.Configuration[$"{ExtractionOptions.SectionName}:SocialApiUrl"]
    ?? throw new InvalidOperationException(
        "Falta configurar Extraction:SocialApiUrl.");

builder.Services.AddOptions<ExtractionOptions>()
    .Bind(builder.Configuration.GetSection(ExtractionOptions.SectionName))
    .Validate(opt => !string.IsNullOrWhiteSpace(opt.SocialApiUrl) && Uri.IsWellFormedUriString(opt.SocialApiUrl, UriKind.Absolute),
        "Extraction:SocialApiUrl debe ser una URL válida.")
    .Validate(opt => opt.IntervalSeconds > 0,
        "Extraction:IntervalSeconds debe ser mayor que 0.")
    .Validate(opt => !string.IsNullOrWhiteSpace(opt.SurveyFile),
        "Extraction:SurveyFile es obligatorio.")
    .ValidateOnStart();



builder.Services.AddHttpClient(ApiExtractor.HttpClientName, client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddOpinionesData(connectionString);

builder.Services.AddSingleton<IExtractor, CsvExtractor>();
builder.Services.AddSingleton<IExtractor, DatabaseExtractor>();
builder.Services.AddSingleton<IExtractor, ApiExtractor>();
builder.Services.AddSingleton<ExtractionPipeline>();
builder.Services.AddHostedService<ExtractionWorker>();

var host = builder.Build();
await host.RunAsync();
