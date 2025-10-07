using MediatR;
using InventarioDDD.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Application.EventHandlers;

public class IngredienteRegistradoHandler : INotificationHandler<IngredienteRegistradoEvent>
{
    private readonly ILogger<IngredienteRegistradoHandler> _logger;

    public IngredienteRegistradoHandler(ILogger<IngredienteRegistradoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(IngredienteRegistradoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Ingrediente registrado: ID {IngredienteId}, Nombre: {Nombre}",
            notification.IngredienteId,
            notification.Nombre);

        // Aquí se pueden agregar otras acciones como:
        // - Enviar notificación a un sistema externo
        // - Registrar en un log de auditoría
        // - Actualizar cachés

        return Task.CompletedTask;
    }
}
