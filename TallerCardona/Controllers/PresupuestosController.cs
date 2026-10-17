
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Services;

namespace TallerCardona.Controllers;

public class PresupuestosController : Controller
{
    private readonly IPresupuestoRepositorio _presupuestos;

    public PresupuestosController(IPresupuestoRepositorio presupuestos)
    {
        _presupuestos = presupuestos;
    }

    public IActionResult Index()
    {
        return View(_presupuestos.Listar());
    }

    public IActionResult Details(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return NotFound();

        return View(presupuesto);
    }
}
