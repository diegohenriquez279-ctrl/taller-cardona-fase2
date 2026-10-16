namespace TallerCardona.Models;

public class Pago
{
    public int Id { get; set; }
    public strimg Cliente { get; set; } = "";
    public strimg Servicio {get; set; } = "";
    public decimal Monto {get; set;}
    public string Estado {get; set;} = "Pagado"
    public DateTime Fecha {get; set;} = DateTime.Now    
    public string MetodoPago {get; set;}= "Efectivo"    
    }
public class PagoDashboarDVM
{
    public decimal TotalRecaudado {get; set;}= 3600;
    public decimal PagosPendientesMonto {get; set;}= 1050;
    public int PagosPendientesCount {get; set;}= 5;
    public int TotalServicios {get; set;}= 25;
    public list<pago> PagosRecientes {get; set;}= new();
