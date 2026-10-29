namespace PuntoPyme.Domain.Entities;

public class Venta
{
    public int Id { get; set; }

    /// <summary>Lo genera el dispositivo. Hace idempotente la sincronización (HU-07).</summary>
    public Guid IdLocal { get; set; }

    /// <summary>Hora real de la venta (no la de sincronización).</summary>
    public DateTime FechaVentaUtc { get; set; }
    public DateTime FechaRegistroUtc { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string MetodoPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal? MontoRecibido { get; set; }
    public decimal? Cambio { get; set; }

    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}
