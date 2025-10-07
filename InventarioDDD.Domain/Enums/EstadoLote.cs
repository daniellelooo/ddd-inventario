namespace InventarioDDD.Domain.Enums;

/// <summary>
/// Estados posibles de un lote de ingredientes
/// </summary>
public enum EstadoLote
{
    /// <summary>
    /// Lote disponible para consumo
    /// </summary>
    Disponible,

    /// <summary>
    /// Lote agotado (cantidad = 0)
    /// </summary>
    Agotado,

    /// <summary>
    /// Lote vencido
    /// </summary>
    Vencido,

    /// <summary>
    /// Lote reservado para un pedido
    /// </summary>
    Reservado
}
