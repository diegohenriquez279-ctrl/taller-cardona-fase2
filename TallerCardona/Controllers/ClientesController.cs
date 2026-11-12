using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

// El controlador ya no habla con el repositorio: recibe las peticiones,
// llama al servicio y decide qué vista mostrar.
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    // GET: /Clientes?busqueda=carlos
    public IActionResult Index(string? busqueda)
    {
        ViewData["Busqueda"] = busqueda;
        var clientes = _clienteService.Listar(busqueda);
        return View(clientes);
    }

    // GET: /Clientes/Resumen
    public IActionResult Resumen()
    {
        var resumen = _clienteService.ListarResumen();
        return View(resumen);
    }

    // GET: /Clientes/Details/1
    public IActionResult Details(int id)
    {
        var cliente = _clienteService.ObtenerPorId(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    // GET: /Clientes/Create
    public IActionResult Create()
    {
        return View(new Cliente());
    }

    // POST: /Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);

        var resultado = _clienteService.Crear(cliente);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(cliente);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Edit/1
    public IActionResult Edit(int id)
    {
        var cliente = _clienteService.ObtenerPorId(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();
        if (!ModelState.IsValid) return View(cliente);

        var resultado = _clienteService.Actualizar(cliente);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(cliente);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Delete/1
    public IActionResult Delete(int id)
    {
        var cliente = _clienteService.ObtenerPorId(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Delete/1
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var resultado = _clienteService.Eliminar(id);
        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }
}
