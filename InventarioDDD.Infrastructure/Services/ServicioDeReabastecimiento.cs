using MediatR;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Enums;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Events;

namespace InventarioDDD.Infrastructure.Services;

/// <summary>
/// Implementación del Servicio de Reabastecimiento Automático
/// </summary>
public class ServicioDeReabastecimiento : IServicioDeReabastecimiento
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IOrdenDeCompraRepository _ordenRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IPublisher _publisher;

    public ServicioDeReabastecimiento(
        IIngredienteRepository ingredienteRepository,
        IOrdenDeCompraRepository ordenRepository,
        IProveedorRepository proveedorRepository,
        IPublisher publisher)
    {
        _ingredienteRepository = ingredienteRepository;
        _ordenRepository = ordenRepository;
        _proveedorRepository = proveedorRepository;
        _publisher = publisher;
    }

    public async Task GenerarOrdenDeCompraAutomaticaAsync()
    {
        var ingredientes = await _ingredienteRepository.ObtenerActivosAsync();
        var ingredientesParaReabastecer = ingredientes.Where(i => i.NecesitaReabastecimiento()).ToList();

        foreach (var ingrediente in ingredientesParaReabastecer)
        {
            var proveedorId = await SugerirProveedorAsync(ingrediente.Id);

            if (!proveedorId.HasValue)
                continue;

            var fechaEsperada = DateTime.Now.AddDays(7); // 7 días por defecto
            var orden = new OrdenDeCompra(0, proveedorId.Value, fechaEsperada);

            var cantidadReabastecimiento = ingrediente.CalcularCantidadReabastecimiento();

            orden.AgregarItem(
                ingrediente.Id,
                cantidadReabastecimiento,
                ingrediente.PrecioReferencia ?? new PrecioConMoneda(0, "BOB"));

            var ordenGuardada = await _ordenRepository.GuardarAsync(orden);

            // Publicar evento
            await _publisher.Publish(new OrdenDeCompraGeneradaEvent(
                ordenGuardada.Id,
                proveedorId.Value,
                new List<long> { ingrediente.Id }));
        }
    }

    public async Task<double> CalcularPuntoDeReordenAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException($"Ingrediente {ingredienteId} no encontrado");

        // Punto de reorden = Stock Mínimo + (Consumo Diario Promedio * Días de Entrega)
        var stockMinimo = ingrediente.RangoDeStock.Minimo;
        var consumoDiarioPromedio = await CalcularConsumoDiarioPromedioAsync(ingrediente);
        var diasEntrega = 7; // Asumir 7 días

        return stockMinimo + (consumoDiarioPromedio * diasEntrega);
    }

    public async Task<long?> SugerirProveedorAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            return null;

        if (!ingrediente.ProveedoresIds.Any())
            return null;

        // Por ahora retornar el primer proveedor activo
        foreach (var proveedorId in ingrediente.ProveedoresIds)
        {
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId);
            if (proveedor != null && proveedor.Activo)
                return proveedorId;
        }

        return ingrediente.ProveedoresIds.FirstOrDefault();
    }

    private async Task<double> CalcularConsumoDiarioPromedioAsync(Domain.Aggregates.Ingrediente ingrediente)
    {
        // Simplificado: asumir consumo basado en la diferencia entre óptimo y actual
        await Task.CompletedTask;
        return (ingrediente.RangoDeStock.Optimo - ingrediente.CalcularStockDisponible().Valor) / 30.0;
    }
}
