# tienda-back
Backend para el proyecto de tienda

## Análisis de calidad con SonarQube Cloud

Este proyecto incluye un workflow de GitHub Actions (`.github/workflows/sonarqube.yml`) para ejecutar el análisis de calidad con [SonarQube Cloud](https://sonarcloud.io/).

### Requisitos
- Tener una cuenta en [SonarQube Cloud](https://sonarcloud.io/)
- Crear una organización y un proyecto en SonarQube Cloud
- Tener acceso al repositorio en GitHub con permisos para configurar secretos

### Configuración

1. **Generar token de SonarQube Cloud**
   - Ir a SonarQube Cloud → My Account → Security → Generate Tokens
   - Guardar el token generado

2. **Agregar secreto al repositorio GitHub**
   - En GitHub: Settings → Secrets and variables → Actions → New repository secret
   - Nombre: `SONAR_TOKEN`
   - Valor: el token generado anteriormente

3. **Actualizar el workflow**
   - Editar `.github/workflows/sonarqube.yml`
   - Reemplazar `TU_CLAVE_PROYECTO` con la clave del proyecto (Project Key)
   - Reemplazar `TU_ORGANIZACION` con el nombre de la organización en SonarQube Cloud

### Ejecución
El workflow se ejecuta automáticamente en:
- `push` a la rama `main`
- `pull_request` con los eventos `opened`, `synchronize`, `reopened`

### Ejecución local (opcional)
Para ejecutar el análisis localmente con el SonarScanner de .NET:

```bash
dotnet tool install --global dotnet-sonarscanner
dotnet sonarscanner begin /k:"TU_CLAVE_PROYECTO" /o:"TU_ORGANIZACION" /d:sonar.token="TU_TOKEN_LOCAL" /d:sonar.host.url="https://sonarcloud.io"
dotnet build --no-incremental
dotnet sonarscanner end /d:sonar.token="TU_TOKEN_LOCAL"
```
## Migraciones de base de datos (Postgres)

### Desarrollo con PostgreSQL local
1. Levantar Postgres (Docker):
```bash
docker run --name pg-puntopyme -e POSTGRES_PASSWORD=TU_PASSWORD_LOCAL -p 5432:5432 -d postgres:16
```
2. Configurar user-secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=puntopyme_dev;Username=postgres;Password=TU_PASSWORD_LOCAL"
```
3. Registrar DbContext en Program.cs:
```csharp
builder.Services.AddDbContext<PuntoPymeDbContext>(opciones =>
    opciones.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```
4. Aplicar migraciones:
```bash
dotnet ef migrations list
dotnet ef database update
```
5. Construir desde base limpia:
```bash
dotnet ef database drop --force
dotnet ef database update
```
6. Generar script idempotente:
```bash
dotnet ef migrations script --idempotent -o migraciones.sql
```
7. Respaldo (dev):
```bash
pg_dump -U postgres -d puntopyme_dev -F c -f respaldo_dev.backup
```
8. Revertir y reaplicar (dev):
```bash
pg_dump -U postgres -d puntopyme_dev -F c -f respaldo_dev.backup
dotnet ef database update AgregarStockMinimoAProductos
dotnet ef migrations list
dotnet ef database update
dotnet ef database drop --force
dotnet ef database update
```
