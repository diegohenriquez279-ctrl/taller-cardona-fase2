using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Models;
using TallerCardona.Models.Reportes;
using TallerCardona.Repositorios;

namespace TallerCardona.Services;

// Lógica de negocio de las órdenes de trabajo: valida las reglas, usa el
// repositorio para preparar los cambios y la unidad de trabajo para guardarlos.
public class OrdenTrabajoService : IOrdenTrabajoService
{
    private readonly IOrdenTrabajoRepositorio _repositorio;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly ILogger<OrdenTrabajoService> _logger;

    // ILogger: se pide en el constructor y ASP.NET lo entrega (va a consola y a Logs/).
    public OrdenTrabajoService(
        IOrdenTrabajoRepositorio repositorio,
        IUnidadDeTrabajo unidadDeTrabajo,
        ILogger<OrdenTrabajoService> logger)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
        _logger = logger;
    }

    public List<OrdenTrabajo> Listar(string? busqueda = null, EstadoOrden? estado = null,
        DateTime? desde = null, DateTime? hasta = null)
    {
        // Si el usuario puso las fechas al revés, se intercambian
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            (desde, hasta) = (hasta, desde);

        return _repositorio.Listar(busqueda?.Trim(), estado, desde, hasta);
    }

    public OrdenTrabajo? ObtenerPorId(int id) => _repositorio.ObtenerPorId(id);

    // La base solo devuelve los estados que tienen órdenes; aquí se completan
    // los que faltan con 0 para que el tablero muestre siempre las 5 columnas.
    public List<ConteoPorEstado> ConteoPorEstado()
    {
        var conteo = _repositorio.ContarPorEstado();

        return Enum.GetValues<EstadoOrden>()
            .Select(estado => new ConteoPorEstado
            {
                Estado = estado,
                Cantidad = conteo.FirstOrDefault(c => c.Estado == estado)?.Cantidad ?? 0
            })
            .ToList();
    }

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

    public List<Servicio> ListarServiciosActivos() => _repositorio.ListarServiciosActivos();

    public List<Repuesto> ListarRepuestos() => _repositorio.ListarRepuestos();

    public Resultado Crear(OrdenTrabajo orden, IEnumerable<int> servicioIds, IEnumerable<int> repuestoIds)
    {
        // Regla: una orden necesita un vehículo que exista
        if (!_repositorio.ExisteVehiculo(orden.VehiculoId))
        {
            _logger.LogWarning("Crear orden rechazado: el vehículo {VehiculoId} no existe", orden.VehiculoId);
            return Resultado.Error("Seleccione un vehículo registrado.");
        }

        var error = ArmarDetalles(null, servicioIds, repuestoIds, out var servicios, out var repuestos);
        if (error is not null)
        {
            _logger.LogWarning("Crear orden rechazado: {Motivo}", error);
            return Resultado.Error(error);
        }

        // Toda orden nueva empieza en "Recibido" y todavía no tiene fecha de entrega
        orden.Estado = EstadoOrden.Recibido;
        orden.FechaEntrega = null;
        orden.Servicios = servicios;
        orden.Repuestos = repuestos;

        try
        {
            _repositorio.Agregar(orden);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Orden creada: Id {Id}, vehículo {VehiculoId}, {Servicios} servicio(s) y {Repuestos} repuesto(s)",
                orden.Id, orden.VehiculoId, servicios.Count, repuestos.Count);
            return Resultado.Ok($"Orden #{orden.Id} registrada correctamente.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear una orden para el vehículo {VehiculoId}", orden.VehiculoId);
            return Resultado.Error("No se pudo guardar la orden. Intente de nuevo.");
        }
    }

    public Resultado Actualizar(OrdenTrabajo orden, IEnumerable<int> servicioIds, IEnumerable<int> repuestoIds)
    {
        var existente = _repositorio.ObtenerPorId(orden.Id);
        if (existente is null)
            return Resultado.Error("La orden no existe.");

        if (!_repositorio.ExisteVehiculo(orden.VehiculoId))
            return Resultado.Error("Seleccione un vehículo registrado.");

        var errorEstado = ValidarCambioDeEstado(existente.Estado, orden.Estado);
        if (errorEstado is not null)
            return Resultado.Error(errorEstado);

        var error = ArmarDetalles(existente, servicioIds, repuestoIds, out var servicios, out var repuestos);
        if (error is not null)
            return Resultado.Error(error);

        orden.FechaEntrega = CalcularFechaEntrega(orden.Estado, existente.FechaEntrega);

        try
        {
            _repositorio.Actualizar(orden);
            _repositorio.ReemplazarDetalles(existente, servicios, repuestos);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Orden actualizada: Id {Id}, estado {Estado}", orden.Id, orden.Estado);
            return Resultado.Ok("Orden de trabajo actualizada correctamente.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar la orden {Id}", orden.Id);
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
        {
            _logger.LogWarning("Cambio de estado rechazado en la orden {Id}: de {Actual} a {Nuevo}", id, orden.Estado, nuevoEstado);
            return Resultado.Error(errorEstado);
        }

        if (orden.Estado == nuevoEstado)
            return Resultado.Ok($"La orden #{id} ya está en «{nuevoEstado.Nombre()}».");

        try
        {
            var anterior = orden.Estado;
            orden.Estado = nuevoEstado;
            orden.FechaEntrega = CalcularFechaEntrega(nuevoEstado, orden.FechaEntrega);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Orden {Id} cambió de estado: {Anterior} -> {Nuevo}", id, anterior, nuevoEstado);
            return Resultado.Ok($"La orden #{id} pasó a «{nuevoEstado.Nombre()}».");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al cambiar el estado de la orden {Id}", id);
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
        {
            _logger.LogWarning("Eliminar orden {Id} rechazado: tiene presupuesto", id);
            return Resultado.Error("No se puede eliminar: la orden ya tiene un presupuesto.");
        }

        if (orden.Pagos.Any())
        {
            _logger.LogWarning("Eliminar orden {Id} rechazado: tiene {Cantidad} pago(s)", id, orden.Pagos.Count);
            return Resultado.Error("No se puede eliminar: la orden ya tiene pagos registrados.");
        }

        try
        {
            _repositorio.Eliminar(id);
            _unidadDeTrabajo.Guardar();
            _logger.LogInformation("Orden eliminada: Id {Id}", id);
            return Resultado.Ok($"Orden #{id} eliminada.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al eliminar la orden {Id}", id);
            return Resultado.Error("No se pudo eliminar la orden. Intente de nuevo.");
        }
    }

    // Convierte los servicios y repuestos marcados en el formulario en líneas
    // de detalle con el precio del catálogo. Devuelve un mensaje si algo no es válido.
    // Regla: un servicio desactivado no se puede agregar (salvo que la orden ya lo tuviera).
    private string? ArmarDetalles(
        OrdenTrabajo? existente,
        IEnumerable<int> servicioIds,
        IEnumerable<int> repuestoIds,
        out List<DetalleServicio> servicios,
        out List<DetalleRepuesto> repuestos)
    {
        servicios = new List<DetalleServicio>();
        repuestos = new List<DetalleRepuesto>();

        var catalogoServicios = _repositorio.ListarServiciosActivos();
        var catalogoRepuestos = _repositorio.ListarRepuestos();

        foreach (var id in servicioIds.Distinct())
        {
            var yaEnLaOrden = existente?.Servicios.FirstOrDefault(d => d.ServicioId == id);
            var servicio = catalogoServicios.FirstOrDefault(s => s.Id == id);

            if (yaEnLaOrden is null && servicio is null)
                return "Uno de los servicios seleccionados no está disponible.";

            servicios.Add(yaEnLaOrden ?? new DetalleServicio
            {
                ServicioId = servicio!.Id,
                Cantidad = 1,
                PrecioUnitario = servicio.PrecioBase
            });
        }

        foreach (var id in repuestoIds.Distinct())
        {
            var yaEnLaOrden = existente?.Repuestos.FirstOrDefault(d => d.RepuestoId == id);
            var repuesto = catalogoRepuestos.FirstOrDefault(r => r.Id == id);

            if (yaEnLaOrden is null && repuesto is null)
                return "Uno de los repuestos seleccionados no está disponible.";

            repuestos.Add(yaEnLaOrden ?? new DetalleRepuesto
            {
                RepuestoId = repuesto!.Id,
                Cantidad = 1,
                PrecioUnitario = repuesto.Precio
            });
        }

        return null;
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
