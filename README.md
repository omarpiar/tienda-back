# tienda-back
Backend para el proyecto de tienda

## Calidad de código (SonarQube Cloud)

[SonarQube Cloud](https://sonarcloud.io/) es una plataforma de análisis estático que detecta bugs, vulnerabilidades, code smells, cobertura y deuda técnica. En este repositorio se integra mediante GitHub Actions.

### Workflow
- Archivo: `.github/workflows/sonarqube.yml`
- Rama objetivo: `develop` (push) y `pull_request` (`opened`, `synchronize`, `reopened`)
- Clave/organización actuales en el workflow: `k:"omarpiar_tienda-back"`, `o:"omarpiar"`

### Cómo crear el token y configurar el secreto
1. Ir a [SonarQube Cloud](https://sonarcloud.io/) → My Account → Security → Generate Tokens y generar un token.
2. En GitHub: `Settings → Secrets and variables → Actions → New repository secret`
3. Nombre del secreto: `SONAR_TOKEN`
4. Valor: pegar el token generado.

### Ver resultados
Los resultados se visualizan en SonarQube Cloud para el proyecto `omarpiar_tienda-back` (o el proyecto configurado). También se reflejan en el check del Pull Request asociado.

### Ejecución local (opcional)
```bash
dotnet tool install --global dotnet-sonarscanner
dotnet sonarscanner begin /k:"omarpiar_tienda-back" /o:"omarpiar" /d:sonar.token="TU_TOKEN_LOCAL" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.coverage.exclusions="**/Migrations/**,**/*.Designer.cs"
dotnet build --no-incremental
dotnet test --no-build --collect:"XPlat Code Coverage;Format=opencover" --results-directory TestResults
dotnet sonarscanner end /d:sonar.token="TU_TOKEN_LOCAL"
```

## Migraciones (Entity Framework Core + PostgreSQL)

### Instalación de herramientas
```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet ef --version
```

### Cadena de conexión con user-secrets (no versionar credenciales)
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=puntopyme_dev;Username=postgres;Password=TU_PASSWORD_LOCAL"
```

Registrar en `Program.cs`:
```csharp
builder.Services.AddDbContext<PuntoPymeDbContext>(opciones =>
    opciones.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Comandos
- Crear migración: `dotnet ef migrations add NombreMigracion`
- Listar migraciones: `dotnet ef migrations list`
- Aplicar migraciones: `dotnet ef database update`
- Revertir a una migración específica: `dotnet ef database update NombreMigracionAnterior`
- Eliminar base y reconstruir desde cero (solo desarrollo): `dotnet ef database drop --force` y `dotnet ef database update`
- Generar script idempotente: `dotnet ef migrations script --idempotent -o migraciones.sql`
- Respaldo (solo desarrollo): `pg_dump -U postgres -d puntopyme_dev -F c -f respaldo_dev.backup`

> **Nota:** Nunca se publican credenciales en el repositorio. Usa `user-secrets` (desarrollo) o variables de entorno/secrets en CI/CD.
