using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

[Authorize(Policy = "AdminORecepcion")]
public class VehiculosController : Controller
{
    private readonly IVehiculoService _vehiculos;
    private readonly IClienteService _clientes;

    public VehiculosController(IVehiculoService vehiculos, IClienteService clientes)
    {
        _vehiculos = vehiculos;
        _clientes = clientes;
    }

    public IActionResult Index(string? busqueda)
    {
        ViewData["Busqueda"] = busqueda;
        return View(_vehiculos.Listar(busqueda));
    }

    // Endpoint JSON usado por la búsqueda en vivo de la lista.
    [HttpGet]
    public IActionResult Buscar(string? texto)
    {
        var vehiculos = _vehiculos.Listar(texto)
            .Select(v => new
            {
                v.Id,
                v.Placa,
                v.Marca,
                v.Modelo,
                v.Anio,
                ClienteNombre = v.Cliente?.Nombre ?? "—",
                v.Kilometraje
            });

        return Json(vehiculos);
    }

    public IActionResult Resumen() => View(_vehiculos.ListarResumen());

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
        if (!ModelState.IsValid)
        {
            CargarClientes(vehiculo.ClienteId);
            return View(vehiculo);
        }

        var resultado = _vehiculos.Crear(vehiculo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            CargarClientes(vehiculo.ClienteId);
            return View(vehiculo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
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

        if (!ModelState.IsValid)
        {
            CargarClientes(vehiculo.ClienteId);
            return View(vehiculo);
        }

        var resultado = _vehiculos.Actualizar(vehiculo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            CargarClientes(vehiculo.ClienteId);
            return View(vehiculo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "SoloAdministrador")]
    public IActionResult Delete(int id)
    {
        var vehiculo = _vehiculos.ObtenerPorId(id);
        if (vehiculo is null) return NotFound();
        return View(vehiculo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SoloAdministrador")]
    public IActionResult DeleteConfirmed(int id)
    {
        var resultado = _vehiculos.Eliminar(id);
        if (!resultado.Exito)
            TempData["Error"] = resultado.Mensaje;
        else
            TempData["Mensaje"] = resultado.Mensaje;

        return RedirectToAction(nameof(Index));
    }

    private void CargarClientes(int? clienteSeleccionado = null)
    {
        ViewBag.Clientes = new SelectList(
            _clientes.Listar(),
            nameof(Cliente.Id),
            nameof(Cliente.Nombre),
            clienteSeleccionado);
    }
}
