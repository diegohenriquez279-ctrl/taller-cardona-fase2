using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

// Reglas de negocio del módulo Vehículos.
// El servicio depende de interfaces y confirma los cambios mediante la unidad de trabajo.
public class VehiculoService : IVehiculoService
{
    private readonly IVehiculoRepositorio _repositorio;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public VehiculoService(IVehiculoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<Vehiculo> Listar(string? busqueda = null) =>
        _repositorio.Listar(busqueda?.Trim());

    public List<VehiculoResumen> ListarResumen() => _repositorio.ListarResumen();

    public Vehiculo? ObtenerPorId(int id) => _repositorio.ObtenerPorId(id);

    public Resultado Crear(Vehiculo vehiculo)
    {
        vehiculo.Placa = NormalizarPlaca(vehiculo.Placa);

        if (!_repositorio.ExisteCliente(vehiculo.ClienteId))
            return Resultado.Error("Seleccione un cliente registrado.");

        if (_repositorio.PlacaEnUso(vehiculo.Placa))
            return Resultado.Error("Ya existe un vehículo registrado con esa placa.");

        try
        {
            _repositorio.Agregar(vehiculo);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Vehículo registrado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo guardar el vehículo. Revise que la placa no esté registrada e intente de nuevo.");
        }
    }

    public Resultado Actualizar(Vehiculo vehiculo)
    {
        if (_repositorio.ObtenerPorId(vehiculo.Id) is null)
            return Resultado.Error("El vehículo no existe.");

        vehiculo.Placa = NormalizarPlaca(vehiculo.Placa);

        if (!_repositorio.ExisteCliente(vehiculo.ClienteId))
            return Resultado.Error("Seleccione un cliente registrado.");

        if (_repositorio.PlacaEnUso(vehiculo.Placa, vehiculo.Id))
            return Resultado.Error("Ya existe otro vehículo registrado con esa placa.");

        try
        {
            _repositorio.Actualizar(vehiculo);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Vehículo actualizado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo actualizar el vehículo. Revise que la placa no esté registrada e intente de nuevo.");
        }
    }

    public Resultado Eliminar(int id)
    {
        var vehiculo = _repositorio.ObtenerPorId(id);
        if (vehiculo is null)
            return Resultado.Error("El vehículo no existe.");

        if (vehiculo.Ordenes.Any())
            return Resultado.Error("No se puede eliminar un vehículo que ya tiene historial de órdenes de trabajo.");

        try
        {
            _repositorio.Eliminar(id);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Vehículo eliminado.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo eliminar el vehículo. Verifique que no tenga historial asociado e intente de nuevo.");
        }
    }

    private static string NormalizarPlaca(string placa) =>
        placa.Trim().ToUpperInvariant();
}
