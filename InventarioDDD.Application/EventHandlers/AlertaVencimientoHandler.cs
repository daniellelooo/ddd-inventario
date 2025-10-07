using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class AlertaVencimientoHandler : INotificationHandler<AlertaVencimientoEvent>
{
    private readonly ILogger<AlertaVencimientoHandler> _logger;

    public AlertaVencimientoHandler(ILogger<AlertaVencimientoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AlertaVencimientoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "⚠️ ALERTA: Lote próximo a vencer - Ingrediente {IngredienteId} '{NombreIngrediente}', Lote {LoteId}: Vence en {DiasHastaVencimiento} días",
            notification.IngredienteId,
            notification.NombreIngrediente,
            notification.LoteId,
            notification.DiasHastaVencimiento);

        // Aquí se pueden implementar acciones como:
        // - Enviar notificación urgente al responsable
        // - Crear promoción para consumo rápido
        // - Ajustar prioridad en sistema de producción
        // - Generar alerta en sistema de gestión

        return Task.CompletedTask;
    }
}
