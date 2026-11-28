using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models.Dtos;
using TallerCardona.Services;

namespace TallerCardona.Controllers.Api;

// ======================================================================
//  EJEMPLO PARA TODO EL EQUIPO: API REST de Clientes
//  Para su módulo copien este archivo y cambien:
//    1) la ruta  [Route("api/...")]
//    2) la política  [Authorize(Policy = "...")]
//    3) el servicio (IVehiculoService, IOrdenTrabajoService...) y el DTO
// ======================================================================

/// <summary>Consulta y mantenimiento de clientes (Administrador y Recepción).</summary>
[Route("api/clientes")]                    // todas las rutas empiezan con /api/clientes
[Authorize(Policy = "AdminORecepcion")]   // se suma al JWT que exige ApiControllerBase
public class ClientesApiController : ApiControllerBase
{
    // Usamos el SERVICIO (reglas de negocio), nunca el repositorio directamente:
    // así la API y las vistas MVC aplican exactamente las mismas reglas.
    private readonly IClienteService _clienteService;

    public ClientesApiController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>Lista los clientes ordenados por nombre.</summary>
    /// <param name="busqueda">Texto opcional para filtrar por nombre o teléfono.</param>
    /// <response code="200">Lista de clientes (puede venir vacía).</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol del usuario no tiene permiso.</response>
    [HttpGet]   // GET /api/clientes?busqueda=ana
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<ClienteDto>> Listar([FromQuery] string? busqueda)
    {
        var clientes = _clienteService.Listar(busqueda)
            .Select(ClienteDto.DesdeEntidad)
            .ToList();
        return Ok(clientes);
    }

    /// <summary>Obtiene un cliente por su id.</summary>
    /// <param name="id">Id del cliente.</param>
    /// <response code="200">El cliente encontrado.</response>
    /// <response code="404">No existe un cliente con ese id.</response>
    [HttpGet("{id:int}")]   // GET /api/clientes/5   (":int" = solo acepta números)
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ClienteDto> Obtener(int id)
    {
        var cliente = _clienteService.ObtenerPorId(id);
        if (cliente is null) return NoExiste(id);
        return Ok(ClienteDto.DesdeEntidad(cliente));
    }

    /// <summary>Registra un cliente nuevo.</summary>
    /// <remarks>El teléfono no puede repetirse. El campo Id del cuerpo se ignora.</remarks>
    /// <response code="201">Cliente creado. El encabezado Location indica su dirección.</response>
    /// <response code="400">Datos inválidos o regla de negocio incumplida (teléfono repetido).</response>
    [HttpPost]   // POST /api/clientes   (cuerpo JSON con los datos)
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ClienteDto> Crear(ClienteDto dto)
    {
        // No hace falta revisar ModelState: [ApiController] ya respondió 400 si algo no cumple
        // las validaciones del DTO ([Required], [RegularExpression]...).
        var cliente = dto.AEntidad();

        var resultado = _clienteService.Crear(cliente);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        // 201 Created + encabezado Location: /api/clientes/{id nuevo}
        var creado = _clienteService.ObtenerPorId(cliente.Id) ?? cliente;
        return CreatedAtAction(nameof(Obtener), new { id = cliente.Id }, ClienteDto.DesdeEntidad(creado));
    }

    /// <summary>Actualiza los datos de un cliente.</summary>
    /// <param name="id">Id del cliente a modificar.</param>
    /// <param name="dto">Datos nuevos.</param>
    /// <response code="204">Actualizado correctamente (sin contenido).</response>
    /// <response code="400">Datos inválidos, id distinto al del cuerpo o regla de negocio incumplida.</response>
    /// <response code="404">No existe un cliente con ese id.</response>
    [HttpPut("{id:int}")]   // PUT /api/clientes/5
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Actualizar(int id, ClienteDto dto)
    {
        // Si el cuerpo trae un Id, debe coincidir con el de la ruta (0 = no lo mandó).
        if (dto.Id != 0 && dto.Id != id)
            return ErrorDeNegocio("El id de la ruta no coincide con el id del cuerpo.");

        if (_clienteService.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = _clienteService.Actualizar(dto.AEntidad(id));
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        return NoContent();
    }

    /// <summary>Elimina un cliente (solo Administrador).</summary>
    /// <remarks>No se puede eliminar un cliente que tiene vehículos registrados.</remarks>
    /// <param name="id">Id del cliente a eliminar.</param>
    /// <response code="204">Eliminado correctamente.</response>
    /// <response code="400">Regla de negocio incumplida (tiene vehículos).</response>
    /// <response code="404">No existe un cliente con ese id.</response>
    [HttpDelete("{id:int}")]   // DELETE /api/clientes/5
    [Authorize(Policy = "SoloAdministrador")]   // además de AdminORecepcion: en la práctica, solo admin
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Eliminar(int id)
    {
        if (_clienteService.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = _clienteService.Eliminar(id);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        return NoContent();
    }

    // ---------- Respuestas de error con formato estándar (ProblemDetails) ----------
    // El campo "detail" lleva el mensaje del Resultado del servicio, listo para mostrar.

    private ObjectResult ErrorDeNegocio(string mensaje) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "No se pudo completar la operación", detail: mensaje);

    private ObjectResult NoExiste(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado", detail: $"No existe un cliente con id {id}.");
}
