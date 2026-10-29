namespace PuntoPyme.Domain.Entities;

public class VentaDetalle
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }

    /// <summary>Precio con el que se vendió; no cambia si el producto cambia de precio después.</summary>
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
