namespace OpinionesProcess.Models;

public sealed class ResultadoExtraccionFuente
{
    public required string NombreFuente { get; init; }

    public Guid LoteId { get; init; }

    public int Extraidos { get; init; }

    public int GuardadosEnStaging { get; init; }

    public long DuracionMilisegundos { get; init; }

    public bool Exitoso { get; init; }

    public string? Error { get; init; }
}
