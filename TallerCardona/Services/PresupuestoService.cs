
using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

public class PresupuestoService : IPresupuestoService
{
    private readonly IPresupuestoRepositorio _presupuestos;
    private readonly IOrdenTrabajoRepositorio _ordenes;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly AppDbContext _context;

    public PresupuestoService(
        IPresupuestoRepositorio presupuestos,
        IOrdenTrabajoRepositorio ordenes,
        IUnidadDeTrabajo unidadDeTrabajo,
        AppDbContext context)
    {
        _presupuestos = presupuestos;
        _ordenes = ordenes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _context = context;
    }

    public List<Presupuesto> Listar() =>
        _presupuestos.Listar();

    public Presupuesto? ObtenerPorId(int id) =>
        _presupuestos.ObtenerPorId(id);

    public Resultado Crear(int ordenTrabajoId)
    {
        var orden = _ordenes.ObtenerPorId(ordenTrabajoId);

        if (orden is null)
            return Resultado.Error("No se encontró la orden de trabajo.");

        if (orden.Presupuesto is not null ||
            _presupuestos.Listar().Any(p => p.OrdenTrabajoId == orden.Id))
        {
            return Resultado.Error("Esta orden ya tiene un presupuesto.");
        }

        var totalServicios = orden.Servicios
            .Sum(s => s.Cantidad * s.PrecioUnitario);

        var totalRepuestos = orden.Repuestos
            .Sum(r => r.Cantidad * r.PrecioUnitario);

        var total = totalServicios + totalRepuestos;

        if (total <= 0)
            return Resultado.Error(
                "No se puede crear un presupuesto de cero. Verifique los servicios y repuestos.");

        var presupuesto = new Presupuesto
        {
            OrdenTrabajoId = orden.Id,
            TotalEstimado = total,
            Estado = EstadoPresupuesto.Pendiente
        };

        try
        {
            _presupuestos.Agregar(presupuesto);
            _unidadDeTrabajo.Guardar();

            return Resultado.Ok("Presupuesto creado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo guardar el presupuesto.");
        }
    }

    public Resultado Autorizar(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return Resultado.Error("El presupuesto no existe.");

        if (presupuesto.Estado != EstadoPresupuesto.Pendiente)
            return Resultado.Error(
                "Solo se pueden autorizar presupuestos pendientes.");

        try
        {
            presupuesto.Estado = EstadoPresupuesto.Autorizado;
            _presupuestos.Actualizar(presupuesto);
            _unidadDeTrabajo.Guardar();

            return Resultado.Ok("Presupuesto autorizado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo autorizar el presupuesto.");
        }
    }

    public Resultado Rechazar(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return Resultado.Error("El presupuesto no existe.");

        if (presupuesto.Estado != EstadoPresupuesto.Pendiente)
            return Resultado.Error(
                "Solo se pueden rechazar presupuestos pendientes.");

        try
        {
            presupuesto.Estado = EstadoPresupuesto.Rechazado;
            _presupuestos.Actualizar(presupuesto);
            _unidadDeTrabajo.Guardar();

            return Resultado.Ok("Presupuesto rechazado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo rechazar el presupuesto.");
        }
    }

    public List<PresupuestosPorEstado> ListarPorEstado() =>
        _context.Presupuestos
            .GroupBy(p => p.Estado)
            .Select(g => new PresupuestosPorEstado
            {
                Estado = g.Key,
                Cantidad = g.Count(),
                MontoTotal = g.Sum(p => p.TotalEstimado)
            })
            .ToList();
}
