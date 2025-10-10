using InventarioDDD.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventarioDDD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorRepository _proveedorRepository;
        private readonly ILogger<ProveedoresController> _logger;
        private readonly IMediator _mediator;

        public ProveedoresController(
            IProveedorRepository proveedorRepository,
            ILogger<ProveedoresController> logger,
            IMediator mediator)
        {
            _proveedorRepository = proveedorRepository;
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var proveedores = await _proveedorRepository.ObtenerTodosAsync();

                var resultado = proveedores.Select(agg => new
                {
                    id = agg.Id,
                    nombre = agg.Proveedor.Nombre,
                    nit = agg.Proveedor.NIT,
                    telefono = agg.Proveedor.Telefono,
                    email = agg.Proveedor.Email,
                    direccion = agg.Proveedor.Direccion.ObtenerDireccionCompleta(),
                    personaContacto = agg.Proveedor.PersonaContacto,
                    activo = agg.Proveedor.Activo
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores");
                return StatusCode(500, new { message = "Error al obtener proveedores", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene proveedores activos
        /// </summary>
        [HttpGet("activos")]
        public async Task<IActionResult> ObtenerActivos()
        {
            try
            {
                var proveedores = await _proveedorRepository.ObtenerActivosAsync();

                var resultado = proveedores.Select(agg => new
                {
                    id = agg.Id,
                    nombre = agg.Proveedor.Nombre,
                    nit = agg.Proveedor.NIT,
                    telefono = agg.Proveedor.Telefono,
                    email = agg.Proveedor.Email,
                    direccion = agg.Proveedor.Direccion.ObtenerDireccionCompleta(),
                    personaContacto = agg.Proveedor.PersonaContacto
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores activos");
                return StatusCode(500, new { message = "Error al obtener proveedores activos", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo proveedor
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearProveedorRequest request)
        {
            try
            {
                var comando = new Application.Commands.CrearProveedorCommand
                {
                    Nombre = request.Nombre,
                    NIT = request.NIT,
                    Telefono = request.Telefono,
                    Email = request.Email,
                    Calle = request.Calle,
                    Ciudad = request.Ciudad,
                    CodigoPostal = request.CodigoPostal,
                    Pais = request.Pais,
                    PersonaContacto = request.PersonaContacto
                };

                var proveedorId = await _mediator.Send(comando);

                return CreatedAtAction(nameof(ObtenerTodos), new { id = proveedorId }, new { id = proveedorId, message = "Proveedor creado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor");
                return StatusCode(500, new { message = "Error al crear el proveedor", error = ex.Message });
            }
        }
        /// <summary>
        /// Elimina un proveedor por id
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            try
            {
                await _proveedorRepository.EliminarAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor");
                return StatusCode(500, new { message = "Error al eliminar proveedor", error = ex.Message });
            }
        }
    }

    public class CrearProveedorRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string NIT { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Pais { get; set; } = "Colombia";
        public string PersonaContacto { get; set; } = string.Empty;
    }
}
