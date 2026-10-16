using TallerCardona.Models;

namespace TallerCardona.Services;

public class VehiculoRepositorioMemoria : IVehiculoRepositorio
{
    private readonly IClienteRepositorio _clientes;
    private readonly List<Vehiculo> _vehiculos;
    private int _siguienteId = 4;

    public VehiculoRepositorioMemoria(IClienteRepositorio clientes)
    {
        _clientes = clientes;
        _vehiculos = new List<Vehiculo>
        {
            new() { Id = 1, Placa = "P123-456", Marca = "Toyota", Modelo = "Corolla", Anio = 2018, Color = "Gris", Kilometraje = 68500, ClienteId = 1 },
            new() { Id = 2, Placa = "P789-012", Marca = "Honda", Modelo = "Civic", Anio = 2020, Color = "Azul", Kilometraje = 42300, ClienteId = 2 },
            new() { Id = 3, Placa = "P345-678", Marca = "Nissan", Modelo = "Versa", Anio = 2017, Color = "Blanco", Kilometraje = 91200, ClienteId = 3 }
        };

        foreach (var vehiculo in _vehiculos)
        {
            vehiculo.Cliente = _clientes.ObtenerPorId(vehiculo.ClienteId);
            vehiculo.Cliente?.Vehiculos.Add(vehiculo);
        }
    }

    public List<Vehiculo> Listar(string? busqueda = null)
    {
        var resultado = _vehiculos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            resultado = resultado.Where(v =>
                v.Placa.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                v.Marca.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                v.Modelo.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                (v.Cliente?.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return resultado.OrderBy(v => v.Placa).ToList();
    }

    public Vehiculo? ObtenerPorId(int id) => _vehiculos.FirstOrDefault(v => v.Id == id);

    public bool PlacaEnUso(string placa, int? excluirId = null) => _vehiculos.Any(v =>
        v.Id != excluirId && string.Equals(v.Placa, placa.Trim(), StringComparison.OrdinalIgnoreCase));

    public void Agregar(Vehiculo vehiculo)
    {
        vehiculo.Id = _siguienteId++;
        vehiculo.Placa = vehiculo.Placa.Trim().ToUpperInvariant();
        vehiculo.Cliente = _clientes.ObtenerPorId(vehiculo.ClienteId);
        _vehiculos.Add(vehiculo);
        vehiculo.Cliente?.Vehiculos.Add(vehiculo);
    }

    public void Actualizar(Vehiculo vehiculo)
    {
        var existente = ObtenerPorId(vehiculo.Id);
        if (existente is null) return;

        existente.Cliente?.Vehiculos.Remove(existente);
        existente.Placa = vehiculo.Placa.Trim().ToUpperInvariant();
        existente.Marca = vehiculo.Marca;
        existente.Modelo = vehiculo.Modelo;
        existente.Anio = vehiculo.Anio;
        existente.Color = vehiculo.Color;
        existente.Kilometraje = vehiculo.Kilometraje;
        existente.ClienteId = vehiculo.ClienteId;
        existente.Cliente = _clientes.ObtenerPorId(vehiculo.ClienteId);
        existente.Cliente?.Vehiculos.Add(existente);
    }

    public void Eliminar(int id)
    {
        var vehiculo = ObtenerPorId(id);
        if (vehiculo is null) return;

        vehiculo.Cliente?.Vehiculos.Remove(vehiculo);
        _vehiculos.Remove(vehiculo);
    }
}
