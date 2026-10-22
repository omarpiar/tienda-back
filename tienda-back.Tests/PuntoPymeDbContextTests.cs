using Microsoft.EntityFrameworkCore;
using tienda_back.Data;
using tienda_back.Models;

namespace tienda_back.Tests;

public class PuntoPymeDbContextTests
{
    private static PuntoPymeDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<PuntoPymeDbContext>()
            .UseNpgsql("Host=localhost;Database=tienda_test;Username=test;Password=test")
            .Options;

        return new PuntoPymeDbContext(options);
    }

    [Fact]
    public void Productos_ExponeDbSet()
    {
        using var context = CrearContexto();

        Assert.NotNull(context.Productos);
    }

    [Fact]
    public void Modelo_ConfiguraStockMinimoPorDefectoEIndiceUnicoDeSku()
    {
        using var context = CrearContexto();
        var entity = context.Model.FindEntityType(typeof(Producto));

        Assert.NotNull(entity);

        var stockMinimo = entity.FindProperty(nameof(Producto.StockMinimo));
        Assert.NotNull(stockMinimo);
        Assert.Equal(5, stockMinimo.GetDefaultValue());

        var skuIndex = entity.GetIndexes()
            .Single(i => i.Properties.Any(p => p.Name == nameof(Producto.Sku)));

        Assert.True(skuIndex.IsUnique);
    }
}
