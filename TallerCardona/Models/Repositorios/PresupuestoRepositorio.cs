
using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;

namespace TallerCardona.Repositorios;

// Repositorio de presupuestos conectado a SQL Server mediante Entity Framework.
public class PresupuestoRepositorio : IPresupuestoRepositorio
{
    private readonly AppDbContext _context;

    public PresupuestoRepositorio(AppDbContext context)
    {
        _context = context;
    }

    // Lista los presupuestos con los datos de sus órdenes.
    public List<Presupuesto> Listar() =>
        _context.Presupuestos
            .Include(p => p.OrdenTrabajo)
                .ThenInclude(o => o!.Vehiculo)
                    .ThenInclude(v => v!.Cliente)
            .OrderByDescending(p => p.Fecha)
            .ToList();

    // Busca un presupuesto por su identificador.
    public Presupuesto? ObtenerPorId(int id) =>
        _context.Presupuestos
            .Include(p => p.OrdenTrabajo)
                .ThenInclude(o => o!.Vehiculo)
                    .ThenInclude(v => v!.Cliente)
            .FirstOrDefault(p => p.Id == id);

    // Agrega un presupuesto para guardarlo mediante la unidad de trabajo.
    public void Agregar(Presupuesto presupuesto)
    {
        _context.Presupuestos.Add(presupuesto);
    }

    // Actualiza los datos de un presupuesto existente.
    public void Actualizar(Presupuesto presupuesto)
    {
        var existente = _context.Presupuestos.Find(presupuesto.Id);

        if (existente is null)
            return;

        _context.Entry(existente).CurrentValues.SetValues(presupuesto);
    }
}
