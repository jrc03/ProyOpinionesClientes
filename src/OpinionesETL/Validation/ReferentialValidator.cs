namespace OpinionesETL.Validation;

public class ReferentialValidator(IEnumerable<string> clientesValidos, IEnumerable<string> productosValidos)
{
    private readonly HashSet<string> _clientesValidos = new HashSet<string>(clientesValidos);
    private readonly HashSet<string> _productosValidos = new HashSet<string>(productosValidos);

    public bool ClienteExiste(string? idCliente) =>
        idCliente is not null && _clientesValidos.Contains(idCliente);

    public bool ProductoExiste(string? idProducto) =>
        idProducto is not null && _productosValidos.Contains(idProducto);
}
