using CsvHelper.Configuration.Attributes;

namespace OpinionesETL.Models;

public class ProductoRecord
{
    [Name("IdProducto")] public string IdProducto { get; set; } = "";
    [Name("Nombre")] public string Nombre { get; set; } = "";
    [Name("Categoría")] public string? Categoria { get; set; }
}
