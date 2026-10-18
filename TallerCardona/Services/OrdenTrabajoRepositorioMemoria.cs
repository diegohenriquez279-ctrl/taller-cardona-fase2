
using TallerCardona.Models;

namespace TallerCardona.Services;

public class OrdenTrabajoRepositorioMemoria : IOrdenTrabajoRepositorio
{
    private readonly IVehiculoRepositorio _vehiculos;
    private readonly List<OrdenTrabajo> _ordenes = new();
    private int _siguienteId = 1;

    public OrdenTrabajoRepositorioMemoria(IVehiculoRepositorio vehiculos)
    {
        _vehiculos = vehiculos;
    }

    public List<OrdenTrabajo> Listar(
        string? busqueda = null,
        EstadoOrden? estado = null)
    {
        var resultado = _ordenes.AsEnumerable();

        if (estado.HasValue)
            resultado = resultado.Where(o => o.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            resultado = resultado.Where(o =>
                o.Id.ToString() == busqueda.Trim() ||
                o.ProblemaReportado.Contains(
                    busqueda, StringComparison.OrdinalIgnoreCase) ||
                (o.Vehiculo?.Placa.Contains(
                    busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Vehiculo?.Marca.Contains(
                    busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Vehiculo?.Modelo.Contains(
                    busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (o.Vehiculo?.Cliente?.Nombre.Contains(
                    busqueda, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return resultado
            .OrderByDescending(o => o.FechaIngreso)
            .ToList();
    }

    public OrdenTrabajo? ObtenerPorId(int id) =>
        _ordenes.FirstOrDefault(o => o.Id == id);

    public void Agregar(OrdenTrabajo orden)
    {
        var vehiculo = _vehiculos.ObtenerPorId(orden.VehiculoId);

        if (vehiculo is null)
            throw new ArgumentException("El vehículo indicado no existe.");

        orden.Id = _siguienteId++;
        orden.Vehiculo = vehiculo;

        foreach (var servicio in orden.Servicios)
        {
            servicio.OrdenTrabajoId = orden.Id;
            servicio.OrdenTrabajo = orden;
        }

        foreach (var repuesto in orden.Repuestos)
        {
            repuesto.OrdenTrabajoId = orden.Id;
            repuesto.OrdenTrabajo = orden;
        }

        AjustarFechaEntrega(orden);
        _ordenes.Add(orden);

        if (!vehiculo.Ordenes.Any(o => o.Id == orden.Id))
            vehiculo.Ordenes.Add(orden);
    }

    public void Actualizar(OrdenTrabajo orden)
    {
        var existente = ObtenerPorId(orden.Id);
        if (existente is null) return;

        var vehiculoAnterior = existente.Vehiculo;
        var vehiculoNuevo = _vehiculos.ObtenerPorId(orden.VehiculoId);

        if (vehiculoNuevo is null)
            throw new ArgumentException("El vehículo indicado no existe.");

        if (vehiculoAnterior?.Id != vehiculoNuevo.Id)
        {
            vehiculoAnterior?.Ordenes.Remove(existente);

            if (!vehiculoNuevo.Ordenes.Any(o => o.Id == existente.Id))
                vehiculoNuevo.Ordenes.Add(existente);
        }

        existente.FechaIngreso = orden.FechaIngreso;
        existente.ProblemaReportado = orden.ProblemaReportado;
        existente.Diagnostico = orden.Diagnostico;
        existente.Estado = orden.Estado;
        existente.KilometrajeIngreso = orden.KilometrajeIngreso;
        existente.Observaciones = orden.Observaciones;
        existente.VehiculoId = vehiculoNuevo.Id;
        existente.Vehiculo = vehiculoNuevo;
        existente.Servicios = orden.Servicios;
        existente.Repuestos = orden.Repuestos;

        foreach (var servicio in existente.Servicios)
        {
            servicio.OrdenTrabajoId = existente.Id;
            servicio.OrdenTrabajo = existente;
        }

        foreach (var repuesto in existente.Repuestos)
        {
            repuesto.OrdenTrabajoId = existente.Id;
            repuesto.OrdenTrabajo = existente;
        }

        AjustarFechaEntrega(existente);
    }

    public void CambiarEstado(int id, EstadoOrden estado)
    {
        var orden = ObtenerPorId(id);
        if (orden is null) return;

        orden.Estado = estado;
        AjustarFechaEntrega(orden);
    }

    public void Eliminar(int id)
    {
        var orden = ObtenerPorId(id);
        if (orden is null) return;

        orden.Vehiculo?.Ordenes.Remove(orden);
        _ordenes.Remove(orden);
    }

    public List<Vehiculo> ListarVehiculos() =>
        _vehiculos.Listar();

    private static void AjustarFechaEntrega(OrdenTrabajo orden)
    {
        if (orden.Estado == EstadoOrden.Entregado)
            orden.FechaEntrega ??= DateTime.Now;
        else
            orden.FechaEntrega = null;
    }
}
