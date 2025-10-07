using System.Reflection;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Enums;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.Infrastructure.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de Órdenes de Compra
/// </summary>
public class InMemoryOrdenDeCompraRepository : IOrdenDeCompraRepository
{
    private readonly Dictionary<long, OrdenDeCompra> _ordenes = new();
    private long _nextId = 1;
    private readonly object _lock = new();

    public Task<OrdenDeCompra?> ObtenerPorIdAsync(long id)
    {
        _ordenes.TryGetValue(id, out var orden);
        return Task.FromResult(orden);
    }

    public Task<List<OrdenDeCompra>> ObtenerTodosAsync()
    {
        return Task.FromResult(_ordenes.Values.ToList());
    }

    public Task<OrdenDeCompra> GuardarAsync(OrdenDeCompra orden)
    {
        lock (_lock)
        {
            if (orden.Id == 0)
            {
                var idProperty = typeof(OrdenDeCompra).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                idProperty?.SetValue(orden, _nextId++);
            }

            _ordenes[orden.Id] = orden;
            return Task.FromResult(orden);
        }
    }

    public Task EliminarAsync(long id)
    {
        _ordenes.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<OrdenDeCompra>> ObtenerPorEstadoAsync(EstadoOrden estado)
    {
        var result = _ordenes.Values
            .Where(o => o.Estado == estado)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<OrdenDeCompra>> ObtenerPorProveedorAsync(long proveedorId)
    {
        var result = _ordenes.Values
            .Where(o => o.ProveedorId == proveedorId)
            .ToList();
        return Task.FromResult(result);
    }
}
