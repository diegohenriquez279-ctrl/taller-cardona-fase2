using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

// Controlador de órdenes: recibe lo que manda la vista, llama al servicio
// y decide qué vista mostrar. Ya no habla directo con el repositorio.
public class OrdenesController : Controller
{
    private readonly IOrdenTrabajoService _ordenService;

    public OrdenesController(IOrdenTrabajoService ordenService)
    {
        _ordenService = ordenService;
    }

    // GET: /Ordenes?busqueda=hilux&estado=EnReparacion
    public IActionResult Index(string? busqueda, EstadoOrden? estado)
    {
        ViewData["Busqueda"] = busqueda;
        CargarEstados(estado);
        var ordenes = _ordenService.Listar(busqueda, estado);
        return View(ordenes);
    }

    // GET: /Ordenes/Tablero
    public IActionResult Tablero()
    {
        var ordenes = _ordenService.Listar();
        return View(ordenes);
    }

    // GET: /Ordenes/Details/1
    public IActionResult Details(int id)
    {
        var orden = _ordenService.ObtenerPorId(id);
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

        var resultado = _ordenService.Crear(orden);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            CargarListas(orden.VehiculoId, orden.Estado);
            return View(orden);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Ordenes/Edit/1
    public IActionResult Edit(int id)
    {
        var orden = _ordenService.ObtenerPorId(id);
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

        var resultado = _ordenService.Actualizar(orden);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            CargarListas(orden.VehiculoId, orden.Estado);
            return View(orden);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Ordenes/CambiarEstado/1  (mueve la orden en el tablero)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, EstadoOrden estado, string? volverA)
    {
        var resultado = _ordenService.CambiarEstado(id, estado);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;

        if (volverA == nameof(Details)) return RedirectToAction(nameof(Details), new { id });
        return RedirectToAction(nameof(Tablero));
    }

    // GET: /Ordenes/Delete/1
    public IActionResult Delete(int id)
    {
        var orden = _ordenService.ObtenerPorId(id);
        if (orden is null) return NotFound();
        return View(orden);
    }

    // POST: /Ordenes/Delete/1
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var resultado = _ordenService.Eliminar(id);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    private void CargarListas(int? vehiculoSeleccionado, EstadoOrden estadoSeleccionado)
    {
        var vehiculos = _ordenService.ListarVehiculos()
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
