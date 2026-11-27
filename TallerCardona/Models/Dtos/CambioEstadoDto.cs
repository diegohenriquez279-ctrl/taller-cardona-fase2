using System.Text.Json.Serialization;

namespace TallerCardona.Models.Dtos;

/// <summary>Cuerpo para cambiar solo el estado de una orden.</summary>
public class CambioEstadoDto
{
    /// <summary>Estado nuevo: Recibido, EnDiagnostico, EnReparacion, ListoParaEntrega o Entregado.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EstadoOrden Estado { get; set; }
}
