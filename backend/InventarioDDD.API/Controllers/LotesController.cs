using InventarioDDD.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventarioDDD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LotesController : ControllerBase
    {
        private readonly ILoteRepository _loteRepository;
        private readonly ILogger<LotesController> _logger;

        public LotesController(
            ILoteRepository loteRepository,
            ILogger<LotesController> logger)
        {
            _loteRepository = loteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los lotes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var lotes = await _loteRepository.ObtenerTodosAsync();

                var resultado = lotes.Select(l => new
                {
                    id = l.Id,
                    codigo = l.Codigo,
                    ingredienteId = l.IngredienteId,
                    proveedorId = l.ProveedorId,
                    cantidadInicial = l.CantidadInicial,
                    cantidadDisponible = l.CantidadDisponible,
                    fechaVencimiento = l.FechaVencimiento.Valor,
                    fechaRecepcion = l.FechaRecepcion,
                    precioUnitario = l.PrecioUnitario.Valor,
                    moneda = l.PrecioUnitario.Moneda,
                    vencido = l.Vencido,
                    proximoAVencer = l.EstaProximoAVencer(7)
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes");
                return StatusCode(500, new { message = "Error al obtener lotes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene lotes por ingrediente
        /// </summary>
        [HttpGet("ingrediente/{ingredienteId}")]
        public async Task<IActionResult> ObtenerPorIngrediente(Guid ingredienteId)
        {
            try
            {
                var lotes = await _loteRepository.ObtenerPorIngredienteAsync(ingredienteId);

                var resultado = lotes.Select(l => new
                {
                    id = l.Id,
                    codigo = l.Codigo,
                    cantidadInicial = l.CantidadInicial,
                    cantidadDisponible = l.CantidadDisponible,
                    fechaVencimiento = l.FechaVencimiento.Valor,
                    fechaRecepcion = l.FechaRecepcion,
                    precioUnitario = l.PrecioUnitario.Valor,
                    vencido = l.Vencido,
                    proximoAVencer = l.EstaProximoAVencer(7)
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes por ingrediente");
                return StatusCode(500, new { message = "Error al obtener lotes", error = ex.Message });
            }
        }
    }
}
