# PuntoPyme - Codigo de base de datos (3 pasos = 3 migraciones)

Cada carpeta `PasoN_*` corresponde a UNA migracion. Copia los archivos de un paso,
compila, crea la migracion, revisala y aplicala ANTES de pasar al siguiente. Asi el
historial de GitHub queda limpio (una migracion por cambio) y puedes demostrar la
reversion de cualquiera.

Las carpetas `Domain`, `Application` e `Infrastructure` corresponden a los proyectos
`PuntoPyme.Domain`, `PuntoPyme.Application` y `PuntoPyme.Infrastructure`.
Si usan otros nombres, ajusten los namespaces.

Comando base (desde la carpeta de la solucion):

    dotnet ef migrations add NOMBRE --project src/PuntoPyme.Infrastructure --startup-project src/PuntoPyme.Api
    dotnet ef database update      --project src/PuntoPyme.Infrastructure --startup-project src/PuntoPyme.Api

## Paso 0 - Capas (requisito de los lineamientos)
Ver la seccion 2 del documento "Base de datos y catalogo de APIs". Antes de seguir,
`dotnet ef migrations has-pending-model-changes` debe decir que NO hay cambios pendientes
y `dotnet ef migrations list` debe seguir mostrando tus 3 migraciones aplicadas.

## Paso 1 - Productos  (migracion: AjustarProductosParaInventario)
1. Reemplaza tu clase `Producto` por `Paso1_Productos/Domain/Entities/Producto.cs` (agrega `Categoria`).
2. Copia `ProductoConfiguration.cs` y copia `PuntoPymeDbContext_version_final.cs`
   (solo deja el `DbSet<Producto>` por ahora).
3. `dotnet build` y luego `dotnet ef migrations add AjustarProductosParaInventario ...`
4. La migracion debe contener: AddColumn `Categoria`; AlterColumn de `Nombre`, `Sku`,
   `PrecioVenta`, `CostoCompra`; 3 CheckConstraint. No debe tocar `IX_Productos_Sku`.
5. Tu tabla de desarrollo debe estar sin filas que violen los checks (hoy esta vacia).
6. `dotnet ef database update`.

## Paso 2 - Roles y usuarios  (migracion: CrearTablasRolesYUsuarios)
1. Copia los archivos de `Paso2_RolesUsuarios` a sus proyectos.
2. En `PuntoPyme.Infrastructure` instala: `dotnet add package BCrypt.Net-Next`.
3. Agrega al DbContext: `DbSet<Rol> Roles` y `DbSet<Usuario> Usuarios`.
4. Registra en `Program.cs`: `builder.Services.AddScoped<IPinHasher, BCryptPinHasher>();`
5. Migracion esperada: CreateTable `Roles` con InsertData (2 filas), CreateTable `Usuarios`,
   indices unicos en `Roles.Nombre` y `Usuarios.NombreUsuario`, FK con Restrict.
6. `dotnet ef database update`.
7. Primer administrador (solo desarrollo), en `Program.cs` despues de construir la app:

       if (app.Environment.IsDevelopment())
       {
           using var scope = app.Services.CreateScope();
           var db = scope.ServiceProvider.GetRequiredService<PuntoPymeDbContext>();
           var hasher = scope.ServiceProvider.GetRequiredService<IPinHasher>();
           await DbSeeder.SembrarAdministradorAsync(db, hasher,
               app.Configuration["SeedAdmin:NombreCompleto"],
               app.Configuration["SeedAdmin:NombreUsuario"],
               app.Configuration["SeedAdmin:Pin"]);
       }

   Credenciales con user-secrets (NO en el repositorio), en el proyecto Api:

       dotnet user-secrets set "SeedAdmin:NombreCompleto" "Administrador PuntoPyme"
       dotnet user-secrets set "SeedAdmin:NombreUsuario" "admin"
       dotnet user-secrets set "SeedAdmin:Pin" "ELIGE_UN_PIN_DE_4_A_6_DIGITOS"

## Paso 3 - Ventas  (migracion: CrearTablasVentasYDetalleVentas)
1. Copia los archivos de `Paso3_Ventas`.
2. Agrega al DbContext: `DbSet<Venta> Ventas` y `DbSet<VentaDetalle> VentaDetalles`.
3. Migracion esperada: CreateTable `Ventas` y `VentaDetalles`, indices unicos en `Ventas.IdLocal`,
   indice en `Ventas.FechaVentaUtc`, FKs (Restrict hacia Usuarios y Productos, Cascade de Venta a sus detalles).
4. `dotnet ef database update`.

## Verificacion final
- `dotnet ef migrations list` -> 6 migraciones aplicadas.
- En pgAdmin: `SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';`
  debe mostrar Productos, Roles, Usuarios, Ventas, VentaDetalles y __EFMigrationsHistory.
- Para la demo de reversion: `dotnet ef database update CrearTablasRolesYUsuarios` quita Ventas y
  VentaDetalles; `dotnet ef database update` las vuelve a crear. Haz respaldo antes.

Este codigo no se compilo en el entorno donde se genero (sin SDK de .NET): ejecuta `dotnet build`
despues de cada paso y corrige namespaces o usings si hace falta.
