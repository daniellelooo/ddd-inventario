using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Entities;

/// <summary>
/// Entidad que representa un lote de ingrediente (parte del agregado Ingrediente)
/// </summary>
public class Lote
{
    public long Id { get; private set; }
    public CantidadDisponible Cantidad { get; private set; }
    public FechaVencimiento FechaVencimiento { get; private set; }
    public DateTime FechaRecepcion { get; private set; }
    public long ProveedorId { get; private set; }
    public bool Agotado { get; private set; }

    // Constructor package-private (solo el agregado puede crear lotes)
    internal Lote(long id, CantidadDisponible cantidad, FechaVencimiento fechaVencimiento,
                  DateTime fechaRecepcion, long proveedorId, bool agotado = false)
    {
        Id = id;
        Cantidad = cantidad ?? throw new ArgumentNullException(nameof(cantidad));
        FechaVencimiento = fechaVencimiento ?? throw new ArgumentNullException(nameof(fechaVencimiento));
        FechaRecepcion = fechaRecepcion;
        ProveedorId = proveedorId;
        Agotado = agotado;
    }

    // Métodos de negocio
    public void MarcarAgotado()
    {
        Agotado = true;
    }

    public void ActualizarCantidad(CantidadDisponible nuevaCantidad)
    {
        if (nuevaCantidad == null)
            throw new ArgumentNullException(nameof(nuevaCantidad));

        Cantidad = nuevaCantidad;

        if (nuevaCantidad.Valor <= 0)
            MarcarAgotado();
    }

    public bool EstaVencido() => FechaVencimiento.EstaVencido();

    public bool EstaDisponible() => !Agotado && !EstaVencido();

    public bool ProximoAVencer(int dias) => FechaVencimiento.ProximoAVencer(dias);
}
