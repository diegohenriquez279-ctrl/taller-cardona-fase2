using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IOrdenTrabajoService
{
    List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null);
    OrdenTrabajo? ObtenerPorId(int id);
    List<Vehiculo> ListarVehiculos();
    Resultado Crear(OrdenTrabajo orden);
    Resultado Actualizar(OrdenTrabajo orden);
    Resultado CambiarEstado(int id, EstadoOrden nuevoEstado);
    Resultado Eliminar(int id);
}
