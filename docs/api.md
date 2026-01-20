# API Documentation

## Base URL

```
http://localhost:7185/api
```

## Autenticación

⚠️ **Actualmente el API no requiere autenticación**. Todos los endpoints son públicos.

## Endpoints

### 1. Generar Cupón

Genera un nuevo cupón de descuento para un usuario. Valida que no se hayan generado cupones en los últimos 15 días.

**Endpoint:** `POST /api/coupons/generateCoupon`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "string"
}
```

**Respuesta Exitosa (200 OK):**
```json
{
  "mensaje": "¡Felicidades! Se ha generado un cupón del 25% de descuento para un monto minimo de 50. Puedes canjear hasta el 20/01/2027",
  "exito": true,
  "coupon": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "discountAmount": 25,
    "createdAt": "2026-01-20T00:00:00",
    "expirationDate": "2027-01-20T00:00:00",
    "isValid": true,
    "amountMin": 50
  }
}
```

**Respuesta Error - Ya existe cupón reciente (400 Bad Request):**
```json
{
  "mensaje": "Ya has solicitado un cupón recientemente. Podrás generar uno nuevo en 10 días.",
  "exito": false,
  "coupon": null
}
```

**Respuesta Error - Email inválido (400 Bad Request):**
```json
{
  "mensaje": "El cuerpo de la solicitud es inválido o el email está vacío.",
  "exito": false,
  "coupon": null
}
```

**Ejemplo cURL:**
```bash
curl -X POST http://localhost:7185/api/coupons/generateCoupon \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com"}'
```

**Reglas de Negocio:**
- Descuento aleatorio: 5%, 10%, 15%, 20%, 25% o 50%
- Monto mínimo aleatorio: $0, $5, $10, $15, $20, $25, $50 o $100
- Límite de 1 cupón cada 15 días por usuario
- Cupones válidos por 1 año desde su creación
- Email es único identificador del usuario

---

### 2. Obtener Cupones Válidos de un Usuario

Retorna todos los cupones válidos (no usados y no expirados) de un usuario específico.

**Endpoint:** `GET /api/coupons/getCouponsUser/{correo}`

**Parámetros de Ruta:**
- `correo` (string, requerido) - Email del usuario

**Respuesta Exitosa (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "discountAmount": 25,
    "createdAt": "2026-01-20T00:00:00",
    "expirationDate": "2027-01-20T00:00:00",
    "isValid": true,
    "amountMin": 50
  },
  {
    "id": "7gb95h74-6828-5673-c4gd-3d074g77bgb7",
    "email": "user@example.com",
    "discountAmount": 15,
    "createdAt": "2025-12-15T00:00:00",
    "expirationDate": "2026-12-15T00:00:00",
    "isValid": true,
    "amountMin": 20
  }
]
```

**Respuesta - Sin cupones (200 OK):**
```json
[]
```

**Ejemplo cURL:**
```bash
curl -X GET http://localhost:7185/api/coupons/getCouponsUser/user@example.com
```

**Filtrado:**
- Solo retorna cupones donde `isValid = true`
- Solo retorna cupones donde `expirationDate > hoy`
- Ordenados por fecha de creación

---

### 3. Obtener Cupón por ID

Consulta un cupón específico por su identificador único.

**Endpoint:** `POST /api/coupons/getCoupon/{id}`

⚠️ **Nota**: Este endpoint está marcado como POST pero realiza una operación de lectura. Debería ser GET (ver deuda técnica).

**Parámetros de Ruta:**
- `id` (Guid, requerido) - ID del cupón

**Respuesta Exitosa (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "discountAmount": 25,
  "createdAt": "2026-01-20T00:00:00",
  "expirationDate": "2027-01-20T00:00:00",
  "isValid": true,
  "amountMin": 50
}
```

**Respuesta - Cupón no encontrado (200 OK):**
```json
null
```

**Ejemplo cURL:**
```bash
curl -X POST http://localhost:7185/api/coupons/getCoupon/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

### 4. Invalidar Cupón (Marcar como usado)

Marca un cupón como inválido cuando el usuario lo utiliza en una compra.

**Endpoint:** `PUT /api/coupons/updateUser/{id}`

⚠️ **Nota**: El nombre del endpoint sugiere actualizar usuario, pero realmente invalida un cupón (ver deuda técnica).

**Parámetros de Ruta:**
- `id` (Guid, requerido) - ID del cupón a invalidar

**Respuesta Exitosa (200 OK):**
```json
{
  "mensaje": "El cupon se actualizo exitosamente.",
  "exito": true
}
```

**Respuesta Error - Cupón no encontrado (400 Bad Request):**
```json
{
  "mensaje": "El cupon no pudo ser actualizado.",
  "exito": false
}
```

**Ejemplo cURL:**
```bash
curl -X PUT http://localhost:7185/api/coupons/updateUser/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Comportamiento:**
- Cambia `isValid` de `true` a `false`
- Operación idempotente (llamar múltiples veces no causa error)
- No verifica si el cupón ya estaba inválido

---

## Swagger / OpenAPI

El servicio incluye documentación interactiva Swagger en modo desarrollo:

```
http://localhost:7185/swagger
```

Desde Swagger puedes:
- Ver todos los endpoints disponibles
- Probar las APIs directamente desde el navegador
- Ver esquemas de Request/Response
- Generar clientes de API automáticamente

## Ejemplo Completo: Flujo de Uso de Cupón

### Paso 1: Usuario solicita un cupón
```bash
POST /api/coupons/generateCoupon
Body: { "email": "maria@ejemplo.com" }

Response:
{
  "mensaje": "¡Felicidades! Se ha generado un cupón del 20% de descuento para un monto minimo de 25. Puedes canjear hasta el 20/01/2027",
  "exito": true,
  "coupon": {
    "id": "a1b2c3d4-...",
    "email": "maria@ejemplo.com",
    "discountAmount": 20,
    "amountMin": 25,
    "isValid": true
  }
}
```

### Paso 2: Usuario consulta sus cupones disponibles
```bash
GET /api/coupons/getCouponsUser/maria@ejemplo.com

Response:
[
  {
    "id": "a1b2c3d4-...",
    "discountAmount": 20,
    "amountMin": 25,
    "isValid": true,
    "expirationDate": "2027-01-20T00:00:00"
  }
]
```

### Paso 3: Usuario aplica el cupón en una compra
```bash
PUT /api/coupons/updateUser/a1b2c3d4-...

Response:
{
  "mensaje": "El cupon se actualizo exitosamente.",
  "exito": true
}
```

### Paso 4: Verificar que el cupón ya no aparece como válido
```bash
GET /api/coupons/getCouponsUser/maria@ejemplo.com

Response:
[]  // El cupón usado ya no aparece
```

## Códigos de Estado HTTP

| Código | Significado | Cuándo se usa |
|--------|-------------|---------------|
| 200 OK | Éxito | Operación completada correctamente |
| 400 Bad Request | Error de validación | Email vacío, cupón no encontrado, límite de 15 días |
| 500 Internal Server Error | Error del servidor | Excepción no controlada, error de BD |

## Tipos de Datos

### GenerateCouponDto
```json
{
  "email": "string"  // Requerido, debe ser email válido
}
```

### GetValidCouponsDto
```json
{
  "id": "uuid",           // Identificador único
  "email": "string",      // Email del propietario
  "discountAmount": int,  // Porcentaje: 5, 10, 15, 20, 25, 50
  "createdAt": "datetime",
  "expirationDate": "datetime",
  "isValid": boolean,
  "amountMin": decimal    // Monto: 0, 5, 10, 15, 20, 25, 50, 100
}
```

### ResultadoDTO
```json
{
  "mensaje": "string",           // Mensaje descriptivo
  "exito": boolean,              // true = éxito, false = error
  "coupon": GetValidCouponsDto?  // Opcional, presente en generación exitosa
}
```

## Limitaciones Conocidas

1. **Sin autenticación**: Cualquiera puede generar cupones con cualquier email
2. **Sin validación de email**: No verifica que el email tenga formato válido
3. **Sin rate limiting**: Posible abuso generando múltiples cupones con emails diferentes
4. **Email como identificador**: No hay integración con servicio de usuarios real
5. **Sin paginación**: GET cupones retorna todos los cupones sin límite

## Próximas Mejoras Sugeridas

- [ ] Autenticación JWT o OAuth2
- [ ] Validación de formato de email
- [ ] Rate limiting por IP
- [ ] Paginación en listado de cupones
- [ ] Webhooks para notificar cuando un cupón se usa
- [ ] Endpoint para estadísticas (cupones generados/usados)
- [ ] Filtros adicionales (por fecha, por monto, etc.)
