using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuntoPyme.Application.Auth;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Data;

public static class DbSeeder
{
    /// <summary>
    /// Solo Desarrollo. Crea el primer administrador si no existe ninguno (Seed:AdminUsuario / Seed:AdminPin)
    /// y, si Seed:Demo es true, un cajero de prueba y productos de ejemplo.
    /// </summary>
    public static async Task SembrarAsync(IServiceProvider servicios, IConfiguration config, CancellationToken ct = default)
    {
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PuntoPymeDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPinHasher>();
        var ahora = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;

        if (!await db.Usuarios.AnyAsync(u => u.RolId == Roles.AdministradorId, ct))
        {
            var nombreUsuario = NombreUsuarioNormalizador.Normalizar(config["Seed:AdminUsuario"]);
            var pin = config["Seed:AdminPin"];
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(pin))
                throw new InvalidOperationException(
                    "No hay administradores y faltan Seed:AdminUsuario / Seed:AdminPin (ver LEEME.md).");

            db.Usuarios.Add(Crear(config["Seed:AdminNombre"] ?? "Administrador PuntoPyme", nombreUsuario, pin, Roles.AdministradorId, hasher, ahora));
            await db.SaveChangesAsync(ct);
        }

        if (!config.GetValue<bool>("Seed:Demo")) return;

        var cajero = NombreUsuarioNormalizador.Normalizar(config["Seed:CajeroUsuario"] ?? "laura");
        if (!await db.Usuarios.AnyAsync(u => u.NombreUsuario == cajero, ct))
        {
            db.Usuarios.Add(Crear("Laura Gómez", cajero, config["Seed:CajeroPin"] ?? "4821", Roles.CajeroId, hasher, ahora));
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Productos.AnyAsync(ct))
        {
            db.Productos.AddRange(ProductosDemo());
            await db.SaveChangesAsync(ct);
        }
    }

    private static Usuario Crear(string nombre, string usuario, string pin, int rolId, IPinHasher hasher, DateTime ahora) => new()
    {
        NombreCompleto = nombre, NombreUsuario = usuario, PinHash = hasher.Hash(pin),
        RolId = rolId, Activo = true, FechaCreacionUtc = ahora
    };

    private static Producto[] ProductosDemo() =>
    [
        P("Refresco 600ml", "7501055300013", "Bebidas", 18, 13, 40, 10),
        P("Agua natural 1L", "7501055300020", "Bebidas", 12, 8, 60, 12),
        P("Pan blanco", "7501000100011", "Panadería", 32, 24, 6, 8),
        P("Detergente 1L", "7501025400017", "Limpieza", 45, 33, 22, 6),
        P("Aceite Vegetal 900ml", "770123456789", "Abarrotes", 75, 58, 3, 5),
        P("Arroz 1kg", "7501000200028", "Abarrotes", 28, 21, 35, 8),
        P("Frijol negro 1kg", "7501000200035", "Abarrotes", 38, 29, 0, 8),
        P("Leche entera 1L", "7501020500042", "Lácteos", 27, 21, 18, 10),
        P("Huevo 12 piezas", "7501000300059", "Abarrotes", 52, 42, 4, 6),
        P("Papel higiénico 4 rollos", "7501025400064", "Limpieza", 36, 27, 25, 6),
        P("Jabón de barra", "7501025400071", "Limpieza", 14, 10, 2, 6),
        P("Galletas surtidas", "7501000400086", "Botanas", 22, 16, 30, 8),
    ];

    private static Producto P(string nombre, string sku, string cat, decimal precio, decimal costo, int stock, int minimo) => new()
    {
        Nombre = nombre, Sku = sku, Categoria = cat, PrecioVenta = precio, CostoCompra = costo, Stock = stock, StockMinimo = minimo
    };
}
