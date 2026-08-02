using CsvHelper.Configuration.Attributes;

namespace OpinionesProcess.Models;

public sealed class RegistroEncuesta
{
    [Name("IdOpinion")]
    public string IdOpinion { get; set; } = string.Empty;

    [Name("IdCliente")]
    public string IdCliente { get; set; } = string.Empty;

    [Name("IdProducto")]
    public string IdProducto { get; set; } = string.Empty;

    [Name("Fecha")]
    public string Fecha { get; set; } = string.Empty;

    [Name("Comentario")]
    public string Comentario { get; set; } = string.Empty;

    [Name("Clasificación")]
    public string Clasificacion { get; set; } = string.Empty;

    [Name("PuntajeSatisfacción")]
    public string PuntajeSatisfaccion { get; set; } = string.Empty;

    [Name("Fuente")]
    public string Fuente { get; set; } = string.Empty;
}
