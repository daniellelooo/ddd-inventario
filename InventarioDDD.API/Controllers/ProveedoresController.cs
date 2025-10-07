using MediatR;
using Microsoft.AspNetCore.Mvc;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorRepository _repository;

    public ProveedoresController(IProveedorRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Obtiene todos los proveedores
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProveedorDto>>> ObtenerTodos()
    {
        var proveedores = await _repository.ObtenerTodosAsync();
        return Ok(proveedores.Select(MapearADto).ToList());
    }

    /// <summary>
    /// Obtiene un proveedor por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProveedorDto>> ObtenerPorId(long id)
    {
        var proveedor = await _repository.ObtenerPorIdAsync(id);

        if (proveedor == null)
            return NotFound($"Proveedor {id} no encontrado");

        return Ok(MapearADto(proveedor));
    }

    /// <summary>
    /// Registra un nuevo proveedor
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<long>> Registrar([FromBody] RegistrarProveedorRequest request)
    {
        var proveedor = new Proveedor(
            0,
            request.Nombre,
            request.Contacto,
            request.Telefono,
            request.Direccion,
            request.Email,
            true);

        var guardado = await _repository.GuardarAsync(proveedor);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = guardado.Id }, guardado.Id);
    }

    /// <summary>
    /// Actualiza los datos de un proveedor
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Actualizar(long id, [FromBody] ActualizarProveedorRequest request)
    {
        var proveedor = await _repository.ObtenerPorIdAsync(id);

        if (proveedor == null)
            return NotFound();

        proveedor.ActualizarContacto(request.Contacto, request.Telefono, request.Email);
        proveedor.ActualizarDireccion(request.Direccion);

        await _repository.GuardarAsync(proveedor);

        return NoContent();
    }

    /// <summary>
    /// Activa o desactiva un proveedor
    /// </summary>
    [HttpPatch("{id}/estado")]
    public async Task<ActionResult> CambiarEstado(long id, [FromQuery] bool activo)
    {
        var proveedor = await _repository.ObtenerPorIdAsync(id);

        if (proveedor == null)
            return NotFound();

        if (activo)
            proveedor.Activar();
        else
            proveedor.Desactivar();

        await _repository.GuardarAsync(proveedor);

        return NoContent();
    }

    private static ProveedorDto MapearADto(Proveedor proveedor)
    {
        return new ProveedorDto(
            proveedor.Id,
            proveedor.Nombre,
            proveedor.Contacto,
            proveedor.Telefono,
            proveedor.Direccion,
            proveedor.Email,
            proveedor.Activo);
    }
}

public record ProveedorDto(long Id, string Nombre, string Contacto, string Telefono, string Direccion, string Email, bool Activo);
public record RegistrarProveedorRequest(string Nombre, string Contacto, string Telefono, string Direccion, string Email);
public record ActualizarProveedorRequest(string Contacto, string Telefono, string Direccion, string Email);
