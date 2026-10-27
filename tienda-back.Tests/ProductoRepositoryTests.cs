using Microsoft.EntityFrameworkCore;
using tienda_back.Domain.Entities;
using tienda_back.Infrastructure.Data;
using tienda_back.Infrastructure.Repositories;

namespace tienda_back.Tests;

public class ProductoRepositoryTests
{
    private PuntoPymeDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<PuntoPymeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PuntoPymeDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AgregaProductoYAsignaId()
    {
        using var context = CrearContexto();
        var repo = new ProductoRepository(context);
        var producto = new Producto { Nombre = "Cafe", Sku = "CAF-001", PrecioVenta = 25.50m, CostoCompra = 18.00m, Stock = 10, StockMinimo = 5 };

        var result = await repo.AddAsync(producto);

        Assert.True(result.Id > 0);
        Assert.Equal(1, await context.Productos.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_RetornaTodosLosProductos()
    {
        using var context = CrearContexto();
        context.Productos.AddRange(
            new Producto { Nombre = "Cafe", Sku = "CAF-001" },
            new Producto { Nombre = "Azucar", Sku = "AZU-001" }
        );
        await context.SaveChangesAsync();
        var repo = new ProductoRepository(context);

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_RetornaProductoCuandoExiste()
    {
        using var context = CrearContexto();
        var producto = new Producto { Nombre = "Cafe", Sku = "CAF-001" };
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
        var repo = new ProductoRepository(context);

        var result = await repo.GetByIdAsync(producto.Id);

        Assert.NotNull(result);
        Assert.Equal("CAF-001", result!.Sku);
    }

    [Fact]
    public async Task GetByIdAsync_RetornaNullCuandoNoExiste()
    {
        using var context = CrearContexto();
        var repo = new ProductoRepository(context);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ActualizaProducto()
    {
        using var context = CrearContexto();
        var producto = new Producto { Nombre = "Cafe", Sku = "CAF-001", PrecioVenta = 25.50m };
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
        var repo = new ProductoRepository(context);

        producto.PrecioVenta = 30.00m;
        await repo.UpdateAsync(producto);

        var actualizado = await context.Productos.FindAsync(producto.Id);
        Assert.Equal(30.00m, actualizado!.PrecioVenta);
    }

    [Fact]
    public async Task DeleteAsync_EliminaProductoCuandoExiste()
    {
        using var context = CrearContexto();
        var producto = new Producto { Nombre = "Cafe", Sku = "CAF-001" };
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
        var repo = new ProductoRepository(context);

        await repo.DeleteAsync(producto.Id);

        Assert.Equal(0, await context.Productos.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NoHaceNadaCuandoNoExiste()
    {
        using var context = CrearContexto();
        var repo = new ProductoRepository(context);

        await repo.DeleteAsync(999);

        Assert.Equal(0, await context.Productos.CountAsync());
    }
}
