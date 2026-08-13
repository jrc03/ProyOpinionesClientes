namespace OpinionesData.Models;

public sealed class FuenteDato
{
    public int IdFuente { get; private set; }

    public required string TipoFuente { get; init; }

    public DateTime? FechaCarga { get; init; }
}
