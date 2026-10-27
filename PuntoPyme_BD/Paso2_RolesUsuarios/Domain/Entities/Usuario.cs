namespace PuntoPyme.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Usuario o celular. Se guarda en minusculas y es unico.</summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Hash del PIN. Nunca se guarda el PIN en texto plano.</summary>
    public string PinHash { get; set; } = string.Empty;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public bool Activo { get; set; } = true;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHastaUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}
