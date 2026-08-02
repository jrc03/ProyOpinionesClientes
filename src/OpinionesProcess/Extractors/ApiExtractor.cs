using System.Net.Http.Json;
using System.Text.Json;
using OpinionesData.Models;
using OpinionesProcess.Interfaces;
using OpinionesProcess.Models;

namespace OpinionesProcess.Extractors;

public sealed class ApiExtractor : IExtractor
{
    public const string HttpClientName = "SocialCommentsApi";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public ApiExtractor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string NombreFuente => "RedSocial";

    public async Task<IReadOnlyCollection<OpinionStaging>> ExtractAsync(
        Guid loteId,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        try
        {
            var registros = await client.GetFromJsonAsync<List<ComentarioSocial>>(
                string.Empty,
                JsonOptions,
                cancellationToken) ?? [];

            var extractedAtUtc = DateTime.UtcNow;
            return registros
                .Select(registro => new OpinionStaging
                {
                    LoteId = loteId,
                    Fuente = NombreFuente,
                    OrigenId = registro.IdComment,
                    IdCliente = registro.IdCliente,
                    IdProducto = registro.IdProducto,
                    Fecha = ExtractorValueParser.ParseDate(registro.Fecha),
                    Comentario = registro.Comentario,
                    ClasificacionOrigen = null,
                    PuntajeOrigen = null,
                    FechaExtraccionUtc = extractedAtUtc
                })
                .ToList();
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException(
                "No se pudo consumir la API REST de comentarios sociales.",
                exception);
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                "La API REST de comentarios sociales no respondió a tiempo.",
                exception);
        }
    }
}
