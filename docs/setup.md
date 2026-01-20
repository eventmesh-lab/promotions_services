# Guía de Configuración y Despliegue

## Requisitos Previos

### Para Desarrollo Local
- **.NET SDK 8.0** o superior - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 15** - [Descargar](https://www.postgresql.org/download/)
- **Visual Studio 2022** / **VS Code** / **Rider** (opcional, pero recomendado)

### Para Docker
- **Docker Desktop** 20.10+ - [Descargar](https://www.docker.com/products/docker-desktop)
- **Docker Compose** 2.0+ (incluido en Docker Desktop)

## Variables de Entorno

### Variables Requeridas

| Variable | Descripción | Valor por Defecto | Ejemplo |
|----------|-------------|-------------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Development` | `Production`, `Staging` |
| `ASPNETCORE_URLS` | URL donde escucha el servicio | `http://*:7185` | `http://*:8080` |
| `ConnectionStrings__ConnectionPostgre` | Cadena de conexión a PostgreSQL | Ver `appsettings.json` | `Host=db;Port=5432;Database=promotions-service;Username=postgres;Password=postgres` |

### Configuración de Connection String

**Formato completo:**
```
Host=<host>;Port=<puerto>;Database=<nombre_bd>;Username=<usuario>;Password=<contraseña>
```

**Ejemplos por entorno:**

- **Desarrollo Local:**
  ```
  Host=localhost;Port=5432;Database=promotions-service;Username=postgres;Password=postgres
  ```

- **Docker Compose:**
  ```
  Host=db;Port=5432;Database=promotions-service;Username=postgres;Password=postgres
  ```

- **Producción (ejemplo):**
  ```
  Host=prod-db.example.com;Port=5432;Database=promotions-service;Username=promotions_user;Password=<YOUR_SECURE_PASSWORD>;SSL Mode=Require
  ```

### Variables Opcionales

| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| `Logging__LogLevel__Default` | Nivel de logging | `Information` |
| `Logging__LogLevel__Microsoft.AspNetCore` | Logging de ASP.NET Core | `Warning` |

## Configuración por Entorno

### appsettings.json

Configuración base compartida:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ConnectionPostgre": "Host=localhost;Port=5432;Database=promotions-service;Username=postgres;Password=postgres"
  }
}
```

### appsettings.Development.json

Configuración para desarrollo (sobrescribe valores):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### Variables de Entorno (sobrescriben appsettings)

**Linux/Mac:**
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__ConnectionPostgre="Host=prod-db.example.com;Port=5432;Database=promotions-service;Username=prod_user;Password=<YOUR_SECURE_PASSWORD>"
```

**Windows (PowerShell):**
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__ConnectionPostgre="Host=prod-db.example.com;Port=5432;Database=promotions-service;Username=prod_user;Password=<YOUR_SECURE_PASSWORD>"
```

**Docker Compose (en docker-compose.yml):**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__ConnectionPostgre=Host=db;Port=5432;Database=promotions-service;Username=postgres;Password=postgres
```

## Instalación y Ejecución

### Opción 1: Docker Compose (Recomendado)

**Ventajas:**
- No requiere instalar .NET ni PostgreSQL localmente
- Configuración lista para usar
- Migraciones se ejecutan automáticamente

**Paso a Paso:**

```bash
# 1. Clonar el repositorio
git clone https://github.com/eventmesh-lab/promotions_services.git
cd promotions_services

# 2. Construir y levantar servicios
docker-compose up --build

# El servicio estará disponible en:
# - API: http://localhost:7185
# - Swagger: http://localhost:7185/swagger
# - PostgreSQL: localhost:5432
```

**Comandos útiles:**

```bash
# Ejecutar en background
docker-compose up -d

# Ver logs
docker-compose logs -f promotions

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes (borra la BD)
docker-compose down -v

# Reconstruir imagen
docker-compose build --no-cache
```

### Opción 2: Desarrollo Local

**Requisitos:**
- PostgreSQL debe estar corriendo en localhost:5432
- Base de datos `promotions-service` debe existir

**Paso a Paso:**

```bash
# 1. Clonar repositorio
git clone https://github.com/eventmesh-lab/promotions_services.git
cd promotions_services

# 2. Crear base de datos en PostgreSQL
psql -U postgres -c "CREATE DATABASE \"promotions-service\";"

# 3. Restaurar dependencias
dotnet restore

# 4. Aplicar migraciones
dotnet ef database update --project src/promotions_services.infrastructure --startup-project src/promotions_services.api

# 5. Ejecutar el servicio
dotnet run --project src/promotions_services.api

# El servicio estará disponible en:
# - http://localhost:7185
# - https://localhost:7186 (si está configurado HTTPS)
```

**Ejecutar con Hot Reload (desarrollo):**
```bash
dotnet watch --project src/promotions_services.api
```

### Opción 3: Visual Studio

1. Abrir `promotions_services.sln` con Visual Studio 2022
2. Configurar `promotions_services.api` como proyecto de inicio
3. Configurar connection string en `appsettings.Development.json`
4. Presionar F5 para ejecutar con debugging

## Docker

### Dockerfile

El proyecto incluye un Dockerfile multi-stage para optimizar el tamaño de la imagen:

**Construcción manual:**
```bash
# Construir imagen
docker build -t promotions-service:latest .

# Ejecutar contenedor
docker run -p 7185:7185 \
  -e ConnectionStrings__ConnectionPostgre="Host=host.docker.internal;Port=5432;Database=promotions-service;Username=postgres;Password=postgres" \
  promotions-service:latest
```

**Cambiar puerto:**
```bash
docker build --build-arg APP_PORT=8080 -t promotions-service:latest .
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://*:8080 promotions-service:latest
```

### Docker Compose - Configuración Detallada

El archivo `docker-compose.yml` define 2 servicios:

#### Servicio: db (PostgreSQL)
```yaml
db:
  image: postgres:15-alpine
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres
    POSTGRES_DB: promotions-service
  ports:
    - "5432:5432"
  volumes:
    - promotions_pgdata:/var/lib/postgresql/data
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres -d promotions-service"]
    interval: 5s
    timeout: 3s
    retries: 20
```

#### Servicio: promotions (API)
```yaml
promotions:
  build:
    context: .
    args:
      APP_PORT: 7185
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__ConnectionPostgre: "Host=db;Port=5432;Database=promotions-service;Username=postgres;Password=postgres"
  ports:
    - "7185:7185"
  depends_on:
    db:
      condition: service_healthy
  restart: on-failure
```

## Entity Framework Core - Migraciones

### Ver Migraciones Existentes
```bash
dotnet ef migrations list --project src/promotions_services.infrastructure --startup-project src/promotions_services.api
```

### Crear Nueva Migración
```bash
dotnet ef migrations add <NombreMigracion> \
  --project src/promotions_services.infrastructure \
  --startup-project src/promotions_services.api
```

### Aplicar Migraciones
```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update \
  --project src/promotions_services.infrastructure \
  --startup-project src/promotions_services.api

# Revertir a una migración específica
dotnet ef database update <NombreMigracion> \
  --project src/promotions_services.infrastructure \
  --startup-project src/promotions_services.api
```

### Generar Script SQL
```bash
dotnet ef migrations script \
  --project src/promotions_services.infrastructure \
  --startup-project src/promotions_services.api \
  --output migrations.sql
```

**Nota:** El servicio ejecuta `context.Database.Migrate()` en startup (Program.cs:62), por lo que las migraciones se aplican automáticamente al iniciar.

## Scripts Disponibles

### Desde la solución (.NET CLI)

```bash
# Restaurar dependencias de todos los proyectos
dotnet restore

# Compilar solución completa
dotnet build

# Compilar en modo Release
dotnet build -c Release

# Ejecutar tests
dotnet test

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true

# Ejecutar el API
dotnet run --project src/promotions_services.api

# Publicar para producción
dotnet publish src/promotions_services.api -c Release -o ./publish
```

### Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un proyecto específico
dotnet test tests/promotions_services.api.Tests
dotnet test tests/promotions_services.application.Tests
dotnet test tests/promotions_services.infrastructure.Tests

# Ejecutar tests con logging verbose
dotnet test --logger "console;verbosity=detailed"
```

## Configuración CORS

El servicio está configurado para permitir peticiones desde `http://localhost:3000`:

```csharp
// Program.cs líneas 10-18
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

**Para agregar más orígenes:**

```csharp
policy.WithOrigins(
    "http://localhost:3000",
    "http://localhost:4200",
    "https://mi-app-frontend.com"
)
```

**Para desarrollo (permitir todos los orígenes - NO usar en producción):**

```csharp
policy.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod();
```

## Troubleshooting

### Error: "Cannot connect to PostgreSQL"

**Síntomas:** 
```
Npgsql.NpgsqlException: Failed to connect to [host]
```

**Soluciones:**
1. Verificar que PostgreSQL esté corriendo: `docker ps` o `pg_isready`
2. Verificar connection string en variables de entorno
3. Verificar firewall no bloquee puerto 5432
4. En Docker: usar `host.docker.internal` en lugar de `localhost`

### Error: "A network-related or instance-specific error"

**Causa:** Servicio intenta conectar antes de que PostgreSQL esté listo

**Solución en docker-compose:**
Ya está configurado con `depends_on` + `healthcheck`. Si persiste, aumentar `retries` en healthcheck.

### Error: "Login failed for user"

**Síntomas:**
```
Npgsql.PostgresException: password authentication failed
```

**Soluciones:**
1. Verificar credenciales en connection string
2. Verificar que usuario existe en PostgreSQL:
   ```bash
   docker exec -it <container-id> psql -U postgres -c "\du"
   ```

### Puerto 7185 ya en uso

**Soluciones:**

1. Detener proceso que usa el puerto:
   ```bash
   # Linux/Mac
   lsof -ti:7185 | xargs kill -9
   
   # Windows
   netstat -ano | findstr :7185
   taskkill /PID <PID> /F
   ```

2. Cambiar puerto en docker-compose.yml:
   ```yaml
   ports:
     - "8080:7185"  # Mapear 8080 externo → 7185 interno
   ```

### Migraciones no se aplican

**Verificar:**
1. Connection string es correcta
2. Base de datos existe
3. Usuario tiene permisos para crear tablas

**Forzar aplicación:**
```bash
dotnet ef database update --force \
  --project src/promotions_services.infrastructure \
  --startup-project src/promotions_services.api
```

### Logs para debugging

**Ver logs de Docker Compose:**
```bash
docker-compose logs -f promotions
```

**Aumentar nivel de logging:**
```bash
export Logging__LogLevel__Default=Debug
dotnet run --project src/promotions_services.api
```

## Despliegue en Producción

### Checklist Pre-Producción

- [ ] Cambiar contraseñas por defecto de PostgreSQL
- [ ] Configurar SSL/TLS para conexión a BD (`SSL Mode=Require`)
- [ ] Configurar HTTPS para el servicio
- [ ] Remover Swagger en producción (ya está configurado para solo Development)
- [ ] Configurar logging estructurado (Serilog + Application Insights)
- [ ] Implementar health checks para monitoreo
- [ ] Configurar backups de PostgreSQL
- [ ] Implementar rate limiting
- [ ] Configurar secretos con Azure Key Vault / AWS Secrets Manager
- [ ] Configurar CORS con dominios específicos (no `localhost`)
- [ ] Ejecutar análisis de seguridad (OWASP)
- [ ] Configurar CI/CD pipelines

### Ejemplo: Azure Container Apps

```bash
# 1. Crear Container Registry
az acr create --name myregistry --resource-group mygroup --sku Basic

# 2. Build y push
docker build -t myregistry.azurecr.io/promotions-service:v1 .
docker push myregistry.azurecr.io/promotions-service:v1

# 3. Crear PostgreSQL flexible server
az postgres flexible-server create \
  --name promotions-db \
  --resource-group mygroup \
  --location eastus \
  --admin-user myadmin \
  --admin-password <YOUR_SECURE_PASSWORD> \
  --sku-name Standard_B1ms

# 4. Desplegar Container App
az containerapp create \
  --name promotions-api \
  --resource-group mygroup \
  --image myregistry.azurecr.io/promotions-service:v1 \
  --environment myenv \
  --env-vars "ConnectionStrings__ConnectionPostgre=Host=promotions-db.postgres.database.azure.com;Port=5432;Database=promotions-service;Username=myadmin;Password=<YOUR_SECURE_PASSWORD>;SSL Mode=Require"
```

### Variables de Entorno Producción

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://*:7185
ConnectionStrings__ConnectionPostgre=Host=prod-db.example.com;Port=5432;Database=promotions-service;Username=prod_user;Password=<YOUR_SECURE_PASSWORD>;SSL Mode=Require
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft.AspNetCore=Warning
```

## Recursos Adicionales

- [Documentación .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
