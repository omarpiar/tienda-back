namespace PuntoPyme.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal CostoCompra { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 5;
    public bool Eliminado { get; set; }
    public ICollection<ProductoHistorial> Historial { get; set; } = [];
}
