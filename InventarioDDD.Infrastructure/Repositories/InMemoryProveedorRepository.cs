using System.Reflection;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.Infrastructure.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de Proveedores
/// </summary>
public class InMemoryProveedorRepository : IProveedorRepository
{
    private readonly Dictionary<long, Proveedor> _proveedores = new();
    private long _nextId = 1;
    private readonly object _lock = new();

    public Task<Proveedor?> ObtenerPorIdAsync(long id)
    {
        _proveedores.TryGetValue(id, out var proveedor);
        return Task.FromResult(proveedor);
    }

    public Task<List<Proveedor>> ObtenerTodosAsync()
    {
        return Task.FromResult(_proveedores.Values.ToList());
    }

    public Task<Proveedor> GuardarAsync(Proveedor proveedor)
    {
        lock (_lock)
        {
            if (proveedor.Id == 0)
            {
                var idProperty = typeof(Proveedor).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                idProperty?.SetValue(proveedor, _nextId++);
            }

            _proveedores[proveedor.Id] = proveedor;
            return Task.FromResult(proveedor);
        }
    }

    public Task EliminarAsync(long id)
    {
        _proveedores.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<Proveedor>> ObtenerActivosAsync()
    {
        var result = _proveedores.Values
            .Where(p => p.Activo)
            .ToList();
        return Task.FromResult(result);
    }
}
