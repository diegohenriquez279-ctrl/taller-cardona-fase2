using TallerCardona.Models;
using TallerCardona.Models.Reportes;

namespace TallerCardona.Repositorios;

public interface IClienteRepositorio
{
    List<Cliente> Listar(string? busqueda = null);
    Cliente? ObtenerPorId(int id);
    bool TelefonoEnUso(string telefono, int? excluirId = null);
    List<ClienteResumen> ListarResumen();
    void Agregar(Cliente cliente);
    void Actualizar(Cliente cliente);
    void Eliminar(int id);
}
