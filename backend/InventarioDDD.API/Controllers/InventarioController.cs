using InventarioDDD.Application.Commands;
using InventarioDDD.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventarioDDD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InventarioController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(IMediator mediator, ILogger<InventarioController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Registra el consumo de un ingrediente del inventario
        /// </summary>
        /// <param name="command">Datos del consumo</param>
        /// <returns>Confirmación del consumo</returns>
        [HttpPost("consumo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegistrarConsumo([FromBody] RegistrarConsumoCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                return Ok(new
                {
                    success = true,
                    message = "Consumo registrado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Datos inválidos al registrar consumo");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al registrar consumo");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar consumo");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al registrar el consumo",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene los lotes próximos a vencer
        /// </summary>
        /// <param name="diasAnticipacion">Días de anticipación para la alerta</param>
        /// <returns>Lista de lotes próximos a vencer</returns>
        [HttpGet("lotes/proximos-vencer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerLotesProximosAVencer([FromQuery] int diasAnticipacion = 7)
        {
            try
            {
                var query = new ObtenerLotesProximosAVencerQuery
                {
                    DiasAnticipacion = diasAnticipacion
                };

                var resultado = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = resultado,
                    count = resultado.Count,
                    diasAnticipacion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes próximos a vencer");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los lotes",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene el historial de movimientos de un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="fechaDesde">Fecha inicial (opcional)</param>
        /// <param name="fechaHasta">Fecha final (opcional)</param>
        /// <returns>Historial de movimientos</returns>
        [HttpGet("movimientos/{ingredienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerHistorialMovimientos(
            Guid ingredienteId,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var query = new ObtenerHistorialMovimientosQuery
                {
                    IngredienteId = ingredienteId,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta
                };

                var resultado = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = resultado,
                    count = resultado.Count,
                    filtros = new
                    {
                        ingredienteId,
                        fechaDesde,
                        fechaHasta
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de movimientos para ingrediente {IngredienteId}", ingredienteId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el historial de movimientos",
                    error = ex.Message
                });
            }
        }
    }
}
