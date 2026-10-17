
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

    public List<OrdenTrabajo> Listar()
    {
        return _ordenes
            .OrderByDescending(o => o.FechaIngreso)
            .ToList();
    }

    public OrdenTrabajo? ObtenerPorId(int id)
    {
        return _ordenes.FirstOrDefault(o => o.Id == id);
    }

    public void Agregar(OrdenTrabajo orden)
    {
        var vehiculo = _vehiculos.ObtenerPorId(orden.VehiculoId);

        if (vehiculo is null)
            throw new ArgumentException("El vehículo indicado no existe.");

        orden.Id = _siguienteId++;
        orden.Vehiculo = vehiculo;

        _ordenes.Add(orden);
        vehiculo.Ordenes.Add(orden);
    }

    public void Actualizar(OrdenTrabajo orden)
    {
        var existente = ObtenerPorId(orden.Id);

        if (existente is null)
            return;

        var vehiculoAnterior = existente.Vehiculo;
        var vehiculoNuevo = _vehiculos.ObtenerPorId(orden.VehiculoId);

        if (vehiculoNuevo is null)
            throw new ArgumentException("El vehículo indicado no existe.");

        if (vehiculoAnterior?.Id != vehiculoNuevo.Id)
        {
            vehiculoAnterior?.Ordenes.Remove(existente);
            vehiculoNuevo.Ordenes.Add(existente);
        }

        existente.FechaEntrega = orden.FechaEntrega;
        existente.ProblemaReportado = orden.ProblemaReportado;
        existente.Diagnostico = orden.Diagnostico;
        existente.Estado = orden.Estado;
        existente.KilometrajeIngreso = orden.KilometrajeIngreso;
        existente.Observaciones = orden.Observaciones;
        existente.VehiculoId = vehiculoNuevo.Id;
        existente.Vehiculo = vehiculoNuevo;
        existente.Servicios = orden.Servicios;
        existente.Repuestos = orden.Repuestos;
    }
}
