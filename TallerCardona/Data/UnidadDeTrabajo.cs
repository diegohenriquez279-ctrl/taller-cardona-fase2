namespace TallerCardona.Data;

public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly AppDbContext _context;

    public UnidadDeTrabajo(AppDbContext context)
    {
        _context = context;
    }

    // Devuelve la cantidad de registros afectados.
    public int Guardar() => _context.SaveChanges();
}
