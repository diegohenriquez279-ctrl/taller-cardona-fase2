using System.ComponentModel.DataAnnotations;

namespace TallerCardona.Models;

public class Vehiculo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(10, MinimumLength = 5, ErrorMessage = "La placa debe tener entre 5 y 10 caracteres.")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50, ErrorMessage = "La marca no puede pasar de 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El modelo no puede pasar de 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Range(1950, 2100, ErrorMessage = "Escriba un año válido.")]
    public int Anio { get; set; }

    [StringLength(30, ErrorMessage = "El color no puede pasar de 30 caracteres.")]
    public string? Color { get; set; }

    [Range(typeof(decimal), "0", "9999999", ErrorMessage = "El kilometraje debe ser igual o mayor que cero.")]
    public decimal Kilometraje { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public List<OrdenTrabajo> Ordenes { get; set; } = new();
}
