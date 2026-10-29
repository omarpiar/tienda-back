namespace PuntoPyme.Application.Productos;

public static class EstadoStock
{
    public const string Normal = "Normal";
    public const string Bajo = "Bajo";
    public const string Agotado = "Agotado";

    /// <summary>Agotado si es 0; Bajo si es mayor que 0 y menor o igual al mínimo; Normal en los demás casos.</summary>
    public static string Calcular(int stock, int stockMinimo) =>
        stock <= 0 ? Agotado
        : stock <= stockMinimo ? Bajo
        : Normal;
}

public static class FiltroEstado
{
    public const string Todos = "todos";
    public const string StockBajo = "stockBajo";
    public const string Agotados = "agotados";

    public static string? Canonico(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return Todos;
        if (valor.Equals(Todos, StringComparison.OrdinalIgnoreCase)) return Todos;
        if (valor.Equals(StockBajo, StringComparison.OrdinalIgnoreCase)) return StockBajo;
        if (valor.Equals(Agotados, StringComparison.OrdinalIgnoreCase)) return Agotados;
        return null;
    }
}
