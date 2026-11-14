using TallerCardona.Models;

namespace TallerCardona.Repositorios;

public class PagoRepositorioMemoria : IPagoRepositorio
{
    private readonly List<Pago> _pagos = new();
    private int _siguienteId = 1;

    public List<Pago> Listar() => _pagos;

    public Pago? ObtenerPorId(int id) =>
        _pagos.FirstOrDefault(p => p.Id == id);

    public void Agregar(Pago pago)
    {
        pago.Id = _siguienteId++;
        _pagos.Add(pago);
    }

    public void Eliminar(int id)
    {
        var pago = ObtenerPorId(id);
        if (pago is not null)
            _pagos.Remove(pago);
    }
}
