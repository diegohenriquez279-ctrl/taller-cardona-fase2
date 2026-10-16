using System.ComponentModel.DataAnnotations;

namespace TallerCardona.Models;

public class OrdenTrabajo
{
    public int Id { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public DateTime? FechaEntrega { get; set; }
    [Required(ErrorMessage = "Describa el problema reportado.")]
    [StringLength(500, ErrorMessage = "El problema no puede pasar de 500 caracteres.")]
    public string ProblemaReportado { get; set; } = string.Empty;
    [StringLength(500, ErrorMessage = "El diagnóstico no puede pasar de 500 caracteres.")]
    public string? Diagnostico { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Recibido;
    [Range(0, 2000000, ErrorMessage = "Escriba un kilometraje válido.")]
    public decimal KilometrajeIngreso { get; set; }
    [StringLength(500, ErrorMessage = "Las observaciones no pueden pasar de 500 caracteres.")]
    public string? Observaciones { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un vehículo.")]
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    public List<DetalleServicio> Servicios { get; set; } = new();
    public List<DetalleRepuesto> Repuestos { get; set; } = new();
    public Presupuesto? Presupuesto { get; set; }
    public List<Pago> Pagos { get; set; } = new();
}