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
    private readonly ILogger<VehiculoService> _logger;

    public VehiculoService(
        IVehiculoRepositorio repositorio,
        IUnidadDeTrabajo unidadDeTrabajo,
        ILogger<VehiculoService> logger)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
        _logger = logger;
    }

    public List<Vehiculo> Listar(string? busqueda = null) =>
        _repositorio.Listar(busqueda?.Trim());

    public List<VehiculoResumen> ListarResumen() => _repositorio.ListarResumen();

    public Vehiculo? ObtenerPorId(int id) => _repositorio.ObtenerPorId(id);

    public Resultado Crear(Vehiculo vehiculo)
    {
        vehiculo.Placa = NormalizarPlaca(vehiculo.Placa);

        if (!_repositorio.ExisteCliente(vehiculo.ClienteId))
        {
            _logger.LogWarning("Registro de vehículo rechazado: cliente {ClienteId} inexistente", vehiculo.ClienteId);
            return Resultado.Error("Seleccione un cliente registrado.");
        }

        if (_repositorio.PlacaEnUso(vehiculo.Placa))
        {
            _logger.LogWarning("Registro de vehículo rechazado: placa {Placa} duplicada", vehiculo.Placa);
            return Resultado.Error("Ya existe un vehículo registrado con esa placa.");
        }

        try
        {
            _repositorio.Agregar(vehiculo);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Vehículo {Id} registrado con placa {Placa}", vehiculo.Id, vehiculo.Placa);
            return Resultado.Ok("Vehículo registrado correctamente.");
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning("No se pudo guardar el vehículo con placa {Placa}", vehiculo.Placa);
            return Resultado.Error("No se pudo guardar el vehículo. Revise que la placa no esté registrada e intente de nuevo.");
        }
    }

    public Resultado Actualizar(Vehiculo vehiculo)
    {
        if (_repositorio.ObtenerPorId(vehiculo.Id) is null)
        {
            _logger.LogWarning("Actualización de vehículo rechazada: no existe el id {VehiculoId}", vehiculo.Id);
            return Resultado.Error("El vehículo no existe.");
        }

        vehiculo.Placa = NormalizarPlaca(vehiculo.Placa);

        if (!_repositorio.ExisteCliente(vehiculo.ClienteId))
        {
            _logger.LogWarning("Actualización de vehículo {VehiculoId} rechazada: cliente {ClienteId} inexistente", vehiculo.Id, vehiculo.ClienteId);
            return Resultado.Error("Seleccione un cliente registrado.");
        }

        if (_repositorio.PlacaEnUso(vehiculo.Placa, vehiculo.Id))
        {
            _logger.LogWarning("Actualización de vehículo {VehiculoId} rechazada: placa {Placa} duplicada", vehiculo.Id, vehiculo.Placa);
            return Resultado.Error("Ya existe otro vehículo registrado con esa placa.");
        }

        try
        {
            _repositorio.Actualizar(vehiculo);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Vehículo {Id} actualizado con placa {Placa}", vehiculo.Id, vehiculo.Placa);
            return Resultado.Ok("Vehículo actualizado correctamente.");
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning("No se pudo actualizar el vehículo {VehiculoId}", vehiculo.Id);
            return Resultado.Error("No se pudo actualizar el vehículo. Revise que la placa no esté registrada e intente de nuevo.");
        }
    }

    public Resultado Eliminar(int id)
    {
        var vehiculo = _repositorio.ObtenerPorId(id);
        if (vehiculo is null)
        {
            _logger.LogWarning("Eliminación de vehículo rechazada: no existe el id {VehiculoId}", id);
            return Resultado.Error("El vehículo no existe.");
        }

        if (vehiculo.Ordenes.Any())
        {
            _logger.LogWarning("Eliminación del vehículo {VehiculoId} rechazada: tiene órdenes asociadas", id);
            return Resultado.Error("No se puede eliminar un vehículo que ya tiene historial de órdenes de trabajo.");
        }

        try
        {
            _repositorio.Eliminar(id);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Vehículo {Id} eliminado", id);
            return Resultado.Ok("Vehículo eliminado.");
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning("No se pudo eliminar el vehículo {VehiculoId}", id);
            return Resultado.Error("No se pudo eliminar el vehículo. Verifique que no tenga historial asociado e intente de nuevo.");
        }
    }

    private static string NormalizarPlaca(string placa) =>
        placa.Trim().ToUpperInvariant();
}
