using Microsoft.EntityFrameworkCore;
using OpinionesData.Context;
using OpinionesData.Interfaces;
using OpinionesData.Models;

namespace OpinionesData.Facts;

public sealed class EfOpinionFactWriter(
    IDbContextFactory<OpinionesDbContext> contextFactory)
    : IOpinionFactWriter
{
    private readonly IDbContextFactory<OpinionesDbContext> _contextFactory =
        contextFactory;

    public async Task<int> ReplaceAllAsync(
        IReadOnlyCollection<Opinion> opiniones,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opiniones);

        if (opiniones.Count == 0)
        {
            throw new ArgumentException(
                "No se puede reemplazar la tabla de hechos con una colección vacía.",
                nameof(opiniones));
        }

        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        await using var transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE dbo.Opiniones;",
                cancellationToken);

            await context.Opiniones.AddRangeAsync(
                opiniones,
                cancellationToken);

            var insertedCount =
                await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return insertedCount;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}