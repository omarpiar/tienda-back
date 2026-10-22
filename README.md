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
