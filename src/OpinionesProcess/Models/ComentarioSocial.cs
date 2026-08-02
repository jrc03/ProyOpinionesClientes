using System.Text.Json.Serialization;

namespace OpinionesProcess.Models;

public sealed class ComentarioSocial
{
    [JsonPropertyName("idComment")]
    public string IdComment { get; set; } = string.Empty;

    [JsonPropertyName("idCliente")]
    public string? IdCliente { get; set; }

    [JsonPropertyName("idProducto")]
    public string? IdProducto { get; set; }

    [JsonPropertyName("fuente")]
    public string? Fuente { get; set; }

    [JsonPropertyName("fecha")]
    public string? Fecha { get; set; }

    [JsonPropertyName("comentario")]
    public string? Comentario { get; set; }
}
