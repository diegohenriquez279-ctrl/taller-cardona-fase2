namespace TallerCardona.Models;

public class DetalleRepuesto
{
    public int Id { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;

    public int OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }
    public int RepuestoId { get; set; }
    public Repuesto? Repuesto { get; set; }
}