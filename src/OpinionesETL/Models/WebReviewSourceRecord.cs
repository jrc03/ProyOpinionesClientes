namespace OpinionesETL.Models;

public class WebReviewSourceRecord
{
    public required string IdReview { get; init; }
    public string? IdCliente { get; init; }
    public required string IdProducto { get; init; }
    public DateTime Fecha { get; init; }
    public required string Comentario { get; init; }
    public int Rating { get; init; }
}
