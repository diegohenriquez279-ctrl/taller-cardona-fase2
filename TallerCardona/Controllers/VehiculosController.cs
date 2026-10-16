using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class VehiculosController : Controller
{
    private readonly IVehiculoRepositorio _vehiculos;
    private readonly IClienteRepositorio _clientes;

    public VehiculosController(IVehiculoRepositorio vehiculos, IClienteRepositorio clientes)
    {
        _vehiculos = vehiculos;
        _clientes = clientes;
    }

    public IActionResult Index(string? busqueda)
    {
        ViewData["Busqueda"] = busqueda;
        return View(_vehiculos.Listar(busqueda));
    }

    public IActionResult Details(int id)
    {
        var vehiculo = _vehiculos.ObtenerPorId(id);
        if (vehiculo is null) return NotFound();
        return View(vehiculo);
    }

    public IActionResult Create()
    {
        CargarClientes();
        return View(new Vehiculo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Vehiculo vehiculo)
    {
        ValidarClienteYPlaca(vehiculo);
        if (!ModelState.IsValid)
        {
            CargarClientes(vehiculo.ClienteId);
            return View(vehiculo);
        }

        _vehiculos.Agregar(vehiculo);
        TempData["Mensaje"] = "Vehículo registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var vehiculo = _vehiculos.ObtenerPorId(id);
        if (vehiculo is null) return NotFound();
        CargarClientes(vehiculo.ClienteId);
        return View(vehiculo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Vehiculo vehiculo)
    {
        if (id != vehiculo.Id) return NotFound();
        ValidarClienteYPlaca(vehiculo, id);
        if (!ModelState.IsValid)
        {
            CargarClientes(vehiculo.ClienteId);
            return View(vehiculo);
        }

        _vehiculos.Actualizar(vehiculo);
        TempData["Mensaje"] = "Vehículo actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var vehiculo = _vehiculos.ObtenerPorId(id);
        if (vehiculo is null) return NotFound();
        return View(vehiculo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var vehiculo = _vehiculos.ObtenerPorId(id);
        if (vehiculo is null) return NotFound();

        if (vehiculo.Ordenes.Count > 0)
        {
            TempData["Error"] = "No se puede eliminar un vehículo que ya tiene historial de órdenes de trabajo.";
            return RedirectToAction(nameof(Index));
        }

        _vehiculos.Eliminar(id);
        TempData["Mensaje"] = "Vehículo eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private void CargarClientes(int? clienteSeleccionado = null)
    {
        ViewBag.Clientes = new SelectList(_clientes.Listar(), nameof(Cliente.Id), nameof(Cliente.Nombre), clienteSeleccionado);
    }

    private void ValidarClienteYPlaca(Vehiculo vehiculo, int? excluirId = null)
    {
        if (_clientes.ObtenerPorId(vehiculo.ClienteId) is null)
            ModelState.AddModelError(nameof(vehiculo.ClienteId), "Seleccione un cliente registrado.");

        if (!string.IsNullOrWhiteSpace(vehiculo.Placa) && _vehiculos.PlacaEnUso(vehiculo.Placa, excluirId))
            ModelState.AddModelError(nameof(vehiculo.Placa), "Ya existe un vehículo registrado con esa placa.");
    }
}
