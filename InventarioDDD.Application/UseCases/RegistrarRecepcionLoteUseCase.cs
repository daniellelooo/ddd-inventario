using MediatR;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Events;

namespace InventarioDDD.Application.UseCases;

// ========================================
// COMANDO
// ========================================

/// <summary>
/// Comando para registrar la recepción de un lote
/// </summary>
public record RegistrarRecepcionLoteCommand(
    long IngredienteId,
    double Cantidad,
    DateTime FechaVencimiento,
    long ProveedorId
) : IRequest<long>;

// ========================================
// HANDLER
// ========================================

/// <summary>
/// Handler para registrar recepción de lote
/// </summary>
public class RegistrarRecepcionLoteHandler : IRequestHandler<RegistrarRecepcionLoteCommand, long>
{
    private readonly IIngredienteRepository _repository;
    private readonly IPublisher _publisher;

    public RegistrarRecepcionLoteHandler(IIngredienteRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<long> Handle(RegistrarRecepcionLoteCommand request, CancellationToken cancellationToken)
    {
        // Obtener el agregado
        var ingrediente = await _repository.ObtenerPorIdAsync(request.IngredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException($"Ingrediente {request.IngredienteId} no encontrado");

        var stockAnterior = ingrediente.CalcularStockDisponible().Valor;

        // Crear value objects
        var cantidad = new CantidadDisponible(request.Cantidad, ingrediente.UnidadDeMedida);
        var fechaVencimiento = new FechaVencimiento(request.FechaVencimiento);

        // Agregar lote (método del agregado)
        var nuevoLote = ingrediente.AgregarLote(cantidad, fechaVencimiento, request.ProveedorId);

        // Guardar el agregado
        await _repository.GuardarAsync(ingrediente);

        var stockNuevo = ingrediente.CalcularStockDisponible().Valor;

        // Publicar eventos
        await _publisher.Publish(
            new LoteRecibidoEvent(nuevoLote.Id, request.IngredienteId, request.Cantidad),
            cancellationToken
        );

        await _publisher.Publish(
            new StockActualizadoEvent(ingrediente.Id, stockAnterior, stockNuevo, "ENTRADA"),
            cancellationToken
        );

        return nuevoLote.Id;
    }
}
