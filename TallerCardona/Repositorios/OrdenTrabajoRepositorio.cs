using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Repositorios;

// Repositorio de órdenes de trabajo con Entity Framework Core.
// Solo lee y prepara cambios; el guardado lo hace la unidad de trabajo.
public class OrdenTrabajoRepositorio : IOrdenTrabajoRepositorio
{
    private readonly AppDbContext _context;

    public OrdenTrabajoRepositorio(AppDbContext context)
    {
        _context = context;
    }

    // Consulta con Include (trae el vehículo y su dueño), filtros (Where) y orden (OrderBy)
    public List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null)
    {
        var consulta = _context.OrdenesTrabajo
            .Include(o => o.Vehiculo)
                .ThenInclude(v => v!.Cliente)
            .AsQueryable();

        if (estado.HasValue)
            consulta = consulta.Where(o => o.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            var esNumero = int.TryParse(texto, out var numero);

            consulta = consulta.Where(o =>
                (esNumero && o.Id == numero) ||
                o.ProblemaReportado.Contains(texto) ||
                o.Vehiculo!.Placa.Contains(texto) ||
                o.Vehiculo.Marca.Contains(texto) ||
                o.Vehiculo.Modelo.Contains(texto) ||
                o.Vehiculo.Cliente!.Nombre.Contains(texto));
        }

        return consulta
            .OrderByDescending(o => o.FechaIngreso)
            .ToList();
    }

    // Trae la orden con todo lo relacionado: vehículo, cliente, servicios,
    // repuestos, presupuesto y pagos.
    public OrdenTrabajo? ObtenerPorId(int id) =>
        _context.OrdenesTrabajo
            .Include(o => o.Vehiculo)
                .ThenInclude(v => v!.Cliente)
            .Include(o => o.Servicios)
                .ThenInclude(d => d.Servicio)
            .Include(o => o.Repuestos)
                .ThenInclude(d => d.Repuesto)
            .Include(o => o.Presupuesto)
            .Include(o => o.Pagos)
            .FirstOrDefault(o => o.Id == id);

    // Consulta con agrupación (GroupBy) para el tablero:
    // junta las órdenes por estado y cuenta cuántas hay en cada grupo.
    // SQL Server la ejecuta como SELECT Estado, COUNT(*) ... GROUP BY Estado.
    public List<ConteoPorEstado> ContarPorEstado() =>
        _context.OrdenesTrabajo
            .GroupBy(o => o.Estado)
            .Select(g => new ConteoPorEstado
            {
                Estado = g.Key,
                Cantidad = g.Count()
            })
            .ToList();

    public void Agregar(OrdenTrabajo orden) => _context.OrdenesTrabajo.Add(orden);

    public void Actualizar(OrdenTrabajo orden)
    {
        var existente = _context.OrdenesTrabajo.Find(orden.Id);
        if (existente is null) return;

        // Copia los valores del formulario al registro que ya está en la base
        _context.Entry(existente).CurrentValues.SetValues(orden);
    }

    public void Eliminar(int id)
    {
        var orden = _context.OrdenesTrabajo.Find(id);
        if (orden is not null) _context.OrdenesTrabajo.Remove(orden);
    }

    public List<Vehiculo> ListarVehiculos() =>
        _context.Vehiculos
            .Include(v => v.Cliente)
            .OrderBy(v => v.Placa)
            .ToList();

    public bool ExisteVehiculo(int vehiculoId) =>
        _context.Vehiculos.Any(v => v.Id == vehiculoId);

    // Catálogo para armar la orden. Solo los servicios activos se pueden elegir.
    public List<Servicio> ListarServiciosActivos() =>
        _context.Servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Nombre)
            .ToList();

    public List<Repuesto> ListarRepuestos() =>
        _context.Repuestos
            .OrderBy(r => r.Nombre)
            .ToList();

    // Deja en la orden exactamente los servicios y repuestos indicados:
    // borra los que se quitaron y agrega los nuevos. Los que se mantienen
    // conservan el precio con el que se registraron.
    public void ReemplazarDetalles(OrdenTrabajo orden, List<DetalleServicio> servicios, List<DetalleRepuesto> repuestos)
    {
        var serviciosQuitados = orden.Servicios
            .Where(d => !servicios.Any(n => n.ServicioId == d.ServicioId))
            .ToList();
        _context.DetallesServicio.RemoveRange(serviciosQuitados);

        foreach (var nuevo in servicios.Where(n => !orden.Servicios.Any(d => d.ServicioId == n.ServicioId)))
            orden.Servicios.Add(nuevo);

        var repuestosQuitados = orden.Repuestos
            .Where(d => !repuestos.Any(n => n.RepuestoId == d.RepuestoId))
            .ToList();
        _context.DetallesRepuesto.RemoveRange(repuestosQuitados);

        foreach (var nuevo in repuestos.Where(n => !orden.Repuestos.Any(d => d.RepuestoId == n.RepuestoId)))
            orden.Repuestos.Add(nuevo);
    }
}
