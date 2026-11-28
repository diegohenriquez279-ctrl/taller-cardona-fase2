
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
    private readonly ILogger<PresupuestoService> _logger;

    public PresupuestoService(
        IPresupuestoRepositorio presupuestos,
        IOrdenTrabajoRepositorio ordenes,
        IUnidadDeTrabajo unidadDeTrabajo,
        AppDbContext context,
        ILogger<PresupuestoService> logger)
    {
        _presupuestos = presupuestos;
        _ordenes = ordenes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _context = context;
        _logger = logger;
    }

    public List<Presupuesto> Listar() =>
        _presupuestos.Listar();

    public Presupuesto? ObtenerPorId(int id) =>
        _presupuestos.ObtenerPorId(id);

    public Presupuesto? ObtenerPorOrden(int ordenTrabajoId) =>
        _presupuestos.Listar().FirstOrDefault(p => p.OrdenTrabajoId == ordenTrabajoId);

    public Resultado Crear(int ordenTrabajoId, string? usuario = null)
    {
        var orden = _ordenes.ObtenerPorId(ordenTrabajoId);

        if (orden is null)
            return Rechazo(ordenTrabajoId, "No se encontró la orden de trabajo.");

        // Una orden tiene un solo presupuesto (índice único en la base).
        // Pendiente o Autorizado: no se genera otro. Rechazado: se regenera el mismo registro.
        var existente = orden.Presupuesto;
        if (existente is not null && existente.Estado != EstadoPresupuesto.Rechazado)
            return Rechazo(ordenTrabajoId,
                $"Esta orden ya tiene un presupuesto {existente.Estado.ToString().ToLower()} (N.º {existente.Id}).");

        if (orden.Servicios.Count == 0 && orden.Repuestos.Count == 0)
            return Rechazo(ordenTrabajoId,
                "La orden no tiene servicios ni repuestos. Agréguelos (Editar orden) antes de generar el presupuesto.");

        var totalServicios = orden.Servicios
            .Sum(s => s.Cantidad * s.PrecioUnitario);

        var totalRepuestos = orden.Repuestos
            .Sum(r => r.Cantidad * r.PrecioUnitario);

        var total = totalServicios + totalRepuestos;

        if (total <= 0)
            return Rechazo(ordenTrabajoId,
                "No se puede crear un presupuesto de cero. Verifique los servicios y repuestos.");

        try
        {
            Presupuesto presupuesto;
            if (existente is null)
            {
                presupuesto = new Presupuesto
                {
                    OrdenTrabajoId = orden.Id,
                    TotalEstimado = total,
                    Estado = EstadoPresupuesto.Pendiente
                };
                _presupuestos.Agregar(presupuesto);
            }
            else
            {
                presupuesto = existente;
                presupuesto.TotalEstimado = total;
                presupuesto.Estado = EstadoPresupuesto.Pendiente;
                presupuesto.Fecha = DateTime.Now;
                _presupuestos.Actualizar(presupuesto);
            }

            _unidadDeTrabajo.Guardar();

            _logger.LogInformation(
                "Presupuesto {PresupuestoId} {Accion} para la orden {OrdenTrabajoId}: total {Total} (usuario {Usuario})",
                presupuesto.Id, existente is null ? "generado" : "regenerado", orden.Id, total, usuario ?? "desconocido");

            return Resultado.Ok(existente is null
                ? "Presupuesto generado correctamente."
                : "Presupuesto regenerado: vuelve a quedar Pendiente.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al guardar el presupuesto de la orden {OrdenTrabajoId}", ordenTrabajoId);
            return Resultado.Error("No se pudo guardar el presupuesto.");
        }
    }

    public Resultado Autorizar(int id, string? usuario = null) =>
        CambiarEstado(id, EstadoPresupuesto.Autorizado, usuario);

    public Resultado Rechazar(int id, string? usuario = null) =>
        CambiarEstado(id, EstadoPresupuesto.Rechazado, usuario);

    // Autorizar y rechazar comparten la misma regla: solo desde Pendiente.
    private Resultado CambiarEstado(int id, EstadoPresupuesto nuevo, string? usuario)
    {
        var verbo = nuevo == EstadoPresupuesto.Autorizado ? "autorizar" : "rechazar";
        var presupuesto = _presupuestos.ObtenerPorId(id);

        if (presupuesto is null)
            return Rechazo(null, "El presupuesto no existe.", id);

        if (presupuesto.Estado != EstadoPresupuesto.Pendiente)
            return Rechazo(presupuesto.OrdenTrabajoId,
                $"Solo se pueden {verbo} presupuestos pendientes.", id);

        try
        {
            presupuesto.Estado = nuevo;
            _presupuestos.Actualizar(presupuesto);
            _unidadDeTrabajo.Guardar();

            _logger.LogInformation(
                "Presupuesto {PresupuestoId} {Estado} por {Usuario}: orden {OrdenTrabajoId}, total {Total}",
                id, nuevo == EstadoPresupuesto.Autorizado ? "autorizado" : "rechazado",
                usuario ?? "desconocido", presupuesto.OrdenTrabajoId, presupuesto.TotalEstimado);

            return Resultado.Ok(nuevo == EstadoPresupuesto.Autorizado
                ? "Presupuesto autorizado correctamente."
                : "Presupuesto rechazado correctamente.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al {Verbo} el presupuesto {PresupuestoId}", verbo, id);
            return Resultado.Error($"No se pudo {verbo} el presupuesto.");
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

    // Regla incumplida: queda en la bitácora como Warning y se devuelve el mensaje.
    private Resultado Rechazo(int? ordenTrabajoId, string motivo, int? presupuestoId = null)
    {
        _logger.LogWarning(
            "Presupuesto: regla incumplida (orden {OrdenTrabajoId}, presupuesto {PresupuestoId}). Motivo: {Motivo}",
            ordenTrabajoId, presupuestoId, motivo);
        return Resultado.Error(motivo);
    }
}
