using Microsoft.Extensions.DependencyInjection;
using PuntoPyme.Application.Auth;
using PuntoPyme.Application.Productos;
using PuntoPyme.Application.Reportes;
using PuntoPyme.Application.Usuarios;
using PuntoPyme.Application.Ventas;

namespace PuntoPyme.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<ProductoConsultaService>();
        services.AddScoped<ProductoService>();
        services.AddScoped<VentaService>();
        services.AddScoped<ReporteService>();
        services.AddScoped<UsuarioService>();
        return services;
    }
}
