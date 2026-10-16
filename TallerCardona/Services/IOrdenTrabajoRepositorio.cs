using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IOrdenTrabajoRepositorio
{
    List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null);
    OrdenTrabajo? ObtenerPorId(int id);
    void Agregar(OrdenTrabajo orden);
    void Actualizar(OrdenTrabajo orden);
    void CambiarEstado(int id, EstadoOrden estado);
    void Eliminar(int id);
    List<Vehiculo> ListarVehiculos();
}