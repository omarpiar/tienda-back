using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuntoPyme.Application.Auth;
using PuntoPyme.Application.Productos;
using PuntoPyme.Application.Reportes;
using PuntoPyme.Application.Ventas;
using PuntoPyme.Infrastructure.Repositories;
using PuntoPyme.Infrastructure.Seguridad;

namespace PuntoPyme.Infrastructure;

public static class DependencyInjection
{
    /// <summary>No registra el DbContext: se registra en Program.cs con la cadena de conexión del ambiente.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Seccion));
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IPinHasher, BCryptPinHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IReporteRepository, ReporteRepository>();
        return services;
    }
}
