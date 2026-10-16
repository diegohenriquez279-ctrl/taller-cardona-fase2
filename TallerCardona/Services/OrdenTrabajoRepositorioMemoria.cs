using TallerCardona.Models;

namespace TallerCardona.Services;

public class OrdenTrabajoRepositorioMemoria : IOrdenTrabajoRepositorio
{
    // Vehículos de ejemplo mientras el módulo de Vehículos no esté integrado.
    private readonly List<Vehiculo> _vehiculos = new()
    {
        new Vehiculo { Id = 1, Placa = "P123-456", Marca = "Toyota", Modelo = "Hilux", Anio = 2018, Color = "Blanco", Kilometraje = 95000, ClienteId = 1,
            Cliente = new Cliente { Id = 1, Nombre = "Carlos Martínez", Telefono = "7712-3456" } },
        new Vehiculo { Id = 2, Placa = "P789-012", Marca = "Nissan", Modelo = "Sentra", Anio = 2015, Color = "Gris", Kilometraje = 128000, ClienteId = 2,
            Cliente = new Cliente { Id = 2, Nombre = "María Hernández", Telefono = "7845-9012" } },
        new Vehiculo { Id = 3, Placa = "P345-678", Marca = "Honda", Modelo = "CR-V", Anio = 2020, Color = "Negro", Kilometraje = 42000, ClienteId = 3,
            Cliente = new Cliente { Id = 3, Nombre = "José Ramírez", Telefono = "6098-7654" } }
    };

    private readonly List<OrdenTrabajo> _ordenes;
    private int _siguienteId = 6;

    public OrdenTrabajoRepositorioMemoria()
    {
        _ordenes = new List<OrdenTrabajo>
        {
            Crear(1, 1, "Ruido al frenar y pedal esponjoso", EstadoOrden.Recibido, 95120, DateTime.Now.AddDays(-1)),
            Crear(2, 2, "El motor se calienta en el tráfico", EstadoOrden.EnDiagnostico, 128300, DateTime.Now.AddDays(-2)),
            Crear(3, 3, "Cambio de aceite y revisión de 40,000 km", EstadoOrden.EnReparacion, 42010, DateTime.Now.AddDays(-3)),
            Crear(4, 1, "Falla en el aire acondicionado", EstadoOrden.ListoParaEntrega, 94800, DateTime.Now.AddDays(-5)),
            Crear(5, 2, "Alineado y balanceo", EstadoOrden.Entregado, 127500, DateTime.Now.AddDays(-8))
        };
        _ordenes[4].FechaEntrega = DateTime.Now.AddDays(-6);
    }

    private OrdenTrabajo Crear(int id, int vehiculoId, string problema, EstadoOrden estado, decimal kilometraje, DateTime fechaIngreso) => new()
    {
        Id = id,
        VehiculoId = vehiculoId,
        Vehiculo = _vehiculos.First(v => v.Id == vehiculoId),
        ProblemaReportado = problema,
        Estado = estado,
        KilometrajeIngreso = kilometraje,
        FechaIngreso = fechaIngreso
    };

    public List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null)
    {
        var resultado = _ordenes.AsEnumerable();

        if (estado.HasValue)
        {
            resultado = resultado.Where(o => o.Estado == estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            resultado = resultado.Where(o =>
                o.Id.ToString() == busqueda.Trim() ||
                o.ProblemaReportado.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                (o.Vehiculo?.Placa.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Vehiculo?.Marca.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Vehiculo?.Modelo.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Vehiculo?.Cliente?.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return resultado.OrderByDescending(o => o.FechaIngreso).ToList();
    }

    public OrdenTrabajo? ObtenerPorId(int id) => _ordenes.FirstOrDefault(o => o.Id == id);

    public void Agregar(OrdenTrabajo orden)
    {
        orden.Id = _siguienteId++;
        orden.Vehiculo = _vehiculos.FirstOrDefault(v => v.Id == orden.VehiculoId);
        AjustarFechaEntrega(orden);
        _ordenes.Add(orden);
    }

    public void Actualizar(OrdenTrabajo orden)
    {
        var existente = ObtenerPorId(orden.Id);
        if (existente is null) return;

        existente.VehiculoId = orden.VehiculoId;
        existente.Vehiculo = _vehiculos.FirstOrDefault(v => v.Id == orden.VehiculoId);
        existente.FechaIngreso = orden.FechaIngreso;
        existente.ProblemaReportado = orden.ProblemaReportado;
        existente.Diagnostico = orden.Diagnostico;
        existente.KilometrajeIngreso = orden.KilometrajeIngreso;
        existente.Observaciones = orden.Observaciones;
        existente.Estado = orden.Estado;
        AjustarFechaEntrega(existente);
    }

    public void CambiarEstado(int id, EstadoOrden estado)
    {
        var existente = ObtenerPorId(id);
        if (existente is null) return;

        existente.Estado = estado;
        AjustarFechaEntrega(existente);
    }

    public void Eliminar(int id) => _ordenes.RemoveAll(o => o.Id == id);

    public List<Vehiculo> ListarVehiculos() => _vehiculos.OrderBy(v => v.Placa).ToList();

    // La fecha de entrega se registra sola al pasar a Entregado y se limpia si la orden se reabre.
    private static void AjustarFechaEntrega(OrdenTrabajo orden)
    {
        if (orden.Estado == EstadoOrden.Entregado)
        {
            orden.FechaEntrega ??= DateTime.Now;
        }
        else
        {
            orden.FechaEntrega = null;
        }
    }
}