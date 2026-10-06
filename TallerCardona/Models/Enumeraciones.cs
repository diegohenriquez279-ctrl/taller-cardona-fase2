namespace TallerCardona.Models;

public enum EstadoOrden
{
    Recibido,
    EnDiagnostico,
    EnReparacion,
    ListoParaEntrega,
    Entregado
}

public enum EstadoPresupuesto
{
    Pendiente,
    Autorizado,
    Rechazado
}

public enum MetodoPago
{
    Efectivo,
    Tarjeta,
    Transferencia
}