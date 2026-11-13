using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public interface IOrdenTrabajoRepositorio
{
    List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null);
    OrdenTrabajo? ObtenerPorId(int id);
    void Agregar(OrdenTrabajo orden);
    void Actualizar(OrdenTrabajo orden);
    void Eliminar(int id);
    List<Vehiculo> ListarVehiculos();
    bool ExisteVehiculo(int vehiculoId);
    List<Servicio> ListarServiciosActivos();
    List<Repuesto> ListarRepuestos();
    void ReemplazarDetalles(OrdenTrabajo orden, List<DetalleServicio> servicios, List<DetalleRepuesto> repuestos);
}
