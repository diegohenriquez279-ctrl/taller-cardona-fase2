using System.ComponentModel.DataAnnotations;

namespace TallerCardona.Models.Dtos;

/// <summary>Datos de un cliente que entran y salen por la API.</summary>
/// <remarks>
/// ¿Por qué un DTO y no devolver la entidad Cliente directamente?
///  - Cliente tiene la lista Vehiculos, y cada Vehiculo apunta otra vez a su Cliente:
///    al convertir a JSON se haría un ciclo infinito.
///  - Así decidimos exactamente qué datos se muestran (aquí solo la cantidad de vehículos).
///  - Si mañana cambia la tabla, la API puede seguir igual.
///
/// Para su módulo: copien este archivo, cambien las propiedades y los dos métodos de conversión.
/// </remarks>
public class ClienteDto
{
    /// <summary>Id del cliente. Se ignora al crear (POST).</summary>
    public int Id { get; set; }

    /// <summary>Nombre completo.</summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Teléfono de 8 dígitos, con o sin guion (7777-8888).</summary>
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "Escriba un teléfono válido de 8 dígitos, por ejemplo 7777-8888.")]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Correo electrónico (opcional).</summary>
    [EmailAddress(ErrorMessage = "Escriba un correo válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede pasar de 100 caracteres.")]
    public string? Correo { get; set; }

    /// <summary>Dirección (opcional).</summary>
    [StringLength(200, ErrorMessage = "La dirección no puede pasar de 200 caracteres.")]
    public string? Direccion { get; set; }

    /// <summary>Cantidad de vehículos registrados. Solo lectura: se ignora al crear o editar.</summary>
    public int CantidadVehiculos { get; set; }

    // ---------- Conversiones ----------

    /// <summary>Entidad -> DTO (para responder).</summary>
    public static ClienteDto DesdeEntidad(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nombre = cliente.Nombre,
        Telefono = cliente.Telefono,
        Correo = cliente.Correo,
        Direccion = cliente.Direccion,
        CantidadVehiculos = cliente.Vehiculos.Count
    };

    /// <summary>DTO -> entidad (para crear o actualizar). No copia CantidadVehiculos.</summary>
    public Cliente AEntidad(int id = 0) => new()
    {
        Id = id,
        Nombre = Nombre.Trim(),
        Telefono = Telefono.Trim(),
        Correo = string.IsNullOrWhiteSpace(Correo) ? null : Correo.Trim(),
        Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim()
    };
}
