# Seguridad, API y manejo de errores — Fase 4

Este documento explica cómo quedó protegida la aplicación Taller Cardona, cómo se usa la API y qué tiene que hacer cada integrante en su módulo.

---

## 1. Usuarios y roles de prueba

Se crean solos al arrancar la aplicación (archivo `Data/SeedIdentidad.cs`), solo si no existen.

| Usuario | Contraseña | Rol |
|---|---|---|
| admin@tallercardona.com | Admin123! | Administrador |
| recepcion@tallercardona.com | Recepcion123! | Recepcion |
| mecanico@tallercardona.com | Mecanico123! | Mecanico |

Reglas de contraseña: mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo.
Después de **5 intentos fallidos** la cuenta queda bloqueada **5 minutos**.

Los nombres de los roles están en `Models/Roles.cs` (`Roles.Administrador`, `Roles.Recepcion`, `Roles.Mecanico`). Úsenlos en vez de escribir el texto a mano.

---

## 2. Cómo funciona la seguridad

Hay dos formas de identificarse:

| Parte | Cómo se identifica | Dónde se configura |
|---|---|---|
| Páginas MVC (lo que ve el usuario en el navegador) | **Cookie**: se inicia sesión en `/Account/Login` | `Program.cs` → Identity |
| API (`/api/...`) | **Token JWT** en el encabezado `Authorization: Bearer <token>` | `Program.cs` → JWT y `Controllers/Api/ApiControllerBase.cs` |

La cookie **no sirve** para la API y el token **no sirve** para las páginas MVC.

**Regla general:** todo requiere haber iniciado sesión (política de respaldo). Solo son públicas las páginas con `[AllowAnonymous]`: Inicio, Privacidad, Error, Login, Acceso denegado, el login de la API y Swagger.

### Qué protege cada política

| Política | Roles que pasan | Uso |
|---|---|---|
| `SoloAdministrador` | Administrador | Eliminar registros |
| `AdminORecepcion` | Administrador, Recepcion | Clientes, vehículos, presupuestos, pagos |
| `PersonalTaller` | Administrador, Recepcion, Mecanico | Lo que usa todo el taller (por ejemplo, órdenes de trabajo) |

Las mismas políticas sirven para MVC y para la API: la política solo pide el rol; si se usa cookie o token lo decide el controlador.

Qué pasa cuando no se tiene permiso:

| Situación | Páginas MVC | API |
|---|---|---|
| Sin iniciar sesión | Lleva al login | `401 Unauthorized` |
| Con sesión pero sin el rol | Página "Acceso denegado" | `403 Forbidden` |

### Otras protecciones incluidas

- **Token antiforgery** obligatorio en todos los POST de MVC (filtro global). Los formularios con `asp-action` ya lo incluyen solos.
- **Encabezados de seguridad** en todas las respuestas: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`. HSTS en producción.
- El login solo regresa a direcciones del propio sitio (`returnUrl` validado).

---

## 3. Cómo obtener y usar el token (API)

1. Pedir el token:

```http
POST /api/auth/login
Content-Type: application/json

{ "correo": "admin@tallercardona.com", "clave": "Admin123!" }
```

Respuesta `200`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expira": "2026-11-25T04:36:27Z",
  "roles": ["Administrador"]
}
```

Si el correo o la contraseña están mal: `401`.

2. Mandar el token en cada petición:

```http
GET /api/clientes
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

El token dura **60 minutos** (`Jwt:ExpiraMinutos` en `appsettings.json`). Después hay que pedir otro.

### La clave del token

- En desarrollo está en `appsettings.Development.json` (`Jwt:Key`). Es solo para pruebas.
- En producción (Azure) **no** se pone en ningún archivo: se crea la configuración / variable de entorno `Jwt__Key` con una clave larga y secreta (mínimo 32 caracteres). Si falta, la aplicación no arranca y avisa.

---

## 4. Cómo probar en Swagger

1. Abrir `https://<sitio>/swagger` (funciona también en producción para la demo).
2. Abrir **POST /api/auth/login** → *Try it out* → escribir correo y clave → *Execute*.
3. Copiar el valor de `token` (sin comillas).
4. Clic en el botón **Authorize** (arriba a la derecha), pegar el token y aceptar. No hace falta escribir "Bearer".
5. Probar cualquier endpoint. Con `mecanico` los de clientes dan `403`; con `recepcion`, el DELETE da `403`.

---

## 5. Endpoints de Clientes

Todos requieren token. Rol mínimo: Administrador o Recepcion (DELETE solo Administrador).

| Método | Ruta | Rol | Qué hace | Respuestas |
|---|---|---|---|---|
| POST | `/api/auth/login` | Público | Devuelve el token | 200, 400, 401 |
| GET | `/api/clientes` | Admin / Recepción | Lista clientes; filtro opcional `?busqueda=texto` (nombre o teléfono) | 200, 401, 403 |
| GET | `/api/clientes/{id}` | Admin / Recepción | Un cliente | 200, 404, 401, 403 |
| POST | `/api/clientes` | Admin / Recepción | Crea un cliente | 201 (con encabezado `Location`), 400, 401, 403 |
| PUT | `/api/clientes/{id}` | Admin / Recepción | Modifica un cliente | 204, 400, 404, 401, 403 |
| DELETE | `/api/clientes/{id}` | Administrador | Elimina un cliente sin vehículos | 204, 400, 404, 401, 403 |

Datos de un cliente (`ClienteDto`):

```json
{
  "id": 1,
  "nombre": "Carlos Martínez",
  "telefono": "7712-3456",
  "correo": "carlos.martinez@correo.com",
  "direccion": "Colonia Escalón",
  "cantidadVehiculos": 2
}
```

- `400` por datos inválidos trae la lista de errores por campo (`errors`).
- `400` por regla de negocio (teléfono repetido, cliente con vehículos) trae el mensaje en `detail`.
- `404` trae `detail: "No existe un cliente con id X."`.

---

## 6. Manejo de errores y logs

- Si ocurre un error no previsto, el middleware `Middleware/ManejoErroresMiddleware.cs`:
  - Lo guarda en el log con un **id de seguimiento** de 8 caracteres (ej. `3A077A3E`).
  - En la **API** responde `500` en JSON con un mensaje general y `idSeguimiento`.
  - En las **páginas** muestra "Algo salió mal" con el mismo código, sin detalles técnicos.
- Páginas que no existen (404) muestran "No encontramos lo que busca".
- Para probarlo en desarrollo: `/Home/ProbarError` (en producción responde 404).

**Dónde están los logs:** carpeta `Logs/` dentro del proyecto, un archivo por día: `Logs/taller-20261124.txt`. Se guardan los últimos 30 días. También salen en la consola. La carpeta no se sube a Git.

Para encontrar un error que reportó un usuario, buscar su código en el archivo del día:

```powershell
Select-String -Path TallerCardona\Logs\taller-*.txt -Pattern "3A077A3E" -Context 0,15
```

---

## 7. CÓMO LO USA CADA COMPAÑERO

### 7.1 Proteger su controlador MVC

Agregar la política arriba de la clase (y en las acciones de eliminar, `SoloAdministrador`):

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = "AdminORecepcion")]     // o "PersonalTaller" para órdenes
public class VehiculosController : Controller
{
    // ...

    [Authorize(Policy = "SoloAdministrador")]
    public IActionResult Delete(int id) { ... }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SoloAdministrador")]
    public IActionResult DeleteConfirmed(int id) { ... }
}
```

Para ocultar un botón según el rol en una vista:

```cshtml
@if (User.IsInRole(Roles.Administrador))
{
    <a asp-action="Delete" asp-route-id="@item.Id" class="btn btn-sm btn-outline-danger">Eliminar</a>
}
```

### 7.2 Crear su API

1. **DTO**: copiar `Models/Dtos/ClienteDto.cs` → `VehiculoDto.cs`. Poner sus propiedades con validaciones y los métodos `DesdeEntidad(...)` y `AEntidad(...)`. Nunca devolver la entidad directamente (las relaciones hacen ciclos al convertir a JSON).
2. **Controlador**: copiar `Controllers/Api/ClientesApiController.cs` → `VehiculosApiController.cs` y cambiar:
   - `[Route("api/vehiculos")]`
   - `[Authorize(Policy = "...")]`
   - el servicio (`IVehiculoService`) y el DTO.
3. **Heredar de `ApiControllerBase`** (ya trae JWT, `[ApiController]` y sin antiforgery):

```csharp
[Route("api/vehiculos")]
[Authorize(Policy = "AdminORecepcion")]
public class VehiculosApiController : ApiControllerBase
{
    private readonly IVehiculoService _servicio;
    public VehiculosApiController(IVehiculoService servicio) => _servicio = servicio;

    /// <summary>Lista los vehículos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VehiculoDto>), StatusCodes.Status200OK)]
    public ActionResult<List<VehiculoDto>> Listar() =>
        Ok(_servicio.Listar().Select(VehiculoDto.DesdeEntidad).ToList());
}
```

4. Usar siempre el **servicio**, no el repositorio, para que la API aplique las mismas reglas que las páginas.
5. Los comentarios `///` aparecen solos en Swagger.

### 7.3 Registrar en el log

```csharp
private readonly ILogger<VehiculoService> _logger;

public VehiculoService(..., ILogger<VehiculoService> logger) { _logger = logger; }

_logger.LogInformation("Vehículo creado: Id {Id}, placa {Placa}", v.Id, v.Placa);
_logger.LogWarning("Crear vehículo rechazado: placa {Placa} repetida", v.Placa);
```

Usar `{Nombre}` en el mensaje en lugar de concatenar textos.

### 7.4 Mandar el token antiforgery en un fetch POST

Todo POST a un controlador MVC necesita el token (si no, responde 400). `_Layout` ya lo deja disponible con `window.tokenAntiforgery()`:

```javascript
const respuesta = await fetch('/Ordenes/CambiarEstado', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': window.tokenAntiforgery()
    },
    body: JSON.stringify({ id: 5, estado: 'Terminada' })
});
```

Del lado del servidor la acción no necesita nada extra (`[FromBody]` si recibe JSON). Las llamadas a `/api/...` no usan este token, usan el JWT.

Para una búsqueda en vivo (GET), ver el ejemplo completo en `Views/Clientes/Index.cshtml` + `wwwroot/js/clientes.js` (incluye la función `escapar()` para evitar XSS: úsenla siempre antes de meter texto en `innerHTML`).

---

## 8. Archivos de referencia

| Archivo | Para qué |
|---|---|
| `Program.cs` | Identity, JWT, políticas, Swagger, Serilog, middleware |
| `Data/SeedIdentidad.cs` | Roles y usuarios de prueba |
| `Models/Roles.cs` | Nombres de los roles |
| `Controllers/AccountController.cs` | Login / Logout / Acceso denegado |
| `Controllers/Api/ApiControllerBase.cs` | Base de todas las APIs |
| `Controllers/Api/AuthApiController.cs` | Login de la API (token) |
| `Controllers/Api/ClientesApiController.cs` | **Ejemplo** de API completa |
| `Models/Dtos/ClienteDto.cs` | **Ejemplo** de DTO |
| `Services/TokenService.cs` | Genera el token JWT |
| `Middleware/ManejoErroresMiddleware.cs` | Errores no controlados |
| `wwwroot/js/site.js` | `window.tokenAntiforgery()` |
| `wwwroot/js/clientes.js` | **Ejemplo** de búsqueda en vivo con fetch |
