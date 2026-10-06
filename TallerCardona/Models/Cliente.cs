using System.ComponentModel.DataAnnotations;

namespace TallerCardona.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "Escriba un teléfono válido de 8 dígitos, por ejemplo 7777-8888.")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Escriba un correo válido.")]
    public string? Correo { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }

    public List<Vehiculo> Vehiculos { get; set; } = new();
}