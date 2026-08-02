namespace OpinionesData.Models;

public sealed class OpinionStaging
{
    public long IdStaging { get; private set; }

    public Guid LoteId { get; init; }

    public required string Fuente { get; init; }

    public string? OrigenId { get; init; }

    public string? IdCliente { get; init; }

    public string? IdProducto { get; init; }

    public DateTime? Fecha { get; init; }

    public string? Comentario { get; init; }

    public string? ClasificacionOrigen { get; init; }

    public int? PuntajeOrigen { get; init; }

    public DateTime FechaExtraccionUtc { get; init; } = DateTime.UtcNow;

    public string Estado { get; private set; } = "Pendiente";
}
