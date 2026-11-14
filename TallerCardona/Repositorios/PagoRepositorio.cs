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
}
