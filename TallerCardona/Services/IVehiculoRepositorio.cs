using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IVehiculoRepositorio
{
    List<Vehiculo> Listar(string? busqueda = null);
    Vehiculo? ObtenerPorId(int id);
    bool PlacaEnUso(string placa, int? excluirId = null);
    void Agregar(Vehiculo vehiculo);
    void Actualizar(Vehiculo vehiculo);
    void Eliminar(int id);
}
