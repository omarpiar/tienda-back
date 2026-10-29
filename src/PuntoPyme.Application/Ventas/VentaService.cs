using PuntoPyme.Application.Common;
using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Ventas;

/// <summary>HU-03 (cobrar) y HU-07 (sincronizar ventas hechas sin conexión): API-07.</summary>
public class VentaService(IVentaRepository ventas, IProductoRepository productos, TimeProvider reloj)
{
    public static readonly TimeSpan MargenFuturo = TimeSpan.FromMinutes(5);

    public async Task<ResultadoVenta> RegistrarAsync(RegistrarVentaRequest s, int usuarioId, CancellationToken ct = default)
    {
        var fechaVenta = ValidarEstructura(s);
        var ahora = reloj.GetUtcNow().UtcDateTime;

        if (fechaVenta > ahora + MargenFuturo)
            throw AppException.Validacion("fechaVentaUtc", "La fecha de la venta no puede estar en el futuro.");

        // Idempotencia (HU-07 criterio 4): el mismo idLocal no duplica ni vuelve a descontar.
        var existente = await ventas.ObtenerPorIdLocalAsync(s.IdLocal!.Value, ct);
        if (existente is not null)
            return new ResultadoVenta(VentaMapeo.ADto(existente), Creada: false);

        // Renglones repetidos del mismo producto se suman.
        var pedidos = s.Detalles!
            .GroupBy(d => d.ProductoId)
            .Select(g => (ProductoId: g.Key, Cantidad: g.Sum(x => x.Cantidad)))
            .ToList();

        var encontrados = await productos.ObtenerPorIdsAsync(pedidos.Select(p => p.ProductoId).ToList(), ct);
        var porId = encontrados.ToDictionary(p => p.Id);

        var inexistentes = pedidos.Where(p => !porId.ContainsKey(p.ProductoId)).Select(p => p.ProductoId).ToList();
        if (inexistentes.Count > 0)
            throw new AppException(400, "PRODUCTO_NO_ENCONTRADO", "Algunos productos del carrito ya no existen.")
            { Extensiones = new Dictionary<string, object?> { ["productoIds"] = inexistentes } };

        var faltantes = pedidos
            .Where(p => p.Cantidad > porId[p.ProductoId].Stock)
            .Select(p => new StockFaltante(p.ProductoId, porId[p.ProductoId].Nombre, porId[p.ProductoId].Stock, p.Cantidad))
            .ToList();
        if (faltantes.Count > 0) throw StockInsuficiente(faltantes);

        // D1: el servidor pone el precio vigente.
        var detalles = pedidos.Select(p =>
        {
            var precio = porId[p.ProductoId].PrecioVenta;
            return new VentaDetalle { ProductoId = p.ProductoId, Cantidad = p.Cantidad, PrecioUnitario = precio, Subtotal = precio * p.Cantidad };
        }).ToList();

        var total = detalles.Sum(d => d.Subtotal);
        decimal? recibido = null, cambio = null;

        if (s.MetodoPago == MetodosPago.Efectivo)
        {
            if (s.MontoRecibido is null || s.MontoRecibido < total)
                throw new AppException(400, "MONTO_INSUFICIENTE", "El monto recibido no alcanza para cubrir el total.");
            recibido = s.MontoRecibido;
            cambio = s.MontoRecibido - total;
        }

        var venta = new Venta
        {
            IdLocal = s.IdLocal!.Value,
            FechaVentaUtc = fechaVenta,
            FechaRegistroUtc = ahora,
            UsuarioId = usuarioId,
            MetodoPago = s.MetodoPago!,
            Total = total,
            MontoRecibido = recibido,
            Cambio = cambio,
            Detalles = detalles
        };

        var resultado = await ventas.RegistrarAsync(venta, ct);
        if (resultado.Venta is null) throw StockInsuficiente(resultado.Faltantes);

        return new ResultadoVenta(VentaMapeo.ADto(resultado.Venta), resultado.Creada);
    }

    private static DateTime ValidarEstructura(RegistrarVentaRequest s)
    {
        var errores = new Dictionary<string, string[]>();

        if (s.IdLocal is null || s.IdLocal == Guid.Empty) errores["idLocal"] = ["Falta el identificador de la venta."];
        if (s.FechaVentaUtc is null) errores["fechaVentaUtc"] = ["Falta la fecha de la venta."];
        if (s.MetodoPago is null || !MetodosPago.Todos.Contains(s.MetodoPago))
            errores["metodoPago"] = ["Elige Efectivo o Tarjeta."];
        if (errores.Count > 0) throw AppException.Validacion(errores);

        if (s.Detalles is null || s.Detalles.Count == 0)
            throw new AppException(400, "CARRITO_VACIO", "Agrega al menos un producto para cobrar.");

        if (s.Detalles.Any(d => d.Cantidad <= 0))
            throw AppException.Validacion("detalles", "Cada cantidad debe ser un entero mayor que 0.");

        var f = s.FechaVentaUtc!.Value;
        return f.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(f, DateTimeKind.Utc) : f.ToUniversalTime();
    }

    private static AppException StockInsuficiente(IReadOnlyList<StockFaltante> faltantes) =>
        new(409, "STOCK_INSUFICIENTE", "Algunos productos no tienen existencias suficientes.")
        { Extensiones = new Dictionary<string, object?> { ["faltantes"] = faltantes } };
}
