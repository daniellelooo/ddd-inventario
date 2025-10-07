using InventarioDDD.Domain.Entities;

namespace InventarioDDD.Domain.Repositories;

/// <summary>
/// Repositorio para la entidad Proveedor
/// </summary>
public interface IProveedorRepository
{
    Task<Proveedor?> ObtenerPorIdAsync(long id);
    Task<List<Proveedor>> ObtenerTodosAsync();
    Task<Proveedor> GuardarAsync(Proveedor proveedor);
    Task EliminarAsync(long id);
    Task<List<Proveedor>> ObtenerActivosAsync();
}
