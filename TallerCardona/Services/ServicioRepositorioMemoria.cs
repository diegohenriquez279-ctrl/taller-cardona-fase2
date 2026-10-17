
using TallerCardona.Models;

namespace TallerCardona.Services;

public class ServicioRepositorioMemoria
{
    private readonly List<Servicio> _servicios = new()
    {
        new Servicio
        {
            Id = 1,
            Nombre = "Cambio de aceite",
            Descripcion = "Cambio de aceite del motor",
            PrecioBase = 25.00m,
            Activo = true
        },
        new Servicio
        {
            Id = 2,
            Nombre = "Alineación y balanceo",
            Descripcion = "Alineación de dirección y balanceo de ruedas",
            PrecioBase = 30.00m,
            Activo = true
        },
        new Servicio
        {
            Id = 3,
            Nombre = "Revisión de frenos",
            Descripcion = "Inspección del sistema de frenos",
            PrecioBase = 20.00m,
            Activo = true
        }
    };

    public List<Servicio> Listar()
    {
        return _servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Nombre)
            .ToList();
    }

    public Servicio? ObtenerPorId(int id)
    {
        return _servicios.FirstOrDefault(s => s.Id == id);
    }
}
