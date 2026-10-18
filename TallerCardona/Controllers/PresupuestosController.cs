
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class PresupuestosController : Controller
{
    private readonly IPresupuestoRepositorio _presupuestos;
    private readonly IOrdenTrabajoRepositorio _ordenes;

    public PresupuestosController(
        IPresupuestoRepositorio presupuestos,
        IOrdenTrabajoRepositorio ordenes)
    {
        _presupuestos = presupuestos;
        _ordenes = ordenes;
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
        var orden = _ordenes.ObtenerPorId(ordenTrabajoId);

        if (orden is null)
        {
            TempData["Error"] = "No se encontró la orden de trabajo.";
            return RedirectToAction("Index", "OrdenesTrabajo");
        }

        if (orden.Presupuesto is not null ||
            _presupuestos.Listar().Any(p => p.OrdenTrabajoId == orden.Id))
        {
            TempData["Error"] = "Esta orden ya tiene un presupuesto.";
            return RedirectToAction("Index", "OrdenesTrabajo");
        }

        var totalServicios = orden.Servicios.Sum(s => s.Subtotal);
        var totalRepuestos = orden.Repuestos.Sum(r => r.Subtotal);
        var total = totalServicios + totalRepuestos;

        if (total <= 0)
        {
            TempData["Error"] =
                "No se puede crear un presupuesto de cero. Verifique los servicios y repuestos de la orden.";

            return RedirectToAction("Details", "OrdenesTrabajo",
                new { id = orden.Id });
        }

        var presupuesto = new Presupuesto
        {
            OrdenTrabajoId = orden.Id,
            OrdenTrabajo = orden,
            TotalEstimado = total,
            Estado = EstadoPresupuesto.Pendiente
        };

        _presupuestos.Agregar(presupuesto);

        orden.Presupuesto = presupuesto;
        _ordenes.Actualizar(orden);

        TempData["Mensaje"] = "Presupuesto creado correctamente.";

        return RedirectToAction(nameof(Index));
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
