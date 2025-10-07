using Microsoft.AspNetCore.Mvc;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RotacionController : ControllerBase
{
    private readonly IServicioDeRotacion _servicio;

    public RotacionController(IServicioDeRotacion servicio)
    {
        _servicio = servicio;
    }

    /// <summary>
    /// Calcula el indicador de rotación de un ingrediente (días)
    /// </summary>
    [HttpGet("ingrediente/{id}/indicador")]
    public async Task<ActionResult<double>> CalcularIndicadorRotacion(long id)
    {
        var indicador = await _servicio.CalcularIndicadorRotacionAsync(id);
        return Ok(new { IngredienteId = id, DiasRotacion = indicador });
    }

    /// <summary>
    /// Obtiene ingredientes de lenta rotación
    /// </summary>
    [HttpGet("lenta-rotacion")]
    public async Task<ActionResult<List<IngredienteLentaRotacion>>> ObtenerLentaRotacion()
    {
        var ingredientes = await _servicio.ObtenerIngredientesLentaRotacionAsync();
        return Ok(ingredientes);
    }

    /// <summary>
    /// Proyecta la demanda futura de un ingrediente
    /// </summary>
    [HttpGet("proyectar-demanda/{id}")]
    public async Task<ActionResult<double>> ProyectarDemanda(long id, [FromQuery] int dias = 30)
    {
        var demanda = await _servicio.ProyectarDemandaAsync(id, dias);
        return Ok(new
        {
            IngredienteId = id,
            DiasProyeccion = dias,
            DemandaEstimada = demanda
        });
    }

    /// <summary>
    /// Calcula el consumo diario promedio de un ingrediente
    /// </summary>
    [HttpGet("consumo-promedio/{id}")]
    public async Task<ActionResult<double>> CalcularConsumoPromedio(long id)
    {
        var consumo = await _servicio.CalcularConsumoDiarioPromedioAsync(id);
        return Ok(new { IngredienteId = id, ConsumoDiarioPromedio = consumo });
    }

    /// <summary>
    /// Obtiene ingredientes de alta rotación
    /// </summary>
    [HttpGet("alta-rotacion")]
    public async Task<ActionResult<List<Ingrediente>>> ObtenerAltaRotacion()
    {
        var ingredientes = await _servicio.ObtenerIngredientesAltaRotacionAsync();
        return Ok(ingredientes.Select(i => new
        {
            i.Id,
            i.Nombre,
            StockActual = i.CalcularStockDisponible().Valor
        }));
    }

    /// <summary>
    /// Calcula el tiempo promedio en inventario de un ingrediente
    /// </summary>
    [HttpGet("tiempo-promedio/{id}")]
    public async Task<ActionResult<double>> CalcularTiempoPromedioInventario(long id)
    {
        var tiempo = await _servicio.CalcularTiempoPromedioEnInventarioAsync(id);
        return Ok(new { IngredienteId = id, DiasPromedioEnInventario = tiempo });
    }
}
