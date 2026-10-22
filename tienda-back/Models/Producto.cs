namespace tienda_back.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public decimal CostoCompra { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}
