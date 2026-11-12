namespace PuntoPyme.Application.Categorias;

public record CategoriaDto(int Id, string Nombre, int Productos);

public record CategoriaRequest(string? Nombre);
