using MediatR;

namespace InventarioDDD.Domain.Events;

/// <summary>
/// Interfaz base para todos los eventos de dominio
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OcurridoEn { get; }
}
