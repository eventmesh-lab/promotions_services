# Promotions Services

## Descripción

Microservicio de gestión de cupones promocionales que permite generar, consultar y validar cupones de descuento para usuarios. Resuelve el problema de negocio de automatizar la distribución y control de promociones, evitando el abuso de descuentos mediante reglas de negocio como límites de tiempo entre solicitudes y fechas de expiración.

## Tabla de Contenidos

- [Arquitectura](docs/architecture.md) - Flujo de datos, dependencias y modelo de datos
- [API](docs/api.md) - Documentación de endpoints y contratos
- [Configuración](docs/setup.md) - Guía detallada de instalación y configuración

## Stack Tecnológico

- **Framework**: ASP.NET Core 8.0
- **Lenguaje**: C# (.NET 8.0)
- **Base de Datos**: PostgreSQL 15
- **ORM**: Entity Framework Core 9.0
- **Patrón CQRS**: MediatR 14.0
- **Contenedores**: Docker & Docker Compose
- **Documentación API**: Swagger/OpenAPI
- **Arquitectura**: Clean Architecture (Domain, Application, Infrastructure, API)

## Quick Start

### Usando Docker Compose (Recomendado)

```bash
# Levantar servicio con base de datos
docker-compose up

# El servicio estará disponible en:
# - API: http://localhost:7185
# - Swagger UI: http://localhost:7185/swagger
```

### Desarrollo Local

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar migraciones
dotnet ef database update --project src/promotions_services.infrastructure --startup-project src/promotions_services.api

# Ejecutar el servicio
dotnet run --project src/promotions_services.api

# Ejecutar tests
dotnet test
```

## Características Principales

- ✅ Generación automática de cupones con descuentos y montos mínimos aleatorios
- ✅ Validación de límite de 15 días entre solicitudes por usuario
- ✅ Consulta de cupones válidos por usuario
- ✅ Invalidación de cupones una vez utilizados
- ✅ Cupones con fecha de expiración de 1 año
- ✅ CORS configurado para frontend (localhost:3000)
- ✅ Migraciones automáticas en startup

## Estructura del Proyecto

```
promotions_services/
├── src/
│   ├── promotions_services.api/           # Capa de presentación (Controllers)
│   ├── promotions_services.application/   # Lógica de aplicación (Commands, Queries)
│   ├── promotions_services.domain/        # Entidades y reglas de negocio
│   └── promotions_services.infrastructure/ # Persistencia y servicios externos
├── tests/                                 # Tests unitarios y de integración
├── docs/                                  # Documentación del proyecto
└── docker-compose.yml                     # Orquestación de contenedores
```

## Licencia

Este proyecto es parte de EventMesh Lab.
