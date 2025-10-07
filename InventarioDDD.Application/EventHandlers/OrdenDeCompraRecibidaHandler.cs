using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class OrdenDeCompraRecibidaHandler : INotificationHandler<OrdenDeCompraRecibidaEvent>
{
    private readonly ILogger<OrdenDeCompraRecibidaHandler> _logger;

    public OrdenDeCompraRecibidaHandler(ILogger<OrdenDeCompraRecibidaHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrdenDeCompraRecibidaEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Orden de compra recibida - ID: {OrdenId}, Proveedor: {ProveedorId}",
            notification.OrdenId,
            notification.ProveedorId);

        // Aquí se pueden implementar acciones como:
        // - Actualizar estado en sistema de compras
        // - Notificar al departamento de contabilidad
        // - Generar comprobante de recepción
        // - Actualizar métricas de proveedores (tiempo de entrega)

        return Task.CompletedTask;
    }
}
