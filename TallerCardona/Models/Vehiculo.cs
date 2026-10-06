namespace TallerCardona.Models;

public class Vehiculo
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string? Color { get; set; }
    public decimal Kilometraje { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public List<OrdenTrabajo> Ordenes { get; set; } = new();
}