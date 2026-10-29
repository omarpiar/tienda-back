namespace PuntoPyme.Application.Auth;

public record LoginRequest(string? NombreUsuario, string? Pin, string? Rol);

public record UsuarioSesionDto(int Id, string NombreCompleto, string NombreUsuario, string Rol);

public record LoginResponse(string Token, DateTime ExpiraEnUtc, UsuarioSesionDto Usuario);

public record TokenGenerado(string Token, DateTime ExpiraEnUtc);
