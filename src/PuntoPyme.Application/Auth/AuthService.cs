using System.Text.RegularExpressions;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Constants;

namespace PuntoPyme.Application.Auth;

/// <summary>HU-01 / API-01. Las reglas siguen el orden de la sección 7.1 del catálogo de APIs.</summary>
public partial class AuthService(
    IUsuarioRepository usuarios,
    IPinHasher hasher,
    ITokenService tokens,
    TimeProvider reloj)
{
    public const int MaxIntentos = 5;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(1);
    public const string MensajeCredenciales = "Usuario o PIN incorrectos";

    [GeneratedRegex(@"^\d{4,6}$")]
    private static partial Regex PinValido();

    public async Task<LoginResponse> LoginAsync(LoginRequest solicitud, CancellationToken ct = default)
    {
        ValidarFormato(solicitud);

        var nombre = NombreUsuarioNormalizador.Normalizar(solicitud.NombreUsuario);
        var ahora = reloj.GetUtcNow().UtcDateTime;

        var usuario = await usuarios.ObtenerPorNombreUsuarioAsync(nombre, ct);
        if (usuario is null || !usuario.Activo)
            throw CredencialesInvalidas();

        if (usuario.BloqueadoHastaUtc is { } hasta && hasta > ahora)
        {
            var segundos = (int)Math.Ceiling((hasta - ahora).TotalSeconds);
            throw new AppException(429, "USUARIO_BLOQUEADO",
                $"Demasiados intentos. Intenta de nuevo en {segundos} segundos.")
            { RetryAfterSegundos = segundos };
        }

        if (!hasher.Verificar(solicitud.Pin!, usuario.PinHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= MaxIntentos)
            {
                usuario.BloqueadoHastaUtc = ahora + DuracionBloqueo;
                usuario.IntentosFallidos = 0;
            }
            await usuarios.ActualizarAsync(usuario, ct);
            throw CredencialesInvalidas();
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;
        await usuarios.ActualizarAsync(usuario, ct);

        // D2: un Administrador entra como Administrador o Cajero; un Cajero solo como Cajero.
        if (solicitud.Rol == Roles.Administrador && usuario.RolId != Roles.AdministradorId)
            throw new AppException(403, "ROL_NO_PERMITIDO", "Tu usuario no tiene permiso para entrar como Administrador.");

        var token = tokens.Generar(usuario, solicitud.Rol!);

        return new LoginResponse(
            token.Token,
            token.ExpiraEnUtc,
            new UsuarioSesionDto(usuario.Id, usuario.NombreCompleto, usuario.NombreUsuario, solicitud.Rol!));
    }

    private static void ValidarFormato(LoginRequest s)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(s.NombreUsuario))
            errores["nombreUsuario"] = ["Escribe tu usuario."];

        if (s.Pin is null || !PinValido().IsMatch(s.Pin))
            errores["pin"] = ["El PIN debe tener de 4 a 6 dígitos."];

        if (s.Rol is null || !Roles.Todos.Contains(s.Rol))
            errores["rol"] = ["Elige Administrador o Cajero."];

        if (errores.Count > 0)
            throw new AppException(400, "VALIDACION", "Los datos enviados no son válidos.") { Errores = errores };
    }

    // Mismo error para usuario inexistente, inactivo o PIN incorrecto: no revela cuál falló.
    private static AppException CredencialesInvalidas() =>
        new(401, "CREDENCIALES_INVALIDAS", MensajeCredenciales);
}
