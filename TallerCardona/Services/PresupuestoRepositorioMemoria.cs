
using TallerCardona.Models;

namespace TallerCardona.Services;

public class PresupuestoRepositorioMemoria : IPresupuestoRepositorio
{
    private readonly List<Presupuesto> _presupuestos = new();
    private int _siguienteId = 1;

    public List<Presupuesto> Listar()
    {
        return _presupuestos
            .OrderByDescending(p => p.Fecha)
            .ToList();
    }

    public Presupuesto? ObtenerPorId(int id)
    {
        return _presupuestos.FirstOrDefault(p => p.Id == id);
    }

    public void Agregar(Presupuesto presupuesto)
    {
        presupuesto.Id = _siguienteId++;
        presupuesto.Fecha = DateTime.Now;
        _presupuestos.Add(presupuesto);
    }

    public void Actualizar(Presupuesto presupuesto)
    {
        var existente = ObtenerPorId(presupuesto.Id);

        if (existente is null)
            return;

        existente.Estado = presupuesto.Estado;
        existente.TotalEstimado = presupuesto.TotalEstimado;
        existente.Observaciones = presupuesto.Observaciones;
    }
}
