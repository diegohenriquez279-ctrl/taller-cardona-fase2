
using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IOrdenTrabajoRepositorio
{
    List<OrdenTrabajo> Listar();
    OrdenTrabajo? ObtenerPorId(int id);
    void Agregar(OrdenTrabajo orden);
    void Actualizar(OrdenTrabajo orden);
}
