namespace PuntoPyme.Domain.Constants;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Cajero = "Cajero";

    // Ids sembrados con la tabla Roles
    public const int AdministradorId = 1;
    public const int CajeroId = 2;

    public static readonly string[] Todos = [Administrador, Cajero];

    public static int IdDe(string nombre) => nombre == Administrador ? AdministradorId : CajeroId;
    public static string NombreDe(int id) => id == AdministradorId ? Administrador : Cajero;
}

public static class MetodosPago
{
    public const string Efectivo = "Efectivo";
    public const string Tarjeta = "Tarjeta";
    public static readonly string[] Todos = [Efectivo, Tarjeta];
}
