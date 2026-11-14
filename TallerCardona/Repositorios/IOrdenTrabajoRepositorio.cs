using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Repositorios;

public interface IOrdenTrabajoRepositorio
{
    List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null,
        DateTime? desde = null, DateTime? hasta = null);
    OrdenTrabajo? ObtenerPorId(int id);
    List<ConteoPorEstado> ContarPorEstado();
    void Agregar(OrdenTrabajo orden);
    void Actualizar(OrdenTrabajo orden);
    void Eliminar(int id);
    List<Vehiculo> ListarVehiculos();
    bool ExisteVehiculo(int vehiculoId);
    List<Servicio> ListarServiciosActivos();
    List<Repuesto> ListarRepuestos();
    void ReemplazarDetalles(OrdenTrabajo orden, List<DetalleServicio> servicios, List<DetalleRepuesto> repuestos);
}
