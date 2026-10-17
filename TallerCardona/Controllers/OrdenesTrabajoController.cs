
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class OrdenesTrabajoController : Controller
{
    private readonly IOrdenTrabajoRepositorio _ordenes;
    private readonly IVehiculoRepositorio _vehiculos;

    public OrdenesTrabajoController(
        IOrdenTrabajoRepositorio ordenes,
        IVehiculoRepositorio vehiculos)
    {
        _ordenes = ordenes;
        _vehiculos = vehiculos;
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
        CargarVehiculos();
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
            CargarVehiculos(orden.VehiculoId);
            return View(orden);
        }

        _ordenes.Agregar(orden);

        TempData["Mensaje"] = "Orden de trabajo registrada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private void CargarVehiculos(int? seleccionado = null)
    {
        ViewBag.Vehiculos = new SelectList(
            _vehiculos.Listar(),
            nameof(Vehiculo.Id),
            nameof(Vehiculo.Placa),
            seleccionado);
    }
}
