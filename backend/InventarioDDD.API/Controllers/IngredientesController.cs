using InventarioDDD.Application.Queries;
using InventarioDDD.Domain.Interfaces;
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
        private readonly IIngredienteRepository _ingredienteRepository;

        public IngredientesController(
            IMediator mediator, 
            ILogger<IngredientesController> logger,
            IIngredienteRepository ingredienteRepository)
        {
            _mediator = mediator;
            _logger = logger;
            _ingredienteRepository = ingredienteRepository;
        }

        /// <summary>
        /// Obtiene todos los ingredientes
        /// </summary>
        /// <returns>Lista de todos los ingredientes</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();

                var resultado = ingredientes.Select(agg => new
                {
                    id = agg.Id,
                    nombre = agg.Ingrediente.Nombre,
                    descripcion = agg.Ingrediente.Descripcion,
                    unidadMedida = agg.Ingrediente.UnidadDeMedida.Simbolo,
                    cantidadEnStock = agg.Ingrediente.CantidadEnStock.Valor,
                    stockMinimo = agg.Ingrediente.RangoDeStock.StockMinimo,
                    stockMaximo = agg.Ingrediente.RangoDeStock.StockMaximo,
                    categoriaId = agg.Ingrediente.CategoriaId,
                    activo = agg.Ingrediente.Activo,
                    tieneStockBajo = agg.TieneStockBajo()
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ingredientes");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener los ingredientes",
                    error = ex.Message
                });
            }
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

        /// <summary>
        /// Crea un nuevo ingrediente
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)
        {
            try
            {
                var comando = new Application.Commands.CrearIngredienteCommand
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    UnidadMedida = request.UnidadMedida,
                    StockMinimo = request.StockMinimo,
                    StockMaximo = request.StockMaximo,
                    CategoriaId = request.CategoriaId
                };

                var ingredienteId = await _mediator.Send(comando);

                return CreatedAtAction(nameof(ObtenerTodos), new { id = ingredienteId }, new { id = ingredienteId, message = "Ingrediente creado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ingrediente");
                return StatusCode(500, new { message = "Error al crear el ingrediente", error = ex.Message });
            }
        }
        /// <summary>
        /// Elimina un ingrediente por id
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            try
            {
                await _ingredienteRepository.EliminarAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ingrediente");
                return StatusCode(500, new { message = "Error al eliminar ingrediente", error = ex.Message });
            }
        }
    }

    public class CrearIngredienteRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = "kg";
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
