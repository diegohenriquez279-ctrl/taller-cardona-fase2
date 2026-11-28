using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;

namespace TallerCardona.Controllers;

// Inicio y cierre de sesión de la parte MVC (usa cookie).
// [AllowAnonymous]: estas páginas deben verse sin haber iniciado sesión.
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    // GET: /Account/Login?returnUrl=/Clientes
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        // El UserName de nuestros usuarios es su correo.
        // lockoutOnFailure: true -> cuenta los intentos fallidos y bloquea al 5.º.
        var resultado = await _signInManager.PasswordSignInAsync(
            modelo.Correo, modelo.Clave, modelo.Recordarme, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            _logger.LogInformation("Inicio de sesión: {Correo}", modelo.Correo);

            // Solo se redirige a direcciones de nuestro propio sitio
            // (evita que un enlace malicioso mande al usuario a otra página).
            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return Redirect(modelo.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        if (resultado.IsLockedOut)
        {
            _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Correo}", modelo.Correo);
            ModelState.AddModelError(string.Empty,
                "La cuenta está bloqueada por varios intentos fallidos. Intente de nuevo en unos minutos.");
        }
        else
        {
            _logger.LogWarning("Intento de inicio de sesión fallido: {Correo}", modelo.Correo);
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        }

        return View(modelo);
    }

    // POST: /Account/Logout (se hace por POST para que un enlace externo no cierre la sesión)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccesoDenegado (el usuario tiene sesión pero su rol no alcanza)
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
