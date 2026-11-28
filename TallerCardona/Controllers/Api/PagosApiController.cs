using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models.Dtos;
using TallerCardona.Services;

namespace TallerCardona.Controllers.Api;

/// <summary>API REST de pagos para Administrador y Recepción (eliminar: solo Administrador).</summary>
[Route("api/pagos")]
[Authorize(Policy = "AdminORecepcion")]
public class PagosApiController : ApiControllerBase
{
    // Se usa el SERVICIO (reglas de negocio) para que la API y las pantallas MVC
    // apliquen las mismas reglas: monto > 0, presupuesto Autorizado y no superar el saldo.
    private readonly IPagoService _pagos;

    public PagosApiController(IPagoService pagos) => _pagos = pagos;

    /// <summary>Lista todos los pagos registrados.</summary>
    /// <response code="200">Lista de pagos (puede venir vacía).</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol del usuario no tiene permiso.</response>
    [HttpGet]   // GET /api/pagos
    [ProducesResponseType(typeof(List<PagoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PagoDto>>> Listar() =>
        Ok((await _pagos.ListarAsync()).Select(PagoDto.DesdeEntidad).ToList());

    /// <summary>Obtiene un pago por su identificador.</summary>
    /// <param name="id">Identificador del pago.</param>
    /// <response code="200">El pago encontrado.</response>
    /// <response code="404">No existe un pago con ese id.</response>
    [HttpGet("{id:int}")]   // GET /api/pagos/5
    [ProducesResponseType(typeof(PagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagoDto>> Obtener(int id)
    {
        var pago = await _pagos.ObtenerPorIdAsync(id);
        return pago is null ? NoExiste(id) : Ok(PagoDto.DesdeEntidad(pago));
    }

    /// <summary>Pagos de un presupuesto, con total pagado y saldo pendiente.</summary>
    /// <param name="presupuestoId">Identificador del presupuesto.</param>
    /// <response code="200">Pagos de la orden del presupuesto y su saldo.</response>
    /// <response code="404">No existe un presupuesto con ese id.</response>
    [HttpGet("presupuesto/{presupuestoId:int}")]   // GET /api/pagos/presupuesto/1
    [ProducesResponseType(typeof(PagosDePresupuestoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagosDePresupuestoDto>> PorPresupuesto(int presupuestoId)
    {
        var presupuesto = _pagos.ObtenerPresupuesto(presupuestoId);
        if (presupuesto is null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado",
                detail: $"No existe un presupuesto con id {presupuestoId}.");

        var pagos = await _pagos.ListarPorOrdenAsync(presupuesto.OrdenTrabajoId);
        var totalPagado = pagos.Sum(p => p.Monto);

        return Ok(new PagosDePresupuestoDto
        {
            PresupuestoId = presupuesto.Id,
            OrdenTrabajoId = presupuesto.OrdenTrabajoId,
            EstadoPresupuesto = presupuesto.Estado,
            TotalEstimado = presupuesto.TotalEstimado,
            TotalPagado = totalPagado,
            Saldo = presupuesto.TotalEstimado - totalPagado,
            Pagos = pagos.Select(PagoDto.DesdeEntidad).ToList()
        });
    }

    /// <summary>Registra un pago de una orden con presupuesto Autorizado.</summary>
    /// <remarks>
    /// La fecha y el estado los pone el servidor. Ejemplo de cuerpo:
    ///
    ///     {
    ///       "ordenTrabajoId": 4,
    ///       "monto": 25.00,
    ///       "metodoPago": "Efectivo"
    ///     }
    /// </remarks>
    /// <response code="201">Pago registrado. El encabezado Location indica su dirección.</response>
    /// <response code="400">Datos inválidos o regla incumplida (monto, presupuesto no autorizado, saldo superado).</response>
    [HttpPost]   // POST /api/pagos
    [ProducesResponseType(typeof(PagoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagoDto>> Registrar(PagoCrearDto dto)
    {
        try
        {
            var pago = await _pagos.RegistrarAsync(dto.AEntidad());
            return CreatedAtAction(nameof(Obtener), new { id = pago.Id }, PagoDto.DesdeEntidad(pago));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "No se pudo registrar el pago", detail: ex.Message);
        }
    }

    /// <summary>Elimina un pago (solo Administrador).</summary>
    /// <param name="id">Identificador del pago.</param>
    /// <response code="204">Eliminado correctamente.</response>
    /// <response code="404">No existe un pago con ese id.</response>
    [HttpDelete("{id:int}")]   // DELETE /api/pagos/5
    [Authorize(Policy = "SoloAdministrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Eliminar(int id) =>
        await _pagos.EliminarAsync(id, User.Identity?.Name) ? NoContent() : NoExiste(id);

    private ObjectResult NoExiste(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado", detail: $"No existe un pago con id {id}.");
}
