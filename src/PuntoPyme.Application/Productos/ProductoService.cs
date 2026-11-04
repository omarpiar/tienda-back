using PuntoPyme.Application.Categorias;
using PuntoPyme.Application.Common;
using PuntoPyme.Domain.Constants;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Application.Productos;

/// <summary>Cuerpo de POST (API-05) y PUT (API-06). En PUT el stock se ignora: solo cambia al vender o reponer.</summary>
public record ProductoRequest(
    string? Nombre, string? Sku, int? CategoriaId,
    decimal? PrecioVenta, decimal? CostoCompra, int? Stock, int? StockMinimo);

public record ReponerStockRequest(int? Cantidad);

/// <summary>HU-05 (agregar), HU-06 (editar), reposición, eliminación e historial.</summary>
public class ProductoService(
    IProductoRepository productos,
    ICategoriaRepository categorias,
    IProductoHistorialRepository historial,
    TimeProvider reloj)
{
    public const int StockMinimoPorDefecto = 5;

    public async Task<ProductoListaDto> ObtenerPorSkuAsync(string sku, CancellationToken ct = default)
    {
        var producto = await productos.ObtenerPorSkuAsync(sku.Trim(), ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos un producto con ese código.");
        return ProductoMapeo.ALista(producto);
    }

    public async Task<ProductoDetalleDto> CrearAsync(ProductoRequest s, int usuarioId, CancellationToken ct = default)
    {
        var (nombre, sku, precio, costo, stockMinimo) = Validar(s, esAlta: true);
        var categoria = await ResolverCategoriaAsync(s.CategoriaId, ct);

        if (await productos.ExisteSkuAsync(sku, null, ct))
            throw AppException.Conflicto("SKU_DUPLICADO", "Ya existe un producto con ese código (SKU).");

        var producto = new Producto
        {
            Nombre = nombre, Sku = sku, CategoriaId = categoria?.Id, Categoria = categoria,
            PrecioVenta = precio, CostoCompra = costo,
            Stock = s.Stock ?? 0, StockMinimo = stockMinimo
        };
        await productos.AgregarAsync(producto, ct);
        await RegistrarAsync(producto.Id, usuarioId, AccionesProducto.Creacion,
            $"Producto creado con stock inicial {producto.Stock}.", 0, producto.Stock, ct);
        return ProductoMapeo.ADetalle(producto, incluirCostos: true);
    }

    public async Task<ProductoDetalleDto> EditarAsync(int id, ProductoRequest s, int usuarioId, CancellationToken ct = default)
    {
        var producto = await productos.ObtenerPorIdParaEditarAsync(id, ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos ese producto.");

        var (nombre, sku, precio, costo, stockMinimo) = Validar(s, esAlta: false);
        var categoria = await ResolverCategoriaAsync(s.CategoriaId, ct);

        if (await productos.ExisteSkuAsync(sku, id, ct))
            throw AppException.Conflicto("SKU_DUPLICADO", "Ya existe otro producto con ese código (SKU).");

        var cambios = new List<string>();
        if (producto.Nombre != nombre) cambios.Add("nombre");
        if (producto.Sku != sku) cambios.Add("SKU");
        if (producto.CategoriaId != categoria?.Id) cambios.Add("categoría");
        if (producto.PrecioVenta != precio) cambios.Add("precio de venta");
        if (producto.CostoCompra != costo) cambios.Add("costo de compra");
        if (producto.StockMinimo != stockMinimo) cambios.Add("stock mínimo");

        producto.Nombre = nombre;
        producto.Sku = sku;
        producto.CategoriaId = categoria?.Id;
        producto.Categoria = categoria;
        producto.PrecioVenta = precio;
        producto.CostoCompra = costo;
        producto.StockMinimo = stockMinimo;
        // El stock NO se toca aquí.

        await productos.GuardarCambiosAsync(ct);
        await RegistrarAsync(producto.Id, usuarioId, AccionesProducto.Actualizacion,
            cambios.Count > 0 ? $"Cambios: {string.Join(", ", cambios)}." : "Sin cambios en los datos.",
            producto.Stock, producto.Stock, ct);
        return ProductoMapeo.ADetalle(producto, incluirCostos: true);
    }

    public async Task<ProductoDetalleDto> ReponerStockAsync(int id, ReponerStockRequest s, int usuarioId, CancellationToken ct = default)
    {
        if (s.Cantidad is null or <= 0)
            throw AppException.Validacion("cantidad", "Indica una cantidad mayor que cero.");

        var producto = await productos.ObtenerPorIdParaEditarAsync(id, ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos ese producto.");

        var anterior = producto.Stock;
        producto.Stock += s.Cantidad.Value;

        await productos.GuardarCambiosAsync(ct);
        await RegistrarAsync(producto.Id, usuarioId, AccionesProducto.ReposicionStock,
            $"Se repusieron {s.Cantidad.Value} unidades.", anterior, producto.Stock, ct);

        return ProductoMapeo.ADetalle(producto, incluirCostos: true);
    }

    public async Task EliminarAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        var producto = await productos.ObtenerPorIdParaEditarAsync(id, ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos ese producto.");

        producto.Eliminado = true;
        await productos.GuardarCambiosAsync(ct);
        await RegistrarAsync(producto.Id, usuarioId, AccionesProducto.Eliminacion,
            "Producto eliminado del inventario. Su historial de ventas se conserva.",
            producto.Stock, producto.Stock, ct);
    }

    public async Task<IReadOnlyList<ProductoHistorialDto>> ObtenerHistorialAsync(int id, CancellationToken ct = default)
    {
        _ = await productos.ObtenerPorIdAsync(id, ct)
            ?? throw AppException.NoEncontrado("PRODUCTO_NO_ENCONTRADO", "No encontramos ese producto.");

        var registros = await historial.ListarAsync(id, ct);
        return registros.Select(ProductoMapeo.AHistorial).ToList();
    }

    private async Task<Categoria?> ResolverCategoriaAsync(int? categoriaId, CancellationToken ct)
    {
        if (categoriaId is null) return null;
        if (categoriaId <= 0)
            throw AppException.Validacion("categoriaId", "Elige una categoría válida.");

        return await categorias.ObtenerPorIdParaEditarAsync(categoriaId.Value, ct)
            ?? throw AppException.Validacion("categoriaId", "La categoría seleccionada ya no existe.");
    }

    private Task RegistrarAsync(
        int productoId, int usuarioId, string accion, string descripcion,
        int stockAnterior, int stockNuevo, CancellationToken ct) =>
        historial.AgregarAsync(new ProductoHistorial
        {
            ProductoId = productoId,
            UsuarioId = usuarioId,
            Accion = accion,
            Descripcion = descripcion,
            FechaUtc = reloj.GetUtcNow().UtcDateTime,
            StockAnterior = stockAnterior,
            StockNuevo = stockNuevo
        }, ct);

    private static (string Nombre, string Sku, decimal Precio, decimal Costo, int StockMinimo)
        Validar(ProductoRequest s, bool esAlta)
    {
        var errores = new Dictionary<string, string[]>();
        var nombre = s.Nombre?.Trim() ?? "";
        var sku = s.Sku?.Trim() ?? "";

        if (nombre.Length == 0) errores["nombre"] = ["Escribe el nombre del producto."];
        else if (nombre.Length > 150) errores["nombre"] = ["El nombre admite hasta 150 caracteres."];

        if (sku.Length == 0) errores["sku"] = ["Escribe el código (SKU)."];
        else if (sku.Length > 50) errores["sku"] = ["El código admite hasta 50 caracteres."];

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

        return (nombre, sku, precio, costo, s.StockMinimo ?? StockMinimoPorDefecto);
    }
}
