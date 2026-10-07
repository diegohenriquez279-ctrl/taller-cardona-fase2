using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class ClientesController : Controller
{
    private readonly IClienteRepositorio _repositorio;

    public ClientesController(IClienteRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: /Clientes?busqueda=carlos
    public IActionResult Index(string? busqueda)
    {
        ViewData["Busqueda"] = busqueda;
        var clientes = _repositorio.Listar(busqueda);
        return View(clientes);
    }

    // GET: /Clientes/Details/1
    public IActionResult Details(int id)
    {
        var cliente = _repositorio.ObtenerPorId(id);
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

        _repositorio.Agregar(cliente);
        TempData["Mensaje"] = "Cliente registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Edit/1
    public IActionResult Edit(int id)
    {
        var cliente = _repositorio.ObtenerPorId(id);
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

        _repositorio.Actualizar(cliente);
        TempData["Mensaje"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Delete/1
    public IActionResult Delete(int id)
    {
        var cliente = _repositorio.ObtenerPorId(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Delete/1
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _repositorio.Eliminar(id);
        TempData["Mensaje"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }
}