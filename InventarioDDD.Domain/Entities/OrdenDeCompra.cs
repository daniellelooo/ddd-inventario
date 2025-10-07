using InventarioDDD.Domain.Enums;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Entities;

/// <summary>
/// Entidad Orden de Compra
/// </summary>
public class OrdenDeCompra
{
    private readonly List<ItemOrdenCompra> _items = new();

    public long Id { get; private set; }
    public long ProveedorId { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public DateTime? FechaEsperada { get; private set; }
    public EstadoOrden Estado { get; private set; }
    public IReadOnlyList<ItemOrdenCompra> Items => _items.AsReadOnly();

    // Constructor para EF Core
    private OrdenDeCompra() { }

    public OrdenDeCompra(long id, long proveedorId, DateTime fechaEsperada)
    {
        if (proveedorId <= 0)
            throw new ArgumentException("El proveedor es requerido", nameof(proveedorId));

        Id = id;
        ProveedorId = proveedorId;
        FechaSolicitud = DateTime.Now;
        FechaEsperada = fechaEsperada;
        Estado = EstadoOrden.Pendiente;
    }

    public void AgregarItem(long ingredienteId, CantidadDisponible cantidad, PrecioConMoneda precioUnitario)
    {
        if (ingredienteId <= 0)
            throw new ArgumentException("El ingrediente es requerido", nameof(ingredienteId));

        if (cantidad == null || !cantidad.EsPositivo())
            throw new ArgumentException("La cantidad debe ser positiva", nameof(cantidad));

        if (precioUnitario == null || precioUnitario.Monto <= 0)
            throw new ArgumentException("El precio debe ser positivo", nameof(precioUnitario));

        var item = new ItemOrdenCompra(ingredienteId, cantidad, precioUnitario);
        _items.Add(item);
    }

    public void Aprobar()
    {
        if (Estado != EstadoOrden.Pendiente)
            throw new InvalidOperationException("Solo se pueden aprobar órdenes pendientes");

        if (!_items.Any())
            throw new InvalidOperationException("No se puede aprobar una orden sin items");

        Estado = EstadoOrden.Aprobada;
    }

    public void Cancelar()
    {
        if (Estado == EstadoOrden.Recibida)
            throw new InvalidOperationException("No se puede cancelar una orden ya recibida");

        Estado = EstadoOrden.Cancelada;
    }

    public void MarcarEnTransito()
    {
        if (Estado != EstadoOrden.Aprobada)
            throw new InvalidOperationException("Solo se pueden marcar en tránsito órdenes aprobadas");

        Estado = EstadoOrden.EnTransito;
    }

    public void MarcarRecibida()
    {
        if (Estado != EstadoOrden.EnTransito)
            throw new InvalidOperationException("Solo se pueden marcar como recibidas órdenes en tránsito");

        Estado = EstadoOrden.Recibida;
    }

    public void MarcarParcialmenteRecibida()
    {
        if (Estado != EstadoOrden.EnTransito)
            throw new InvalidOperationException("Solo se pueden marcar parcialmente órdenes en tránsito");

        Estado = EstadoOrden.ParcialmenteRecibida;
    }

    public decimal CalcularTotal()
    {
        return _items.Sum(item => item.PrecioUnitario.Monto * (decimal)item.Cantidad.Valor);
    }
}

/// <summary>
/// Value Object que representa un item de la orden de compra
/// </summary>
public record ItemOrdenCompra(long IngredienteId, CantidadDisponible Cantidad, PrecioConMoneda PrecioUnitario);
