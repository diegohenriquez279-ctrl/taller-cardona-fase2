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
        ViewData["Conteo"] = _ordenService.ConteoPorEstado();
        return View(ordenes);
    }

    // GET: /Ordenes/Details/1
    public IActionResult Details(int id)
    {
        var orden = _ordenService.ObtenerPorId(id);
        if (orden is null) return NotFound();
        ViewData["EstadosPermitidos"] = _ordenService.EstadosPermitidos(orden.Estado);
        return View(orden);
    }

    // GET: /Ordenes/Create
    public IActionResult Create()
    {
        var orden = new OrdenTrabajo();
        CargarListas(orden, null, Array.Empty<int>(), Array.Empty<int>());
        return View(orden);
    }

    // POST: /Ordenes/Create
    // servicioIds y repuestoIds son las casillas marcadas en el formulario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(OrdenTrabajo orden, int[]? servicioIds, int[]? repuestoIds)
    {
        servicioIds ??= Array.Empty<int>();
        repuestoIds ??= Array.Empty<int>();

        if (!ModelState.IsValid)
        {
            CargarListas(orden, null, servicioIds, repuestoIds);
            return View(orden);
        }

        var resultado = _ordenService.Crear(orden, servicioIds, repuestoIds);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            CargarListas(orden, null, servicioIds, repuestoIds);
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
        CargarListas(orden, orden,
            orden.Servicios.Select(d => d.ServicioId),
            orden.Repuestos.Select(d => d.RepuestoId));
        return View(orden);
    }

    // POST: /Ordenes/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, OrdenTrabajo orden, int[]? servicioIds, int[]? repuestoIds)
    {
        if (id != orden.Id) return NotFound();
        var guardada = _ordenService.ObtenerPorId(id);
        if (guardada is null) return NotFound();

        servicioIds ??= Array.Empty<int>();
        repuestoIds ??= Array.Empty<int>();

        if (!ModelState.IsValid)
        {
            CargarListas(orden, guardada, servicioIds, repuestoIds);
            return View(orden);
        }

        var resultado = _ordenService.Actualizar(orden, servicioIds, repuestoIds);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            CargarListas(orden, guardada, servicioIds, repuestoIds);
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

    // Llena las listas del formulario: vehículos, estados, servicios y repuestos.
    // guardada: la orden tal como está en la base (null si es nueva).
    private void CargarListas(OrdenTrabajo orden, OrdenTrabajo? guardada,
        IEnumerable<int> serviciosMarcados, IEnumerable<int> repuestosMarcados)
    {
        CargarListas(orden.VehiculoId, orden.Estado, guardada?.Estado);

        // Los servicios activos más los que la orden ya tenía (aunque luego se hayan desactivado)
        var servicios = _ordenService.ListarServiciosActivos();
        if (guardada is not null)
        {
            servicios.AddRange(guardada.Servicios
                .Where(d => d.Servicio is not null && !servicios.Any(s => s.Id == d.ServicioId))
                .Select(d => d.Servicio!));
        }

        ViewData["Servicios"] = servicios;
        ViewData["Repuestos"] = _ordenService.ListarRepuestos();
        ViewData["ServiciosMarcados"] = serviciosMarcados.ToList();
        ViewData["RepuestosMarcados"] = repuestosMarcados.ToList();
    }

    // estadoBase: el estado que tiene la orden guardada (null si es nueva).
    // Solo se ofrecen los estados a los que la orden puede pasar.
    private void CargarListas(int? vehiculoSeleccionado, EstadoOrden estadoSeleccionado, EstadoOrden? estadoBase = null)
    {
        var vehiculos = _ordenService.ListarVehiculos()
            .Select(v => new
            {
                v.Id,
                Texto = $"{v.Placa} · {v.Marca} {v.Modelo} {v.Anio}" +
                        (v.Cliente is null ? "" : $" ({v.Cliente.Nombre})")
            });
        ViewData["Vehiculos"] = new SelectList(vehiculos, "Id", "Texto", vehiculoSeleccionado);

        var permitidos = estadoBase is null
            ? new List<EstadoOrden> { EstadoOrden.Recibido }
            : _ordenService.EstadosPermitidos(estadoBase.Value);
        ViewData["Estados"] = permitidos
            .Select(e => new SelectListItem(e.Nombre(), e.ToString(), e == estadoSeleccionado))
            .ToList();
    }

    private void CargarEstados(EstadoOrden? seleccionado)
    {
        var estados = Enum.GetValues<EstadoOrden>()
            .Select(e => new SelectListItem(e.Nombre(), e.ToString(), e == seleccionado));
        ViewData["Estados"] = estados.ToList();
    }
}
