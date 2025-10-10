using InventarioDDD.Application.Commands;
using InventarioDDD.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventarioDDD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdenesCompraController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdenesCompraController> _logger;

        public OrdenesCompraController(IMediator mediator, ILogger<OrdenesCompraController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva orden de compra
        /// </summary>
        /// <param name="command">Datos de la orden de compra</param>
        /// <returns>ID de la orden creada</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearOrdenDeCompra([FromBody] CrearOrdenDeCompraCommand command)
        {
            try
            {
                var ordenId = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(CrearOrdenDeCompra),
                    new { id = ordenId },
                    new
                    {
                        success = true,
                        message = "Orden de compra creada exitosamente",
                        data = new { ordenId }
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Datos inválidos al crear orden de compra");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de compra");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear la orden de compra",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Aprueba una orden de compra existente
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="command">Datos de aprobación</param>
        /// <returns>Confirmación de aprobación</returns>
        [HttpPost("{id}/aprobar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AprobarOrden(Guid id, [FromBody] AprobarOrdenDeCompraCommand command)
        {
            try
            {
                command.OrdenId = id;
                var resultado = await _mediator.Send(command);

                return Ok(new
                {
                    success = true,
                    message = "Orden de compra aprobada exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Orden no encontrada: {OrdenId}", id);
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al aprobar orden: {OrdenId}", id);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar orden de compra: {OrdenId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al aprobar la orden de compra",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Registra la recepción de una orden de compra
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="command">Datos de recepción</param>
        /// <returns>Confirmación de recepción</returns>
        [HttpPost("{id}/recibir")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecibirOrden(Guid id, [FromBody] RecibirOrdenDeCompraCommand command)
        {
            try
            {
                command.OrdenId = id;
                var resultado = await _mediator.Send(command);

                return Ok(new
                {
                    success = true,
                    message = "Orden de compra recibida exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Orden no encontrada: {OrdenId}", id);
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recibir orden de compra: {OrdenId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al recibir la orden de compra",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene las órdenes de compra pendientes
        /// </summary>
        /// <returns>Lista de órdenes pendientes</returns>
        [HttpGet("pendientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerOrdenesPendientes()
        {
            try
            {
                var query = new ObtenerOrdenesPendientesQuery();
                var resultado = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = resultado,
                    count = resultado.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes pendientes");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las órdenes pendientes",
                    error = ex.Message
                });
            }
        }
    }
}
