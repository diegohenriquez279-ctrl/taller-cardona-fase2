namespace TallerCardona.Models;

public class Presupuesto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal TotalEstimado { get; set; }
    public EstadoPresupuesto Estado { get; set; } = EstadoPresupuesto.Pendiente;
    public string? Observaciones { get; set; }

    public int OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }
}