
using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Services;

public interface IPresupuestoService
{
    List<Presupuesto> Listar();
    Presupuesto? ObtenerPorId(int id);

    /// <summary>Presupuesto de una orden, o null si no tiene.</summary>
    Presupuesto? ObtenerPorOrden(int ordenTrabajoId);

    /// <summary>
    /// Genera el presupuesto de una orden con el total de sus servicios y repuestos.
    /// Si la orden ya tenía un presupuesto Rechazado, lo regenera (vuelve a Pendiente).
    /// </summary>
    Resultado Crear(int ordenTrabajoId, string? usuario = null);

    Resultado Autorizar(int id, string? usuario = null);
    Resultado Rechazar(int id, string? usuario = null);
    List<PresupuestosPorEstado> ListarPorEstado();
}
