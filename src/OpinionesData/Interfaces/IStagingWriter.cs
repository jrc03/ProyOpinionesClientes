using OpinionesData.Models;

namespace OpinionesData.Interfaces
{
    public interface IStagingWriter
    {
        Task<int> WriteBatchAsync(
            IReadOnlyCollection<OpinionStaging> opiniones,
            CancellationToken cancellationToken = default);
    }
}
