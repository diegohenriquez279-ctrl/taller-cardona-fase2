
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class PresupuestosController : Controller
{
    private readonly IPresupuestoService _presupuestos;

    public PresupuestosController(IPresupuestoService presupuestos)
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
    public IActionResult Crear(int ordenTrabajoId)
    {
        var resultado = _presupuestos.Crear(ordenTrabajoId);

        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Mensaje;

            return RedirectToAction(
                "Details",
                "Ordenes",
                new { id = ordenTrabajoId });
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Autorizar(int id)
    {
        var resultado = _presupuestos.Autorizar(id);

        if (resultado.Exito)
            TempData["Mensaje"] = resultado.Mensaje;
        else
            TempData["Error"] = resultado.Mensaje;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Rechazar(int id)
    {
        var resultado = _presupuestos.Rechazar(id);

        if (resultado.Exito)
            TempData["Mensaje"] = resultado.Mensaje;
        else
            TempData["Error"] = resultado.Mensaje;

        return RedirectToAction(nameof(Index));
    }

    // Consulta LINQ de presupuestos agrupados por estado.
    public IActionResult ReportePorEstado()
    {
        var reporte = _presupuestos.ListarPorEstado();
        return Json(reporte);
    }
}
