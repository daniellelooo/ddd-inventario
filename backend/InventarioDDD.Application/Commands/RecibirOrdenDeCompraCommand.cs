using MediatR;

namespace InventarioDDD.Application.Commands
{
    /// <summary>
    /// Comando para recibir una orden de compra y crear los lotes correspondientes
    /// </summary>
    public class RecibirOrdenDeCompraCommand : IRequest<bool>
    {
        public Guid OrdenId { get; set; }
        public List<RecepcionLoteCommand> Lotes { get; set; } = new();
    }

    public class RecepcionLoteCommand
    {
        public Guid IngredienteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
