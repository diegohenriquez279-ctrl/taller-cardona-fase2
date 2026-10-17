
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
        var duplicado = _presupuestos.Any(p =>
            p.OrdenTrabajoId == presupuesto.OrdenTrabajoId);

        if (duplicado)
        {
            throw new InvalidOperationException(
                "Esta orden de trabajo ya tiene un presupuesto.");
        }

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
        existente.OrdenTrabajoId = presupuesto.OrdenTrabajoId;

        // Conserva la referencia de la orden cuando se proporciona.
        if (presupuesto.OrdenTrabajo is not null)
        {
            existente.OrdenTrabajo = presupuesto.OrdenTrabajo;
        }
    }
}
