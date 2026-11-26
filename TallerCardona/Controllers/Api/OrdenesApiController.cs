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

    /// <summary>Registra una orden nueva.</summary>
    /// <remarks>
    /// Toda orden nueva empieza en Recibido (el campo Estado se ignora). Los servicios y
    /// repuestos se indican con sus ids del catálogo y el precio lo toma el servidor.
    /// Ejemplo de cuerpo:
    ///
    ///     {
    ///       "vehiculoId": 3,
    ///       "problemaReportado": "Ruido al frenar",
    ///       "kilometrajeIngreso": 91500,
    ///       "servicioIds": [3],
    ///       "repuestoIds": [2]
    ///     }
    /// </remarks>
    /// <response code="201">Orden creada. El encabezado Location indica su dirección.</response>
    /// <response code="400">Datos inválidos o regla de negocio incumplida (vehículo o servicio inexistente).</response>
    [HttpPost]   // POST /api/ordenes   (cuerpo JSON con los datos)
    [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<OrdenDto> Crear(OrdenDto dto)
    {
        // No hace falta revisar ModelState: [ApiController] ya respondió 400 si algo no cumple
        // las validaciones del DTO ([Required], [Range]...).
        var orden = dto.AEntidad();

        var resultado = _ordenService.Crear(orden, dto.ServicioIds ?? new(), dto.RepuestoIds ?? new());
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        // 201 Created + encabezado Location: /api/ordenes/{número nuevo}
        var creada = _ordenService.ObtenerPorId(orden.Id) ?? orden;
        return CreatedAtAction(nameof(Obtener), new { id = orden.Id }, OrdenDto.DesdeEntidad(creada, conDetalle: true));
    }

    /// <summary>Actualiza los datos de una orden.</summary>
    /// <remarks>
    /// El estado solo puede avanzar o regresar un paso. Si no se mandan Estado, ServicioIds
    /// o RepuestoIds, se conservan los que ya tiene la orden.
    /// </remarks>
    /// <param name="id">Número de la orden a modificar.</param>
    /// <param name="dto">Datos nuevos.</param>
    /// <response code="204">Actualizada correctamente (sin contenido).</response>
    /// <response code="400">Datos inválidos, id distinto al del cuerpo o regla de negocio incumplida.</response>
    /// <response code="404">No existe una orden con ese número.</response>
    [HttpPut("{id:int}")]   // PUT /api/ordenes/5
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Actualizar(int id, OrdenDto dto)
    {
        // Si el cuerpo trae un Id, debe coincidir con el de la ruta (0 = no lo mandó).
        if (dto.Id != 0 && dto.Id != id)
            return ErrorDeNegocio("El id de la ruta no coincide con el id del cuerpo.");

        var actual = _ordenService.ObtenerPorId(id);
        if (actual is null) return NoExiste(id);

        // Lo que no venga en el cuerpo se conserva como está
        var servicioIds = dto.ServicioIds ?? actual.Servicios.Select(d => d.ServicioId).ToList();
        var repuestoIds = dto.RepuestoIds ?? actual.Repuestos.Select(d => d.RepuestoId).ToList();

        var resultado = _ordenService.Actualizar(dto.AEntidad(id, actual), servicioIds, repuestoIds);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        return NoContent();
    }

    /// <summary>Cambia solo el estado de una orden (avanza o regresa un paso).</summary>
    /// <remarks>
    /// Cuando pasa a Entregado se llena la fecha de entrega y ya no se puede cambiar.
    /// Ejemplo de cuerpo: { "estado": "EnDiagnostico" }
    /// </remarks>
    /// <param name="id">Número de la orden.</param>
    /// <param name="cambio">El estado nuevo.</param>
    /// <response code="200">Estado cambiado. Devuelve la orden actualizada.</response>
    /// <response code="400">Estado inválido o salto de etapa no permitido.</response>
    /// <response code="404">No existe una orden con ese número.</response>
    [HttpPut("{id:int}/estado")]   // PUT /api/ordenes/5/estado
    [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<OrdenDto> CambiarEstado(int id, CambioEstadoDto cambio)
    {
        if (_ordenService.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = _ordenService.CambiarEstado(id, cambio.Estado);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        var orden = _ordenService.ObtenerPorId(id)!;
        return Ok(OrdenDto.DesdeEntidad(orden, conDetalle: true));
    }

    /// <summary>Elimina una orden (solo Administrador).</summary>
    /// <remarks>No se puede eliminar una orden que ya tiene presupuesto o pagos.</remarks>
    /// <param name="id">Número de la orden a eliminar.</param>
    /// <response code="204">Eliminada correctamente.</response>
    /// <response code="400">Regla de negocio incumplida (tiene presupuesto o pagos).</response>
    /// <response code="404">No existe una orden con ese número.</response>
    [HttpDelete("{id:int}")]   // DELETE /api/ordenes/5
    [Authorize(Policy = "SoloAdministrador")]   // además de PersonalTaller: en la práctica, solo admin
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Eliminar(int id)
    {
        if (_ordenService.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = _ordenService.Eliminar(id);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        return NoContent();
    }

    // ---------- Respuestas de error con formato estándar (ProblemDetails) ----------
    // El campo "detail" lleva el mensaje del Resultado del servicio, listo para mostrar.

    private ObjectResult ErrorDeNegocio(string mensaje) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "No se pudo completar la operación", detail: mensaje);

    private ObjectResult NoExiste(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado", detail: $"No existe una orden con número {id}.");
}
