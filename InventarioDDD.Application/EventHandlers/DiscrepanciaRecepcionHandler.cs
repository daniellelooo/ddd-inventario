using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class DiscrepanciaRecepcionHandler : INotificationHandler<DiscrepanciaRecepcionEvent>
{
    private readonly ILogger<DiscrepanciaRecepcionHandler> _logger;

    public DiscrepanciaRecepcionHandler(ILogger<DiscrepanciaRecepcionHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DiscrepanciaRecepcionEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogError(
            "❌ DISCREPANCIA EN RECEPCIÓN - Orden {OrdenId}, Ingrediente {IngredienteId}: {Descripcion}",
            notification.OrdenId,
            notification.IngredienteId,
            notification.Descripcion);

        // Aquí se pueden implementar acciones como:
        // - Crear ticket en sistema de reclamos
        // - Notificar al proveedor sobre la discrepancia
        // - Iniciar proceso de devolución o ajuste
        // - Actualizar métricas de calidad del proveedor
        // - Generar reporte para auditoría

        return Task.CompletedTask;
    }
}
