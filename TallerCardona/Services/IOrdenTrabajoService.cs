using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Services;

public interface IOrdenTrabajoService
{
    List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null,
        DateTime? desde = null, DateTime? hasta = null);
    OrdenTrabajo? ObtenerPorId(int id);
    List<ConteoPorEstado> ConteoPorEstado();
    List<Vehiculo> ListarVehiculos();
    List<EstadoOrden> EstadosPermitidos(EstadoOrden estadoActual);
    List<Servicio> ListarServiciosActivos();
    List<Repuesto> ListarRepuestos();
    Resultado Crear(OrdenTrabajo orden, IEnumerable<int> servicioIds, IEnumerable<int> repuestoIds);
    Resultado Actualizar(OrdenTrabajo orden, IEnumerable<int> servicioIds, IEnumerable<int> repuestoIds);
    Resultado CambiarEstado(int id, EstadoOrden nuevoEstado);
    Resultado Eliminar(int id);
}
