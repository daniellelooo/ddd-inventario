using Microsoft.AspNetCore.Mvc;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Enums;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesDeCompraController : ControllerBase
{
    private readonly IOrdenDeCompraRepository _repository;
    private readonly IIngredienteRepository _ingredienteRepository;

    public OrdenesDeCompraController(
        IOrdenDeCompraRepository repository,
        IIngredienteRepository ingredienteRepository)
    {
        _repository = repository;
        _ingredienteRepository = ingredienteRepository;
    }

    /// <summary>
    /// Obtiene todas las órdenes de compra
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrdenDto>>> ObtenerTodas()
    {
        var ordenes = await _repository.ObtenerTodosAsync();
        return Ok(ordenes.Select(MapearADto).ToList());
    }

    /// <summary>
    /// Obtiene una orden por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrdenDto>> ObtenerPorId(long id)
    {
        var orden = await _repository.ObtenerPorIdAsync(id);

        if (orden == null)
            return NotFound();

        return Ok(MapearADto(orden));
    }

    /// <summary>
    /// Crea una nueva orden de compra
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<long>> Crear([FromBody] CrearOrdenRequest request)
    {
        var orden = new OrdenDeCompra(0, request.ProveedorId, request.FechaEsperada);

        foreach (var item in request.Items)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId);

            if (ingrediente == null)
                return BadRequest($"Ingrediente {item.IngredienteId} no encontrado");

            var cantidad = new CantidadDisponible(item.Cantidad, ingrediente.UnidadDeMedida);
            var precio = new PrecioConMoneda(item.PrecioUnitario, "BOB");

            orden.AgregarItem(item.IngredienteId, cantidad, precio);
        }

        var guardada = await _repository.GuardarAsync(orden);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = guardada.Id }, guardada.Id);
    }

    /// <summary>
    /// Aprueba una orden de compra
    /// </summary>
    [HttpPost("{id}/aprobar")]
    public async Task<ActionResult> Aprobar(long id)
    {
        var orden = await _repository.ObtenerPorIdAsync(id);

        if (orden == null)
            return NotFound();

        orden.Aprobar();
        await _repository.GuardarAsync(orden);

        return NoContent();
    }

    /// <summary>
    /// Cancela una orden de compra
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> Cancelar(long id)
    {
        var orden = await _repository.ObtenerPorIdAsync(id);

        if (orden == null)
            return NotFound();

        orden.Cancelar();
        await _repository.GuardarAsync(orden);

        return NoContent();
    }

    /// <summary>
    /// Marca una orden como en tránsito
    /// </summary>
    [HttpPost("{id}/en-transito")]
    public async Task<ActionResult> MarcarEnTransito(long id)
    {
        var orden = await _repository.ObtenerPorIdAsync(id);

        if (orden == null)
            return NotFound();

        orden.MarcarEnTransito();
        await _repository.GuardarAsync(orden);

        return NoContent();
    }

    /// <summary>
    /// Marca una orden como recibida
    /// </summary>
    [HttpPost("{id}/recibida")]
    public async Task<ActionResult> MarcarRecibida(long id)
    {
        var orden = await _repository.ObtenerPorIdAsync(id);

        if (orden == null)
            return NotFound();

        orden.MarcarRecibida();
        await _repository.GuardarAsync(orden);

        return NoContent();
    }

    /// <summary>
    /// Obtiene órdenes por estado
    /// </summary>
    [HttpGet("por-estado/{estado}")]
    public async Task<ActionResult<List<OrdenDto>>> ObtenerPorEstado(EstadoOrden estado)
    {
        var ordenes = await _repository.ObtenerPorEstadoAsync(estado);
        return Ok(ordenes.Select(MapearADto).ToList());
    }

    private static OrdenDto MapearADto(OrdenDeCompra orden)
    {
        return new OrdenDto(
            orden.Id,
            orden.ProveedorId,
            orden.FechaSolicitud,
            orden.FechaEsperada,
            orden.Estado.ToString(),
            orden.Items.Count,
            orden.CalcularTotal());
    }
}

public record OrdenDto(long Id, long ProveedorId, DateTime FechaSolicitud, DateTime? FechaEsperada, string Estado, int CantidadItems, decimal Total);
public record CrearOrdenRequest(long ProveedorId, DateTime FechaEsperada, List<ItemOrdenRequest> Items);
public record ItemOrdenRequest(long IngredienteId, double Cantidad, decimal PrecioUnitario);
