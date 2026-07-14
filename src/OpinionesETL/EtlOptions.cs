namespace OpinionesETL;

public class EtlOptions
{
    public string ConnectionString { get; set; } = "";
    public string CarpetaDatos { get; set; } = "";
    public string ApiComentariosSocialesUrl { get; set; } = "";
    public int EtlIntervaloSegundos { get; set; } = 120;
    public int EtlDelayInicialSegundos { get; set; } = 3;
    public string ClientesCsv { get; set; } = "clients.csv";
    public string ProductosCsv { get; set; } = "products.csv";
    public string EncuestasCsv { get; set; } = "surveys_part1.csv";
}
