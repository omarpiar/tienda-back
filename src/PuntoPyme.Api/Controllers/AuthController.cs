using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoPyme.Api.Seguridad;
using PuntoPyme.Application.Auth;

namespace PuntoPyme.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>API-01 (HU-01): inicia sesión y devuelve el token con el rol activo.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest solicitud, CancellationToken ct) =>
        Ok(await auth.LoginAsync(solicitud, ct));

    /// <summary>Cambia el rol activo de una sesión ya iniciada y devuelve un token nuevo.</summary>
    [HttpPost("rol")]
    [Authorize(Policy = Politicas.Autenticado)]
    public async Task<ActionResult<LoginResponse>> CambiarRol(CambiarRolRequest solicitud, CancellationToken ct) =>
        Ok(await auth.CambiarRolAsync(User.Id(), solicitud.Rol, ct));
}
