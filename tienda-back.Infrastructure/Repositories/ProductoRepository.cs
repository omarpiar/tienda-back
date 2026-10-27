using Microsoft.EntityFrameworkCore;
using tienda_back.Application.Interfaces;
using tienda_back.Domain.Entities;
using tienda_back.Infrastructure.Data;

namespace tienda_back.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly PuntoPymeDbContext _context;

    public ProductoRepository(PuntoPymeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _context.Productos.ToListAsync();
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        return await _context.Productos.FindAsync(id);
    }

    public async Task<Producto> AddAsync(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task UpdateAsync(Producto producto)
    {
        _context.Entry(producto).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
    }
}