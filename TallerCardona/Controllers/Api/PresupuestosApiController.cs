using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models.Dtos;
using TallerCardona.Services;

namespace TallerCardona.Controllers.Api;

/// <summary>API REST de presupuestos para Administrador y Recepción.</summary>
[Route("api/presupuestos")]
[Authorize(Policy = "AdminORecepcion")]
public class PresupuestosApiController : ApiControllerBase
{
    // Se usa el SERVICIO para aplicar las mismas reglas que las pantallas MVC:
    // un presupuesto por orden, total desde servicios y repuestos, solo se autoriza/rechaza si está Pendiente.
    private readonly IPresupuestoService _presupuestos;

    public PresupuestosApiController(IPresupuestoService presupuestos) => _presupuestos = presupuestos;

    /// <summary>Lista los presupuestos, del más reciente al más antiguo.</summary>
    /// <response code="200">Lista de presupuestos (puede venir vacía).</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol del usuario no tiene permiso.</response>
    [HttpGet]   // GET /api/presupuestos
    [ProducesResponseType(typeof(List<PresupuestoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<PresupuestoDto>> Listar() =>
        Ok(_presupuestos.Listar().Select(PresupuestoDto.DesdeEntidad).ToList());

    /// <summary>Obtiene un presupuesto por su identificador.</summary>
    /// <param name="id">Identificador del presupuesto.</param>
    /// <response code="200">El presupuesto encontrado.</response>
    /// <response code="404">No existe un presupuesto con ese id.</response>
    [HttpGet("{id:int}")]   // GET /api/presupuestos/5
    [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<PresupuestoDto> Obtener(int id)
    {
        var presupuesto = _presupuestos.ObtenerPorId(id);
        return presupuesto is null ? NoExiste(id) : Ok(PresupuestoDto.DesdeEntidad(presupuesto));
    }

    /// <summary>Genera el presupuesto de una orden con el total de sus servicios y repuestos.</summary>
    /// <remarks>
    /// Si la orden ya tiene un presupuesto Pendiente o Autorizado responde 400.
    /// Si el anterior fue Rechazado, se regenera (vuelve a Pendiente con el total recalculado).
    /// </remarks>
    /// <param name="ordenId">Número de la orden de trabajo.</param>
    /// <response code="201">Presupuesto generado. El encabezado Location indica su dirección.</response>
    /// <response code="400">Regla incumplida (orden inexistente, sin servicios ni repuestos, ya tiene presupuesto).</response>
    [HttpPost("orden/{ordenId:int}")]   // POST /api/presupuestos/orden/4
    [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<PresupuestoDto> Generar(int ordenId)
    {
        var resultado = _presupuestos.Crear(ordenId, User.Identity?.Name);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        var presupuesto = _presupuestos.ObtenerPorOrden(ordenId)!;
        return CreatedAtAction(nameof(Obtener), new { id = presupuesto.Id }, PresupuestoDto.DesdeEntidad(presupuesto));
    }

    /// <summary>Autoriza un presupuesto Pendiente (después se le pueden registrar pagos).</summary>
    /// <param name="id">Identificador del presupuesto.</param>
    /// <response code="200">Autorizado. Devuelve el presupuesto actualizado.</response>
    /// <response code="400">El presupuesto no está Pendiente.</response>
    /// <response code="404">No existe un presupuesto con ese id.</response>
    [HttpPost("{id:int}/autorizar")]   // POST /api/presupuestos/5/autorizar
    [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<PresupuestoDto> Autorizar(int id) =>
        Cambiar(id, () => _presupuestos.Autorizar(id, User.Identity?.Name));

    /// <summary>Rechaza un presupuesto Pendiente.</summary>
    /// <param name="id">Identificador del presupuesto.</param>
    /// <response code="200">Rechazado. Devuelve el presupuesto actualizado.</response>
    /// <response code="400">El presupuesto no está Pendiente.</response>
    /// <response code="404">No existe un presupuesto con ese id.</response>
    [HttpPost("{id:int}/rechazar")]   // POST /api/presupuestos/5/rechazar
    [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<PresupuestoDto> Rechazar(int id) =>
        Cambiar(id, () => _presupuestos.Rechazar(id, User.Identity?.Name));

    private ActionResult<PresupuestoDto> Cambiar(int id, Func<Resultado> accion)
    {
        if (_presupuestos.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = accion();
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        return Ok(PresupuestoDto.DesdeEntidad(_presupuestos.ObtenerPorId(id)!));
    }

    private ObjectResult ErrorDeNegocio(string mensaje) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "No se pudo completar la operación", detail: mensaje);

    private ObjectResult NoExiste(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado", detail: $"No existe un presupuesto con id {id}.");
}
