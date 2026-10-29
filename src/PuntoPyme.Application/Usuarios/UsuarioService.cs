using System.Text.RegularExpressions;
using PuntoPyme.Application.Auth;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Usuarios;

public record UsuarioDto(int Id, string NombreCompleto, string NombreUsuario, string Rol, bool Activo);

public record CrearUsuarioRequest(string? NombreCompleto, string? NombreUsuario, string? Pin, string? Rol);

/// <summary>El PIN es opcional: si no viene, el actual no cambia.</summary>
public record EditarUsuarioRequest(string? NombreCompleto, string? NombreUsuario, string? Rol, string? Pin);

public record CambiarEstadoRequest(bool? Activo);

/// <summary>HU-09 (agregar usuario) y HU-10 (editar o desactivar): API-09 a API-13.</summary>
public partial class UsuarioService(
    IUsuarioRepository usuarios,
    IPinHasher hasher,
    TimeProvider reloj)
{
    [GeneratedRegex(@"^\d{4,6}$")]
    private static partial Regex PinValido();

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(string? buscar, string? rol, bool? activo, CancellationToken ct = default)
    {
        if (rol is not null && !Roles.Todos.Contains(rol))
            throw AppException.Validacion("rol", "Usa Administrador o Cajero.");

        var lista = await usuarios.ListarAsync(string.IsNullOrWhiteSpace(buscar) ? null : buscar.Trim(), rol, activo, ct);
        return lista.Select(ADto).ToList();
    }

    public async Task<UsuarioDto> ObtenerAsync(int id, CancellationToken ct = default) =>
        ADto(await Existente(id, ct));

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest s, CancellationToken ct = default)
    {
        var nombreUsuario = NombreUsuarioNormalizador.Normalizar(s.NombreUsuario);
        var errores = ValidarComunes(s.NombreCompleto, nombreUsuario, s.Rol);
        if (s.Pin is null || !PinValido().IsMatch(s.Pin)) errores["pin"] = ["El PIN debe tener de 4 a 6 dígitos."];
        if (errores.Count > 0) throw AppException.Validacion(errores);

        if (await usuarios.ExisteNombreUsuarioAsync(nombreUsuario, null, ct))
            throw AppException.Conflicto("USUARIO_DUPLICADO", "Ya existe un usuario con ese nombre.");

        var usuario = new Usuario
        {
            NombreCompleto = s.NombreCompleto!.Trim(),
            NombreUsuario = nombreUsuario,
            PinHash = hasher.Hash(s.Pin!),
            RolId = Roles.IdDe(s.Rol!),
            Activo = true,
            FechaCreacionUtc = reloj.GetUtcNow().UtcDateTime
        };
        await usuarios.AgregarAsync(usuario, ct);
        return ADto(usuario);
    }

    public async Task<UsuarioDto> EditarAsync(int id, EditarUsuarioRequest s, CancellationToken ct = default)
    {
        var usuario = await Existente(id, ct);

        var nombreUsuario = NombreUsuarioNormalizador.Normalizar(s.NombreUsuario);
        var errores = ValidarComunes(s.NombreCompleto, nombreUsuario, s.Rol);
        var cambiaPin = !string.IsNullOrEmpty(s.Pin);
        if (cambiaPin && !PinValido().IsMatch(s.Pin!)) errores["pin"] = ["El PIN debe tener de 4 a 6 dígitos."];
        if (errores.Count > 0) throw AppException.Validacion(errores);

        if (await usuarios.ExisteNombreUsuarioAsync(nombreUsuario, id, ct))
            throw AppException.Conflicto("USUARIO_DUPLICADO", "Ya existe otro usuario con ese nombre.");

        var nuevoRolId = Roles.IdDe(s.Rol!);
        var dejaDeSerAdmin = usuario.RolId == Roles.AdministradorId && nuevoRolId != Roles.AdministradorId;
        if (dejaDeSerAdmin && usuario.Activo && await usuarios.ContarAdministradoresActivosAsync(ct) <= 1)
            throw UltimoAdministrador();

        usuario.NombreCompleto = s.NombreCompleto!.Trim();
        usuario.NombreUsuario = nombreUsuario;
        usuario.RolId = nuevoRolId; // aplica desde el siguiente inicio de sesión de esa persona
        if (cambiaPin)
        {
            usuario.PinHash = hasher.Hash(s.Pin!);
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHastaUtc = null;
        }

        await usuarios.ActualizarAsync(usuario, ct);
        return ADto(usuario);
    }

    public async Task<UsuarioDto> CambiarEstadoAsync(int id, bool? activo, int usuarioActualId, CancellationToken ct = default)
    {
        if (activo is null) throw AppException.Validacion("activo", "Indica si el usuario queda activo o inactivo.");

        var usuario = await Existente(id, ct);

        if (activo == false && usuario.Activo)
        {
            if (id == usuarioActualId)
                throw AppException.Conflicto("NO_PUEDES_DESACTIVARTE", "No puedes desactivar tu propia cuenta.");

            if (usuario.RolId == Roles.AdministradorId && await usuarios.ContarAdministradoresActivosAsync(ct) <= 1)
                throw UltimoAdministrador();
        }

        usuario.Activo = activo.Value;
        if (activo == true)
        {
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHastaUtc = null;
        }

        await usuarios.ActualizarAsync(usuario, ct);
        return ADto(usuario);
    }

    private async Task<Usuario> Existente(int id, CancellationToken ct) =>
        await usuarios.ObtenerPorIdAsync(id, ct)
        ?? throw AppException.NoEncontrado("USUARIO_NO_ENCONTRADO", "No encontramos ese usuario.");

    private static Dictionary<string, string[]> ValidarComunes(string? nombreCompleto, string nombreUsuario, string? rol)
    {
        var errores = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(nombreCompleto)) errores["nombreCompleto"] = ["Escribe el nombre completo."];
        else if (nombreCompleto.Trim().Length > 150) errores["nombreCompleto"] = ["El nombre admite hasta 150 caracteres."];

        if (nombreUsuario.Length == 0) errores["nombreUsuario"] = ["Escribe el usuario o celular."];
        else if (nombreUsuario.Length > 50) errores["nombreUsuario"] = ["El usuario admite hasta 50 caracteres."];

        if (rol is null || !Roles.Todos.Contains(rol)) errores["rol"] = ["Elige Administrador o Cajero."];
        return errores;
    }

    private static AppException UltimoAdministrador() =>
        AppException.Conflicto("ULTIMO_ADMINISTRADOR", "El sistema debe conservar al menos un administrador activo.");

    private static UsuarioDto ADto(Usuario u) =>
        new(u.Id, u.NombreCompleto, u.NombreUsuario, Roles.NombreDe(u.RolId), u.Activo);
}
