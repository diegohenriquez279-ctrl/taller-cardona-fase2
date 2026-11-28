using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public interface IPagoRepositorio
{
    List<Pago> Listar();
    Pago? ObtenerPorId(int id);
    void Agregar(Pago pago);
    void Eliminar(int id);
    void GuardarCambios();

    // Versiones asíncronas (las usan PagoService, PagosController y la API).
    Task<List<Pago>> ObtenerTodosAsync();
    Task<Pago?> ObtenerPorIdAsync(int id);
    Task<List<Pago>> ObtenerPorOrdenAsync(int ordenTrabajoId);
    Task AgregarAsync(Pago pago);
    Task<bool> EliminarAsync(int id);
    Task GuardarCambiosAsync();
}
