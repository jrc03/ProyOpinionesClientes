namespace OpinionesETL.Models;

public class EtlSourceResult
{
    public required string NombreFuente { get; init; }

    public int Leidos { get; set; }
    public int Insertados { get; set; }
    public int DuplicadosOmitidos { get; set; }
    public int RechazadosSinProducto { get; set; }
    public int RechazadosDatosInvalidos { get; set; }

    public int ClientesNulificados { get; set; }
}
