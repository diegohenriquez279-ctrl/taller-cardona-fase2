using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Services;

public interface IVehiculoService
{
    List<Vehiculo> Listar(string? busqueda = null);
    List<VehiculoResumen> ListarResumen();
    Vehiculo? ObtenerPorId(int id);
    Resultado Crear(Vehiculo vehiculo);
    Resultado Actualizar(Vehiculo vehiculo);
    Resultado Eliminar(int id);
}
