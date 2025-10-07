using System.Reflection;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.Infrastructure.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de Ingredientes
/// </summary>
public class InMemoryIngredienteRepository : IIngredienteRepository
{
    private readonly Dictionary<long, Ingrediente> _ingredientes = new();
    private long _nextId = 1;
    private readonly object _lock = new();

    public Task<Ingrediente?> ObtenerPorIdAsync(long id)
    {
        _ingredientes.TryGetValue(id, out var ingrediente);
        return Task.FromResult(ingrediente);
    }

    public Task<List<Ingrediente>> ObtenerTodosAsync()
    {
        return Task.FromResult(_ingredientes.Values.ToList());
    }

    public Task<Ingrediente> GuardarAsync(Ingrediente ingrediente)
    {
        lock (_lock)
        {
            if (ingrediente.Id == 0)
            {
                // Asignar ID usando reflection (ya que es private set)
                var idProperty = typeof(Ingrediente).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                idProperty?.SetValue(ingrediente, _nextId++);
            }

            _ingredientes[ingrediente.Id] = ingrediente;
            return Task.FromResult(ingrediente);
        }
    }

    public Task EliminarAsync(long id)
    {
        _ingredientes.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<Ingrediente>> ObtenerPorCategoriaAsync(string categoria)
    {
        var result = _ingredientes.Values
            .Where(i => i.Categoria.Nombre.Equals(categoria, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<Ingrediente>> ObtenerActivosAsync()
    {
        var result = _ingredientes.Values
            .Where(i => i.Activo)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<Ingrediente>> ObtenerConStockBajoAsync()
    {
        var result = _ingredientes.Values
            .Where(i => i.Activo && i.EstaEnStockBajo())
            .ToList();
        return Task.FromResult(result);
    }
}
