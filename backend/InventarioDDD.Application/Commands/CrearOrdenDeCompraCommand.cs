using MediatR;

namespace InventarioDDD.Application.Commands
{
    /// <summary>
    /// Comando para crear una nueva orden de compra
    /// </summary>
    public class CrearOrdenDeCompraCommand : IRequest<Guid>
    {
        public Guid ProveedorId { get; set; }
        public DateTime? FechaEntregaEsperada { get; set; }
        public List<DetalleOrdenCommand> Detalles { get; set; } = new();
    }

    public class DetalleOrdenCommand
    {
        public Guid IngredienteId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Moneda { get; set; } = "COP";
    }
}
