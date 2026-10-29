using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Productos;

/// <summary>Compartido con Omar (API-04, API-05, API-06 devuelven estas mismas formas).</summary>
public static class ProductoMapeo
{
    public static ProductoListaDto ALista(Producto p) =>
        new(p.Id, p.Nombre, p.Sku, p.Categoria, p.PrecioVenta, p.Stock, p.StockMinimo,
            EstadoStock.Calcular(p.Stock, p.StockMinimo));

    public static ProductoDetalleDto ADetalle(Producto p, bool incluirCostos)
    {
        decimal? costo = null, ganancia = null, margen = null;
        if (incluirCostos)
        {
            costo = p.CostoCompra;
            ganancia = p.PrecioVenta - p.CostoCompra;
            // D3: margen sobre el precio de venta, 1 decimal (17 de 75 = 22.7 %)
            margen = p.PrecioVenta == 0
                ? 0m
                : Math.Round(ganancia.Value / p.PrecioVenta * 100m, 1, MidpointRounding.AwayFromZero);
        }

        return new ProductoDetalleDto(p.Id, p.Nombre, p.Sku, p.Categoria, p.PrecioVenta,
            costo, ganancia, margen, p.Stock, p.StockMinimo, EstadoStock.Calcular(p.Stock, p.StockMinimo));
    }
}
