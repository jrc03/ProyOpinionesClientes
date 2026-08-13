using OpinionesData.Models;

namespace OpinionesData.Interfaces;

public interface IOpinionLoadReader
{
    Task<IReadOnlyCollection<OpinionStaging>> ReadStagingBatchAsync(
        Guid loteId,
        CancellationToken cancellationToken = default);

    Task<ReferenciasCargaOpinion> ReadReferencesAsync(
        CancellationToken cancellationToken = default);
}
