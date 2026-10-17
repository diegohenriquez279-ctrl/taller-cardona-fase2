
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class PresupuestosController : Controller
{
    private readonly IPresupuestoRepositorio _presupuestos;

    public PresupuestosController(IPresupuestoRepositorio presupuestos)
    {
        _presupuestos = presupuestos;
    }

    public IActionResult Index()
    {
        return View(_presupuestos.Listar());
    }

    public IActionResult Details(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return NotFound();

        return View(presupuesto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Autorizar(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return NotFound();

        if (presupuesto.Estado != EstadoPresupuesto.Pendiente)
        {
            TempData["Error"] =
                "Solo se pueden autorizar presupuestos pendientes.";

            return RedirectToAction(nameof(Index));
        }

        presupuesto.Estado = EstadoPresupuesto.Autorizado;
        _presupuestos.Actualizar(presupuesto);

        TempData["Mensaje"] =
            "Presupuesto autorizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Rechazar(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return NotFound();

        if (presupuesto.Estado != EstadoPresupuesto.Pendiente)
        {
            TempData["Error"] =
                "Solo se pueden rechazar presupuestos pendientes.";

            return RedirectToAction(nameof(Index));
        }

        presupuesto.Estado = EstadoPresupuesto.Rechazado;
        _presupuestos.Actualizar(presupuesto);

        TempData["Mensaje"] = "Presupuesto rechazado.";

        return RedirectToAction(nameof(Index));
    }
}
