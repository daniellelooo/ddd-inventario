using Microsoft.AspNetCore.Mvc;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IServicioDeInventario _servicio;
    private readonly IServicioDeConsumo _servicioConsumo;
    private readonly IServicioDeReabastecimiento _servicioReabastecimiento;

    public InventarioController(
        IServicioDeInventario servicio,
        IServicioDeConsumo servicioConsumo,
        IServicioDeReabastecimiento servicioReabastecimiento)
    {
        _servicio = servicio;
        _servicioConsumo = servicioConsumo;
        _servicioReabastecimiento = servicioReabastecimiento;
    }

    /// <summary>
    /// Consulta el stock disponible de un ingrediente
    /// </summary>
    [HttpGet("stock/{id}")]
    public async Task<ActionResult> ConsultarStock(long id)
    {
        var stock = await _servicio.ConsultarStockDisponibleAsync(id);
        return Ok(new
        {
            IngredienteId = id,
            StockDisponible = stock.Valor,
            UnidadMedida = stock.UnidadDeMedida.Simbolo
        });
    }

    /// <summary>
    /// Obtiene todos los lotes de un ingrediente
    /// </summary>
    [HttpGet("lotes/{id}")]
    public async Task<ActionResult<List<Lote>>> ObtenerLotes(long id)
    {
        var lotes = await _servicio.ObtenerLotesPorIngredienteAsync(id);
        return Ok(lotes.Select(l => new
        {
            l.Id,
            Cantidad = l.Cantidad.Valor,
            UnidadMedida = l.Cantidad.UnidadDeMedida.Simbolo,
            FechaVencimiento = l.FechaVencimiento.Fecha,
            l.FechaRecepcion,
            l.ProveedorId,
            l.Agotado,
            EstaVencido = l.EstaVencido()
        }));
    }

    /// <summary>
    /// Descontar ingredientes para un pedido
    /// </summary>
    [HttpPost("consumir")]
    public async Task<ActionResult> ConsumirIngredientes([FromBody] ConsumirRequest request)
    {
        await _servicioConsumo.DescontarIngredientesAsync(request.PedidoId, request.IngredientesIds);
        return Ok(new { Mensaje = "Ingredientes consumidos exitosamente" });
    }

    /// <summary>
    /// Validar stock suficiente para ingredientes
    /// </summary>
    [HttpPost("validar-stock")]
    public async Task<ActionResult<bool>> ValidarStock([FromBody] List<long> ingredientesIds)
    {
        var suficiente = await _servicioConsumo.ValidarStockSuficienteAsync(ingredientesIds);
        return Ok(new { StockSuficiente = suficiente });
    }

    /// <summary>
    /// Generar órdenes de compra automáticas
    /// </summary>
    [HttpPost("generar-ordenes-automaticas")]
    public async Task<ActionResult> GenerarOrdenesAutomaticas()
    {
        await _servicioReabastecimiento.GenerarOrdenDeCompraAutomaticaAsync();
        return Ok(new { Mensaje = "Órdenes de compra generadas automáticamente" });
    }

    /// <summary>
    /// Calcular punto de reorden para un ingrediente
    /// </summary>
    [HttpGet("punto-reorden/{id}")]
    public async Task<ActionResult<double>> CalcularPuntoReorden(long id)
    {
        var puntoReorden = await _servicioReabastecimiento.CalcularPuntoDeReordenAsync(id);
        return Ok(new { IngredienteId = id, PuntoDeReorden = puntoReorden });
    }

    /// <summary>
    /// Sugerir proveedor para un ingrediente
    /// </summary>
    [HttpGet("sugerir-proveedor/{id}")]
    public async Task<ActionResult<long?>> SugerirProveedor(long id)
    {
        var proveedorId = await _servicioReabastecimiento.SugerirProveedorAsync(id);
        return Ok(new { IngredienteId = id, ProveedorSugerido = proveedorId });
    }
}

public record ConsumirRequest(long PedidoId, List<long> IngredientesIds);
