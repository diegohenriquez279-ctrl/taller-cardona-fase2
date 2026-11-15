
using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Services;

public interface IPresupuestoService
{
    List<Presupuesto> Listar();
    Presupuesto? ObtenerPorId(int id);
    Resultado Crear(int ordenTrabajoId);
    Resultado Autorizar(int id);
    Resultado Rechazar(int id);
    List<PresupuestosPorEstado> ListarPorEstado();
}
