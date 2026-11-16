using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Repositorios;

// Acceso a Vehículos con Entity Framework Core.
// El repositorio consulta y prepara cambios; el guardado corresponde a la unidad de trabajo.
public class VehiculoRepositorio : IVehiculoRepositorio
{
    private readonly AppDbContext _context;

    public VehiculoRepositorio(AppDbContext context)
    {
        _context = context;
    }

    // Búsqueda por placa, marca, modelo o nombre del cliente; orden alfabético por placa.
    public List<Vehiculo> Listar(string? busqueda = null)
    {
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(v => v.Cliente)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            consulta = consulta.Where(v =>
                v.Placa.Contains(texto) ||
                v.Marca.Contains(texto) ||
                v.Modelo.Contains(texto) ||
                v.Cliente!.Nombre.Contains(texto));
        }

        return consulta.OrderBy(v => v.Placa).ToList();
    }

    // Consulta LINQ Join: une Vehiculos.ClienteId con Clientes.Id y proyecta
    // solo los campos que necesita el reporte de propietarios.
    public List<VehiculoResumen> ListarResumen() =>
        _context.Vehiculos
            .AsNoTracking()
            .Join(
                _context.Clientes.AsNoTracking(),
                vehiculo => vehiculo.ClienteId,
                cliente => cliente.Id,
                (vehiculo, cliente) => new VehiculoResumen
                {
                    Id = vehiculo.Id,
                    Placa = vehiculo.Placa,
                    Marca = vehiculo.Marca,
                    Modelo = vehiculo.Modelo,
                    Anio = vehiculo.Anio,
                    ClienteNombre = cliente.Nombre
                })
            .OrderBy(resultado => resultado.Placa)
            .ToList();

    // Incluye las órdenes porque el servicio necesita bloquear el borrado
    // cuando el vehículo ya forma parte del historial del taller.
    public Vehiculo? ObtenerPorId(int id) =>
        _context.Vehiculos
            .Include(v => v.Cliente)
            .Include(v => v.Ordenes)
            .FirstOrDefault(v => v.Id == id);

    public bool PlacaEnUso(string placa, int? excluirId = null)
    {
        var placaNormalizada = placa.Trim().ToUpperInvariant();
        return _context.Vehiculos.Any(v =>
            v.Placa == placaNormalizada &&
            (excluirId == null || v.Id != excluirId.Value));
    }

    public bool ExisteCliente(int clienteId) =>
        _context.Clientes.Any(c => c.Id == clienteId);

    public void Agregar(Vehiculo vehiculo) => _context.Vehiculos.Add(vehiculo);

    public void Actualizar(Vehiculo vehiculo)
    {
        var existente = _context.Vehiculos.Find(vehiculo.Id);
        if (existente is null) return;

        _context.Entry(existente).CurrentValues.SetValues(vehiculo);
    }

    public void Eliminar(int id)
    {
        var existente = _context.Vehiculos.Find(id);
        if (existente is not null)
            _context.Vehiculos.Remove(existente);
    }
}
