using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class OrdenDeCompraGeneradaHandler : INotificationHandler<OrdenDeCompraGeneradaEvent>
{
    private readonly ILogger<OrdenDeCompraGeneradaHandler> _logger;

    public OrdenDeCompraGeneradaHandler(ILogger<OrdenDeCompraGeneradaHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrdenDeCompraGeneradaEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Orden de compra generada - ID: {OrdenId}, Proveedor: {ProveedorId}, Ingredientes: {CantidadIngredientes}",
            notification.OrdenId,
            notification.ProveedorId,
            notification.IngredientesIds.Count);

        // Aquí se pueden implementar acciones como:
        // - Enviar email al proveedor
        // - Notificar al departamento de compras
        // - Actualizar presupuesto
        // - Generar documento PDF

        return Task.CompletedTask;
    }
}
