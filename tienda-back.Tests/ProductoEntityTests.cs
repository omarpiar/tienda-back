using tienda_back.Domain.Entities;

namespace tienda_back.Tests;

public class ProductoEntityTests
{
    [Fact]
    public void Producto_PropiedadesSeAsignanCorrectamente()
    {
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Monitor",
            Sku = "MON-001",
            PrecioVenta = 1500.00m,
            CostoCompra = 1200.00m,
            Stock = 15,
            StockMinimo = 3
        };

        Assert.Equal(1, producto.Id);
        Assert.Equal("Monitor", producto.Nombre);
        Assert.Equal("MON-001", producto.Sku);
        Assert.Equal(1500.00m, producto.PrecioVenta);
        Assert.Equal(1200.00m, producto.CostoCompra);
        Assert.Equal(15, producto.Stock);
        Assert.Equal(3, producto.StockMinimo);
    }

    [Fact]
    public void Producto_ValoresPorDefecto()
    {
        var producto = new Producto();

        Assert.Equal(0, producto.Id);
        Assert.Equal(string.Empty, producto.Nombre);
        Assert.Equal(string.Empty, producto.Sku);
        Assert.Equal(0m, producto.PrecioVenta);
        Assert.Equal(0m, producto.CostoCompra);
        Assert.Equal(0, producto.Stock);
        Assert.Equal(0, producto.StockMinimo);
    }
}
