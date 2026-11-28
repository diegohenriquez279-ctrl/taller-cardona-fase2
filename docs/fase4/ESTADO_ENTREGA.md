# Estado de la entrega — Fase 4 (API, Seguridad y Despliegue)

Corte: **26/11/2026**, rama `main` después de integrar `modulo/ordenes` y los ajustes de Diego.
Verificado ejecutando la app en desarrollo (`dotnet run`) y publicada en IIS (`http://localhost:8080`) con los tres usuarios de prueba (admin, recepción, mecánico).

Responsables: **Diego** (Clientes + base: seguridad, API, Swagger, logging, despliegue) · **Kimberly** (Vehículos) · **Ariel** (Órdenes de trabajo) · **Andrea** (Presupuestos y catálogo) · **Carlos** (Pagos).

Leyenda: ✅ hecho · ⚠️ parcial · ❌ falta

---

## Tabla por criterio

| # | Criterio | Estado | Qué está hecho | Evidencia | Qué falta y de quién |
|---|---|---|---|---|---|
| 1 | **API REST** | ⚠️ | `POST /api/auth/login` (token JWT). **Clientes** y **Órdenes** cuentan con sus APIs y DTO. **Vehículos** ya tiene GET lista/filtro, GET por id, POST, PUT y DELETE, con DTO y acceso por servicio. | `Controllers/Api/VehiculosApiController.cs`, `Models/Dtos/VehiculoDto.cs`, `docs/fase4/VEHICULOS_API.md`, `/swagger` | **Kimberly**: falta verificar los endpoints de Vehículos en Swagger y guardar evidencia. **Andrea**: API de Presupuestos/catálogo. **Carlos**: API de Pagos. **Ariel**: `/api/ordenes/conteo-por-estado` devuelve el estado como número (0–4) en vez de texto. |
| 2 | **Seguridad (autenticación y autorización)** | ⚠️ | Identity con usuarios y roles; cookie para MVC, JWT para API y políticas `SoloAdministrador`, `AdminORecepcion`, `PersonalTaller`. Clientes y Órdenes aplican sus políticas. **Vehículos** ahora exige Administrador/Recepción en MVC y API; eliminar requiere Administrador. | `Program.cs`, `Data/SeedIdentidad.cs`, `Controllers/VehiculosController.cs`, `Controllers/Api/VehiculosApiController.cs` | **Kimberly**: pendiente probar MVC/API con los tres roles. **Andrea** y **Carlos**: revisar permisos en sus módulos. **Diego**: sigue pendiente una pantalla de gestión de cuentas, pues solo están los usuarios sembrados (ver “Pendientes detectados”). |
| 3 | **Protección (datos y ataques comunes)** | ✅ | Antiforgery global en todos los POST de MVC (también en el fetch del tablero: sin token → 400); contraseñas fuertes y bloqueo tras 5 intentos; encabezados `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; HSTS en producción; validación con DTO en la API; secretos fuera del repo (user-secrets en desarrollo, variables del sitio en IIS); paquetes NuGet sin vulnerabilidades (build con 0 advertencias). | `Program.cs`, `Models/Dtos/ClienteDto.cs`, `docs/fase4/DESPLIEGUE.md` §3.6 | Opcional (**Diego**): el sitio de IIS solo tiene HTTP; para HTTPS haría falta un binding con certificado. |
| 4 | **Comunicación asíncrona** | ⚠️ | **Clientes** y **Vehículos** tienen búsqueda en vivo con `fetch` mientras se escribe. **Órdenes** cambia el estado sin recargar, con `fetch` y token antiforgery. | `wwwroot/js/clientes.js`, `wwwroot/js/vehiculos.js`, `wwwroot/js/tablero.js` | **Kimberly**: falta probar la búsqueda en vivo de Vehículos en el navegador y guardar evidencia. Revisar si la rúbrica exige una función asíncrona para Presupuestos y Pagos. |
| 5 | **Documentación y pruebas de API con Swagger** | ⚠️ | Swagger está configurado con autenticación Bearer y documentación XML. Se probaron las APIs de Clientes y Órdenes. Vehículos cuenta con comentarios XML y guía de endpoints. | `/swagger`, `docs/fase4/VEHICULOS_API.md`, `TallerCardona.csproj` | **Kimberly**: probar y capturar la API de Vehículos. Aún faltan las APIs de Presupuestos/catálogo y Pagos. |
| 6 | **Logging y manejo de excepciones** | ⚠️ | Serilog guarda logs diarios y el middleware maneja errores globales. Hay logs de negocio en Clientes, Órdenes y **Vehículos** (operaciones y rechazos). | `Program.cs`, `Middleware/ManejoErroresMiddleware.cs`, `Services/VehiculoService.cs` | **Kimberly**: verificar los registros de Vehículos en consola/archivo. **Andrea** y **Carlos**: completar los logs de negocio de sus módulos. |
| 7 | **Despliegue (IIS o Azure)** | ✅ | IIS local: sitio y pool `TallerCardona` (No Managed Code) en el puerto 8080, Hosting Bundle 10.0.15, variables de producción a nivel del sitio, login SQL para `IIS APPPOOL\TallerCardona` (db_datareader + db_datawriter), permisos de escritura en Logs, regla de firewall 8080 (red privada). Validado: login, roles, Swagger, token, logs. | `Properties/PublishProfiles/IIS.pubxml`, `docs/fase4/DESPLIEGUE.md`, http://localhost:8080 | **Diego**: tomar las capturas de `DESPLIEGUE.md` §7. Para abrir desde otro dispositivo la Wi‑Fi debe estar como **red privada** (hoy está como Pública; ver §3.9). |
| 8 | **Producto final (funciona integrado)** | ⚠️ | Compila sin errores ni advertencias; todas las pantallas del menú abren con los 3 roles; Clientes, Vehículos y Órdenes con CRUD completo en SQL Server. Ajustes de integración de Diego: se registró `IPresupuestoService` (Presupuestos fallaba al abrir) y se corrigió una redirección a un controlador inexistente (`OrdenesTrabajo` → `Ordenes`). | Commits "Integración: …" en `main` | **Andrea**: Presupuestos y catálogo siguen **en memoria** (se pierden al reiniciar — en IIS, cada vez que se recicla el pool); `PresupuestoService` mezcla repositorio en memoria con EF; no hay botón para crear presupuesto en ninguna vista ni editar/eliminar; no hay pantallas del catálogo. **Carlos**: Pagos **en memoria**; no hay `PagoService`; falta la vista `Details`; `Delete` sin botón; **no hay enlace a Pagos en el menú**. |
| 9 | **Manuales (técnico y de usuario)** | ❌ | Hay insumos técnicos y documentos UX por módulo, incluido `docs/ux/UX_Vehiculos.md`, y la guía de endpoints de Vehículos. | `docs/`, `docs/ux/UX_Vehiculos.md`, `docs/fase4/VEHICULOS_API.md` | **Diego**: compilar el manual técnico con capturas. **Cada integrante** debe preparar su sección de uso del módulo con imágenes; confirmar que UX Vehículos cumple la plantilla requerida. |
| 10 | **Git (ramas, commits, integración)** | ⚠️ | Una rama por módulo e integración a `main` mediante pull request, con commits de prefijo descriptivo. `modulo/ordenes` ya está integrada. El PR #20 de Kimberly incorpora API REST y búsqueda en vivo; este commit actualiza la rama con `main` y resuelve los conflictos documentales. | [PR #20](https://github.com/diegohenriquez279-ctrl/taller-cardona-fase2/pull/20), `git log --graph --oneline` | El PR de Vehículos queda pendiente de revisión y fusión; las pruebas de Swagger siguen pendientes. `modulo/presupuestos` debe actualizarse desde `main`; la rama de Pagos debe recrearse por Carlos. |
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
