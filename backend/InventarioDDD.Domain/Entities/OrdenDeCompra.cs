using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Entities
{
    public class OrdenDeCompra
    {
        public Guid Id { get; private set; }
        public string Numero { get; private set; }
        public Guid IngredienteId { get; private set; }
        public Guid ProveedorId { get; private set; }
        public CantidadDisponible Cantidad { get; private set; }
        public PrecioConMoneda PrecioUnitario { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime FechaEsperada { get; private set; }
        public DateTime? FechaRecepcion { get; private set; }
        public EstadoOrden Estado { get; private set; }
        public string Observaciones { get; private set; }

        // Propiedades de navegación
        public Ingrediente? Ingrediente { get; private set; }
        public Proveedor? Proveedor { get; private set; }

        private OrdenDeCompra()
        {
            Numero = string.Empty;
            Cantidad = null!;
            PrecioUnitario = null!;
            Observaciones = string.Empty;
        }

        public OrdenDeCompra(string numero, Guid ingredienteId, Guid proveedorId,
                           CantidadDisponible cantidad, PrecioConMoneda precioUnitario,
                           DateTime fechaEsperada, string observaciones = "")
        {
            Id = Guid.NewGuid();
            Numero = numero ?? throw new ArgumentNullException(nameof(numero));
            IngredienteId = ingredienteId;
            ProveedorId = proveedorId;
            Cantidad = cantidad ?? throw new ArgumentNullException(nameof(cantidad));
            PrecioUnitario = precioUnitario ?? throw new ArgumentNullException(nameof(precioUnitario));
            FechaCreacion = DateTime.UtcNow;
            FechaEsperada = fechaEsperada;
            Estado = EstadoOrden.Pendiente;
            Observaciones = observaciones ?? "";
        }

        public void Aprobar()
        {
            if (Estado != EstadoOrden.Pendiente)
                throw new InvalidOperationException("Solo se pueden aprobar órdenes pendientes");

            Estado = EstadoOrden.Aprobada;
        }

        public void MarcarEnvioPendiente()
        {
            if (Estado != EstadoOrden.Aprobada)
                throw new InvalidOperationException("Solo se pueden marcar como envío pendiente las órdenes aprobadas");

            Estado = EstadoOrden.EnvioPendiente;
        }

        public void MarcarRecibida(DateTime? fechaRecepcion = null)
        {
            if (Estado != EstadoOrden.EnvioPendiente && Estado != EstadoOrden.Aprobada)
                throw new InvalidOperationException("Solo se pueden marcar como recibidas las órdenes en envío pendiente o aprobadas");

            Estado = EstadoOrden.Recibida;
            FechaRecepcion = fechaRecepcion ?? DateTime.UtcNow;
        }

        public void Cancelar()
        {
            if (Estado == EstadoOrden.Recibida)
                throw new InvalidOperationException("No se pueden cancelar órdenes ya recibidas");

            Estado = EstadoOrden.Cancelada;
        }

        public void ActualizarObservaciones(string nuevasObservaciones)
        {
            Observaciones = nuevasObservaciones ?? "";
        }

        public PrecioConMoneda CalcularTotal()
        {
            return new PrecioConMoneda(Cantidad.Valor * PrecioUnitario.Valor, PrecioUnitario.Moneda);
        }

        public bool PuedeSerModificada()
        {
            return Estado == EstadoOrden.Pendiente;
        }

        public bool EstaVencida()
        {
            return Estado != EstadoOrden.Recibida &&
                   Estado != EstadoOrden.Cancelada &&
                   FechaEsperada < DateTime.UtcNow;
        }
    }
}