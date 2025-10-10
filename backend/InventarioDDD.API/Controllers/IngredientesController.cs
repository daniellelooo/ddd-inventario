using InventarioDDD.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventarioDDD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class IngredientesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IngredientesController> _logger;

        public IngredientesController(IMediator mediator, ILogger<IngredientesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de ingredientes que requieren reabastecimiento
        /// </summary>
        /// <returns>Lista de ingredientes con stock bajo</returns>
        [HttpGet("reabastecer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerIngredientesParaReabastecer()
        {
            try
            {
                var query = new ObtenerIngredientesParaReabastecerQuery();
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
                _logger.LogError(ex, "Error al obtener ingredientes para reabastecer");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los ingredientes",
                    error = ex.Message
                });
            }
        }
    }
}
