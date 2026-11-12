
using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public interface IServicioRepositorio
{
    List<Servicio> Listar();
    Servicio? ObtenerPorId(int id);
}
