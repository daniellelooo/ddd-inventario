using MediatR;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Events;

namespace InventarioDDD.Infrastructure.Services;

/// <summary>
/// Implementación del Servicio de Consumo con FIFO
/// </summary>
public class ServicioDeConsumo : IServicioDeConsumo
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IPublisher _publisher;

    public ServicioDeConsumo(IIngredienteRepository ingredienteRepository, IPublisher publisher)
    {
        _ingredienteRepository = ingredienteRepository;
        _publisher = publisher;
    }

    public async Task DescontarIngredientesAsync(long pedidoId, List<long> ingredientesIds)
    {
        foreach (var ingredienteId in ingredientesIds)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

            if (ingrediente == null)
                throw new InvalidOperationException($"Ingrediente {ingredienteId} no encontrado");

            // Asumimos cantidad 1 por simplicidad; en producción vendría del pedido
            var cantidad = new CantidadDisponible(1.0, ingrediente.UnidadDeMedida);

            var stockAnterior = ingrediente.CalcularStockDisponible().Valor;

            // Aplicar FIFO (método del agregado)
            ingrediente.Consumir(cantidad);

            await _ingredienteRepository.GuardarAsync(ingrediente);

            var stockNuevo = ingrediente.CalcularStockDisponible().Valor;

            // Publicar evento
            await _publisher.Publish(new StockActualizadoEvent(
                ingredienteId, stockAnterior, stockNuevo, "CONSUMO"));

            // Si está en stock bajo, publicar alerta
            if (ingrediente.EstaEnStockBajo())
            {
                await _publisher.Publish(new AlertaStockBajoEvent(
                    ingredienteId,
                    ingrediente.Nombre,
                    stockNuevo,
                    ingrediente.RangoDeStock.Minimo));
            }
        }

        // Publicar evento de consumo
        await _publisher.Publish(new IngredientesConsumidosEvent(pedidoId, ingredientesIds));
    }

    public async Task AplicarFIFOAsync(long ingredienteId, double cantidadRequerida)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException($"Ingrediente {ingredienteId} no encontrado");

        var cantidad = new CantidadDisponible(cantidadRequerida, ingrediente.UnidadDeMedida);

        // El método Consumir del agregado ya aplica FIFO
        ingrediente.Consumir(cantidad);

        await _ingredienteRepository.GuardarAsync(ingrediente);
    }

    public async Task<bool> ValidarStockSuficienteAsync(List<long> ingredientesIds)
    {
        foreach (var ingredienteId in ingredientesIds)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

            if (ingrediente == null)
                return false;

            // Validar que tenga al menos 1 unidad
            var cantidad = new CantidadDisponible(1.0, ingrediente.UnidadDeMedida);

            if (!ingrediente.TieneStockSuficiente(cantidad))
                return false;
        }

        return true;
    }
}
