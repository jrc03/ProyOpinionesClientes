using CsvHelper.Configuration.Attributes;

namespace OpinionesETL.Models;

public class ClienteRecord
{
    [Name("IdCliente")] public string IdCliente { get; set; } = "";
    [Name("Nombre")] public string Nombre { get; set; } = "";
    [Name("Email")] public string? Email { get; set; }
}
