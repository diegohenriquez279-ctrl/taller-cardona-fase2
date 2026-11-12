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

    public ClienteService(IClienteRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
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
            return Resultado.Error("Ya existe un cliente registrado con ese teléfono.");

        try
        {
            _repositorio.Agregar(cliente);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Cliente registrado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo guardar el cliente. Intente de nuevo.");
        }
    }

    public Resultado Actualizar(Cliente cliente)
    {
        cliente.Telefono = NormalizarTelefono(cliente.Telefono);

        if (_repositorio.TelefonoEnUso(cliente.Telefono, cliente.Id))
            return Resultado.Error("Ya existe otro cliente registrado con ese teléfono.");

        try
        {
            _repositorio.Actualizar(cliente);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Cliente actualizado correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo actualizar el cliente. Intente de nuevo.");
        }
    }

    public Resultado Eliminar(int id)
    {
        var cliente = _repositorio.ObtenerPorId(id);
        if (cliente is null)
            return Resultado.Error("El cliente no existe.");

        // Regla: no se elimina un cliente que tiene vehículos registrados
        if (cliente.Vehiculos.Any())
            return Resultado.Error("No se puede eliminar: el cliente tiene vehículos registrados.");

        try
        {
            _repositorio.Eliminar(id);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Cliente eliminado.");
        }
        catch (DbUpdateException)
        {
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
