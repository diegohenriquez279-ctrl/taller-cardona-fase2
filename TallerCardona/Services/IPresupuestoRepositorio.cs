
using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IPresupuestoRepositorio
{
    List<Presupuesto> Listar();
    Presupuesto? ObtenerPorId(int id);
    void Agregar(Presupuesto presupuesto);
    void Actualizar(Presupuesto presupuesto);
}
