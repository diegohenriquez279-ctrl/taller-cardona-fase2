using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Repositorios;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

[Authorize(Policy = "AdminORecepcion")]
public class PagosController : Controller
{
    private readonly IPagoService _pagos;
    private readonly IOrdenTrabajoRepositorio _ordenes;
    private readonly IPresupuestoRepositorio _presupuestos;

    public PagosController(
        IPagoService pagos,
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
        if (pago is null) return NotFound();
        return View(pago);
    }

    public IActionResult Create(int ordenTrabajoId)
    {
        var orden = _ordenes.ObtenerPorId(ordenTrabajoId);
        if (orden is null)
        {
            TempData["Error"] = "No se encontró la orden de trabajo.";
            return RedirectToAction("Index", "Ordenes");
        }

        var presupuesto = _presupuestos.Listar()
            .FirstOrDefault(p => p.OrdenTrabajoId == orden.Id);

        if (presupuesto is null || presupuesto.Estado != EstadoPresupuesto.Autorizado)
        {
            TempData["Error"] = "Solo se pueden registrar pagos de órdenes con presupuesto Autorizado.";
            return RedirectToAction("Index", "Ordenes");
        }

        var yaPagado = _pagos.Listar()
            .Where(p => p.OrdenTrabajoId == orden.Id)
            .Sum(p => p.Monto);

        ViewBag.Orden = orden;
        ViewBag.Presupuesto = presupuesto;
        ViewBag.YaPagado = yaPagado;
        ViewBag.SaldoPendiente = presupuesto.TotalEstimado - yaPagado;

        return View(new Pago { OrdenTrabajoId = orden.Id, Fecha = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Pago pago)
    {
        try
        {
            _pagos.Registrar(pago);
            TempData["Mensaje"] = "Pago registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create), new { ordenTrabajoId = pago.OrdenTrabajoId });
        }
    }

    // GET: /Pagos/Delete/1  (confirmación; solo Administrador)
    [Authorize(Policy = "SoloAdministrador")]
    public IActionResult Delete(int id)
    {
        var pago = _pagos.ObtenerPorId(id);
        if (pago is null) return NotFound();
        return View(pago);
    }

    // POST: /Pagos/Delete/1  (solo Administrador)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SoloAdministrador")]
    public IActionResult DeleteConfirmed(int id)
    {
        var pago = _pagos.ObtenerPorId(id);
        if (pago is null) return NotFound();

        _pagos.Eliminar(id);
        TempData["Mensaje"] = "Pago eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
