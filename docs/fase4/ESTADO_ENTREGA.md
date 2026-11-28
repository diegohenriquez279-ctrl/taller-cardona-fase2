# Estado de la entrega — Fase 4 (API, Seguridad y Despliegue)

Corte: **27/11/2026** (entrega final), rama `main` con `modulo/vehiculos` y `modulo/pagos` integradas, más el cierre de pendientes de Diego (menú, Pagos, conteo por estado).
Verificado ejecutando la app en desarrollo (`dotnet run`) con los tres usuarios de prueba (admin, recepción, mecánico). **Presupuestos queda fuera de este cierre: sin cambios, lo decide el equipo aparte.**

Responsables: **Diego** (Clientes + base: seguridad, API, Swagger, logging, despliegue) · **Kimberly** (Vehículos) · **Ariel** (Órdenes de trabajo) · **Andrea** (Presupuestos y catálogo) · **Carlos** (Pagos).

Leyenda: ✅ hecho · ⚠️ parcial · ❌ falta

---

## Tabla por criterio

| # | Criterio | Estado | Qué está hecho | Evidencia | Qué falta y de quién |
|---|---|---|---|---|---|
| 1 | **API REST** | ⚠️ | `POST /api/auth/login` (token JWT). **Clientes**, **Vehículos**, **Órdenes** y **Pagos** con API REST, DTO y acceso por servicio. **Pagos** (nuevo): `GET /api/pagos`, `GET /api/pagos/{id}`, `GET /api/pagos/presupuesto/{presupuestoId}` (pagos + saldo), `POST /api/pagos` (400 ProblemDetails con el mensaje de la regla) y `DELETE /api/pagos/{id}` (solo Administrador). `/api/ordenes/conteo-por-estado` devuelve el **nombre** del estado (`"Recibido"`, `"EnDiagnostico"`…) gracias a `JsonStringEnumConverter` global. | `Controllers/Api/PagosApiController.cs`, `Models/Dtos/PagoDto.cs`, `Controllers/Api/VehiculosApiController.cs`, `Program.cs`, `/swagger` | **Andrea / equipo**: API de Presupuestos y catálogo (pendiente, sin cambios en este cierre). |
| 2 | **Seguridad (autenticación y autorización)** | ⚠️ | Identity con usuarios y roles; cookie para MVC, JWT para API y políticas `SoloAdministrador`, `AdminORecepcion`, `PersonalTaller`. Clientes, Vehículos, Órdenes y **Pagos** aplican sus políticas. En **Pagos** Recepción lista, ve y registra; **eliminar es solo del Administrador** (MVC `Delete` GET/POST y `DELETE /api/pagos/{id}`) y el botón Eliminar solo se muestra a ese rol. Mecánico no entra a Pagos (MVC → acceso denegado, API → 403). | `Program.cs`, `Controllers/PagosController.cs`, `Views/Pagos/Index.cshtml`, `Views/Pagos/Details.cshtml` | **Andrea / equipo**: permisos de Presupuestos. **Diego**: pantalla de gestión de cuentas (ver “Pendientes detectados”). |
| 3 | **Protección (datos y ataques comunes)** | ✅ | Antiforgery global en todos los POST de MVC (también en el fetch del tablero: sin token → 400); contraseñas fuertes y bloqueo tras 5 intentos; encabezados `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; HSTS en producción; validación con DTO en la API; secretos fuera del repo (user-secrets en desarrollo, variables del sitio en IIS); paquetes NuGet sin vulnerabilidades (build con 0 advertencias). | `Program.cs`, `Models/Dtos/ClienteDto.cs`, `docs/fase4/DESPLIEGUE.md` §3.6 | Opcional (**Diego**): el sitio de IIS solo tiene HTTP; para HTTPS haría falta un binding con certificado. |
| 4 | **Comunicación asíncrona** | ✅ | **Clientes** y **Vehículos**: búsqueda en vivo con `fetch` mientras se escribe (probada en navegador). **Órdenes**: cambio de estado sin recargar en el tablero, con `fetch` y token antiforgery (probado después del cambio a estados con nombre). **Pagos**: repositorio, servicio, controlador MVC y API **async** (`ToListAsync`, `FindAsync`, `SaveChangesAsync`). | `wwwroot/js/clientes.js`, `wwwroot/js/vehiculos.js`, `wwwroot/js/tablero.js`, `Repositorios/PagoRepositorio.cs` | Presupuestos: lo decide el equipo. |
| 5 | **Documentación y pruebas de API con Swagger** | ⚠️ | Swagger con autenticación Bearer y documentación XML. Probados con token: Clientes, Vehículos (GET/POST/DELETE), Órdenes (incluido conteo con nombres) y Pagos (GET/POST/DELETE, por presupuesto, 400/403/404). | `/swagger`, `docs/fase4/SEGURIDAD_Y_API.md` §5.1–5.2, `docs/fase4/VEHICULOS_API.md` | Falta la API de Presupuestos/catálogo. Capturas de Swagger para el manual/video. |
| 6 | **Logging y manejo de excepciones** | ✅ | Serilog guarda logs diarios y el middleware maneja errores globales. Logs de negocio en Clientes, Órdenes, Vehículos y **Pagos**: pago registrado (Information: id, orden, presupuesto, monto), pago rechazado por regla (Warning, con el motivo) y pago eliminado (Information, con el usuario). Verificado en `Logs/taller-20261127.txt`. | `Program.cs`, `Middleware/ManejoErroresMiddleware.cs`, `Services/PagoService.cs` | Presupuestos: lo decide el equipo. |
| 7 | **Despliegue (IIS o Azure)** | ✅ | IIS local: sitio y pool `TallerCardona` (No Managed Code) en el puerto 8080, Hosting Bundle 10.0.15, variables de producción a nivel del sitio, login SQL para `IIS APPPOOL\TallerCardona` (db_datareader + db_datawriter), permisos de escritura en Logs, regla de firewall 8080 (red privada). Validado: login, roles, Swagger, token, logs. | `Properties/PublishProfiles/IIS.pubxml`, `docs/fase4/DESPLIEGUE.md`, http://localhost:8080 | **Diego**: tomar las capturas de `DESPLIEGUE.md` §7. Para abrir desde otro dispositivo la Wi‑Fi debe estar como **red privada** (hoy está como Pública; ver §3.9). |
| 8 | **Producto final (funciona integrado)** | ⚠️ | Compila sin errores ni advertencias. Menú principal corregido (faltaba un `</ul>` en `_Layout`; probado en escritorio y con el botón hamburguesa en móvil). Clientes, Vehículos, Órdenes y Pagos con CRUD en SQL Server. Vehículos integrado (Kimberly) y probado; se corrigió la edición, que la validación del navegador bloqueaba por el formato del kilometraje. Pagos integrado (Carlos): `PagoService` con reglas (monto > 0, presupuesto Autorizado, no superar el saldo), vista `Details`, confirmación de `Delete` y enlace en el menú. | Commits del 27/11/2026 en `main` | **Presupuestos (pendiente, sin cambios)**: sigue **en memoria** y Pagos lee el presupuesto desde ese repositorio (`IPresupuestoRepositorio`), así que solo se pueden registrar pagos de órdenes cuyo presupuesto se creó y autorizó en memoria; los presupuestos de la base (semilla) no se ven, y todo se pierde al reiniciar o reciclar el pool. No hay botón para crear presupuestos. |
| 9 | **Manuales (técnico y de usuario)** | ❌ | Hay insumos técnicos y documentos UX por módulo, incluido `docs/ux/UX_Vehiculos.md`, y la guía de endpoints de Vehículos. | `docs/`, `docs/ux/UX_Vehiculos.md`, `docs/fase4/VEHICULOS_API.md` | **Diego**: compilar el manual técnico con capturas. **Cada integrante** debe preparar su sección de uso del módulo con imágenes; confirmar que UX Vehículos cumple la plantilla requerida. |
| 10 | **Git (ramas, commits, integración)** | ✅ | Una rama por módulo e integración a `main` mediante pull request (#20 Vehículos, #21 Pagos), con commits descriptivos. Cierre del 27/11 con commits separados directo en `main`. | `git log --graph --oneline` | Ramas `modulo/presupuestos` y `modulo/presupuesto`: las decide el equipo. |
| 11 | **Video** | ❌ | Guion sugerido para la parte de despliegue: lista de capturas de `DESPLIEGUE.md` §7. | — | **Todo el equipo**: grabarlo. Cada uno muestra su módulo (CRUD, su API en Swagger, su función asíncrona); **Diego** muestra seguridad, Swagger con token, logs y el despliegue en IIS. No mostrar el valor de `Jwt__Key`. |

---

## Pendientes detectados en pruebas

| Fecha | Pendiente | Detalle | Responsable |
|---|---|---|---|
| 27/11/2026 | **Crear cuentas de usuario** | Al probar la app publicada desde un celular no había credenciales ni forma de crear una cuenta: en el login no existe la opción y solo están los 3 usuarios de prueba de `SeedIdentidad`. Propuesta: pantalla **solo para el Administrador** (política `SoloAdministrador`) para crear usuarios y asignarles rol (Administrador, Recepción o Mecánico), en vez de un registro abierto, porque es un sistema interno y cada cuenta necesita un rol. | Diego |
| 27/11/2026 | **Republicar en IIS los cambios del cierre** | `dotnet publish` a `C:\inetpub\TallerCardona` y el reinicio del pool `TallerCardona` requieren PowerShell **como administrador** (ver `DESPLIEGUE.md`). Después, comprobar en http://localhost:8080 login, menú, Pagos y Swagger. | Diego |

---

## Cierre del 27/11/2026 (Diego)

1. Menú principal: se cerró el primer `<ul>` de `_Layout.cshtml`.
2. Pagos: eliminar solo para Administrador (MVC y API) y botón oculto para los demás roles.
3. Pagos: API REST con DTOs, repositorio/servicio async y bitácora con Serilog.
4. API: los enums se serializan como texto (conteo por estado con nombres).
5. Vehículos: corrección del formulario de edición (kilometraje).

### Resultado de las pruebas (27/11/2026, `dotnet run`)

| Prueba | Admin | Recepción | Mecánico |
|---|---|---|---|
| Menú completo y bien cerrado (escritorio y hamburguesa en móvil) | ✅ | ✅ | ✅ |
| Pagos: listado | ✅ con Eliminar | ✅ sin Eliminar | Acceso denegado (correcto) |
| `/Pagos/Delete/1` | ✅ confirma y elimina | Acceso denegado (correcto) | Acceso denegado (correcto) |
| Registrar pago válido (MVC y API) | ✅ | ✅ | — |
| Pago inválido (supera saldo / sin presupuesto / monto 0) muestra el mensaje | ✅ | ✅ | — |
| Vehículos: búsqueda en vivo y CRUD | ✅ | | |
| Tablero: cambio de estado con `fetch` | ✅ | | |
| `/api/pagos` GET / POST / DELETE | ✅ (DELETE 204) | ✅ (DELETE 403) | 403 (correcto) |
| `/api/vehiculos` GET / POST / DELETE | ✅ | | |
| `/api/ordenes/conteo-por-estado` con nombres | ✅ | | |
| Entradas de Pagos en `Logs/taller-20261127.txt` | ✅ | | |

## Lo que hizo Diego en el corte del 26/11

1. Integró `modulo/ordenes` en `main` (sin conflictos), compiló y verificó que no hay cambios de modelo pendientes.
2. `UserSecretsId` en el proyecto: la cadena local (`localhost\SQLEXPRESS`) queda en user-secrets, no en el repo.
3. Integración: registro de `IPresupuestoService` y corrección de la redirección de Presupuestos.
4. Despliegue en IIS local completo y documentado (`DESPLIEGUE.md`).
5. Este reporte.

## Resultado de las pruebas del corte anterior (26/11/2026)

| Prueba | Admin | Recepción | Mecánico |
|---|---|---|---|
| Login MVC | ✅ | ✅ | ✅ |
| Clientes (MVC, búsqueda en vivo) | ✅ | ✅ | 403 (correcto) |
| Eliminar cliente / orden | ✅ | 403 (correcto) | 403 (correcto) |
| Vehículos, Órdenes, Tablero | ✅ | ✅ | ✅ |
| Presupuestos, Pagos | ✅ | ✅ | ✅ (**no debería** tener acceso a todo; ver criterio 2) |
| `/api/auth/login` | ✅ | ✅ | ✅ |
| `/api/clientes` | ✅ | ✅ | 403 (correcto) |
| `/api/ordenes` (+ conteo) | ✅ | ✅ | ✅ |
| CRUD por API de clientes (201/204/404, 400 con datos inválidos) | ✅ | | |
| Cambio de estado por `fetch` en el tablero (sin token → 400) | ✅ | | |
| Página 404 amable / error 500 con id de seguimiento en el log | ✅ | | |

## URLs del despliegue

- Local: http://localhost:8080 — Swagger: http://localhost:8080/swagger
- Red local: http://192.168.0.100:8080 (IP del 26/11/2026; confirmar con `ipconfig`; requiere red privada)
