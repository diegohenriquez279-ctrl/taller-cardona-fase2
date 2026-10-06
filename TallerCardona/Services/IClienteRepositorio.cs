using TallerCardona.Models;

namespace TallerCardona.Services;

public interface IClienteRepositorio
{
    List<Cliente> Listar(string? busqueda = null);
    Cliente? ObtenerPorId(int id);
    void Agregar(Cliente cliente);
    void Actualizar(Cliente cliente);
    void Eliminar(int id);
}