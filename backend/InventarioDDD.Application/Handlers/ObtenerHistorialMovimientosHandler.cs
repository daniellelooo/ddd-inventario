using InventarioDDD.Application.DTOs;
using InventarioDDD.Application.Queries;
using InventarioDDD.Domain.Interfaces;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para consultar el historial de movimientos de un ingrediente
    /// </summary>
    public class ObtenerHistorialMovimientosHandler : IRequestHandler<ObtenerHistorialMovimientosQuery, List<MovimientoInventarioDto>>
    {
        private readonly IMovimientoInventarioRepository _movimientoRepository;
        private readonly IIngredienteRepository _ingredienteRepository;

        public ObtenerHistorialMovimientosHandler(
            IMovimientoInventarioRepository movimientoRepository,
            IIngredienteRepository ingredienteRepository)
        {
            _movimientoRepository = movimientoRepository;
            _ingredienteRepository = ingredienteRepository;
        }

        public async Task<List<MovimientoInventarioDto>> Handle(ObtenerHistorialMovimientosQuery request, CancellationToken cancellationToken)
        {
            var movimientos = await _movimientoRepository.ObtenerHistorialAsync(
                request.IngredienteId,
                request.FechaDesde,
                request.FechaHasta
            );

            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(request.IngredienteId);
            var nombreIngrediente = ingrediente?.Ingrediente.Nombre ?? "Desconocido";

            var resultado = movimientos.Select(m => new MovimientoInventarioDto
            {
                Id = m.Id,
                IngredienteId = m.IngredienteId,
                IngredienteNombre = nombreIngrediente,
                TipoMovimiento = m.TipoMovimiento.ToString(),
                Cantidad = m.Cantidad,
                UnidadMedida = m.UnidadDeMedida.ToString(),
                FechaMovimiento = m.FechaMovimiento,
                Motivo = m.Motivo,
                DocumentoReferencia = m.DocumentoReferencia,
                Observaciones = m.Observaciones
            }).ToList();

            return resultado;
        }
    }
}
