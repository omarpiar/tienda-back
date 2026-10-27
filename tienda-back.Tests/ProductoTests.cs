using tienda_back.Models;

namespace tienda_back.Tests;

public class ProductoTests
{
    [Fact]
    public void Propiedades_SeAsignanYLeenCorrectamente()
    {
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Cafe",
            Sku = "CAF-001",
            PrecioVenta = 25.50m,
            CostoCompra = 18.00m,
            Stock = 10,
            StockMinimo = 5
        };

        Assert.Equal(1, producto.Id);
        Assert.Equal("Cafe", producto.Nombre);
        Assert.Equal("CAF-001", producto.Sku);
        Assert.Equal(25.50m, producto.PrecioVenta);
        Assert.Equal(18.00m, producto.CostoCompra);
        Assert.Equal(10, producto.Stock);
        Assert.Equal(5, producto.StockMinimo);
    }

    [Fact]
    public void ValoresPorDefecto_SonVacios()
    {
        var producto = new Producto();

        Assert.Equal(string.Empty, producto.Nombre);
        Assert.Equal(string.Empty, producto.Sku);
    }
}
