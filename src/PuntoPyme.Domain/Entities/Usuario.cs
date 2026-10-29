namespace PuntoPyme.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Usuario o celular. Siempre en minúsculas y sin espacios.</summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Hash BCrypt del PIN. Nunca el PIN.</summary>
    public string PinHash { get; set; } = string.Empty;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public bool Activo { get; set; } = true;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHastaUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}
