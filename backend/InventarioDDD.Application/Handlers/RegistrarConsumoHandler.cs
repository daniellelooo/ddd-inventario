using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Enums;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;
using InventarioDDD.Domain.ValueObjects;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para procesar el consumo de ingredientes del inventario
    /// </summary>
    public class RegistrarConsumoHandler : IRequestHandler<RegistrarConsumoCommand, bool>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;
        private readonly ServicioDeConsumo _servicioConsumo;

        public RegistrarConsumoHandler(
            IIngredienteRepository ingredienteRepository,
            IMovimientoInventarioRepository movimientoRepository,
            ServicioDeConsumo servicioConsumo)
        {
            _ingredienteRepository = ingredienteRepository;
            _movimientoRepository = movimientoRepository;
            _servicioConsumo = servicioConsumo;
        }

        public async Task<bool> Handle(RegistrarConsumoCommand request, CancellationToken cancellationToken)
        {
            // Obtener el ingrediente con sus lotes
            var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(request.IngredienteId);
            if (ingredienteAggregate == null)
                throw new ArgumentException($"Ingrediente con ID {request.IngredienteId} no encontrado");

            // Validar el consumo
            var validacion = _servicioConsumo.ValidarConsumo(ingredienteAggregate, request.Cantidad, request.Motivo);
            if (!validacion.EsValido)
                throw new InvalidOperationException($"Validación fallida: {string.Join(", ", validacion.Errores)}");

            // Ejecutar el consumo (FIFO) - El Ingrediente maneja la lógica
            ingredienteAggregate.Ingrediente.ConsumirIngrediente(request.Cantidad);

            // Crear movimiento de salida general
            var movimiento = new MovimientoInventario(
                request.IngredienteId,
                TipoMovimiento.Salida,
                request.Cantidad,
                ingredienteAggregate.Ingrediente.UnidadDeMedida,
                request.Motivo,
                null, // No se asocia a un lote específico
                null,
                request.UsuarioId,
                request.Observaciones
            );

            await _movimientoRepository.GuardarMovimientoAsync(movimiento);

            // Guardar cambios
            await _ingredienteRepository.GuardarAsync(ingredienteAggregate);

            return true;
        }
    }
}
