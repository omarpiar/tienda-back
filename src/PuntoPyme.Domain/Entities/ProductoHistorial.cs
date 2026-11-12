namespace PuntoPyme.Domain.Entities;

public class ProductoHistorial
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Accion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
}
