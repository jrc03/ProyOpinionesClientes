namespace OpinionesETL.Models;

public class Cliente
{
    public required string IdCliente { get; init; }
    public required string Nombre { get; init; }
    public string? Email { get; init; }
}
