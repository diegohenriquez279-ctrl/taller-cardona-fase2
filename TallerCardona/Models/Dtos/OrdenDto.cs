using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TallerCardona.Models.Dtos;

/// <summary>Datos de una orden de trabajo que entran y salen por la API.</summary>
/// <remarks>
/// En vez del objeto Vehiculo completo (que apunta a su Cliente, y el Cliente otra vez a
/// sus vehículos: ciclo infinito al convertir a JSON), la API manda solo la placa, la
/// descripción del vehículo y el nombre del cliente.
///
/// Para crear o editar se mandan los ids de los servicios y repuestos del catálogo
/// (ServicioIds, RepuestoIds); el precio lo pone el servidor desde el catálogo.
/// </remarks>
public class OrdenDto
{
    /// <summary>Id de la orden. Se ignora al crear (POST).</summary>
    public int Id { get; set; }

    /// <summary>Vehículo que ingresó al taller.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Indique el id de un vehículo registrado.")]
    public int VehiculoId { get; set; }

    /// <summary>Fecha y hora de ingreso. Si no se manda al crear, se usa la fecha actual.</summary>
    public DateTime? FechaIngreso { get; set; }

    /// <summary>Falla o trabajo que reporta el cliente.</summary>
    [Required(ErrorMessage = "Describa el problema reportado.")]
    [StringLength(500, ErrorMessage = "El problema no puede pasar de 500 caracteres.")]
    public string ProblemaReportado { get; set; } = string.Empty;

    /// <summary>Diagnóstico del mecánico (opcional).</summary>
    [StringLength(500, ErrorMessage = "El diagnóstico no puede pasar de 500 caracteres.")]
    public string? Diagnostico { get; set; }

    /// <summary>Kilometraje del vehículo al ingresar.</summary>
    [Range(0, 2000000, ErrorMessage = "Escriba un kilometraje válido.")]
    public decimal KilometrajeIngreso { get; set; }

    /// <summary>Notas adicionales (opcional).</summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden pasar de 500 caracteres.")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Estado: Recibido, EnDiagnostico, EnReparacion, ListoParaEntrega o Entregado.
    /// Al crear siempre empieza en Recibido. Al editar avanza o regresa un solo paso;
    /// si no se manda, se conserva el actual.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EstadoOrden? Estado { get; set; }

    /// <summary>Ids de los servicios del catálogo. Al editar, si no se manda, se conservan los actuales.</summary>
    public List<int>? ServicioIds { get; set; }

    /// <summary>Ids de los repuestos del catálogo. Al editar, si no se manda, se conservan los actuales.</summary>
    public List<int>? RepuestoIds { get; set; }

    // ---------- Solo lectura (se ignoran al crear o editar) ----------

    /// <summary>Fecha de entrega. La llena el servidor cuando la orden pasa a Entregado.</summary>
    public DateTime? FechaEntrega { get; set; }

    /// <summary>Placa del vehículo.</summary>
    public string? Placa { get; set; }

    /// <summary>Marca, modelo y año del vehículo.</summary>
    public string? Vehiculo { get; set; }

    /// <summary>Nombre del cliente dueño del vehículo.</summary>
    public string? Cliente { get; set; }

    /// <summary>Servicios de la orden con su precio (solo en GET por id).</summary>
    public List<LineaOrdenDto>? Servicios { get; set; }

    /// <summary>Repuestos de la orden con su precio (solo en GET por id).</summary>
    public List<LineaOrdenDto>? Repuestos { get; set; }

    /// <summary>Total de servicios y repuestos (solo en GET por id).</summary>
    public decimal? Total { get; set; }

    // ---------- Conversiones ----------

    /// <summary>Entidad -> DTO (para responder).</summary>
    /// <param name="orden">Orden con su Vehiculo y Cliente cargados (Include).</param>
    /// <param name="conDetalle">true = incluye servicios, repuestos y total (requiere que vengan cargados).</param>
    public static OrdenDto DesdeEntidad(OrdenTrabajo orden, bool conDetalle)
    {
        var dto = new OrdenDto
        {
            Id = orden.Id,
            VehiculoId = orden.VehiculoId,
            FechaIngreso = orden.FechaIngreso,
            FechaEntrega = orden.FechaEntrega,
            ProblemaReportado = orden.ProblemaReportado,
            Diagnostico = orden.Diagnostico,
            KilometrajeIngreso = orden.KilometrajeIngreso,
            Observaciones = orden.Observaciones,
            Estado = orden.Estado,
            Placa = orden.Vehiculo?.Placa,
            Vehiculo = orden.Vehiculo is null ? null : $"{orden.Vehiculo.Marca} {orden.Vehiculo.Modelo} {orden.Vehiculo.Anio}",
            Cliente = orden.Vehiculo?.Cliente?.Nombre
        };

        if (conDetalle)
        {
            dto.ServicioIds = orden.Servicios.Select(d => d.ServicioId).ToList();
            dto.RepuestoIds = orden.Repuestos.Select(d => d.RepuestoId).ToList();
            dto.Servicios = orden.Servicios
                .Select(d => new LineaOrdenDto(d.ServicioId, d.Servicio?.Nombre ?? "Servicio", d.Cantidad, d.PrecioUnitario))
                .ToList();
            dto.Repuestos = orden.Repuestos
                .Select(d => new LineaOrdenDto(d.RepuestoId, d.Repuesto?.Nombre ?? "Repuesto", d.Cantidad, d.PrecioUnitario))
                .ToList();
            dto.Total = orden.Servicios.Sum(d => d.Subtotal) + orden.Repuestos.Sum(d => d.Subtotal);
        }

        return dto;
    }

    /// <summary>DTO -> entidad (para crear o actualizar). No copia los campos de solo lectura.</summary>
    /// <param name="id">0 al crear; el id de la ruta al editar.</param>
    /// <param name="actual">La orden guardada (al editar), para conservar lo que no se mande.</param>
    public OrdenTrabajo AEntidad(int id = 0, OrdenTrabajo? actual = null) => new()
    {
        Id = id,
        VehiculoId = VehiculoId,
        FechaIngreso = FechaIngreso ?? actual?.FechaIngreso ?? DateTime.Now,
        ProblemaReportado = ProblemaReportado.Trim(),
        Diagnostico = string.IsNullOrWhiteSpace(Diagnostico) ? null : Diagnostico.Trim(),
        KilometrajeIngreso = KilometrajeIngreso,
        Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones.Trim(),
        Estado = Estado ?? actual?.Estado ?? EstadoOrden.Recibido
    };
}

/// <summary>Una línea de servicio o repuesto dentro de una orden.</summary>
/// <param name="Id">Id del servicio o repuesto en el catálogo.</param>
/// <param name="Nombre">Nombre en el catálogo.</param>
/// <param name="Cantidad">Cantidad aplicada.</param>
/// <param name="PrecioUnitario">Precio guardado al registrarlo.</param>
public record LineaOrdenDto(int Id, string Nombre, int Cantidad, decimal PrecioUnitario)
{
    /// <summary>Cantidad por precio.</summary>
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
