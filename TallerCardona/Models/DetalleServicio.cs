namespace TallerCardona.Models;

public class DetalleServicio
{
    public int Id { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;

    public int OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }
    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }
}