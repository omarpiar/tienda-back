using tienda_back.Application.Interfaces;
using tienda_back.Domain.Entities;

namespace tienda_back.Application.Services;

public class ProductoService
{
    private readonly IProductoRepository _productoRepository;

    public ProductoService(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _productoRepository.GetAllAsync();
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        return await _productoRepository.GetByIdAsync(id);
    }

    public async Task<Producto> CreateAsync(Producto producto)
    {
        return await _productoRepository.AddAsync(producto);
    }

    public async Task UpdateAsync(Producto producto)
    {
        await _productoRepository.UpdateAsync(producto);
    }

    public async Task DeleteAsync(int id)
    {
        await _productoRepository.DeleteAsync(id);
    }
}