namespace OpinionesData.Models;

public sealed class ResenaWebOrigen
{
    public required string IdReview { get; init; }

    public string? IdCliente { get; init; }

    public required string IdProducto { get; init; }

    public DateTime Fecha { get; init; }

    public required string Comentario { get; init; }

    public int Rating { get; init; }
}
