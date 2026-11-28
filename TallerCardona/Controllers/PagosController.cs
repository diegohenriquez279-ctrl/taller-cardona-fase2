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

    public PagosController(IPagoService pagos, IOrdenTrabajoRepositorio ordenes)
    {
        _pagos = pagos;
        _ordenes = ordenes;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _pagos.ListarAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var pago = await _pagos.ObtenerPorIdAsync(id);
        if (pago is null) return NotFound();
        return View(pago);
    }

    public async Task<IActionResult> Create(int ordenTrabajoId)
    {
        var orden = _ordenes.ObtenerPorId(ordenTrabajoId);
        if (orden is null)
        {
            TempData["Error"] = "No se encontró la orden de trabajo.";
            return RedirectToAction("Index", "Ordenes");
        }

        var presupuesto = _pagos.ObtenerPresupuestoDeOrden(orden.Id);

        if (presupuesto is null || presupuesto.Estado != EstadoPresupuesto.Autorizado)
        {
            TempData["Error"] = "Solo se pueden registrar pagos de órdenes con presupuesto Autorizado.";
            return RedirectToAction("Index", "Ordenes");
        }

        var yaPagado = (await _pagos.ListarPorOrdenAsync(orden.Id)).Sum(p => p.Monto);

        ViewBag.Orden = orden;
        ViewBag.Presupuesto = presupuesto;
        ViewBag.YaPagado = yaPagado;
        ViewBag.SaldoPendiente = presupuesto.TotalEstimado - yaPagado;

        return View(new Pago { OrdenTrabajoId = orden.Id, Fecha = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pago pago)
    {
        try
        {
            await _pagos.RegistrarAsync(pago);
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
    public async Task<IActionResult> Delete(int id)
    {
        var pago = await _pagos.ObtenerPorIdAsync(id);
        if (pago is null) return NotFound();
        return View(pago);
    }

    // POST: /Pagos/Delete/1  (solo Administrador)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SoloAdministrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _pagos.EliminarAsync(id, User.Identity?.Name)) return NotFound();

        TempData["Mensaje"] = "Pago eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
