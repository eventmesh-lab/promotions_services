# Arquitectura del Sistema

## Visión General

El servicio implementa **Clean Architecture** con separación clara de responsabilidades en 4 capas:

1. **Domain** - Entidades y reglas de negocio puras
2. **Application** - Casos de uso con patrón CQRS (Commands & Queries)
3. **Infrastructure** - Implementaciones de persistencia y servicios externos
4. **API** - Controladores y configuración web

## Flujo de Datos

### Petición HTTP → Respuesta

```
1. Cliente HTTP
   ↓
2. [API Layer] CouponController
   │ - Valida request
   │ - Extrae parámetros
   ↓
3. [Application Layer] MediatR
   │ - Despacha Command/Query al Handler correspondiente
   ↓
4. [Application Layer] Handler (GenerateCouponHandler, GetValidCouponsHandler, etc.)
   │ - Ejecuta lógica de negocio
   │ - Llama al Repository a través de interface (ICouponRepository)
   ↓
5. [Infrastructure Layer] Repository (CouponRepository)
   │ - Mapea Domain Entity → Persistence Model (CouponPostgres)
   │ - Ejecuta operación en DbContext
   ↓
6. [Infrastructure Layer] Entity Framework Core
   │ - Genera SQL
   │ - Ejecuta contra PostgreSQL
   ↓
7. PostgreSQL Database
   │ - Retorna datos
   ↓
8. [Respuesta inversa por las capas]
   │ - Mapper: CouponPostgres → Coupon (Domain)
   │ - Handler: Coupon → DTO (GetValidCouponsDto/ResultadoDTO)
   │ - Controller: DTO → JSON Response
   ↓
9. Cliente HTTP recibe JSON
```

### Ejemplo Concreto: Generar un Cupón

```
POST /api/coupons/generateCoupon
Body: { "email": "user@example.com" }

→ CouponController.CreateUser()
  → MediatR.Send(GenerateCouponCommand)
    → GenerateCouponHandler.Handle()
      ├── Consulta último cupón del usuario (Repository)
      ├── Valida regla de 15 días entre cupones
      ├── Genera descuento aleatorio (5-50%)
      ├── Genera monto mínimo aleatorio (0-100)
      ├── Crea entidad Coupon (Domain)
      └── Guarda en BD (Repository)
    → Retorna ResultadoDTO
  → Retorna 200 OK con mensaje de éxito/error
```

## Dependencias Externas

### Base de Datos

- **PostgreSQL 15** (puerto 5432)
  - Base de datos: `promotions-service`
  - Tablas: `Coupons` (principal)
  - ORM: Entity Framework Core con Npgsql
  - Connection String: Configurada en `appsettings.json` / Variables de entorno

### Otros Microservicios

**NOTA**: Se detectó código comentado que sugiere una dependencia eliminada:
```csharp
// Comentado en GetValidCouponsByUserHandler.cs líneas 28-36
// var idUsuario = await _usuarioService.ObtenerUsuarioPorEmailAsync(request.correo);
```

Actualmente el servicio **NO** llama a otros microservicios. Funciona de forma autónoma usando solo el email del usuario como identificador.

### APIs de Terceros

- **Ninguna detectada** - El servicio no integra con APIs externas actualmente

### Configuraciones CORS

- Permite peticiones desde: `http://localhost:3000`
- Configurado para desarrollo con frontend local (probablemente React/Vue/Angular)

## Modelo de Datos

### Entidad Principal: `Coupon`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | Guid | Identificador único del cupón |
| `Email` | string | Email del usuario propietario |
| `DiscountAmount` | int | Porcentaje de descuento (5, 10, 15, 20, 25, 50) |
| `AmountMin` | decimal | Monto mínimo de compra requerido (0, 5, 10, 15, 20, 25, 50, 100) |
| `CreatedAt` | DateTime | Fecha de creación (se usa `DateTime.Today`) |
| `ExpirationDate` | DateTime | Fecha de expiración (1 año desde creación) |
| `IsValid` | bool | Si el cupón está activo (true) o fue usado (false) |

### Entidad Secundaria: `CouponUser` 

**NOTA**: Esta entidad existe en el modelo de dominio pero **NO se utiliza** actualmente:

```csharp
public class CouponUser
{
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedDate { get; set; }
}
```

No tiene configuración en EF Core ni se referencia en ningún handler. Parece ser código preparatorio para una futura relación many-to-many entre usuarios y cupones.

### Enumeraciones

**EnumAmountDiscount** (Porcentajes de descuento):
```csharp
Cinco = 5, Diez = 10, Quince = 15, Veinte = 20, Veinticinco = 25, Cincuenta = 50
```

**EnumAmountMin** (Montos mínimos):
```csharp
Zero = 0, Cinco = 5, Diez = 10, Quince = 15, Veinte = 20, Veinticinco = 25, Cincuenta = 50, Cien = 100
```

## Patrones de Diseño Implementados

### 1. CQRS (Command Query Responsibility Segregation)

- **Commands**: `GenerateCouponCommand` - Operaciones de escritura
- **Queries**: `GetValidCouponsByUserQuery`, `GetValidCouponQuery` - Operaciones de lectura
- **Mediador**: MediatR gestiona el dispatch

### 2. Repository Pattern

- Interface: `ICouponRepository` (en Domain)
- Implementación: `CouponRepository` (en Infrastructure)
- Abstrae el acceso a datos y permite testabilidad

### 3. Mapper Pattern

- `CouponMapper.ToPostgres()` - Domain → Persistence
- `CouponMapper.ToDomain()` - Persistence → Domain
- Separa modelos de dominio de modelos de base de datos

### 4. DTO (Data Transfer Objects)

- `GenerateCouponDto` - Input para crear cupón
- `GetValidCouponsDto` - Output con datos de cupón
- `ResultadoDTO` - Wrapper de respuesta con estado éxito/error

## Deuda Técnica Detectada

### 🔴 Crítico

1. **Console.WriteLine en Producción**
   - **Ubicación**: `GenerateCouponHandler.cs:28`, `CouponRepository.cs:56-64`, `GetValidCouponsByUserHandler.cs:46`
   - **Problema**: Uso de `Console.WriteLine()` para debugging en lugar de logging estructurado
   - **Impacto**: Logs no estructurados, difíciles de monitorear en producción
   - **Recomendación**: Reemplazar con `ILogger<T>` de Microsoft.Extensions.Logging

2. **Uso de DateTime.Today en lugar de DateTime.UtcNow**
   - **Ubicación**: `Coupon.cs:25` - `CreatedAt = DateTime.Today`
   - **Problema**: `DateTime.Today` usa hora local (00:00:00), pero en la validación usa `DateTime.UtcNow`
   - **Impacto**: Inconsistencia temporal que puede causar errores en la validación de 15 días
   - **Recomendación**: Usar `DateTime.UtcNow` consistentemente en toda la aplicación

3. **Lógica de Validación Duplicada**
   - **Ubicación**: `GenerateCouponHandler.cs:53-67`
   - **Problema**: Condición `< 15` validada dos veces anidadamente
   - **Código problemático**:
   ```csharp
   if ((DateTime.UtcNow - ultimoCupon.CreatedAt).TotalDays < 15)
   {
       var diasDesdeUltimo = (DateTime.UtcNow - ultimoCupon.CreatedAt).TotalDays;
       if (diasDesdeUltimo < 15) // ← Validación redundante
       {
           // ...
       }
   }
   ```
   - **Recomendación**: Simplificar eliminando el `if` anidado

### 🟡 Medio

4. **Entidad No Utilizada**
   - **Ubicación**: `CouponUser.cs`
   - **Problema**: Entidad definida pero nunca utilizada en la aplicación
   - **Impacto**: Código muerto que confunde sobre la arquitectura real
   - **Recomendación**: Eliminar o implementar la funcionalidad completa

5. **Código Comentado**
   - **Ubicación**: `GetValidCouponsByUserHandler.cs:28-36`
   - **Problema**: Código comentado referenciando un servicio de usuario inexistente
   - **Impacto**: Confusión sobre dependencias, sugiere refactoring incompleto
   - **Recomendación**: Eliminar código comentado

6. **Inconsistencia en Nombres de Métodos**
   - **Ubicación**: `CouponController.cs:25` - Método `CreateUser` que realmente genera cupón
   - **Problema**: Nombre no refleja la funcionalidad
   - **Impacto**: Confusión al leer el código
   - **Recomendación**: Renombrar a `GenerateCoupon`

7. **Endpoint con Verbo HTTP Incorrecto**
   - **Ubicación**: `CouponController.cs:53` - `[HttpPost] getCoupon/{id}`
   - **Problema**: GET disfrazado de POST (no modifica datos)
   - **Impacto**: Viola principios REST, no cacheable
   - **Recomendación**: Cambiar a `[HttpGet]`

8. **Falta de Validación de Negocio**
   - **Ubicación**: `GetValidCouponsDto` y `Coupon` entity
   - **Problema**: No valida que `ExpirationDate > CreatedAt`, no valida formato de email
   - **Impacto**: Posibles datos inconsistentes en BD
   - **Recomendación**: Agregar Data Annotations o FluentValidation

### 🟢 Menor

9. **Using Statements No Utilizados**
   - **Ubicación**: Varios archivos con `using System.Text;`, `using System.Linq;` sin uso
   - **Impacto**: Mínimo, pero ensucian el código
   - **Recomendación**: Ejecutar `dotnet format` o configurar IDE para limpieza automática

10. **Falta de Documentación XML**
   - **Ubicación**: Mayoría de clases públicas
   - **Problema**: Solo algunas clases tienen `<summary>`, inconsistente
   - **Impacto**: Dificulta mantenimiento y generación de documentación automática
   - **Recomendación**: Agregar XML docs a APIs públicas

11. **SignalR Registrado pero No Usado**
   - **Ubicación**: `Program.cs:33` - `builder.Services.AddSignalR()`
   - **Problema**: SignalR configurado pero sin hubs implementados
   - **Impacto**: Recurso registrado innecesariamente
   - **Recomendación**: Eliminar si no se planea usar, o implementar notificaciones en tiempo real

12. **WeatherForecast Scaffold Sin Eliminar**
   - **Ubicación**: `WeatherForecast.cs` en API project
   - **Problema**: Archivo de template de .NET sin eliminar
   - **Impacto**: Código de ejemplo que no pertenece al proyecto real
   - **Recomendación**: Eliminar archivo

## Recomendaciones de Mejora

### Seguridad
- Implementar autenticación/autorización (el endpoint está abierto)
- Validar y sanitizar inputs (especialmente emails)
- Rate limiting para prevenir abuso de generación de cupones

### Performance
- Agregar índice en columna `Email` de tabla Coupons (consultas frecuentes)
- Considerar caching para cupones válidos por usuario

### Observabilidad
- Implementar logging estructurado con Serilog o Application Insights
- Agregar health checks
- Implementar métricas de negocio (cupones generados/día, tasa de uso, etc.)

### Testing
- Agregar tests de integración para los endpoints
- Tests unitarios para validaciones de negocio (regla de 15 días)
- Tests para edge cases (cupones expirados, timezone issues)
