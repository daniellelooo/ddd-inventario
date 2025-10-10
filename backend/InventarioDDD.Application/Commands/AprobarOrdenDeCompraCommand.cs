using MediatR;

namespace InventarioDDD.Application.Commands
{
    /// <summary>
    /// Comando para aprobar una orden de compra
    /// </summary>
    public class AprobarOrdenDeCompraCommand : IRequest<bool>
    {
        public Guid OrdenId { get; set; }
        public Guid? AprobadorId { get; set; }
    }
}
