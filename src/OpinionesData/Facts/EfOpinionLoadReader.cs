using Microsoft.EntityFrameworkCore;
using OpinionesData.Context;
using OpinionesData.Interfaces;
using OpinionesData.Models;

namespace OpinionesData.Facts;

public sealed class EfOpinionLoadReader(
    IDbContextFactory<OpinionesDbContext> contextFactory)
    : IOpinionLoadReader
{
    private readonly IDbContextFactory<OpinionesDbContext> _contextFactory =
        contextFactory;

    public async Task<IReadOnlyCollection<OpinionStaging>> ReadStagingBatchAsync(
        Guid loteId,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.OpinionesStaging
            .AsNoTracking()
            .Where(opinion => opinion.LoteId == loteId)
            .OrderBy(opinion => opinion.IdStaging)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferenciasCargaOpinion> ReadReferencesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        var idsClientes = await context.Clientes
            .AsNoTracking()
            .Select(cliente => cliente.IdCliente)
            .ToListAsync(cancellationToken);

        var idsProductos = await context.Productos
            .AsNoTracking()
            .Select(producto => producto.IdProducto)
            .ToListAsync(cancellationToken);

        var fuentes = await context.FuentesDatos
            .AsNoTracking()
            .Select(fuente => new { fuente.TipoFuente, fuente.IdFuente })
            .ToListAsync(cancellationToken);

        return new ReferenciasCargaOpinion
        {
            IdsClientes = idsClientes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            IdsProductos = idsProductos.ToHashSet(StringComparer.OrdinalIgnoreCase),
            FuentesPorTipo = fuentes.ToDictionary(
                fuente => fuente.TipoFuente,
                fuente => fuente.IdFuente,
                StringComparer.OrdinalIgnoreCase)
        };
    }
}
