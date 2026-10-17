
using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IServicioRepositorio
{
    List<Servicio> Listar();
    Servicio? ObtenerPorId(int id);
}
