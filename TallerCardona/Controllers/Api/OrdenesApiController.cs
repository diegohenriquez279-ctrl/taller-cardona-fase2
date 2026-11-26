using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;
using TallerCardona.Models.Dtos;
using TallerCardona.Models.Reportes;
using TallerCardona.Services;

namespace TallerCardona.Controllers.Api;

/// <summary>Consulta y mantenimiento de órdenes de trabajo (todo el personal del taller).</summary>
[Route("api/ordenes")]                   // todas las rutas empiezan con /api/ordenes
[Authorize(Policy = "PersonalTaller")]   // los tres roles; se suma al JWT que exige ApiControllerBase
public class OrdenesApiController : ApiControllerBase
{
    // Usamos el SERVICIO (reglas de negocio), nunca el repositorio directamente:
    // así la API y las pantallas MVC aplican exactamente las mismas reglas
    // (vehículo existente, estado un paso a la vez, no eliminar con presupuesto...).
    private readonly IOrdenTrabajoService _ordenService;

    public OrdenesApiController(IOrdenTrabajoService ordenService)
    {
        _ordenService = ordenService;
    }

    /// <summary>Lista las órdenes, de la más reciente a la más antigua.</summary>
    /// <remarks>
    /// Los filtros se pueden combinar. Esta lista no trae servicios, repuestos ni total:
    /// para eso use GET /api/ordenes/{id}.
    /// </remarks>
    /// <param name="busqueda">Texto opcional: número de orden, placa, marca, modelo, cliente o problema.</param>
    /// <param name="estado">Estado opcional: Recibido, EnDiagnostico, EnReparacion, ListoParaEntrega o Entregado.</param>
    /// <param name="desde">Fecha de ingreso desde (opcional, formato 2026-11-01).</param>
    /// <param name="hasta">Fecha de ingreso hasta, día completo incluido (opcional).</param>
    /// <response code="200">Lista de órdenes (puede venir vacía).</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol del usuario no tiene permiso.</response>
    [HttpGet]   // GET /api/ordenes?estado=EnReparacion&desde=2026-11-01
    [ProducesResponseType(typeof(List<OrdenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<OrdenDto>> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] EstadoOrden? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var ordenes = _ordenService.Listar(busqueda, estado, desde, hasta)
            .Select(o => OrdenDto.DesdeEntidad(o, conDetalle: false))
            .ToList();
        return Ok(ordenes);
    }

    /// <summary>Obtiene una orden con sus servicios, repuestos y total.</summary>
    /// <param name="id">Número de la orden.</param>
    /// <response code="200">La orden encontrada.</response>
    /// <response code="404">No existe una orden con ese número.</response>
    [HttpGet("{id:int}")]   // GET /api/ordenes/5
    [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<OrdenDto> Obtener(int id)
    {
        var orden = _ordenService.ObtenerPorId(id);
        if (orden is null) return NoExiste(id);
        return Ok(OrdenDto.DesdeEntidad(orden, conDetalle: true));
    }

    /// <summary>Cuántas órdenes hay en cada estado (los mismos números del tablero).</summary>
    /// <remarks>Sale de la consulta LINQ con GroupBy; los estados sin órdenes vienen con 0.</remarks>
    /// <response code="200">Un elemento por cada estado.</response>
    [HttpGet("conteo-por-estado")]   // GET /api/ordenes/conteo-por-estado
    [ProducesResponseType(typeof(List<ConteoPorEstado>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<ConteoPorEstado>> ConteoPorEstado() =>
        Ok(_ordenService.ConteoPorEstado());

    // ---------- Respuestas de error con formato estándar (ProblemDetails) ----------
    // El campo "detail" lleva el mensaje del Resultado del servicio, listo para mostrar.

    private ObjectResult ErrorDeNegocio(string mensaje) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "No se pudo completar la operación", detail: mensaje);

    private ObjectResult NoExiste(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado", detail: $"No existe una orden con número {id}.");
}
