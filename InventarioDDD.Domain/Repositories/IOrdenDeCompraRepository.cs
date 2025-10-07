using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Repositories;

/// <summary>
/// Repositorio para la entidad OrdenDeCompra
/// </summary>
public interface IOrdenDeCompraRepository
{
    Task<OrdenDeCompra?> ObtenerPorIdAsync(long id);
    Task<List<OrdenDeCompra>> ObtenerTodosAsync();
    Task<OrdenDeCompra> GuardarAsync(OrdenDeCompra orden);
    Task EliminarAsync(long id);
    Task<List<OrdenDeCompra>> ObtenerPorEstadoAsync(EstadoOrden estado);
    Task<List<OrdenDeCompra>> ObtenerPorProveedorAsync(long proveedorId);
}
