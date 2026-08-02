using OpinionesData.Interfaces;
using OpinionesData.Models;
using OpinionesProcess.Interfaces;

namespace OpinionesProcess.Extractors;

public sealed class DatabaseExtractor : IExtractor
{
    private readonly IWebReviewReader _reader;

    public DatabaseExtractor(IWebReviewReader reader)
    {
        _reader = reader;
    }

    public string NombreFuente => "ReseñaWeb";

    public async Task<IReadOnlyCollection<OpinionStaging>> ExtractAsync(
        Guid loteId,
        CancellationToken cancellationToken = default)
    {
        var records = await _reader.ReadAsync(cancellationToken);
        var extractedAtUtc = DateTime.UtcNow;

        return records
            .Select(registro => new OpinionStaging
            {
                LoteId = loteId,
                Fuente = NombreFuente,
                OrigenId = registro.IdReview,
                IdCliente = registro.IdCliente,
                IdProducto = registro.IdProducto,
                Fecha = registro.Fecha,
                Comentario = registro.Comentario,
                ClasificacionOrigen = null,
                PuntajeOrigen = registro.Rating,
                FechaExtraccionUtc = extractedAtUtc
            })
            .ToList();
    }
}
