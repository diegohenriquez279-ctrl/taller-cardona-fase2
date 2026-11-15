using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Repositorios;

public interface IVehiculoRepositorio
{
    List<Vehiculo> Listar(string? busqueda = null);
    List<VehiculoResumen> ListarResumen();
    Vehiculo? ObtenerPorId(int id);
    bool PlacaEnUso(string placa, int? excluirId = null);
    bool ExisteCliente(int clienteId);
    void Agregar(Vehiculo vehiculo);
    void Actualizar(Vehiculo vehiculo);
    void Eliminar(int id);
}
