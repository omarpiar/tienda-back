using Moq;
using tienda_back.Application.Interfaces;
using tienda_back.Application.Services;
using tienda_back.Domain.Entities;

namespace tienda_back.Tests;

public class ProductoServiceTests
{
    [Fact]
    public async Task GetAllAsync_RetornaListaDeProductos()
    {
        var mockRepo = new Mock<IProductoRepository>();
        var productos = new List<Producto>
        {
            new Producto { Id = 1, Nombre = "Cafe", Sku = "CAF-001", PrecioVenta = 25.50m, CostoCompra = 18.00m, Stock = 10, StockMinimo = 5 },
            new Producto { Id = 2, Nombre = "Azucar", Sku = "AZU-001", PrecioVenta = 15.00m, CostoCompra = 10.00m, Stock = 20, StockMinimo = 8 }
        };
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(productos);
        var service = new ProductoService(mockRepo.Object);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count());
        mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_RetornaProductoCuandoExiste()
    {
        var mockRepo = new Mock<IProductoRepository>();
        var producto = new Producto { Id = 1, Nombre = "Cafe", Sku = "CAF-001" };
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);
        var service = new ProductoService(mockRepo.Object);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Cafe", result!.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_RetornaNullCuandoNoExiste()
    {
        var mockRepo = new Mock<IProductoRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Producto?)null);
        var service = new ProductoService(mockRepo.Object);

        var result = await service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_LlamaRepositorioYRetornaProducto()
    {
        var mockRepo = new Mock<IProductoRepository>();
        var producto = new Producto { Id = 1, Nombre = "Te", Sku = "TE-001" };
        mockRepo.Setup(r => r.AddAsync(producto)).ReturnsAsync(producto);
        var service = new ProductoService(mockRepo.Object);

        var result = await service.CreateAsync(producto);

        Assert.Equal(producto, result);
        mockRepo.Verify(r => r.AddAsync(producto), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_LlamaRepositorio()
    {
        var mockRepo = new Mock<IProductoRepository>();
        var producto = new Producto { Id = 1, Nombre = "Cafe", Sku = "CAF-001" };
        var service = new ProductoService(mockRepo.Object);

        await service.UpdateAsync(producto);

        mockRepo.Verify(r => r.UpdateAsync(producto), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_LlamaRepositorio()
    {
        var mockRepo = new Mock<IProductoRepository>();
        var service = new ProductoService(mockRepo.Object);

        await service.DeleteAsync(1);

        mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}
