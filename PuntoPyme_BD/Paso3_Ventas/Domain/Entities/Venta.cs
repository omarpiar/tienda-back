using PuntoPyme.Domain.Enums;

namespace PuntoPyme.Domain.Entities;

public class Venta
{
    public int Id { get; set; }

    /// <summary>Identificador generado en el dispositivo. Evita duplicados al sincronizar.</summary>
    public Guid IdLocal { get; set; }

    /// <summary>Fecha y hora reales en que ocurrio la venta (UTC), aunque se sincronice despues.</summary>
    public DateTime FechaVentaUtc { get; set; }

    /// <summary>Fecha y hora en que la venta llego al servidor (UTC).</summary>
    public DateTime FechaRegistroUtc { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public MetodoPago MetodoPago { get; set; }
    public decimal Total { get; set; }
    public decimal? MontoRecibido { get; set; }
    public decimal? Cambio { get; set; }

    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}
