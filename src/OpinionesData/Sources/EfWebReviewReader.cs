using Microsoft.EntityFrameworkCore;
using OpinionesData.Context;
using OpinionesData.Interfaces;
using OpinionesData.Models;

namespace OpinionesData.Sources;

public sealed class EfWebReviewReader : IWebReviewReader
{
    private readonly IDbContextFactory<OpinionesDbContext> _contextFactory;

    public EfWebReviewReader(
        IDbContextFactory<OpinionesDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyCollection<ResenaWebOrigen>> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(
            cancellationToken);

        return await context.ResenasWebOrigen
            .FromSqlRaw("EXEC dbo.sp_ObtenerResenasWebOrigen")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
