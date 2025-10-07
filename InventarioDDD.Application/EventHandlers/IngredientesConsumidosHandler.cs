using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class IngredientesConsumidosHandler : INotificationHandler<IngredientesConsumidosEvent>
{
    private readonly ILogger<IngredientesConsumidosHandler> _logger;

    public IngredientesConsumidosHandler(ILogger<IngredientesConsumidosHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(IngredientesConsumidosEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Ingredientes consumidos - Pedido {PedidoId}: {CantidadIngredientes} ingredientes consumidos",
            notification.PedidoId,
            notification.IngredientesIds.Count);

        // Aquí se pueden implementar acciones como:
        // - Actualizar estadísticas de consumo
        // - Calcular costos del pedido
        // - Actualizar proyecciones de demanda
        // - Registrar en histórico de consumo

        return Task.CompletedTask;
    }
}
