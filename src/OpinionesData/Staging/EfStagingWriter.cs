using Microsoft.EntityFrameworkCore;
using OpinionesData.Context;
using OpinionesData.Interfaces;
using OpinionesData.Models;

namespace OpinionesData.Staging;

public sealed class EfStagingWriter(
    IDbContextFactory<OpinionesDbContext> contextFactory) : IStagingWriter
{
    private readonly IDbContextFactory<OpinionesDbContext> _contextFactory = contextFactory;

    public async Task<int> WriteBatchAsync(
        IReadOnlyCollection<OpinionStaging> opiniones,
        CancellationToken cancellationToken = default)
    {
        if (opiniones.Count == 0)
            return 0;

        await using var context = await _contextFactory.CreateDbContextAsync(
            cancellationToken);

        await context.OpinionesStaging.AddRangeAsync(
            opiniones,
            cancellationToken);

        return await context.SaveChangesAsync(cancellationToken);
    }
}
