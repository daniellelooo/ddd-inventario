using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class AlertaStockBajoHandler : INotificationHandler<AlertaStockBajoEvent>
{
    private readonly ILogger<AlertaStockBajoHandler> _logger;

    public AlertaStockBajoHandler(ILogger<AlertaStockBajoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AlertaStockBajoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "⚠️ ALERTA: Stock bajo para ingrediente {IngredienteId} '{Nombre}' - Stock actual: {StockActual}, Mínimo: {StockMinimo}",
            notification.IngredienteId,
            notification.NombreIngrediente,
            notification.StockActual,
            notification.StockMinimo);

        // Aquí se pueden implementar acciones como:
        // - Enviar email a responsables de compras
        // - Crear notificación push
        // - Generar orden de compra automática
        // - Registrar en sistema de alertas

        return Task.CompletedTask;
    }
}
