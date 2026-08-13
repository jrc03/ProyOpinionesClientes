using OpinionesData.Models;
using OpinionesProcess.Models;

namespace OpinionesProcess.Transformers;

public sealed class OpinionTransformer
{
    public ResultadoTransformacionOpiniones Transform(
        IReadOnlyCollection<OpinionStaging> stagingRecords,
        ReferenciasCargaOpinion references)
    {
        ArgumentNullException.ThrowIfNull(stagingRecords);
        ArgumentNullException.ThrowIfNull(references);

        var opiniones = new List<Opinion>(stagingRecords.Count);
        var uniqueOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var invalidData = 0;
        var missingProducts = 0;
        var missingSources = 0;
        var nullifiedClients = 0;
        var duplicates = 0;

        foreach (var record in stagingRecords)
        {
            var comentario = record.Comentario?.Trim();
            if (string.IsNullOrWhiteSpace(comentario) || record.Fecha is null)
            {
                invalidData++;
                continue;
            }

            var idProducto = IdNormalizer.Normalize(record.IdProducto);
            if (idProducto is null || !references.IdsProductos.Contains(idProducto))
            {
                missingProducts++;
                continue;
            }

            if (!references.FuentesPorTipo.TryGetValue(record.Fuente, out var idFuente))
            {
                missingSources++;
                continue;
            }

            var origenId = string.IsNullOrWhiteSpace(record.OrigenId)
                ? null
                : record.OrigenId.Trim();

            if (origenId is not null &&
                !uniqueOrigins.Add($"{idFuente}:{origenId}"))
            {
                duplicates++;
                continue;
            }

            var idCliente = IdNormalizer.Normalize(record.IdCliente);
            if (idCliente is not null && !references.IdsClientes.Contains(idCliente))
            {
                idCliente = null;
                nullifiedClients++;
            }

            opiniones.Add(new Opinion
            {
                IdCliente = idCliente,
                IdProducto = idProducto,
                IdFuente = idFuente,
                Fecha = record.Fecha.Value,
                Comentario = comentario,
                Clasificacion = SentimentClassifier.Classify(
                    comentario,
                    record.ClasificacionOrigen,
                    record.PuntajeOrigen),
                PuntajeSatisfaccion = record.PuntajeOrigen is >= 1 and <= 5
                    ? record.PuntajeOrigen
                    : null,
                OrigenId = origenId
            });
        }

        return new ResultadoTransformacionOpiniones
        {
            Opiniones = opiniones,
            Leidos = stagingRecords.Count,
            RechazadosDatosInvalidos = invalidData,
            RechazadosSinProducto = missingProducts,
            RechazadosSinFuente = missingSources,
            ClientesNulificados = nullifiedClients,
            DuplicadosOmitidos = duplicates
        };
    }
}
