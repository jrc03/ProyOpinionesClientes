using OpinionesData.Models;

namespace OpinionesProcess.Models;

public sealed class ResultadoTransformacionOpiniones
{
    public required IReadOnlyCollection<Opinion> Opiniones { get; init; }

    public int Leidos { get; init; }

    public int RechazadosDatosInvalidos { get; init; }

    public int RechazadosSinProducto { get; init; }

    public int RechazadosSinFuente { get; init; }

    public int ClientesNulificados { get; init; }

    public int DuplicadosOmitidos { get; init; }
}
