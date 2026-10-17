
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class OrdenesTrabajoController : Controller
{
    private readonly IOrdenTrabajoRepositorio _ordenes;
    private readonly IVehiculoRepositorio _vehiculos;
    private readonly IServicioRepositorio _servicios;
    private readonly IRepuestoRepositorio _repuestos;

    public OrdenesTrabajoController(
        IOrdenTrabajoRepositorio ordenes,
        IVehiculoRepositorio vehiculos,
        IServicioRepositorio servicios,
        IRepuestoRepositorio repuestos)
    {
        _ordenes = ordenes;
        _vehiculos = vehiculos;
        _servicios = servicios;
        _repuestos = repuestos;
    }

    public IActionResult Index()
    {
        return View(_ordenes.Listar());
    }

    public IActionResult Details(int id)
    {
        var orden = _ordenes.ObtenerPorId(id);

        if (orden is null)
            return NotFound();

        return View(orden);
    }

    public IActionResult Create()
    {
        CargarListas();
        return View(new OrdenTrabajo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(OrdenTrabajo orden)
    {
        if (_vehiculos.ObtenerPorId(orden.VehiculoId) is null)
        {
            ModelState.AddModelError(
                nameof(orden.VehiculoId),
                "Seleccione un vehículo registrado.");
        }

        if (!ModelState.IsValid)
        {
            CargarListas(orden.VehiculoId);
            return View(orden);
        }

        _ordenes.Agregar(orden);

        TempData["Mensaje"] =
            "Orden de trabajo registrada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private void CargarListas(int? vehiculoSeleccionado = null)
    {
        ViewBag.Vehiculos = new SelectList(
            _vehiculos.Listar(),
            nameof(Vehiculo.Id),
            nameof(Vehiculo.Placa),
            vehiculoSeleccionado);

        ViewBag.Servicios = new SelectList(
            _servicios.Listar(),
            nameof(Servicio.Id),
            nameof(Servicio.Nombre));

        ViewBag.Repuestos = new SelectList(
            _repuestos.Listar(),
            nameof(Repuesto.Id),
            nameof(Repuesto.Nombre));
    }
}
