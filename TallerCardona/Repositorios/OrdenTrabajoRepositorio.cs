using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;

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
}
