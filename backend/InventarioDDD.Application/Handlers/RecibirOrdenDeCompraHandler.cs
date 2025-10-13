using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;
using InventarioDDD.Domain.ValueObjects;
using MediatR;

namespace InventarioDDD.Application.Handlers
{

    public class RecibirOrdenDeCompraHandler : IRequestHandler<RecibirOrdenDeCompraCommand, bool>
    {
        private readonly IOrdenDeCompraRepository _ordenRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly ServicioDeRecepcion _servicioRecepcion;

        public RecibirOrdenDeCompraHandler(
            IOrdenDeCompraRepository ordenRepository,
            ILoteRepository loteRepository,
            IIngredienteRepository ingredienteRepository,
            ServicioDeRecepcion servicioRecepcion)
        {
            _ordenRepository = ordenRepository;
            _loteRepository = loteRepository;
            _ingredienteRepository = ingredienteRepository;
            _servicioRecepcion = servicioRecepcion;
        }

        public async Task<bool> Handle(RecibirOrdenDeCompraCommand request, CancellationToken cancellationToken)
        {
            // Obtener la orden (es un aggregate)
            var ordenAggregate = await _ordenRepository.ObtenerPorIdAsync(request.OrdenId);
            if (ordenAggregate == null)
                throw new ArgumentException($"Orden de compra con ID {request.OrdenId} no encontrada");

            // Recibir la orden (el aggregate crea el lote internamente)
            ordenAggregate.RecibirOrden(DateTime.UtcNow);

            // Obtener el lote creado por el aggregate
            var loteCreado = ordenAggregate.LotesRecibidos.LastOrDefault();
            if (loteCreado != null)
            {
                // Guardar el lote en el repositorio
                await _loteRepository.GuardarAsync(loteCreado);

                // Obtener el ingrediente para actualizar stock
                var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(ordenAggregate.OrdenDeCompra.IngredienteId);
                if (ingredienteAggregate != null)
                {
                    // Agregar el lote al ingrediente
                    ingredienteAggregate.Ingrediente.AgregarLote(loteCreado);

                    // Guardar el ingrediente actualizado
                    await _ingredienteRepository.GuardarAsync(ingredienteAggregate);
                }
            }

            // Guardar la orden actualizada
            await _ordenRepository.GuardarAsync(ordenAggregate);

            return true;
        }
    }
}
