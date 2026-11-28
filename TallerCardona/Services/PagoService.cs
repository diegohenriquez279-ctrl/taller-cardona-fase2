using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

public class PagoService : IPagoService
{
    private readonly IPagoRepositorio _pagos;
    private readonly AppDbContext _context;

    public PagoService(IPagoRepositorio pagos, AppDbContext context)
    {
        _pagos = pagos;
        _context = context;
    }

    public List<Pago> Listar() => _pagos.Listar();
    public Pago? ObtenerPorId(int id) => _pagos.ObtenerPorId(id);

    public void Registrar(Pago pago)
    {
        if (pago.Monto <= 0)
            throw new InvalidOperationException("El monto debe ser mayor que cero.");

        var orden = _context.OrdenesTrabajo
            .Include(o => o.Presupuesto)
            .Include(o => o.Pagos)
            .FirstOrDefault(o => o.Id == pago.OrdenTrabajoId);

        if (orden is null)
            throw new InvalidOperationException("La orden de trabajo no existe.");
        if (orden.Presupuesto is null)
            throw new InvalidOperationException("La orden no tiene presupuesto.");
        if (orden.Presupuesto.Estado != EstadoPresupuesto.Autorizado)
            throw new InvalidOperationException("El presupuesto no está autorizado.");

        decimal yaPagado = orden.Pagos.Sum(p => p.Monto);
        decimal saldo = orden.Presupuesto.TotalEstimado - yaPagado;

        if (pago.Monto > saldo)
            throw new InvalidOperationException($"El monto supera el saldo pendiente de {saldo:C}.");

        pago.Fecha = DateTime.Now;
        pago.Estado = "Registrado";

        _pagos.Agregar(pago);
        _pagos.GuardarCambios();
    }

    public void Eliminar(int id)
    {
        _pagos.Eliminar(id);
        _pagos.GuardarCambios();
    }
}
