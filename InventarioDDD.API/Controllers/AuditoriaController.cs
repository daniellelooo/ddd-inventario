using Microsoft.AspNetCore.Mvc;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditoriaController : ControllerBase
{
    private readonly IServicioDeAuditoria _servicio;
    private readonly IServicioDeInventario _servicioInventario;

    public AuditoriaController(IServicioDeAuditoria servicio, IServicioDeInventario servicioInventario)
    {
        _servicio = servicio;
        _servicioInventario = servicioInventario;
    }

    /// <summary>
    /// Genera reporte de consumo en un período
    /// </summary>
    [HttpGet("reporte-consumo")]
    public async Task<ActionResult<ReporteConsumo>> GenerarReporteConsumo(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var reporte = await _servicio.GenerarReporteConsumoAsync(desde, hasta);
        return Ok(reporte);
    }

    /// <summary>
    /// Genera reporte de mermas en un período
    /// </summary>
    [HttpGet("reporte-mermas")]
    public async Task<ActionResult<ReporteMermas>> GenerarReporteMermas(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var reporte = await _servicio.GenerarReporteMermasAsync(desde, hasta);
        return Ok(reporte);
    }

    /// <summary>
    /// Compara inventario físico vs sistema
    /// </summary>
    [HttpPost("comparar-inventario")]
    public async Task<ActionResult<List<Discrepancia>>> CompararInventarioFisico(
        [FromBody] Dictionary<long, double> inventarioFisico)
    {
        var discrepancias = await _servicio.CompararInventarioFisicoAsync(inventarioFisico);
        return Ok(discrepancias);
    }

    /// <summary>
    /// Calcula la valoración total del inventario
    /// </summary>
    [HttpGet("valoracion-inventario")]
    public async Task<ActionResult<decimal>> CalcularValoracionInventario()
    {
        var valoracion = await _servicioInventario.CalcularValoracionInventarioAsync();
        return Ok(new { ValorTotal = valoracion, Moneda = "BOB" });
    }
}
