
using TallerCardona.Models;

namespace TallerCardona.Services;

public class RepuestoRepositorioMemoria : IRepuestoRepositorio
{
    private readonly List<Repuesto> _repuestos = new()
    {
        new Repuesto
        {
            Id = 1,
            Nombre = "Filtro de aceite",
            Descripcion = "Filtro para mantenimiento del motor",
            Precio = 8.00m,
            Stock = 15
        },
        new Repuesto
        {
            Id = 2,
            Nombre = "Pastillas de freno",
            Descripcion = "Juego de pastillas para frenos",
            Precio = 22.00m,
            Stock = 10
        },
        new Repuesto
        {
            Id = 3,
            Nombre = "Bujías",
            Descripcion = "Juego de bujías para motor",
            Precio = 12.00m,
            Stock = 20
        }
    };

    public List<Repuesto> Listar()
    {
        return _repuestos
            .Where(r => r.Stock > 0)
            .OrderBy(r => r.Nombre)
            .ToList();
    }

    public Repuesto? ObtenerPorId(int id)
    {
        return _repuestos.FirstOrDefault(r => r.Id == id);
    }
}
