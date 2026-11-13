using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

// Lógica de negocio de las órdenes de trabajo: valida las reglas, usa el
// repositorio para preparar los cambios y la unidad de trabajo para guardarlos.
public class OrdenTrabajoService : IOrdenTrabajoService
{
    private readonly IOrdenTrabajoRepositorio _repositorio;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public OrdenTrabajoService(IOrdenTrabajoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null) =>
        _repositorio.Listar(busqueda?.Trim(), estado);

    public OrdenTrabajo? ObtenerPorId(int id) => _repositorio.ObtenerPorId(id);

    public List<Vehiculo> ListarVehiculos() => _repositorio.ListarVehiculos();

    // Regla: el estado avanza en orden
    // Recibido → En diagnóstico → En reparación → Listo para entrega → Entregado.
    // Se puede avanzar o regresar un solo paso (para corregir un error),
    // no se pueden saltar etapas y una orden entregada ya no cambia.
    public List<EstadoOrden> EstadosPermitidos(EstadoOrden estadoActual)
    {
        if (estadoActual == EstadoOrden.Entregado)
            return new List<EstadoOrden> { estadoActual };

        return Enum.GetValues<EstadoOrden>()
            .Where(e => Math.Abs((int)e - (int)estadoActual) <= 1)
            .ToList();
    }

    public Resultado Crear(OrdenTrabajo orden)
    {
        // Regla: una orden necesita un vehículo que exista
        if (!_repositorio.ExisteVehiculo(orden.VehiculoId))
            return Resultado.Error("Seleccione un vehículo registrado.");

        // Toda orden nueva empieza en "Recibido" y todavía no tiene fecha de entrega
        orden.Estado = EstadoOrden.Recibido;
        orden.FechaEntrega = null;

        try
        {
            _repositorio.Agregar(orden);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok($"Orden #{orden.Id} registrada correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo guardar la orden. Intente de nuevo.");
        }
    }

    public Resultado Actualizar(OrdenTrabajo orden)
    {
        var existente = _repositorio.ObtenerPorId(orden.Id);
        if (existente is null)
            return Resultado.Error("La orden no existe.");

        if (!_repositorio.ExisteVehiculo(orden.VehiculoId))
            return Resultado.Error("Seleccione un vehículo registrado.");

        var errorEstado = ValidarCambioDeEstado(existente.Estado, orden.Estado);
        if (errorEstado is not null)
            return Resultado.Error(errorEstado);

        orden.FechaEntrega = CalcularFechaEntrega(orden.Estado, existente.FechaEntrega);

        try
        {
            _repositorio.Actualizar(orden);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok("Orden de trabajo actualizada correctamente.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo actualizar la orden. Intente de nuevo.");
        }
    }

    public Resultado CambiarEstado(int id, EstadoOrden nuevoEstado)
    {
        var orden = _repositorio.ObtenerPorId(id);
        if (orden is null)
            return Resultado.Error("La orden no existe.");

        var errorEstado = ValidarCambioDeEstado(orden.Estado, nuevoEstado);
        if (errorEstado is not null)
            return Resultado.Error(errorEstado);

        if (orden.Estado == nuevoEstado)
            return Resultado.Ok($"La orden #{id} ya está en «{nuevoEstado.Nombre()}».");

        try
        {
            orden.Estado = nuevoEstado;
            orden.FechaEntrega = CalcularFechaEntrega(nuevoEstado, orden.FechaEntrega);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok($"La orden #{id} pasó a «{nuevoEstado.Nombre()}».");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo cambiar el estado. Intente de nuevo.");
        }
    }

    public Resultado Eliminar(int id)
    {
        var orden = _repositorio.ObtenerPorId(id);
        if (orden is null)
            return Resultado.Error("La orden no existe.");

        // Regla: no se elimina una orden que ya tiene presupuesto o pagos
        if (orden.Presupuesto is not null)
            return Resultado.Error("No se puede eliminar: la orden ya tiene un presupuesto.");

        if (orden.Pagos.Any())
            return Resultado.Error("No se puede eliminar: la orden ya tiene pagos registrados.");

        try
        {
            _repositorio.Eliminar(id);
            _unidadDeTrabajo.Guardar();
            return Resultado.Ok($"Orden #{id} eliminada.");
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo eliminar la orden. Intente de nuevo.");
        }
    }

    // Devuelve el mensaje de error si el cambio no está permitido, o null si sí.
    private string? ValidarCambioDeEstado(EstadoOrden actual, EstadoOrden nuevo)
    {
        if (!Enum.IsDefined(nuevo))
            return "El estado indicado no es válido.";

        if (actual == EstadoOrden.Entregado && nuevo != actual)
            return "La orden ya fue entregada y su estado no se puede cambiar.";

        if (!EstadosPermitidos(actual).Contains(nuevo))
            return $"No se puede pasar de «{actual.Nombre()}» a «{nuevo.Nombre()}»: el estado avanza un paso a la vez.";

        return null;
    }

    // Regla: cuando la orden pasa a "Entregado" se llena la fecha de entrega
    // automáticamente; si vuelve a otro estado, se borra.
    private static DateTime? CalcularFechaEntrega(EstadoOrden estado, DateTime? fechaActual) =>
        estado == EstadoOrden.Entregado ? fechaActual ?? DateTime.Now : null;
}
