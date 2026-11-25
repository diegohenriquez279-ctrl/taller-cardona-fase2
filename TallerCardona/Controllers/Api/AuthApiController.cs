using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Services;

namespace TallerCardona.Controllers.Api;

/// <summary>Inicio de sesión de la API: entrega el token JWT.</summary>
[Route("api/auth")]
public class AuthApiController : ApiControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        TokenService tokenService,
        ILogger<AuthApiController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>Recibe correo y contraseña y devuelve un token JWT con los roles del usuario.</summary>
    /// <remarks>
    /// Use el token en las demás peticiones con el encabezado: <c>Authorization: Bearer {token}</c>.
    /// Respeta el bloqueo de Identity (5 intentos fallidos).
    /// </remarks>
    /// <response code="200">Credenciales correctas: token, expiración y roles.</response>
    /// <response code="400">Faltan datos o el correo no es válido.</response>
    /// <response code="401">Correo o contraseña incorrectos, o cuenta bloqueada.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginSolicitud solicitud)
    {
        var usuario = await _userManager.FindByEmailAsync(solicitud.Correo);
        if (usuario is null)
        {
            _logger.LogWarning("API login fallido (no existe): {Correo}", solicitud.Correo);
            return CredencialesInvalidas();
        }

        // CheckPasswordSignInAsync valida la contraseña SIN crear cookie y cuenta intentos fallidos.
        var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, solicitud.Clave, lockoutOnFailure: true);
        if (!resultado.Succeeded)
        {
            _logger.LogWarning("API login fallido: {Correo} (bloqueado: {Bloqueado})", solicitud.Correo, resultado.IsLockedOut);
            return CredencialesInvalidas(resultado.IsLockedOut);
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var (token, expira) = _tokenService.Generar(usuario, roles);

        _logger.LogInformation("API login correcto: {Correo}", solicitud.Correo);
        return Ok(new LoginRespuesta(token, expira, roles.ToList()));
    }

    private ObjectResult CredencialesInvalidas(bool bloqueado = false) =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "No autorizado",
            detail: bloqueado
                ? "La cuenta está bloqueada por varios intentos fallidos. Intente más tarde."
                : "Correo o contraseña incorrectos.");
}

/// <summary>Datos para iniciar sesión en la API.</summary>
/// <param name="Correo">Correo del usuario, por ejemplo admin@tallercardona.com</param>
/// <param name="Clave">Contraseña del usuario.</param>
public record LoginSolicitud(
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escriba un correo válido.")]
    string Correo,
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Clave);

/// <summary>Respuesta del login de la API.</summary>
/// <param name="Token">Token JWT para el encabezado Authorization: Bearer.</param>
/// <param name="Expira">Fecha y hora (UTC) en que el token deja de servir.</param>
/// <param name="Roles">Roles del usuario.</param>
public record LoginRespuesta(string Token, DateTime Expira, List<string> Roles);
