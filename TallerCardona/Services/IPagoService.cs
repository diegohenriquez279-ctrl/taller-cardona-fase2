using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IPagoService
{
    Task<List<Pago>> ListarAsync();
    Task<Pago?> ObtenerPorIdAsync(int id);
    Task<List<Pago>> ListarPorOrdenAsync(int ordenTrabajoId);

    /// <summary>Presupuesto por id (se lee con IPresupuestoRepositorio).</summary>
    Presupuesto? ObtenerPresupuesto(int presupuestoId);

    /// <summary>Presupuesto de una orden de trabajo, o null si no tiene.</summary>
    Presupuesto? ObtenerPresupuestoDeOrden(int ordenTrabajoId);

    /// <summary>
    /// Registra un pago. Lanza InvalidOperationException si no cumple una regla:
    /// monto mayor que cero, presupuesto Autorizado y no superar el saldo pendiente.
    /// </summary>
    Task<Pago> RegistrarAsync(Pago pago);

    /// <summary>Elimina un pago. Devuelve false si no existe.</summary>
    Task<bool> EliminarAsync(int id, string? usuario);
}
