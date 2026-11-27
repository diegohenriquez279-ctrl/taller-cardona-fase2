# Estado de la entrega — Fase 4 (API, Seguridad y Despliegue)

Corte: **26/11/2026**, rama `main` después de integrar `modulo/ordenes` y los ajustes de Diego.
Verificado ejecutando la app en desarrollo (`dotnet run`) y publicada en IIS (`http://localhost:8080`) con los tres usuarios de prueba (admin, recepción, mecánico).

Responsables: **Diego** (Clientes + base: seguridad, API, Swagger, logging, despliegue) · **Kimberly** (Vehículos) · **Ariel** (Órdenes de trabajo) · **Andrea** (Presupuestos y catálogo) · **Carlos** (Pagos).

Leyenda: ✅ hecho · ⚠️ parcial · ❌ falta

---

## Tabla por criterio

| # | Criterio | Estado | Qué está hecho | Evidencia | Qué falta y de quién |
|---|---|---|---|---|---|
| 1 | **API REST** | ⚠️ | `POST /api/auth/login` (token JWT). **Clientes**: GET lista/búsqueda, GET id, POST, PUT, DELETE con DTO y validaciones. **Órdenes**: GET con filtros, GET id, conteo por estado, POST, PUT, PUT estado, DELETE. Códigos 200/201/204/400/401/403/404 probados. | `Controllers/Api/ClientesApiController.cs`, `Controllers/Api/OrdenesApiController.cs`, `Controllers/Api/AuthApiController.cs`, http://localhost:8080/swagger | **Kimberly**: API de Vehículos. **Andrea**: API de Presupuestos/catálogo. **Carlos**: API de Pagos. (Plantilla a copiar: `ClientesApiController` + `docs/fase4/SEGURIDAD_Y_API.md`). **Ariel** (menor): `/api/ordenes/conteo-por-estado` devuelve el estado como número (0–4) en vez de texto. |
| 2 | **Seguridad (autenticación y autorización)** | ⚠️ | Identity con usuarios y roles sembrados; cookie para MVC y JWT para la API; políticas `SoloAdministrador`, `AdminORecepcion`, `PersonalTaller`; política de respaldo (todo exige sesión). Clientes y Órdenes aplican sus políticas (mecánico → 403 en Clientes; solo admin elimina). | `Program.cs`, `Data/SeedIdentidad.cs`, `docs/fase4/SEGURIDAD_Y_API.md` | **Kimberly**: `VehiculosController` no tiene `[Authorize(Policy=...)]` → el mecánico puede crear/editar/**eliminar** vehículos. **Andrea**: `PresupuestosController` sin política (el mecánico puede autorizar/rechazar). **Carlos**: `PagosController` sin política (el mecánico puede registrar/eliminar pagos). **Diego**: no hay forma de crear cuentas desde la app (solo existen los 3 usuarios sembrados); falta una pantalla de gestión de usuarios (ver "Pendientes detectados"). |
| 3 | **Protección (datos y ataques comunes)** | ✅ | Antiforgery global en todos los POST de MVC (también en el fetch del tablero: sin token → 400); contraseñas fuertes y bloqueo tras 5 intentos; encabezados `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; HSTS en producción; validación con DTO en la API; secretos fuera del repo (user-secrets en desarrollo, variables del sitio en IIS); paquetes NuGet sin vulnerabilidades (build con 0 advertencias). | `Program.cs`, `Models/Dtos/ClienteDto.cs`, `docs/fase4/DESPLIEGUE.md` §3.6 | Opcional (**Diego**): el sitio de IIS solo tiene HTTP; para HTTPS haría falta un binding con certificado. |
| 4 | **Comunicación asíncrona** | ⚠️ | **Clientes**: búsqueda en vivo con `fetch` + debounce + escape de HTML. **Órdenes**: tablero que cambia el estado sin recargar (`fetch` a `CambiarEstadoAjax` con token antiforgery), probado ✅. | `wwwroot/js/clientes.js`, `wwwroot/js/tablero.js`, `ClientesController.Buscar`, `OrdenesController.CambiarEstadoAjax` | **Kimberly**, **Andrea**, **Carlos**: si la rúbrica pide una función asíncrona por módulo, no tienen ninguna todavía. |
| 5 | **Documentación y pruebas de API con Swagger** | ✅ | Swagger UI disponible también en producción, con botón *Authorize* (Bearer) y comentarios XML de cada endpoint. Probado: login → token → GET/POST/PUT/DELETE de clientes y órdenes; sin token 401, rol sin permiso 403. | http://localhost:8080/swagger, `TallerCardona.csproj` (`GenerateDocumentationFile`) | Aparecerán solos los endpoints de Vehículos/Presupuestos/Pagos cuando **Kimberly/Andrea/Carlos** creen sus APIs. **Diego**: capturas de Swagger (lista en `DESPLIEGUE.md` §7). |
| 6 | **Logging y manejo de excepciones** | ⚠️ | Serilog a consola y archivo diario (`Logs/taller-AAAAMMDD.txt`, 30 días); una línea por petición; middleware global: API → JSON ProblemDetails, MVC → página de error con id de seguimiento; páginas amables 404/403. Logs de negocio en Clientes, Órdenes, login MVC y login API (intentos fallidos como WRN). En IIS los logs se escriben en `C:\inetpub\TallerCardona\Logs` ✅. | `Program.cs`, `Middleware/ManejoErroresMiddleware.cs`, `Services/ClienteService.cs`, `Services/OrdenTrabajoService.cs`, `C:\inetpub\TallerCardona\Logs\` | **Kimberly** (`VehiculoService`), **Andrea** (`PresupuestoService`) y **Carlos** (`PagosController`): no registran nada en el log (crear/editar/eliminar/rechazos). |
| 7 | **Despliegue (IIS o Azure)** | ✅ | IIS local: sitio y pool `TallerCardona` (No Managed Code) en el puerto 8080, Hosting Bundle 10.0.15, variables de producción a nivel del sitio, login SQL para `IIS APPPOOL\TallerCardona` (db_datareader + db_datawriter), permisos de escritura en Logs, regla de firewall 8080 (red privada). Validado: login, roles, Swagger, token, logs. | `Properties/PublishProfiles/IIS.pubxml`, `docs/fase4/DESPLIEGUE.md`, http://localhost:8080 | **Diego**: tomar las capturas de `DESPLIEGUE.md` §7. Para abrir desde otro dispositivo la Wi‑Fi debe estar como **red privada** (hoy está como Pública; ver §3.9). |
| 8 | **Producto final (funciona integrado)** | ⚠️ | Compila sin errores ni advertencias; todas las pantallas del menú abren con los 3 roles; Clientes, Vehículos y Órdenes con CRUD completo en SQL Server. Ajustes de integración de Diego: se registró `IPresupuestoService` (Presupuestos fallaba al abrir) y se corrigió una redirección a un controlador inexistente (`OrdenesTrabajo` → `Ordenes`). | Commits "Integración: …" en `main` | **Andrea**: Presupuestos y catálogo siguen **en memoria** (se pierden al reiniciar — en IIS, cada vez que se recicla el pool); `PresupuestoService` mezcla repositorio en memoria con EF; no hay botón para crear presupuesto en ninguna vista ni editar/eliminar; no hay pantallas del catálogo. **Carlos**: Pagos **en memoria**; no hay `PagoService`; falta la vista `Details`; `Delete` sin botón; **no hay enlace a Pagos en el menú**. |
| 9 | **Manuales (técnico y de usuario)** | ❌ | Insumos listos para el manual técnico: `SEGURIDAD_Y_API.md`, `DESPLIEGUE.md`, `docs/arquitectura/`, documentos UX por módulo en `docs/ux/`. | `docs/` | **Diego**: armar el manual técnico con esos documentos + capturas. **Cada integrante** (Kimberly, Ariel, Andrea, Carlos y Diego): la sección del manual de usuario de su módulo. Falta `docs/ux` de Vehículos (**Kimberly**). |
| 10 | **Git (ramas, commits, integración)** | ⚠️ | Una rama por módulo, integración a `main` con `merge --no-ff`, commits pequeños con prefijo (`Clientes:`, `Órdenes:`, `Base:`, `Integración:`, `Despliegue:`, `Docs:`). `modulo/ordenes` (8 commits de Ariel) integrada el 26/11. | `git log --graph --oneline main` | **Kimberly**, **Andrea**, **Carlos**: no hay commits de Fase 4 en sus ramas. `modulo/presupuestos` está 45 commits detrás de `main` (Andrea debe hacer `git merge main` antes de seguir). La rama de pagos fue borrada del remoto (Carlos debe crear `modulo/pagos` desde `main`). |
| 11 | **Video** | ❌ | Guion sugerido para la parte de despliegue: lista de capturas de `DESPLIEGUE.md` §7. | — | **Todo el equipo**: grabarlo. Cada uno muestra su módulo (CRUD, su API en Swagger, su función asíncrona); **Diego** muestra seguridad, Swagger con token, logs y el despliegue en IIS. No mostrar el valor de `Jwt__Key`. |

---

## Pendientes detectados en pruebas

| Fecha | Pendiente | Detalle | Responsable |
|---|---|---|---|
| 27/11/2026 | **Crear cuentas de usuario** | Al probar la app publicada desde un celular no había credenciales ni forma de crear una cuenta: en el login no existe la opción y solo están los 3 usuarios de prueba de `SeedIdentidad`. Propuesta: pantalla **solo para el Administrador** (política `SoloAdministrador`) para crear usuarios y asignarles rol (Administrador, Recepción o Mecánico), en vez de un registro abierto, porque es un sistema interno y cada cuenta necesita un rol. | Diego |

---

## Lo que hizo Diego en este corte

1. Integró `modulo/ordenes` en `main` (sin conflictos), compiló y verificó que no hay cambios de modelo pendientes.
2. `UserSecretsId` en el proyecto: la cadena local (`localhost\SQLEXPRESS`) queda en user-secrets, no en el repo.
3. Integración: registro de `IPresupuestoService` y corrección de la redirección de Presupuestos.
4. Despliegue en IIS local completo y documentado (`DESPLIEGUE.md`).
5. Este reporte.

## Resultado de las pruebas (26/11/2026)

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
