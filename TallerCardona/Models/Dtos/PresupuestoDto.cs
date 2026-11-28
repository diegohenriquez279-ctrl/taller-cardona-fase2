using TallerCardona.Models;

namespace TallerCardona.Models.Dtos;

/// <summary>Presupuesto tal como lo devuelve la API.</summary>
public class PresupuestoDto
{
    public int Id { get; set; }
    public int OrdenTrabajoId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal TotalEstimado { get; set; }

    /// <summary>Pendiente, Autorizado o Rechazado.</summary>
    public EstadoPresupuesto Estado { get; set; }

    public string? Observaciones { get; set; }

    /// <summary>Placa del vehículo de la orden (solo lectura).</summary>
    public string? Placa { get; set; }

    /// <summary>Cliente dueño del vehículo (solo lectura).</summary>
    public string? Cliente { get; set; }

    public static PresupuestoDto DesdeEntidad(Presupuesto p) => new()
    {
        Id = p.Id,
        OrdenTrabajoId = p.OrdenTrabajoId,
        Fecha = p.Fecha,
        TotalEstimado = p.TotalEstimado,
        Estado = p.Estado,
        Observaciones = p.Observaciones,
        Placa = p.OrdenTrabajo?.Vehiculo?.Placa,
        Cliente = p.OrdenTrabajo?.Vehiculo?.Cliente?.Nombre
    };
}
