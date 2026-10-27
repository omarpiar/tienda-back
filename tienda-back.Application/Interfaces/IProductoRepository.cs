using tienda_back.Domain.Entities;

namespace tienda_back.Application.Interfaces;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync();
    Task<Producto?> GetByIdAsync(int id);
    Task<Producto> AddAsync(Producto producto);
    Task UpdateAsync(Producto producto);
    Task DeleteAsync(int id);
}