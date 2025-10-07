using MediatR;
using Microsoft.AspNetCore.Mvc;
using InventarioDDD.Application.UseCases;

namespace InventarioDDD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngredientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public IngredientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los ingredientes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<IngredienteDto>>> ObtenerTodos()
    {
        var ingredientes = await _mediator.Send(new ObtenerTodosIngredientesQuery());
        return Ok(ingredientes);
    }

    /// <summary>
    /// Obtiene un ingrediente por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<IngredienteDto>> ObtenerPorId(long id)
    {
        var ingrediente = await _mediator.Send(new ObtenerIngredientePorIdQuery(id));

        if (ingrediente == null)
            return NotFound($"Ingrediente {id} no encontrado");

        return Ok(ingrediente);
    }

    /// <summary>
    /// Registra un nuevo ingrediente
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<long>> Registrar([FromBody] RegistrarIngredienteCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
    }

    /// <summary>
    /// Registra la recepción de un lote para un ingrediente
    /// </summary>
    [HttpPost("{id}/lotes")]
    public async Task<ActionResult<long>> RegistrarLote(long id, [FromBody] RegistrarLoteRequest request)
    {
        var command = new RegistrarRecepcionLoteCommand(
            id,
            request.Cantidad,
            request.FechaVencimiento,
            request.ProveedorId
        );

        var loteId = await _mediator.Send(command);

        return CreatedAtAction(nameof(ObtenerPorId), new { id }, loteId);
    }
}

/// <summary>
/// Request para registrar un lote
/// </summary>
public record RegistrarLoteRequest(
    double Cantidad,
    DateTime FechaVencimiento,
    long ProveedorId
);
