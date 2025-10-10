using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Entities
{
    public class Lote
    {
        public Guid Id { get; private set; }
        public string Codigo { get; private set; }
        public Guid IngredienteId { get; private set; }
        public Guid ProveedorId { get; private set; }
        public decimal CantidadInicial { get; private set; }
        public decimal CantidadDisponible { get; private set; }
        public FechaVencimiento FechaVencimiento { get; private set; }
        public DateTime FechaRecepcion { get; private set; }
        public PrecioConMoneda PrecioUnitario { get; private set; }
        public Guid? OrdenDeCompraId { get; private set; }
        public bool Vencido => FechaVencimiento.EstaVencido;

        private Lote()
        {
            Codigo = string.Empty;
            FechaVencimiento = null!;
            PrecioUnitario = null!;
        }

        public Lote(string codigo, Guid ingredienteId, Guid proveedorId, decimal cantidadInicial,
                   FechaVencimiento fechaVencimiento, PrecioConMoneda precioUnitario,
                   Guid? ordenDeCompraId = null)
        {
            if (cantidadInicial <= 0)
                throw new ArgumentException("La cantidad inicial debe ser mayor a cero");

            Id = Guid.NewGuid();
            Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
            IngredienteId = ingredienteId;
            ProveedorId = proveedorId;
            CantidadInicial = cantidadInicial;
            CantidadDisponible = cantidadInicial;
            FechaVencimiento = fechaVencimiento ?? throw new ArgumentNullException(nameof(fechaVencimiento));
            FechaRecepcion = DateTime.UtcNow;
            PrecioUnitario = precioUnitario ?? throw new ArgumentNullException(nameof(precioUnitario));
            OrdenDeCompraId = ordenDeCompraId;
        }

        public void Consumir(decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a consumir debe ser mayor a cero");

            if (cantidad > CantidadDisponible)
                throw new InvalidOperationException("No hay suficiente cantidad disponible en el lote");

            if (Vencido)
                throw new InvalidOperationException("No se puede consumir de un lote vencido");

            CantidadDisponible -= cantidad;
        }

        public bool EstaVencido()
        {
            return FechaVencimiento.EstaVencido;
        }

        public bool EstaProximoAVencer(int diasAnticipacion = 7)
        {
            return FechaVencimiento.VenceEnLosPróximosDias(diasAnticipacion);
        }

        public decimal CalcularValorInventario()
        {
            return CantidadDisponible * PrecioUnitario.Valor;
        }
    }
}