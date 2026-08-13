namespace OpinionesData.Models;

public sealed class Opinion
{
    public int IdOpinion { get; private set; }

    public string? IdCliente { get; init; }

    public required string IdProducto { get; init; }

    public int IdFuente { get; init; }

    public DateTime Fecha { get; init; }

    public required string Comentario { get; init; }

    public required string Clasificacion { get; init; }

    public int? PuntajeSatisfaccion { get; init; }

    public string? OrigenId { get; init; }
}