using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class LoteRecibidoHandler : INotificationHandler<LoteRecibidoEvent>
{
    private readonly ILogger<LoteRecibidoHandler> _logger;

    public LoteRecibidoHandler(ILogger<LoteRecibidoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LoteRecibidoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Lote recibido - Lote {LoteId} para ingrediente {IngredienteId}: Cantidad {Cantidad}",
            notification.LoteId,
            notification.IngredienteId,
            notification.Cantidad);

        return Task.CompletedTask;
    }
}
