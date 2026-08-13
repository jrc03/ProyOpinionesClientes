namespace OpinionesData.Models;

public sealed class Producto
{
    public required string IdProducto { get; init; }

    public required string Nombre { get; init; }

    public string? Categoria { get; init; }
}
