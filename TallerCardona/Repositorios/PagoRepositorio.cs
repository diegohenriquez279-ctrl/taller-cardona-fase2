using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;

namespace TallerCardona.Repositorios;

// Repositorio de pagos con Entity Framework Core.
public class PagoRepositorio : IPagoRepositorio
{
    private readonly AppDbContext _context;

    public PagoRepositorio(AppDbContext context)
    {
        _context = context;
    }

    public List<Pago> Listar() =>
        _context.Pagos.OrderBy(p => p.Id).ToList();

    public Pago? ObtenerPorId(int id) =>
        _context.Pagos.FirstOrDefault(p => p.Id == id);

    public void Agregar(Pago pago) => _context.Pagos.Add(pago);

    public void Eliminar(int id)
    {
        var pago = _context.Pagos.Find(id);
        if (pago is not null) _context.Pagos.Remove(pago);
    }
    public void GuardarCambios() => _context.SaveChanges();

    // ---------- Versiones asíncronas ----------

    public Task<List<Pago>> ObtenerTodosAsync() =>
        _context.Pagos.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

    public async Task<Pago?> ObtenerPorIdAsync(int id) =>
        await _context.Pagos.FindAsync(id);

    public Task<List<Pago>> ObtenerPorOrdenAsync(int ordenTrabajoId) =>
        _context.Pagos.AsNoTracking()
            .Where(p => p.OrdenTrabajoId == ordenTrabajoId)
            .OrderBy(p => p.Fecha)
            .ToListAsync();

    public async Task AgregarAsync(Pago pago) => await _context.Pagos.AddAsync(pago);

    public async Task<bool> EliminarAsync(int id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        if (pago is null) return false;
        _context.Pagos.Remove(pago);
        return true;
    }

    public Task GuardarCambiosAsync() => _context.SaveChangesAsync();
}
