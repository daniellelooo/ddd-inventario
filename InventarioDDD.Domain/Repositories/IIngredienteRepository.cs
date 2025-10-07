using InventarioDDD.Domain.Aggregates;

namespace InventarioDDD.Domain.Repositories;

/// <summary>
/// Repositorio para el agregado Ingrediente
/// </summary>
public interface IIngredienteRepository
{
    Task<Ingrediente?> ObtenerPorIdAsync(long id);
    Task<List<Ingrediente>> ObtenerTodosAsync();
    Task<Ingrediente> GuardarAsync(Ingrediente ingrediente);
    Task EliminarAsync(long id);
    Task<List<Ingrediente>> ObtenerPorCategoriaAsync(string categoria);
    Task<List<Ingrediente>> ObtenerActivosAsync();
    Task<List<Ingrediente>> ObtenerConStockBajoAsync();
}
