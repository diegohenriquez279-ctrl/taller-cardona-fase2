using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TallerCardona.Controllers.Api;

/// <summary>
/// Clase base para TODOS los controladores de la API.
/// </summary>
/// <remarks>
/// Para crear su API, hereden de esta clase:
///   [Route("api/vehiculos")]
///   [Authorize(Policy = "AdminORecepcion")]
///   public class VehiculosApiController : ApiControllerBase { ... }
///
/// Lo que ya trae:
///   [ApiController]          -> valida el modelo solo (devuelve 400 si faltan datos) y lee el JSON del cuerpo.
///   [IgnoreAntiforgeryToken] -> la API usa token JWT, no el token antiforgery de los formularios MVC.
///   [Authorize(JWT)]         -> solo acepta el encabezado "Authorization: Bearer token"; la cookie del
///                               login MVC NO sirve aquí. Sin token responde 401 (no redirige al login).
/// </remarks>
[ApiController]
[IgnoreAntiforgeryToken]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
}
