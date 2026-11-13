namespace TallerCardona.Models.Reportes;

// Resultado de la consulta LINQ del tablero: cuántas órdenes hay en cada estado.
public class ConteoPorEstado
{
    public EstadoOrden Estado { get; set; }
    public int Cantidad { get; set; }
}
