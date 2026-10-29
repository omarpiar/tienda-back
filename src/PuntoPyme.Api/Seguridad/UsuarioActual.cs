using System.Security.Claims;

namespace PuntoPyme.Api.Seguridad;

public static class UsuarioActual
{
    /// <summary>Id del usuario del token (claim "sub").</summary>
    public static int Id(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue("sub"), out var id) ? id : 0;
}
