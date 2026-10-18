
using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IRepuestoRepositorio
{
    List<Repuesto> Listar();
    Repuesto? ObtenerPorId(int id);
}
