using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class LoteVencidoHandler : INotificationHandler<LoteVencidoEvent>
{
    private readonly ILogger<LoteVencidoHandler> _logger;

    public LoteVencidoHandler(ILogger<LoteVencidoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LoteVencidoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "⚠️ Lote vencido - Ingrediente {IngredienteId}, Lote {LoteId}, Fecha Vencimiento: {FechaVencimiento}, Cantidad Afectada: {CantidadAfectada}",
            notification.IngredienteId,
            notification.LoteId,
            notification.FechaVencimiento,
            notification.CantidadAfectada);

        // Aquí se pueden implementar acciones como:
        // - Registrar merma en inventario
        // - Notificar al departamento de control de calidad
        // - Generar reporte de pérdidas
        // - Actualizar KPIs de desperdicio

        return Task.CompletedTask;
    }
}
