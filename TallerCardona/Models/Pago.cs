namespace TallerCardona.Models;

public class Pago
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Monto { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public string Estado { get; set; } = "Registrado";

    public int OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }
}