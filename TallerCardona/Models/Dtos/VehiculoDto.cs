using System.ComponentModel.DataAnnotations;
using TallerCardona.Models;

namespace TallerCardona.Models.Dtos;

/// <summary>Datos permitidos para consultar y mantener vehículos mediante la API.</summary>
public class VehiculoDto
{
    /// <summary>Identificador; se ignora al crear y debe coincidir con la ruta al editar.</summary>
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

    /// <summary>Nombre del cliente propietario; solo se devuelve en las respuestas.</summary>
    public string? ClienteNombre { get; set; }

    public static VehiculoDto DesdeEntidad(Vehiculo vehiculo) => new()
    {
        Id = vehiculo.Id,
        Placa = vehiculo.Placa,
        Marca = vehiculo.Marca,
        Modelo = vehiculo.Modelo,
        Anio = vehiculo.Anio,
        Color = vehiculo.Color,
        Kilometraje = vehiculo.Kilometraje,
        ClienteId = vehiculo.ClienteId,
        ClienteNombre = vehiculo.Cliente?.Nombre
    };

    public Vehiculo AEntidad(int id = 0) => new()
    {
        Id = id,
        Placa = Placa.Trim(),
        Marca = Marca.Trim(),
        Modelo = Modelo.Trim(),
        Anio = Anio,
        Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim(),
        Kilometraje = Kilometraje,
        ClienteId = ClienteId
    };
}
