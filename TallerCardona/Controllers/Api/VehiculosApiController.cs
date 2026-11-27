using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models.Dtos;
using TallerCardona.Services;

namespace TallerCardona.Controllers.Api;

/// <summary>API REST de vehículos para Administrador y Recepción.</summary>
[Route("api/vehiculos")]
[Authorize(Policy = "AdminORecepcion")]
public class VehiculosApiController : ApiControllerBase
{
    private readonly IVehiculoService _vehiculos;

    public VehiculosApiController(IVehiculoService vehiculos) => _vehiculos = vehiculos;

    /// <summary>Lista vehículos y permite filtrar por placa, marca, modelo o cliente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VehiculoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<VehiculoDto>> Listar([FromQuery] string? busqueda) =>
        Ok(_vehiculos.Listar(busqueda).Select(VehiculoDto.DesdeEntidad).ToList());

    /// <summary>Obtiene un vehículo por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VehiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<VehiculoDto> Obtener(int id)
    {
        var vehiculo = _vehiculos.ObtenerPorId(id);
        return vehiculo is null ? NoExiste(id) : Ok(VehiculoDto.DesdeEntidad(vehiculo));
    }

    /// <summary>Registra un vehículo nuevo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VehiculoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<VehiculoDto> Crear(VehiculoDto dto)
    {
        var vehiculo = dto.AEntidad();
        var resultado = _vehiculos.Crear(vehiculo);
        if (!resultado.Exito) return ErrorDeNegocio(resultado.Mensaje);

        var creado = _vehiculos.ObtenerPorId(vehiculo.Id) ?? vehiculo;
        return CreatedAtAction(nameof(Obtener), new { id = vehiculo.Id }, VehiculoDto.DesdeEntidad(creado));
    }

    /// <summary>Actualiza un vehículo existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Actualizar(int id, VehiculoDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
            return ErrorDeNegocio("El id de la ruta no coincide con el id del cuerpo.");

        if (_vehiculos.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = _vehiculos.Actualizar(dto.AEntidad(id));
        return resultado.Exito ? NoContent() : ErrorDeNegocio(resultado.Mensaje);
    }

    /// <summary>Elimina un vehículo sin órdenes asociadas (solo Administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SoloAdministrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Eliminar(int id)
    {
        if (_vehiculos.ObtenerPorId(id) is null) return NoExiste(id);

        var resultado = _vehiculos.Eliminar(id);
        return resultado.Exito ? NoContent() : ErrorDeNegocio(resultado.Mensaje);
    }

    private ObjectResult ErrorDeNegocio(string mensaje) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "No se pudo completar la operación", detail: mensaje);

    private ObjectResult NoExiste(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontrado", detail: $"No existe un vehículo con id {id}.");
}
