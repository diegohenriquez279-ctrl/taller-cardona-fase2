
using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public interface IRepuestoRepositorio
{
    List<Repuesto> Listar();
    Repuesto? ObtenerPorId(int id);
}
