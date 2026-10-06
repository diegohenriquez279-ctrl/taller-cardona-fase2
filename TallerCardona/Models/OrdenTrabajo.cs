using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TallerCardona.Models;

public class OrdenTrabajo
{
    public int Id { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public DateTime? FechaEntrega { get; set; }
    public string ProblemaReportado { get; set; } = string.Empty;
    public string? Diagnostico { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Recibido;
    public decimal KilometrajeIngreso { get; set; }
    public string? Observaciones { get; set; }

    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    public List<DetalleServicio> Servicios { get; set; } = new();
    public List<DetalleRepuesto> Repuestos { get; set; } = new();
    public Presupuesto? Presupuesto { get; set; }
    public List<Pago> Pagos { get; set; } = new();
}