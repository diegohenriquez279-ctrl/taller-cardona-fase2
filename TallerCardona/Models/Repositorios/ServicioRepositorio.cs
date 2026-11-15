
using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public class ServicioRepositorio : IServicioRepositorio
{
    private readonly AppDbContext _context;

    public ServicioRepositorio(AppDbContext context)
    {
        _context = context;
    }

    public List<Servicio> Listar() =>
        _context.Servicios
            .OrderBy(s => s.Nombre)
            .ToList();

    public Servicio? ObtenerPorId(int id) =>
        _context.Servicios
            .FirstOrDefault(s => s.Id == id);
}
