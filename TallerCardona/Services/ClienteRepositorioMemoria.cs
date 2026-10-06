using TallerCardona.Models;

namespace TallerCardona.Services;

public class ClienteRepositorioMemoria : IClienteRepositorio
{
    private readonly List<Cliente> _clientes = new()
    {
        new Cliente { Id = 1, Nombre = "Carlos Martínez", Telefono = "7712-3456", Correo = "carlos.martinez@correo.com", Direccion = "Colonia Escalón, La Libertad" },
        new Cliente { Id = 2, Nombre = "María Hernández", Telefono = "7845-9012", Direccion = "Cantón Melara, La Libertad" },
        new Cliente { Id = 3, Nombre = "José Ramírez", Telefono = "6098-7654", Correo = "jose.ramirez@correo.com" }
    };

    private int _siguienteId = 4;

    public List<Cliente> Listar(string? busqueda = null)
    {
        var resultado = _clientes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            resultado = resultado.Where(c =>
                c.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                c.Telefono.Contains(busqueda));
        }

        return resultado.OrderBy(c => c.Nombre).ToList();
    }

    public Cliente? ObtenerPorId(int id) => _clientes.FirstOrDefault(c => c.Id == id);

    public void Agregar(Cliente cliente)
    {
        cliente.Id = _siguienteId++;
        _clientes.Add(cliente);
    }

    public void Actualizar(Cliente cliente)
    {
        var existente = ObtenerPorId(cliente.Id);
        if (existente is null) return;

        existente.Nombre = cliente.Nombre;
        existente.Telefono = cliente.Telefono;
        existente.Correo = cliente.Correo;
        existente.Direccion = cliente.Direccion;
    }

    public void Eliminar(int id) => _clientes.RemoveAll(c => c.Id == id);
}