namespace PuntoPyme.Application.Auth;

public record LoginRequest(string? NombreUsuario, string? Pin);

public record CambiarRolRequest(string? Rol);

public record UsuarioSesionDto(
    int Id, string NombreCompleto, string NombreUsuario,
    string Rol, IReadOnlyList<string> Roles);

public record LoginResponse(string Token, DateTime ExpiraEnUtc, UsuarioSesionDto Usuario);

public record TokenGenerado(string Token, DateTime ExpiraEnUtc);
