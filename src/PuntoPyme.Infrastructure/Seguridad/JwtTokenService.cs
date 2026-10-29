using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PuntoPyme.Application.Auth;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Infrastructure.Seguridad;

/// <summary>Claims: sub (id de usuario), name (nombre de usuario) y role (rol activo).</summary>
public class JwtTokenService(IOptions<JwtOptions> opciones, TimeProvider reloj) : ITokenService
{
    private readonly JwtOptions _o = opciones.Value;

    public TokenGenerado Generar(Usuario usuario, string rolActivo)
    {
        if (string.IsNullOrEmpty(_o.Key) || _o.Key.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Key debe tener al menos 32 caracteres. Configúrala con dotnet user-secrets.");

        var ahora = reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddMinutes(_o.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _o.Issuer,
            Audience = _o.Audience,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", usuario.Id.ToString()),
                new Claim("name", usuario.NombreUsuario),
                new Claim("role", rolActivo)
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        return new TokenGenerado(new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
