# Estado de la entrega — Fase 4 (API, Seguridad y Despliegue)

Corte: **27/11/2026** (entrega final), rama `main`: Vehículos y Pagos integrados, cierre de pendientes de Diego y **Presupuestos persistido en SQL Server** sobre el repositorio EF de Andrea.
Verificado ejecutando la app en desarrollo (`dotnet run`) y publicada en IIS (http://localhost:8080) con los tres usuarios de prueba (admin, recepción, mecánico).

Responsables: **Diego** (Clientes + base: seguridad, API, Swagger, logging, despliegue) · **Kimberly** (Vehículos) · **Ariel** (Órdenes de trabajo) · **Andrea** (Presupuestos y catálogo) · **Carlos** (Pagos).

Leyenda: ✅ hecho · ⚠️ parcial · ❌ falta

---

## Tabla por criterio

| # | Criterio | Estado | Qué está hecho | Evidencia | Qué falta y de quién |
|---|---|---|---|---|---|
| 1 | **API REST** | ✅ | `POST /api/auth/login` (token JWT). **Clientes**, **Vehículos**, **Órdenes**, **Pagos** y **Presupuestos** con API REST, DTO y acceso por servicio. **Presupuestos** (nuevo): `GET /api/presupuestos`, `GET /api/presupuestos/{id}`, `POST /api/presupuestos/orden/{ordenId}` (generar), `POST /api/presupuestos/{id}/autorizar` y `/rechazar`; reglas incumplidas → 400 ProblemDetails. Pagos: listar, obtener, por presupuesto con saldo, registrar y eliminar (solo Administrador). Los enums viajan como texto (`JsonStringEnumConverter`). | `Controllers/Api/PresupuestosApiController.cs`, `Controllers/Api/PagosApiController.cs`, `Models/Dtos/`, `/swagger` | Catálogo (servicios/repuestos) sin API ni pantallas de mantenimiento: se usa solo desde las órdenes. |
| 2 | **Seguridad (autenticación y autorización)** | ⚠️ | Identity con usuarios y roles; cookie para MVC, JWT para API y políticas `SoloAdministrador`, `AdminORecepcion`, `PersonalTaller`. Todos los módulos aplican su política. **Presupuestos**: `AdminORecepcion` a nivel de clase (generar, autorizar, rechazar; no existe eliminar). **Pagos**: eliminar solo Administrador. Mecánico no entra a Presupuestos ni Pagos; el menú ya no le muestra esos enlaces y tampoco ve la tarjeta de presupuesto/pagos en el detalle de la orden. | `Program.cs`, `Controllers/PresupuestosController.cs`, `Controllers/PagosController.cs`, `Views/Shared/_Layout.cshtml` | **Diego**: pantalla de gestión de cuentas (ver “Pendientes detectados”). |
| 3 | **Protección (datos y ataques comunes)** | ✅ | Antiforgery global en todos los POST de MVC (también en el fetch del tablero: sin token → 400); contraseñas fuertes y bloqueo tras 5 intentos; encabezados `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; HSTS en producción; validación con DTO en la API; secretos fuera del repo (user-secrets en desarrollo, variables del sitio en IIS); paquetes NuGet sin vulnerabilidades (build con 0 advertencias). | `Program.cs`, `Models/Dtos/ClienteDto.cs`, `docs/fase4/DESPLIEGUE.md` §3.6 | Opcional (**Diego**): el sitio de IIS solo tiene HTTP; para HTTPS haría falta un binding con certificado. |
| 4 | **Comunicación asíncrona** | ✅ | **Clientes** y **Vehículos**: búsqueda en vivo con `fetch`. **Órdenes**: cambio de estado sin recargar en el tablero. **Pagos**: repositorio, servicio, controlador y API **async**. | `wwwroot/js/clientes.js`, `wwwroot/js/vehiculos.js`, `wwwroot/js/tablero.js`, `Repositorios/PagoRepositorio.cs` | — |
| 5 | **Documentación y pruebas de API con Swagger** | ✅ | Swagger con autenticación Bearer y documentación XML. Probados con token: Clientes, Vehículos, Órdenes (conteo con nombres), Pagos y Presupuestos (200/201/400/401/403/404). | `/swagger`, `docs/fase4/SEGURIDAD_Y_API.md` §5.1–5.3, `docs/fase4/VEHICULOS_API.md` | Capturas de Swagger para el manual/video. |
| 6 | **Logging y manejo de excepciones** | ✅ | Serilog con logs diarios y middleware de errores globales. Logs de negocio en Clientes, Órdenes, Vehículos, **Pagos** (registrado, rechazado, eliminado con usuario) y **Presupuestos** (generado/regenerado con id, orden y total; autorizado/rechazado con usuario; regla incumplida como Warning). Verificado en `Logs/taller-20261127.txt` (desarrollo e IIS). | `Program.cs`, `Middleware/ManejoErroresMiddleware.cs`, `Services/PagoService.cs`, `Services/PresupuestoService.cs` | — |
| 7 | **Despliegue (IIS o Azure)** | ✅ | IIS local: sitio y pool `TallerCardona` (No Managed Code) en el puerto 8080, Hosting Bundle 10.0.15, variables de producción a nivel del sitio, login SQL para `IIS APPPOOL\TallerCardona` (db_datareader + db_datawriter), permisos de escritura en Logs, regla de firewall 8080 (red privada). Validado: login, roles, Swagger, token, logs. | `Properties/PublishProfiles/IIS.pubxml`, `docs/fase4/DESPLIEGUE.md`, http://localhost:8080 | **Diego**: tomar las capturas de `DESPLIEGUE.md` §7. Para abrir desde otro dispositivo la Wi‑Fi debe estar como **red privada** (hoy está como Pública; ver §3.9). |
| 8 | **Producto final (funciona integrado)** | ✅ | Compila sin errores ni advertencias. Menú corregido. Clientes, Vehículos, Órdenes, **Presupuestos** y Pagos persistidos en SQL Server. **Presupuestos**: `Program.cs` registra con `AddScoped` los repositorios EF de Andrea (`Models/Repositorios/PresupuestoRepositorio.cs`, `ServicioRepositorio.cs`, `RepuestoRepositorio.cs`); los repositorios en memoria quedan en el proyecto con un comentario de que ya no se registran. Sin migración nueva (las tablas y la semilla ya estaban en `InicialTallerCardona`). Botón **Generar presupuesto** en el detalle de la orden (total = servicios + repuestos; mensaje claro si la orden no tiene detalles; si el anterior fue Rechazado se regenera el mismo registro, porque la base permite un presupuesto por orden). Pagos lee los presupuestos de la base y el saldo baja al pagar. Reporte por estado en Presupuestos/Index con datos reales. Probado: los datos siguen tras reiniciar la app y tras reciclar el pool de IIS. | Commits del 27/11/2026 en `main` | Opcional: pantallas de mantenimiento del catálogo. |
| 9 | **Manuales (técnico y de usuario)** | ❌ | Hay insumos técnicos y documentos UX por módulo, incluido `docs/ux/UX_Vehiculos.md`, y la guía de endpoints de Vehículos. | `docs/`, `docs/ux/UX_Vehiculos.md`, `docs/fase4/VEHICULOS_API.md` | **Diego**: compilar el manual técnico con capturas. **Cada integrante** debe preparar su sección de uso del módulo con imágenes; confirmar que UX Vehículos cumple la plantilla requerida. |
| 10 | **Git (ramas, commits, integración)** | ✅ | Una rama por módulo e integración a `main` mediante pull request (#20 Vehículos, #21 Pagos), con commits descriptivos. Cierres del 27/11 con commits separados directo en `main`; Presupuestos parte del código de Andrea ya integrado en `main`. | `git log --graph --oneline` | Las ramas `modulo/presupuestos` y `modulo/presupuesto` quedaron superadas por `main` (no se borraron). |
| 11 | **Video** | ❌ | Guion sugerido para la parte de despliegue: lista de capturas de `DESPLIEGUE.md` §7. | — | **Todo el equipo**: grabarlo. Cada uno muestra su módulo (CRUD, su API en Swagger, su función asíncrona); **Diego** muestra seguridad, Swagger con token, logs y el despliegue en IIS. No mostrar el valor de `Jwt__Key`. |

---

## Pendientes detectados en pruebas

| Fecha | Pendiente | Detalle | Responsable |
|---|---|---|---|
| 27/11/2026 | **Crear cuentas de usuario** | Al probar la app publicada desde un celular no había credenciales ni forma de crear una cuenta: en el login no existe la opción y solo están los 3 usuarios de prueba de `SeedIdentidad`. Propuesta: pantalla **solo para el Administrador** (política `SoloAdministrador`) para crear usuarios y asignarles rol (Administrador, Recepción o Mecánico), en vez de un registro abierto, porque es un sistema interno y cada cuenta necesita un rol. | Diego |

---

## Cierre de Presupuestos (27/11/2026, Diego sobre el código de Andrea)

1. Persistencia: repositorios EF de Andrea registrados con `AddScoped` en lugar de los Singleton en memoria.
2. Botón **Generar presupuesto** en el detalle de la orden; reglas en `PresupuestoService`.
3. Política `AdminORecepcion` en `PresupuestosController`; botones y menú ocultos para el mecánico.
4. Bitácora con `ILogger<PresupuestoService>`.
5. API REST de Presupuestos.

### Resultado de las pruebas de Presupuestos

| Prueba | Resultado |
|---|---|
| `dotnet build` | ✅ 0 errores, 0 advertencias; sin cambios de modelo pendientes |
| Generar desde una orden sin servicios ni repuestos | ✅ muestra “La orden no tiene servicios ni repuestos…” y no crea nada |
| Generar desde la orden 4 (recepción) → Autorizar → pagar $10 | ✅ total $15.00, el saldo baja a $5.00 |
| Reiniciar la app (`dotnet run`) y consultar | ✅ presupuesto y pago siguen en la base |
| Mecánico en `/Presupuestos` y `/api/presupuestos` | ✅ acceso denegado / 403; sin enlaces en el menú |
| Reporte por estado (Index y `/Presupuestos/ReportePorEstado`) | ✅ datos de la base |
| API: generar repetido / sin detalles / autorizar no pendiente → 400; id inexistente → 404; sin token → 401 | ✅ |
| API: rechazar y regenerar (vuelve a Pendiente) | ✅ |
| IIS (8080): generar (orden 5) → autorizar → pagar $10 → reciclar el pool | ✅ el saldo de $15.00 se mantiene después de reciclar |
| Entradas de `PresupuestoService` en `Logs/taller-20261127.txt` | ✅ desarrollo e IIS |

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
| IIS republicado (pool detenido → `dotnet publish -c Release` → pool iniciado): login, menú, Pagos, Swagger y `/api/pagos` en http://localhost:8080 | ✅ | ✅ (DELETE 403) | 403 (correcto) |

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
