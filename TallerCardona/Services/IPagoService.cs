using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IPagoService
{
    List<Pago> Listar();
    Pago? ObtenerPorId(int id);
    void Registrar(Pago pago);
    void Eliminar(int id);
}
