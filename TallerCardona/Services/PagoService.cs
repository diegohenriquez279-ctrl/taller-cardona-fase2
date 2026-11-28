using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

public class PagoService : IPagoService
{
    private readonly IPagoRepositorio _pagos;
    private readonly IPresupuestoRepositorio _presupuestos;
    private readonly AppDbContext _context;
    private readonly ILogger<PagoService> _logger;

    public PagoService(
        IPagoRepositorio pagos,
        IPresupuestoRepositorio presupuestos,
        AppDbContext context,
        ILogger<PagoService> logger)
    {
        _pagos = pagos;
        _presupuestos = presupuestos;
        _context = context;
        _logger = logger;
    }

    public Task<List<Pago>> ListarAsync() => _pagos.ObtenerTodosAsync();
    public Task<Pago?> ObtenerPorIdAsync(int id) => _pagos.ObtenerPorIdAsync(id);
    public Task<List<Pago>> ListarPorOrdenAsync(int ordenTrabajoId) => _pagos.ObtenerPorOrdenAsync(ordenTrabajoId);

    public Presupuesto? ObtenerPresupuesto(int presupuestoId) =>
        _presupuestos.ObtenerPorId(presupuestoId);

    public Presupuesto? ObtenerPresupuestoDeOrden(int ordenTrabajoId) =>
        _presupuestos.Listar().FirstOrDefault(p => p.OrdenTrabajoId == ordenTrabajoId);

    public async Task<Pago> RegistrarAsync(Pago pago)
    {
        if (pago.Monto <= 0)
            Rechazar(pago, "El monto debe ser mayor que cero.");

        if (!await _context.OrdenesTrabajo.AnyAsync(o => o.Id == pago.OrdenTrabajoId))
            Rechazar(pago, "La orden de trabajo no existe.");

        var presupuesto = ObtenerPresupuestoDeOrden(pago.OrdenTrabajoId);
        if (presupuesto is null)
            Rechazar(pago, "La orden no tiene presupuesto.");
        if (presupuesto.Estado != EstadoPresupuesto.Autorizado)
            Rechazar(pago, "El presupuesto no está autorizado.");

        var yaPagado = (await _pagos.ObtenerPorOrdenAsync(pago.OrdenTrabajoId)).Sum(p => p.Monto);
        var saldo = presupuesto.TotalEstimado - yaPagado;

        if (pago.Monto > saldo)
            Rechazar(pago, $"El monto supera el saldo pendiente de {saldo:C}.");

        pago.Id = 0;
        pago.Fecha = DateTime.Now;
        pago.Estado = "Registrado";

        await _pagos.AgregarAsync(pago);
        await _pagos.GuardarCambiosAsync();

        _logger.LogInformation(
            "Pago {PagoId} registrado: orden {OrdenTrabajoId}, presupuesto {PresupuestoId}, monto {Monto}, método {MetodoPago}",
            pago.Id, pago.OrdenTrabajoId, presupuesto.Id, pago.Monto, pago.MetodoPago);

        return pago;
    }

    public async Task<bool> EliminarAsync(int id, string? usuario)
    {
        var pago = await _pagos.ObtenerPorIdAsync(id);
        if (pago is null) return false;

        await _pagos.EliminarAsync(id);
        await _pagos.GuardarCambiosAsync();

        _logger.LogInformation(
            "Pago {PagoId} eliminado por {Usuario}: orden {OrdenTrabajoId}, monto {Monto}",
            id, usuario ?? "desconocido", pago.OrdenTrabajoId, pago.Monto);
        return true;
    }

    // Deja constancia en la bitácora y lanza la excepción con el mensaje para el usuario.
    [DoesNotReturn]
    private void Rechazar(Pago pago, string motivo)
    {
        _logger.LogWarning(
            "Pago rechazado: orden {OrdenTrabajoId}, monto {Monto}. Motivo: {Motivo}",
            pago.OrdenTrabajoId, pago.Monto, motivo);
        throw new InvalidOperationException(motivo);
    }
}
