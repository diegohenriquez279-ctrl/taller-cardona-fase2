using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

// Lógica de negocio de clientes: valida las reglas, usa el repositorio
// para preparar los cambios y la unidad de trabajo para guardarlos.
public class ClienteService : IClienteService
{
    private readonly IClienteRepositorio _repositorio;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly ILogger<ClienteService> _logger;

    // ILogger: se pide en el constructor y ASP.NET lo entrega (va a consola y a Logs/).
    // Usen {Nombre} en el mensaje en vez de concatenar: así el dato queda separado en el log.
    public ClienteService(IClienteRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo,
        ILogger<ClienteService> logger)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
        _logger = logger;
    }

    public List<Cliente> Listar(string? busqueda = null) =>
        _repositorio.Listar(busqueda?.Trim());

    public Cliente? ObtenerPorId(int id) => _repositorio.ObtenerPorId(id);

    public List<ClienteResumen> ListarResumen() => _repositorio.ListarResumen();

    public Resultado Crear(Cliente cliente)
    {
        cliente.Telefono = NormalizarTelefono(cliente.Telefono);

        // Regla: no puede haber dos clientes con el mismo teléfono
        if (_repositorio.TelefonoEnUso(cliente.Telefono))
        {
            _logger.LogWarning("Crear cliente rechazado: teléfono {Telefono} ya registrado", cliente.Telefono);
            return Resultado.Error("Ya existe un cliente registrado con ese teléfono.");
        }

        try
        {
            _repositorio.Agregar(cliente);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Cliente creado: Id {Id}, {Nombre}", cliente.Id, cliente.Nombre);
            return Resultado.Ok("Cliente registrado correctamente.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear el cliente {Nombre}", cliente.Nombre);
            return Resultado.Error("No se pudo guardar el cliente. Intente de nuevo.");
        }
    }

    public Resultado Actualizar(Cliente cliente)
    {
        cliente.Telefono = NormalizarTelefono(cliente.Telefono);

        if (_repositorio.TelefonoEnUso(cliente.Telefono, cliente.Id))
        {
            _logger.LogWarning("Actualizar cliente {Id} rechazado: teléfono {Telefono} ya registrado",
                cliente.Id, cliente.Telefono);
            return Resultado.Error("Ya existe otro cliente registrado con ese teléfono.");
        }

        try
        {
            _repositorio.Actualizar(cliente);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Cliente actualizado: Id {Id}, {Nombre}", cliente.Id, cliente.Nombre);
            return Resultado.Ok("Cliente actualizado correctamente.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar el cliente {Id}", cliente.Id);
            return Resultado.Error("No se pudo actualizar el cliente. Intente de nuevo.");
        }
    }

    public Resultado Eliminar(int id)
    {
        var cliente = _repositorio.ObtenerPorId(id);
        if (cliente is null)
        {
            _logger.LogWarning("Eliminar cliente rechazado: el cliente {Id} no existe", id);
            return Resultado.Error("El cliente no existe.");
        }

        // Regla: no se elimina un cliente que tiene vehículos registrados
        if (cliente.Vehiculos.Any())
        {
            _logger.LogWarning("Eliminar cliente {Id} rechazado: tiene {Cantidad} vehículo(s)",
                id, cliente.Vehiculos.Count);
            return Resultado.Error("No se puede eliminar: el cliente tiene vehículos registrados.");
        }

        try
        {
            _repositorio.Eliminar(id);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Cliente eliminado: Id {Id}, {Nombre}", id, cliente.Nombre);
            return Resultado.Ok("Cliente eliminado.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al eliminar el cliente {Id}", id);
            return Resultado.Error("No se pudo eliminar el cliente. Intente de nuevo.");
        }
    }

    // Guarda el teléfono siempre con el mismo formato: 7777-8888
    private static string NormalizarTelefono(string telefono)
    {
        var digitos = new string(telefono.Where(char.IsDigit).ToArray());
        return digitos.Length == 8 ? $"{digitos[..4]}-{digitos[4..]}" : telefono.Trim();
    }
}
