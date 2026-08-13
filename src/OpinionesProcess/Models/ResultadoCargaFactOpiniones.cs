namespace OpinionesProcess.Models;

public sealed class ResultadoCargaFactOpiniones
{
    public Guid LoteId { get; init; }

    public int Leidos { get; init; }

    public int Cargados { get; init; }

    public int RechazadosDatosInvalidos { get; init; }

    public int RechazadosSinProducto { get; init; }

    public int RechazadosSinFuente { get; init; }

    public int ClientesNulificados { get; init; }

    public int DuplicadosOmitidos { get; init; }

    public long DuracionMilisegundos { get; init; }
}
