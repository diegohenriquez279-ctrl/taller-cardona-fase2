using System.ComponentModel.DataAnnotations;
using TallerCardona.Models;

namespace TallerCardona.Models.Dtos;

/// <summary>Pago tal como lo devuelve la API.</summary>
public class PagoDto
{
    public int Id { get; set; }
    public int OrdenTrabajoId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }

    /// <summary>Efectivo, Tarjeta o Transferencia.</summary>
    public MetodoPago MetodoPago { get; set; }

    public string Estado { get; set; } = string.Empty;

    public static PagoDto DesdeEntidad(Pago pago) => new()
    {
        Id = pago.Id,
        OrdenTrabajoId = pago.OrdenTrabajoId,
        Fecha = pago.Fecha,
        Monto = pago.Monto,
        MetodoPago = pago.MetodoPago,
        Estado = pago.Estado
    };
}

/// <summary>Datos para registrar un pago. La fecha y el estado los pone el servidor.</summary>
public class PagoCrearDto
{
    /// <summary>Orden de trabajo que se paga (debe tener presupuesto Autorizado).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Indique la orden de trabajo.")]
    public int OrdenTrabajoId { get; set; }

    /// <summary>Monto en USD; mayor que cero y sin superar el saldo pendiente.</summary>
    [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }

    /// <summary>Efectivo, Tarjeta o Transferencia.</summary>
    [EnumDataType(typeof(MetodoPago), ErrorMessage = "Método de pago inválido: use Efectivo, Tarjeta o Transferencia.")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    public Pago AEntidad() => new()
    {
        OrdenTrabajoId = OrdenTrabajoId,
        Monto = Monto,
        MetodoPago = MetodoPago
    };
}

/// <summary>Pagos de un presupuesto con el total pagado y el saldo pendiente.</summary>
public class PagosDePresupuestoDto
{
    public int PresupuestoId { get; set; }
    public int OrdenTrabajoId { get; set; }

    /// <summary>Pendiente, Autorizado o Rechazado.</summary>
    public EstadoPresupuesto EstadoPresupuesto { get; set; }

    public decimal TotalEstimado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal Saldo { get; set; }
    public List<PagoDto> Pagos { get; set; } = new();
}
