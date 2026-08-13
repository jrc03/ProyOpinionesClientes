namespace OpinionesData.Models;

public sealed class ReferenciasCargaOpinion
{
    public required IReadOnlySet<string> IdsClientes { get; init; }

    public required IReadOnlySet<string> IdsProductos { get; init; }

    public required IReadOnlyDictionary<string, int> FuentesPorTipo { get; init; }
}
