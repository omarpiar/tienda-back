using PuntoPyme.Application.Productos;
using PuntoPyme.Domain.Entities;

namespace PuntoPyme.Tests;

public class ProductoMapeoTests
{
    [Fact]
    public void ALista_mapea_la_categoria_y_el_estado_de_stock()
    {
        var conCategoria = new Producto
        {
            Id = 1,
            Nombre = "Agua",
            Sku = "A-1",
            CategoriaId = 4,
            Categoria = new Categoria { Id = 4, Nombre = "Bebidas" },
            PrecioVenta = 12m,
            Stock = 0,
            StockMinimo = 5
        };
        var sinCategoria = new Producto { Id = 2, Nombre = "Varios", Sku = "V-1", Stock = 10, StockMinimo = 5 };

        var d = ProductoMapeo.ALista(conCategoria);
        var sin = ProductoMapeo.ALista(sinCategoria);

        Assert.Equal(4, d.CategoriaId);
        Assert.Equal("Bebidas", d.Categoria);
        Assert.Equal("Agotado", d.EstadoStock);
        Assert.Null(sin.CategoriaId);
        Assert.Null(sin.Categoria);
        Assert.Equal("Normal", sin.EstadoStock);
    }

    [Fact]
    public void ADetalle_para_cajero_no_expone_los_costos()
    {
        var p = new Producto
        {
            Id = 1,
            Nombre = "Agua",
            Sku = "A-1",
            PrecioVenta = 12m,
            CostoCompra = 8m,
            Stock = 3,
            StockMinimo = 5
        };

        var d = ProductoMapeo.ADetalle(p, incluirCostos: false);

        Assert.Null(d.CostoCompra);
        Assert.Null(d.Ganancia);
        Assert.Null(d.MargenPorcentaje);
        Assert.Equal("Bajo", d.EstadoStock);
    }

    [Fact]
    public void ADetalle_con_precio_cero_devuelve_margen_cero()
    {
        var p = new Producto { PrecioVenta = 0m, CostoCompra = 5m };

        var d = ProductoMapeo.ADetalle(p, incluirCostos: true);

        Assert.Equal(5m, d.CostoCompra);
        Assert.Equal(-5m, d.Ganancia);
        Assert.Equal(0m, d.MargenPorcentaje);
    }

    [Fact]
    public void AHistorial_usa_el_nombre_del_usuario_o_un_texto_vacio()
    {
        var conUsuario = new ProductoHistorial
        {
            Id = 10,
            Accion = "Creación",
            Descripcion = "Producto creado.",
            FechaUtc = new DateTime(2026, 11, 4, 8, 0, 0, DateTimeKind.Utc),
            StockAnterior = 0,
            StockNuevo = 5,
            UsuarioId = 7,
            Usuario = new Usuario { NombreCompleto = "Ana Pérez" }
        };
        var sinUsuario = new ProductoHistorial
        {
            Id = 11,
            Accion = "Actualización",
            Descripcion = "Producto actualizado.",
            FechaUtc = new DateTime(2026, 11, 4, 9, 0, 0, DateTimeKind.Utc),
            StockAnterior = 5,
            StockNuevo = 5,
            UsuarioId = 8,
            Usuario = null!
        };

        var d = ProductoMapeo.AHistorial(conUsuario);
        var sin = ProductoMapeo.AHistorial(sinUsuario);

        Assert.Equal("Ana Pérez", d.Usuario);
        Assert.Equal(5, d.StockNuevo);
        Assert.Equal("", sin.Usuario);
    }

    [Fact]
    public void Los_dtos_conservan_sus_valores()
    {
        var lista = new ProductoListaDto(1, "Agua", "A-1", 4, "Bebidas", 12m, 3, 5, "Bajo");
        var detalle = new ProductoDetalleDto(1, "Agua", "A-1", 4, "Bebidas", 12m, 8m, 4m, 33.3m, 3, 5, "Bajo");
        var historial = new ProductoHistorialDto(
            10, "Creación", "Producto creado.", new DateTime(2026, 11, 4, 8, 0, 0, DateTimeKind.Utc),
            0, 3, 7, "Ana Pérez");
        var pagina = new PaginaResultado<ProductoListaDto>([lista], 1, 1, 50);

        Assert.Equal("Agua", lista.Nombre);
        Assert.Equal("Bebidas", lista.Categoria);
        Assert.Equal(12m, detalle.PrecioVenta);
        Assert.Equal(33.3m, detalle.MargenPorcentaje);
        Assert.Equal("Creación", historial.Accion);
        Assert.Equal("Ana Pérez", historial.Usuario);
        Assert.Same(lista, Assert.Single(pagina.Items));
        Assert.Equal(1, pagina.Total);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(50, pagina.TamanoPagina);
    }
}
