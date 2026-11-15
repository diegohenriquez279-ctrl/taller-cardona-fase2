namespace TallerCardona.Models.Reportes;

// Proyección para el reporte de vehículos y sus propietarios.
// No corresponde a una tabla de la base de datos.
public class VehiculoResumen
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
}
