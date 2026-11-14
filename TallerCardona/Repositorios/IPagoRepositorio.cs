using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public interface IPagoRepositorio
{
    List<Pago> Listar();
    Pago? ObtenerPorId(int id);
    void Agregar(Pago pago);
    void Eliminar(int id);
}
