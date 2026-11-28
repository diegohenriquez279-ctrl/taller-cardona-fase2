using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TallerCardona.Middleware;
using TallerCardona.Models;

namespace TallerCardona.Controllers
{
    // [AllowAnonymous]: la portada, privacidad y la página de error se ven sin iniciar sesión.
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Página de error. La muestra ManejoErroresMiddleware con el id de seguimiento
        // que quedó en el log (así se puede buscar el error en Logs/).
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var id = HttpContext.Items[ManejoErroresMiddleware.ClaveIdSeguimiento] as string
                     ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(new ErrorViewModel { RequestId = id });
        }

        // Página amable para 404 y otros códigos de error (la usa UseStatusCodePagesWithReExecute).
        // GET: /Home/Estado/404
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Estado(int id)
        {
            ViewData["Codigo"] = id;
            return View("NoEncontrado");
        }

        // SOLO EN DESARROLLO: provoca un error para probar el log y la página de error.
        // GET: /Home/ProbarError
        public IActionResult ProbarError([FromServices] IWebHostEnvironment entorno)
        {
            if (!entorno.IsDevelopment()) return NotFound();
            throw new InvalidOperationException("Error de prueba provocado desde /Home/ProbarError.");
        }
    }
}
