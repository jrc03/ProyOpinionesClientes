using OpinionesData.Models;

namespace OpinionesData.Interfaces;

public interface IOpinionFactWriter
{
    Task<int> ReplaceAllAsync(
        IReadOnlyCollection<Opinion> opiniones,
        CancellationToken cancellationToken = default);
}