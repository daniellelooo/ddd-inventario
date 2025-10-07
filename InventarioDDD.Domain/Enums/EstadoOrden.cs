namespace InventarioDDD.Domain.Enums;

/// <summary>
/// Estados posibles de una orden de compra
/// </summary>
public enum EstadoOrden
{
    /// <summary>
    /// Orden pendiente de aprobación
    /// </summary>
    Pendiente,

    /// <summary>
    /// Orden aprobada
    /// </summary>
    Aprobada,

    /// <summary>
    /// Orden en tránsito
    /// </summary>
    EnTransito,

    /// <summary>
    /// Orden recibida completamente
    /// </summary>
    Recibida,

    /// <summary>
    /// Orden cancelada
    /// </summary>
    Cancelada,

    /// <summary>
    /// Orden parcialmente recibida
    /// </summary>
    ParcialmenteRecibida
}
