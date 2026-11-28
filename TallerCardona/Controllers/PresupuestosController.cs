
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

// Presupuestos: Administrador y Recepción (generar, autorizar, rechazar, consultar).
// Mecánico no tiene acceso. No hay acción Delete (un presupuesto se rechaza, no se borra).
[Authorize(Policy = "AdminORecepcion")]
public class PresupuestosController : Controller
{
    private readonly IPresupuestoService _presupuestos;

    public PresupuestosController(IPresupuestoService presupuestos)
    {
        _presupuestos = presupuestos;
    }

    public IActionResult Index()
    {
        ViewData["Reporte"] = _presupuestos.ListarPorEstado();
        return View(_presupuestos.Listar());
    }

    public IActionResult Details(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return NotFound();

        return View(presupuesto);
    }

    // POST: /Presupuestos/Crear  (botón "Generar presupuesto" en el detalle de la orden)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(int ordenTrabajoId)
    {
        var resultado = _presupuestos.Crear(ordenTrabajoId, User.Identity?.Name);

        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Mensaje;

            return RedirectToAction(
                "Details",
                "Ordenes",
                new { id = ordenTrabajoId });
        }

        TempData["Mensaje"] = resultado.Mensaje;
        var creado = _presupuestos.ObtenerPorOrden(ordenTrabajoId);
        return creado is null
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id = creado.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Autorizar(int id, string? volverA)
    {
        var resultado = _presupuestos.Autorizar(id, User.Identity?.Name);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
        return Volver(id, volverA);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Rechazar(int id, string? volverA)
    {
        var resultado = _presupuestos.Rechazar(id, User.Identity?.Name);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
        return Volver(id, volverA);
    }

    // Consulta LINQ de presupuestos agrupados por estado (JSON).
    public IActionResult ReportePorEstado()
    {
        var reporte = _presupuestos.ListarPorEstado();
        return Json(reporte);
    }

    private IActionResult Volver(int id, string? volverA) =>
        volverA == nameof(Details)
            ? RedirectToAction(nameof(Details), new { id })
            : RedirectToAction(nameof(Index));
}
