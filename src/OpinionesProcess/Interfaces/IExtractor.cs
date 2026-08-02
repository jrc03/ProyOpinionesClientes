using OpinionesData.Models;

namespace OpinionesProcess.Interfaces
{
    public interface IExtractor
    {
        string NombreFuente { get; }

        Task<IReadOnlyCollection<OpinionStaging>> ExtractAsync(
            Guid loteId,
            CancellationToken cancellationToken = default);
    }
}
