using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class StockActualizadoHandler : INotificationHandler<StockActualizadoEvent>
{
    private readonly ILogger<StockActualizadoHandler> _logger;

    public StockActualizadoHandler(ILogger<StockActualizadoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(StockActualizadoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Stock actualizado - Ingrediente {IngredienteId}: Stock Anterior {StockAnterior} → Stock Nuevo {StockNuevo}",
            notification.IngredienteId,
            notification.StockAnterior,
            notification.StockNuevo);

        // Aquí se puede implementar lógica adicional como:
        // - Actualizar dashboards en tiempo real
        // - Notificar a sistemas de reportería
        // - Sincronizar con sistemas externos

        return Task.CompletedTask;
    }
}
