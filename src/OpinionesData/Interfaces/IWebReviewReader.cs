using OpinionesData.Models;

namespace OpinionesData.Interfaces;

public interface IWebReviewReader
{
    Task<IReadOnlyCollection<ResenaWebOrigen>> ReadAsync(
        CancellationToken cancellationToken = default);
}
