using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Services;

public interface IClienteService
{
    List<Cliente> Listar(string? busqueda = null);
    Cliente? ObtenerPorId(int id);
    List<ClienteResumen> ListarResumen();
    Resultado Crear(Cliente cliente);
    Resultado Actualizar(Cliente cliente);
    Resultado Eliminar(int id);
}
