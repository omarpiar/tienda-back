using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Ventas;

public interface IVentaRepository
{
    /// <summary>Venta con Usuario y Detalles.Producto cargados, o null.</summary>
    Task<Venta?> ObtenerPorIdLocalAsync(Guid idLocal, CancellationToken ct = default);

    /// <summary>
    /// En una sola transacción: descuenta el stock con una actualización condicional
    /// (Stock >= cantidad) e inserta la venta con sus renglones. Si algún producto no alcanza,
    /// deshace todo y devuelve los faltantes. Si el idLocal ya existe (carrera), devuelve esa venta con Creada = false.
    /// </summary>
    Task<RegistroVentaResultado> RegistrarAsync(Venta venta, CancellationToken ct = default);
}
