using System.Net.Http.Json;
using System.Text.Json;
using OpinionesETL.Models;

namespace OpinionesETL.Extractors;

public class SocialCommentsApiExtractor
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public SocialCommentsApiExtractor(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SocialCommentRecord>> LeerAsync()
    {
        try
        {
            // Petición a la dirección base configurada (BaseAddress)
            var registros = await _http.GetFromJsonAsync<List<SocialCommentRecord>>("", JsonOptions);
            return registros ?? [];
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"No se pudo consumir la API REST de comentarios sociales a través del HttpClient.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException(
                $"La API REST de comentarios sociales no respondió a tiempo.", ex);
        }
    }
}
