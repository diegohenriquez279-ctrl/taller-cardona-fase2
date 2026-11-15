
using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public class RepuestoRepositorio : IRepuestoRepositorio
{
    private readonly AppDbContext _context;

    public RepuestoRepositorio(AppDbContext context)
    {
        _context = context;
    }

    public List<Repuesto> Listar() =>
        _context.Repuestos
            .OrderBy(r => r.Nombre)
            .ToList();

    public Repuesto? ObtenerPorId(int id) =>
        _context.Repuestos
            .FirstOrDefault(r => r.Id == id);
}
