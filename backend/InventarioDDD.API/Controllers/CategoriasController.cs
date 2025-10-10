using InventarioDDD.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventarioDDD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ILogger<CategoriasController> _logger;
        private readonly IMediator _mediator;

        public CategoriasController(
            ICategoriaRepository categoriaRepository,
            ILogger<CategoriasController> logger,
            IMediator mediator)
        {
            _categoriaRepository = categoriaRepository;
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene todas las categorías
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            try
            {
                var categorias = await _categoriaRepository.ObtenerTodosAsync();

                var resultado = categorias.Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    descripcion = c.Descripcion,
                    activa = c.Activa,
                    fechaCreacion = c.FechaCreacion
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { message = "Error al obtener categorías", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene categorías activas
        /// </summary>
        [HttpGet("activas")]
        public async Task<IActionResult> ObtenerActivas()
        {
            try
            {
                var categorias = await _categoriaRepository.ObtenerActivasAsync();

                var resultado = categorias.Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre,
                    descripcion = c.Descripcion
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías activas");
                return StatusCode(500, new { message = "Error al obtener categorías activas", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearCategoriaRequest request)
        {
            try
            {
                var comando = new Application.Commands.CrearCategoriaCommand
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion
                };

                var categoriaId = await _mediator.Send(comando);

                return CreatedAtAction(nameof(ObtenerTodas), new { id = categoriaId }, new { id = categoriaId, message = "Categoría creada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return StatusCode(500, new { message = "Error al crear la categoría", error = ex.Message });
            }
        }
    }

    public class CrearCategoriaRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
