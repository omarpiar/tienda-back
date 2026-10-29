using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Productos;

/// <summary>Cuerpo de POST (API-05) y PUT (API-06). En PUT el stock se ignora: solo cambia al vender.</summary>
public record ProductoRequest(
    string? Nombre, string? Sku, string? Categoria,
    decimal? PrecioVenta, decimal? CostoCompra, int? Stock, int? StockMinimo);

/// <summary>HU-05 (agregar), HU-06 (editar) y búsqueda por código escaneado (API-04, HU-02).</summary>
public class ProductoService(IProductoRepository productos)
{
    public const int StockMinimoPorDefecto = 5;

    public async Task<ProductoListaDto> ObtenerPorSkuAsync(string sku, CancellationToken ct = default)
    {
        var producto = await productos.ObtenerPorSkuAsync(sku.Trim(), ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos un producto con ese código.");
        return ProductoMapeo.ALista(producto);
    }

    public async Task<ProductoDetalleDto> CrearAsync(ProductoRequest s, CancellationToken ct = default)
    {
        var (nombre, sku, categoria, precio, costo, stockMinimo) = Validar(s, esAlta: true);

        if (await productos.ExisteSkuAsync(sku, null, ct))
            throw AppException.Conflicto("SKU_DUPLICADO", "Ya existe un producto con ese código (SKU).");

        var producto = new Producto
        {
            Nombre = nombre, Sku = sku, Categoria = categoria,
            PrecioVenta = precio, CostoCompra = costo,
            Stock = s.Stock ?? 0, StockMinimo = stockMinimo
        };
        await productos.AgregarAsync(producto, ct);
        return ProductoMapeo.ADetalle(producto, incluirCostos: true);
    }

    public async Task<ProductoDetalleDto> EditarAsync(int id, ProductoRequest s, CancellationToken ct = default)
    {
        var producto = await productos.ObtenerPorIdParaEditarAsync(id, ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos ese producto.");

        var (nombre, sku, categoria, precio, costo, stockMinimo) = Validar(s, esAlta: false);

        if (await productos.ExisteSkuAsync(sku, id, ct))
            throw AppException.Conflicto("SKU_DUPLICADO", "Ya existe otro producto con ese código (SKU).");

        producto.Nombre = nombre;
        producto.Sku = sku;
        producto.Categoria = categoria;
        producto.PrecioVenta = precio;
        producto.CostoCompra = costo;
        producto.StockMinimo = stockMinimo;
        // El stock NO se toca aquí.

        await productos.GuardarCambiosAsync(ct);
        return ProductoMapeo.ADetalle(producto, incluirCostos: true);
    }

    private static (string Nombre, string Sku, string? Categoria, decimal Precio, decimal Costo, int StockMinimo)
        Validar(ProductoRequest s, bool esAlta)
    {
        var errores = new Dictionary<string, string[]>();
        var nombre = s.Nombre?.Trim() ?? "";
        var sku = s.Sku?.Trim() ?? "";
        var categoria = string.IsNullOrWhiteSpace(s.Categoria) ? null : s.Categoria.Trim();

        if (nombre.Length == 0) errores["nombre"] = ["Escribe el nombre del producto."];
        else if (nombre.Length > 150) errores["nombre"] = ["El nombre admite hasta 150 caracteres."];

        if (sku.Length == 0) errores["sku"] = ["Escribe el código (SKU)."];
        else if (sku.Length > 50) errores["sku"] = ["El código admite hasta 50 caracteres."];

        if (categoria is { Length: > 100 }) errores["categoria"] = ["La categoría admite hasta 100 caracteres."];

        if (s.PrecioVenta is null) errores["precioVenta"] = ["Escribe el precio de venta."];
        else if (s.PrecioVenta < 0) errores["precioVenta"] = ["El precio no puede ser negativo."];

        if (s.CostoCompra < 0) errores["costoCompra"] = ["El costo no puede ser negativo."];
        if (esAlta && s.Stock < 0) errores["stock"] = ["El stock no puede ser negativo."];
        if (s.StockMinimo < 0) errores["stockMinimo"] = ["El stock mínimo no puede ser negativo."];

        if (errores.Count > 0) throw AppException.Validacion(errores);

        var precio = Math.Round(s.PrecioVenta!.Value, 2);
        var costo = Math.Round(s.CostoCompra ?? 0m, 2);

        if (precio < costo)
            throw new AppException(400, "PRECIO_MENOR_AL_COSTO", "El precio de venta no puede ser menor al costo de compra.");

        return (nombre, sku, categoria, precio, costo, s.StockMinimo ?? StockMinimoPorDefecto);
    }
}
