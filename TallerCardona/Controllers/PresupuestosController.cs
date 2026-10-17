
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;

namespace TallerCardona.Controllers;

public class PresupuestosController : Controller
{
    public IActionResult Index()
    {
        var presupuestos = new List<Presupuesto>();

        return View(presupuestos);
    }
}
