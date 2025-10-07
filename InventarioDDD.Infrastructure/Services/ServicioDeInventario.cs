using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Infrastructure.Services;

/// <summary>
/// Implementación del Servicio de Inventario
/// </summary>
public class ServicioDeInventario : IServicioDeInventario
{
    private readonly IIngredienteRepository _ingredienteRepository;

    public ServicioDeInventario(IIngredienteRepository ingredienteRepository)
    {
        _ingredienteRepository = ingredienteRepository;
    }

    public async Task<CantidadDisponible> ConsultarStockDisponibleAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException($"Ingrediente {ingredienteId} no encontrado");

        return ingrediente.CalcularStockDisponible();
    }

    public async Task<bool> VerificarDisponibilidadParaPedidoAsync(long pedidoId)
    {
        // TODO: Implementar cuando exista entidad Pedido completa
        return await Task.FromResult(true);
    }

    public async Task<List<Ingrediente>> ObtenerIngredientesPorCategoriaAsync(string categoria)
    {
        return await _ingredienteRepository.ObtenerPorCategoriaAsync(categoria);
    }

    public async Task<decimal> CalcularValoracionInventarioAsync()
    {
        var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();

        decimal valoracionTotal = 0m;

        foreach (var ingrediente in ingredientes.Where(i => i.Activo))
        {
            var stockDisponible = ingrediente.CalcularStockDisponible().Valor;
            var precio = ingrediente.PrecioReferencia?.Monto ?? 0m;

            valoracionTotal += (decimal)stockDisponible * precio;
        }

        return valoracionTotal;
    }

    public async Task<List<Lote>> ObtenerLotesPorIngredienteAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException($"Ingrediente {ingredienteId} no encontrado");

        return ingrediente.Lotes.ToList();
    }
}
