namespace TallerCardona.Models;

public static class EstadoOrdenExtensiones
{
    public static string Nombre(this EstadoOrden estado) => estado switch
    {
        EstadoOrden.Recibido => "Recibido",
        EstadoOrden.EnDiagnostico => "En diagnóstico",
        EstadoOrden.EnReparacion => "En reparación",
        EstadoOrden.ListoParaEntrega => "Listo para entrega",
        EstadoOrden.Entregado => "Entregado",
        _ => estado.ToString()
    };

    // Clase de Bootstrap para el distintivo (badge) de cada estado.
    public static string ClaseBadge(this EstadoOrden estado) => estado switch
    {
        EstadoOrden.Recibido => "text-bg-secondary",
        EstadoOrden.EnDiagnostico => "text-bg-info",
        EstadoOrden.EnReparacion => "text-bg-warning",
        EstadoOrden.ListoParaEntrega => "text-bg-primary",
        EstadoOrden.Entregado => "text-bg-success",
        _ => "text-bg-light"
    };
}