using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Auth;

public interface IUsuarioRepository
{
    /// <summary>Usuario rastreado por EF, o null. El nombre ya viene normalizado.</summary>
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuarioNormalizado, CancellationToken ct = default);

    /// <summary>Usuario rastreado por EF, o null.</summary>
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Usuario>> ListarAsync(string? buscar, string? rol, bool? activo, CancellationToken ct = default);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuarioNormalizado, int? excluirId, CancellationToken ct = default);
    Task<int> ContarAdministradoresActivosAsync(CancellationToken ct = default);
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);

    /// <summary>Guarda los cambios de un usuario ya consultado.</summary>
    Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
}

public interface IPinHasher
{
    string Hash(string pin);
    bool Verificar(string pin, string hash);
}

public interface ITokenService
{
    /// <summary>Genera el JWT con el rol activo (el elegido en el login).</summary>
    TokenGenerado Generar(Usuario usuario, string rolActivo);
}
