using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Interfaces;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para aprobar una orden de compra
    /// </summary>
    public class AprobarOrdenDeCompraHandler : IRequestHandler<AprobarOrdenDeCompraCommand, bool>
    {
        private readonly IOrdenDeCompraRepository _ordenRepository;

        public AprobarOrdenDeCompraHandler(IOrdenDeCompraRepository ordenRepository)
        {
            _ordenRepository = ordenRepository;
        }

        public async Task<bool> Handle(AprobarOrdenDeCompraCommand request, CancellationToken cancellationToken)
        {
            // Obtener la orden (es un aggregate)
            var ordenAggregate = await _ordenRepository.ObtenerPorIdAsync(request.OrdenId);
            if (ordenAggregate == null)
                throw new ArgumentException($"Orden de compra con ID {request.OrdenId} no encontrada");

            // Aprobar la orden (método del aggregate)
            var aprobadorId = request.AprobadorId ?? Guid.Empty;
            ordenAggregate.Aprobar(aprobadorId);

            // Guardar cambios
            await _ordenRepository.GuardarAsync(ordenAggregate);

            return true;
        }
    }
}
