# Despliegue en IIS local — Fase 4 (criterio 7)

Este documento explica cómo se publicó Taller Cardona en **IIS** (el servidor web de Windows) en la computadora de Diego, para que se pueda abrir en `http://localhost:8080` y desde otro dispositivo de la misma red.

Fecha del despliegue: 26/11/2026.

---

## 1. Resumen

| Dato | Valor |
|---|---|
| Servidor | IIS 10 en Windows 11 |
| Carpeta publicada | `C:\inetpub\TallerCardona` |
| Sitio de IIS | `TallerCardona`, puerto **8080** (HTTP) |
| Application Pool | `TallerCardona`, *No Managed Code*, identidad `ApplicationPoolIdentity` |
| Base de datos | SQL Server Express `localhost\SQLEXPRESS`, base `TallerCardonaDB` |
| Entorno | `Production` |
| Logs de la app | `C:\inetpub\TallerCardona\Logs\taller-AAAAMMDD.txt` |
| URL local | http://localhost:8080 |
| URL en la red | `http://<IP de la PC>:8080` (el 26/11 era http://192.168.0.100:8080) |
| Swagger | http://localhost:8080/swagger |

---

## 2. Requisitos

1. Windows 10/11 con permisos de administrador.
2. **IIS** con estas características: servidor web, documento predeterminado, contenido estático, errores HTTP, registro HTTP, filtrado de solicitudes, compresión estática, WebSockets, consola de administración y herramientas de scripts.
3. **.NET 10 Hosting Bundle** (instala el runtime de ASP.NET Core y el *ASP.NET Core Module* que conecta IIS con la app). Se instaló la versión 10.0.15.
4. **SQL Server Express** (`localhost\SQLEXPRESS`) con la base `TallerCardonaDB` ya creada con las migraciones (`dotnet ef database update`).
5. **.NET SDK 10** para compilar y publicar.

---

## 3. Pasos que se hicieron (con los comandos)

Todos los comandos se ejecutan en **PowerShell como administrador**.
`appcmd` está en `C:\Windows\System32\inetsrv\appcmd.exe`; en los ejemplos se guarda en una variable:

```powershell
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"
```

### 3.1 Habilitar IIS

```powershell
Enable-WindowsOptionalFeature -Online -All -NoRestart -FeatureName `
  IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-DefaultDocument, `
  IIS-StaticContent, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestFiltering, `
  IIS-HttpCompressionStatic, IIS-ApplicationDevelopment, IIS-WebSockets, `
  IIS-ManagementConsole, IIS-ManagementScriptingTools
```

No pidió reiniciar Windows.

### 3.2 Instalar el .NET 10 Hosting Bundle

```powershell
winget install --id Microsoft.DotNet.HostingBundle.10 -e --silent --accept-package-agreements --accept-source-agreements
iisreset /restart
```

(Alternativa sin winget: descargar "Hosting Bundle" desde https://dotnet.microsoft.com/download/dotnet/10.0.)

Para comprobarlo: debe existir `C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll`.

### 3.3 Crear el Application Pool

```powershell
& $appcmd add apppool /name:TallerCardona
& $appcmd set apppool TallerCardona /managedRuntimeVersion:"" /managedPipelineMode:Integrated /processModel.identityType:ApplicationPoolIdentity /processModel.loadUserProfile:true
```

- `managedRuntimeVersion:""` = **No Managed Code** (ASP.NET Core no usa el .NET Framework clásico).
- `loadUserProfile:true` = ASP.NET Core guarda sus claves de protección de datos (cookies de sesión) en el perfil del pool, así las sesiones no se pierden cada vez que el pool se reinicia.

### 3.4 Publicar la aplicación

Se creó el perfil `TallerCardona/Properties/PublishProfiles/IIS.pubxml` (está en el repositorio, no tiene secretos). Publica en modo Release a `C:\inetpub\TallerCardona`:

```powershell
cd "C:\Users\ADMIN\Desktop\universidad 2 ciclo\TallerCardona\TallerCardona"
dotnet publish -c Release -p:PublishProfile=IIS
```

> En el perfil están `PublishUrl` (la usa Visual Studio) y `PublishDir` (la usa `dotnet publish`). Si falta `PublishDir`, `dotnet publish` deja los archivos en `bin\Release\net10.0\publish`.

### 3.5 Crear el sitio en el puerto 8080

```powershell
& $appcmd add site /name:TallerCardona /bindings:"http/*:8080:" /physicalPath:C:\inetpub\TallerCardona
& $appcmd set app "TallerCardona/" /applicationPool:TallerCardona
```

### 3.6 Variables de producción (a nivel del sitio, NO en el repositorio)

Se guardan en `C:\Windows\System32\inetsrv\config\applicationHost.config`, dentro de la sección del sitio `TallerCardona`. Así:

- no quedan en GitHub, y
- no se borran al volver a publicar (el `web.config` de la carpeta sí se reemplaza en cada publicación).

| Variable | Valor | Para qué |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Modo producción (sin páginas de error con detalle, HSTS, etc.) |
| `ConnectionStrings__DefaultConnection` | `Server=localhost\SQLEXPRESS;Database=TallerCardonaDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true` | Cadena de conexión de producción |
| `Jwt__Key` | clave aleatoria de 128 caracteres hexadecimales (512 bits) | Firma de los tokens de la API |

El doble guion bajo `__` equivale a `:` en `appsettings.json` (`Jwt__Key` = `Jwt:Key`).

Comandos (la clave JWT se genera al momento y **no se escribe en ningún archivo del proyecto**):

```powershell
$bytes = New-Object byte[] 64
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$clave = -join ($bytes | ForEach-Object { $_.ToString('x2') })

$vars = [ordered]@{
  'ASPNETCORE_ENVIRONMENT' = 'Production'
  'ConnectionStrings__DefaultConnection' = 'Server=localhost\SQLEXPRESS;Database=TallerCardonaDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
  'Jwt__Key' = $clave
}
foreach ($n in $vars.Keys) {
  & $appcmd set config TallerCardona -section:system.webServer/aspNetCore /-"environmentVariables.[name='$n']" /commit:apphost 2>$null
  & $appcmd set config TallerCardona -section:system.webServer/aspNetCore /+"environmentVariables.[name='$n',value='$($vars[$n])']" /commit:apphost
}
```

Para **ver** los nombres configurados (en IIS Manager: sitio TallerCardona → *Configuration Editor* → `system.webServer/aspNetCore` → `environmentVariables`):

```powershell
& $appcmd list config TallerCardona -section:system.webServer/aspNetCore
```

> Si se cambia la clave JWT, los tokens emitidos antes dejan de servir (hay que volver a hacer login en la API). Es lo esperado.

### 3.7 Permisos en SQL Server

El Application Pool entra a SQL Server con la cuenta virtual **`IIS APPPOOL\TallerCardona`**. Se creó su login y usuario con permisos de lectura y escritura (suficiente para Identity, que solo lee y escribe en sus tablas `AspNet*`; las migraciones se aplican aparte con `dotnet ef database update`):

```sql
CREATE LOGIN [IIS APPPOOL\TallerCardona] FROM WINDOWS WITH DEFAULT_DATABASE = [TallerCardonaDB];
USE [TallerCardonaDB];
CREATE USER [IIS APPPOOL\TallerCardona] FOR LOGIN [IIS APPPOOL\TallerCardona];
ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\TallerCardona];
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\TallerCardona];
```

Se ejecutó con: `sqlcmd -S localhost\SQLEXPRESS -E -C -Q "<script>"`.

> El login solo se puede crear **después** de crear el Application Pool (antes, Windows no conoce la cuenta `IIS APPPOOL\TallerCardona`).

### 3.8 Permisos de carpetas

```powershell
New-Item -ItemType Directory -Force C:\inetpub\TallerCardona\Logs
icacls C:\inetpub\TallerCardona      /grant "IIS APPPOOL\TallerCardona:(OI)(CI)RX"   # leer la app
icacls C:\inetpub\TallerCardona\Logs /grant "IIS APPPOOL\TallerCardona:(OI)(CI)M"    # escribir los logs
```

### 3.9 Firewall (para entrar desde otro dispositivo)

```powershell
New-NetFirewallRule -DisplayName "TallerCardona IIS 8080" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow -Profile Private
```

La regla vale solo para **redes privadas**. Si Windows tiene marcada la Wi‑Fi como **Pública**, la regla no se aplica y los otros dispositivos no podrán entrar. Para revisarlo y cambiarlo:

```powershell
Get-NetConnectionProfile
Set-NetConnectionProfile -InterfaceAlias "Wi-Fi 2" -NetworkCategory Private   # solo en la red de tu casa
```

(o en *Configuración → Red e Internet → Wi‑Fi → propiedades de la red → Red privada*).

La IP local se ve con `ipconfig` (línea "Dirección IPv4"). Puede cambiar si el router asigna otra.

### 3.10 Arrancar

```powershell
& $appcmd start apppool TallerCardona
& $appcmd start site TallerCardona
```

---

## 4. Cómo actualizar la publicación (cuando haya cambios nuevos)

En PowerShell **como administrador**:

```powershell
cd "C:\Users\ADMIN\Desktop\universidad 2 ciclo\TallerCardona"
git checkout main
git pull
cd TallerCardona
dotnet ef database update                     # solo si hay migraciones nuevas
& "$env:windir\system32\inetsrv\appcmd.exe" stop apppool TallerCardona
dotnet publish -c Release -p:PublishProfile=IIS
& "$env:windir\system32\inetsrv\appcmd.exe" start apppool TallerCardona
```

Se detiene el pool porque, mientras corre, IIS tiene bloqueado `TallerCardona.dll` y la copia falla.
Las variables de entorno y la carpeta `Logs` no se tocan al republicar.

---

## 5. Cómo validar

| Prueba | Cómo | Resultado esperado (26/11/2026) |
|---|---|---|
| La app abre | http://localhost:8080 | Redirige a `/Account/Login` con estilos ✅ |
| Login MVC | Entrar con admin, recepción y mecánico | Entra; mecánico recibe "Acceso denegado" en Clientes ✅ |
| Swagger en producción | http://localhost:8080/swagger | Carga la página con AuthApi, ClientesApi, OrdenesApi ✅ |
| Token | `POST /api/auth/login` con `{"correo":"admin@tallercardona.com","clave":"Admin123!"}` | 200 con `token` ✅ (clave mala → 401) |
| API con token | Botón *Authorize* → `GET /api/clientes` | 200; sin token → 401; mecánico → 403 ✅ |
| Página inexistente | http://localhost:8080/NoExiste | Página amable de error 404 ✅ |
| Logs | Abrir `C:\inetpub\TallerCardona\Logs\taller-AAAAMMDD.txt` | Una línea por petición, logins fallidos como WRN ✅ |
| Desde la red | `http://<IP>:8080` desde el celular (misma Wi‑Fi) | Requiere red **privada** (ver 3.9) |

Prueba rápida del token desde PowerShell:

```powershell
$r = Invoke-RestMethod -Method Post -Uri http://localhost:8080/api/auth/login -ContentType "application/json" -Body '{"correo":"admin@tallercardona.com","clave":"Admin123!"}'
Invoke-RestMethod -Uri http://localhost:8080/api/clientes -Headers @{ Authorization = "Bearer $($r.token)" }
```

### Advertencia normal en el log

`Failed to determine the https port for redirect.` aparece porque el sitio solo tiene HTTP (puerto 8080). No es un error: la app sigue funcionando por HTTP. Para HTTPS habría que agregar un binding con certificado en IIS.

---

## 6. Si algo falla

| Síntoma | Causa probable | Solución |
|---|---|---|
| **HTTP Error 500.19** | Falta el Hosting Bundle | Instalarlo y `iisreset` |
| **HTTP Error 500.30 / 500.31** | La app no arranca (falta runtime, error en `Program.cs`) | Revisar el *Visor de eventos → Registros de Windows → Aplicación* (origen "IIS AspNetCore Module V2") |
| Error de login a SQL (`Login failed for user 'IIS APPPOOL\TallerCardona'`) | Falta el login/usuario en SQL | Repetir el paso 3.7 |
| No se crean archivos en `Logs` | Falta permiso de escritura | Repetir el paso 3.8 |
| La API responde 401 con un token "bueno" | Token generado con otra clave (ej. en desarrollo) | Pedir un token nuevo en `http://localhost:8080/api/auth/login` |
| Desde el celular no abre | Red pública o firewall | Paso 3.9 |
| `dotnet publish` falla por archivo en uso | El pool sigue corriendo | Detener el pool antes (sección 4) |

---

## 7. Capturas para la evidencia (manual técnico y video)

1. *Activar o desactivar características de Windows* con **Internet Information Services** marcado.
2. `dotnet --list-runtimes` mostrando `Microsoft.AspNetCore.App 10.0.x` y, en *Aplicaciones instaladas*, "Microsoft .NET … Windows Server Hosting".
3. **IIS Manager → Application Pools**: `TallerCardona` con *.NET CLR Version = No Managed Code* e *Identity = ApplicationPoolIdentity*.
4. **IIS Manager → Sites**: `TallerCardona` con binding `http *:8080` y ruta `C:\inetpub\TallerCardona`.
5. **Configuration Editor** del sitio (`system.webServer/aspNetCore → environmentVariables`) mostrando los **nombres** de las 3 variables (tapar o recortar el valor de `Jwt__Key` y no mostrarlo en el video).
6. Terminal con `dotnet publish -c Release -p:PublishProfile=IIS` terminado sin errores.
7. Explorador en `C:\inetpub\TallerCardona` (con `web.config`, `TallerCardona.dll`, `Logs`).
8. **SSMS → Security → Logins**: `IIS APPPOOL\TallerCardona`, y en *TallerCardonaDB → Users* sus roles `db_datareader` y `db_datawriter`.
9. Propiedades → Seguridad de la carpeta `Logs` con el permiso *Modificar* para `TallerCardona`.
10. *Firewall de Windows → Reglas de entrada*: "TallerCardona IIS 8080" (Privado, Permitir).
11. Navegador en `http://localhost:8080` con la pantalla de login y luego dentro de la app (un rol por captura: admin, recepción, mecánico con "Acceso denegado" en Clientes).
12. `http://localhost:8080/swagger`: `POST /api/auth/login` con el token en la respuesta, botón **Authorize** y un `GET /api/clientes` con 200 (y uno sin token con 401).
13. El archivo `C:\inetpub\TallerCardona\Logs\taller-AAAAMMDD.txt` abierto mostrando peticiones y un login fallido.
14. El celular u otra PC abriendo `http://<IP>:8080` (con `ipconfig` visible en la PC).
