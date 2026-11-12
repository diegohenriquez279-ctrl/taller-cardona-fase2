namespace TallerCardona.Models.Reportes;

// Resultado de la consulta LINQ "Resumen de clientes":
// no es una tabla, solo guarda lo que devuelve la consulta.
public class ClienteResumen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int CantidadVehiculos { get; set; }
    public int CantidadOrdenes { get; set; }
}
