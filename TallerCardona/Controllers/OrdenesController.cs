using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class OrdenesController : Controller
{
    private readonly IOrdenTrabajoRepositorio _repositorio;

    public OrdenesController(IOrdenTrabajoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: /Ordenes?busqueda=hilux&estado=EnReparacion
    public IActionResult Index(string? busqueda, EstadoOrden? estado)
    {
        ViewData["Busqueda"] = busqueda;
        CargarEstados(estado);
        var ordenes = _repositorio.Listar(busqueda, estado);
        return View(ordenes);
    }

    // GET: /Ordenes/Tablero
    public IActionResult Tablero()
    {
        var ordenes = _repositorio.Listar();
        return View(ordenes);
    }

    // GET: /Ordenes/Details/1
    public IActionResult Details(int id)
    {
        var orden = _repositorio.ObtenerPorId(id);
        if (orden is null) return NotFound();
        return View(orden);
    }

    // GET: /Ordenes/Create
    public IActionResult Create()
    {
        CargarListas(null, EstadoOrden.Recibido);
        return View(new OrdenTrabajo());
    }

    // POST: /Ordenes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(OrdenTrabajo orden)
    {
        if (!ModelState.IsValid)
        {
            CargarListas(orden.VehiculoId, orden.Estado);
            return View(orden);
        }

        _repositorio.Agregar(orden);
        TempData["Mensaje"] = "Orden de trabajo registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Ordenes/Edit/1
    public IActionResult Edit(int id)
    {
        var orden = _repositorio.ObtenerPorId(id);
        if (orden is null) return NotFound();
        CargarListas(orden.VehiculoId, orden.Estado);
        return View(orden);
    }

    // POST: /Ordenes/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, OrdenTrabajo orden)
    {
        if (id != orden.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            CargarListas(orden.VehiculoId, orden.Estado);
            return View(orden);
        }

        _repositorio.Actualizar(orden);
        TempData["Mensaje"] = "Orden de trabajo actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Ordenes/CambiarEstado/1  (mueve la orden en el tablero)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, EstadoOrden estado, string? volverA)
    {
        var orden = _repositorio.ObtenerPorId(id);
        if (orden is null) return NotFound();
        if (!Enum.IsDefined(estado)) return BadRequest();

        _repositorio.CambiarEstado(id, estado);
        TempData["Mensaje"] = $"La orden #{id} pasó a «{estado.Nombre()}».";

        if (volverA == nameof(Details)) return RedirectToAction(nameof(Details), new { id });
        return RedirectToAction(nameof(Tablero));
    }

    // GET: /Ordenes/Delete/1
    public IActionResult Delete(int id)
    {
        var orden = _repositorio.ObtenerPorId(id);
        if (orden is null) return NotFound();
        return View(orden);
    }

    // POST: /Ordenes/Delete/1
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _repositorio.Eliminar(id);
        TempData["Mensaje"] = "Orden de trabajo eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private void CargarListas(int? vehiculoSeleccionado, EstadoOrden estadoSeleccionado)
    {
        var vehiculos = _repositorio.ListarVehiculos()
            .Select(v => new
            {
                v.Id,
                Texto = $"{v.Placa} · {v.Marca} {v.Modelo} {v.Anio}" +
                        (v.Cliente is null ? "" : $" ({v.Cliente.Nombre})")
            });
        ViewData["Vehiculos"] = new SelectList(vehiculos, "Id", "Texto", vehiculoSeleccionado);
        CargarEstados(estadoSeleccionado);
    }

    private void CargarEstados(EstadoOrden? seleccionado)
    {
        var estados = Enum.GetValues<EstadoOrden>()
            .Select(e => new SelectListItem(e.Nombre(), e.ToString(), e == seleccionado));
        ViewData["Estados"] = estados.ToList();
    }
}
