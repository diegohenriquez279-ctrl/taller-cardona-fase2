using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class PagosController : Controller
{
    private readonly IPagoRepositorio _pagos;
    private readonly IOrdenTrabajoRepositorio _ordenes;
    private readonly IPresupuestoRepositorio _presupuestos;

    public PagosController(
        IPagoRepositorio pagos,
        IOrdenTrabajoRepositorio ordenes,
        IPresupuestoRepositorio presupuestos)
    {
        _pagos = pagos;
        _ordenes = ordenes;
        _presupuestos = presupuestos;
    }

    public IActionResult Index()
    {
        return View(_pagos.Listar());
    }

    public IActionResult Details(int id)
    {
        var pago = _pagos.ObtenerPorId(id);

        if (pago is null)
            return NotFound();

        return View(pago);
    }

    // GET: /Pagos/Create?ordenTrabajoId=1
    // Muestra el formulario para registrar un pago
    public IActionResult Create(int ordenTrabajoId)
    {
        var orden = _ordenes.ObtenerPorId(ordenTrabajoId);

        if (orden is null)
        {
            TempData["Error"] = "No se encontró la orden de trabajo.";
            return RedirectToAction("Index", "OrdenesTrabajo");
        }

        var presupuesto = _presupuestos.Listar()
            .FirstOrDefault(p => p.OrdenTrabajoId == orden.Id);

        if (presupuesto is null || presupuesto.Estado != EstadoPresupuesto.Autorizado)
        {
            TempData["Error"] = "Solo se pueden registrar pagos de órdenes con presupuesto Autorizado.";
            return RedirectToAction("Index", "OrdenesTrabajo");
        }

        var yaPagado = _pagos.Listar()
            .Where(p => p.OrdenTrabajoId == orden.Id)
            .Sum(p => p.Monto);

        ViewBag.Orden = orden;
        ViewBag.Presupuesto = presupuesto;
        ViewBag.YaPagado = yaPagado;
        ViewBag.SaldoPendiente = presupuesto.TotalEstimado - yaPagado;

        return View(new Pago
        {
            OrdenTrabajoId = orden.Id,
            Fecha = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Pago pago)
    {
        var orden = _ordenes.ObtenerPorId(pago.OrdenTrabajoId);

        if (orden is null)
        {
            TempData["Error"] = "No se encontró la orden de trabajo.";
            return RedirectToAction("Index", "OrdenesTrabajo");
        }

        var presupuesto = _presupuestos.Listar()
            .FirstOrDefault(p => p.OrdenTrabajoId == orden.Id);

        if (presupuesto is null || presupuesto.Estado != EstadoPresupuesto.Autorizado)
        {
            TempData["Error"] = "Solo se pueden registrar pagos de órdenes con presupuesto Autorizado.";
            return RedirectToAction("Index", "OrdenesTrabajo");
        }

        // Regla 1: monto mayor que cero
        if (pago.Monto <= 0)
        {
            TempData["Error"] = "El monto debe ser mayor que cero.";
            return RedirectToAction(nameof(Create), new { ordenTrabajoId = pago.OrdenTrabajoId });
        }

        // Regla 2: no pagar más del saldo
        var yaPagado = _pagos.Listar()
            .Where(p => p.OrdenTrabajoId == orden.Id)
            .Sum(p => p.Monto);

        if (yaPagado + pago.Monto > presupuesto.TotalEstimado)
        {
            TempData["Error"] = $"No se puede registrar. Saldo pendiente: {presupuesto.TotalEstimado - yaPagado:C}.";
            return RedirectToAction(nameof(Create), new { ordenTrabajoId = pago.OrdenTrabajoId });
        }

        pago.OrdenTrabajo = orden;
        _pagos.Agregar(pago);

        TempData["Mensaje"] = "Pago registrado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var pago = _pagos.ObtenerPorId(id);

        if (pago is null)
            return NotFound();

        _pagos.Eliminar(id);

        TempData["Mensaje"] = "Pago eliminado.";

        return RedirectToAction(nameof(Index));
    }
}
