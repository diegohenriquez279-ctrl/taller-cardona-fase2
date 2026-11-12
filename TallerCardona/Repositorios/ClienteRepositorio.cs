using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Repositorios;

// Repositorio de clientes con Entity Framework Core.
// Solo lee y prepara cambios; el guardado lo hace la unidad de trabajo.
public class ClienteRepositorio : IClienteRepositorio
{
    private readonly AppDbContext _context;

    public ClienteRepositorio(AppDbContext context)
    {
        _context = context;
    }

    // Consulta con filtro (Where) y ordenamiento (OrderBy)
    public List<Cliente> Listar(string? busqueda = null)
    {
        var consulta = _context.Clientes
            .Include(c => c.Vehiculos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            consulta = consulta.Where(c =>
                c.Nombre.Contains(busqueda) || c.Telefono.Contains(busqueda));
        }

        return consulta.OrderBy(c => c.Nombre).ToList();
    }

    public Cliente? ObtenerPorId(int id) =>
        _context.Clientes
            .Include(c => c.Vehiculos)
            .FirstOrDefault(c => c.Id == id);

    public bool TelefonoEnUso(string telefono, int? excluirId = null) =>
        _context.Clientes.Any(c =>
            c.Telefono == telefono && (excluirId == null || c.Id != excluirId));

    // Consulta con proyección (Select), conteo de relacionados y ordenamiento
    public List<ClienteResumen> ListarResumen() =>
        _context.Clientes
            .Select(c => new ClienteResumen
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Telefono = c.Telefono,
                CantidadVehiculos = c.Vehiculos.Count,
                CantidadOrdenes = c.Vehiculos.SelectMany(v => v.Ordenes).Count()
            })
            .OrderByDescending(r => r.CantidadOrdenes)
            .ThenBy(r => r.Nombre)
            .ToList();

    public void Agregar(Cliente cliente) => _context.Clientes.Add(cliente);

    public void Actualizar(Cliente cliente)
    {
        var existente = _context.Clientes.Find(cliente.Id);
        if (existente is null) return;

        // Copia los valores del formulario al registro que ya está en la base
        _context.Entry(existente).CurrentValues.SetValues(cliente);
    }

    public void Eliminar(int id)
    {
        var cliente = _context.Clientes.Find(id);
        if (cliente is not null) _context.Clientes.Remove(cliente);
    }
}
