using InventarioDDD.Application.DTOs;
using InventarioDDD.Application.Queries;
using InventarioDDD.Domain.Interfaces;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para consultar lotes próximos a vencer
    /// </summary>
    public class ObtenerLotesProximosAVencerHandler : IRequestHandler<ObtenerLotesProximosAVencerQuery, List<LoteDto>>
    {
        private readonly ILoteRepository _loteRepository;
        private readonly IIngredienteRepository _ingredienteRepository;

        public ObtenerLotesProximosAVencerHandler(
            ILoteRepository loteRepository,
            IIngredienteRepository ingredienteRepository)
        {
            _loteRepository = loteRepository;
            _ingredienteRepository = ingredienteRepository;
        }

        public async Task<List<LoteDto>> Handle(ObtenerLotesProximosAVencerQuery request, CancellationToken cancellationToken)
        {
            var lotes = await _loteRepository.ObtenerProximosAVencerAsync(request.DiasAnticipacion);

            var resultado = new List<LoteDto>();

            foreach (var lote in lotes)
            {
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(lote.IngredienteId);

                resultado.Add(new LoteDto
                {
                    Id = lote.Id,
                    Codigo = lote.Codigo,
                    IngredienteId = lote.IngredienteId,
                    IngredienteNombre = ingrediente?.Ingrediente.Nombre ?? "Desconocido",
                    CantidadInicial = lote.CantidadInicial,
                    CantidadDisponible = lote.CantidadDisponible,
                    FechaVencimiento = lote.FechaVencimiento.Valor,
                    FechaRecepcion = lote.FechaRecepcion,
                    PrecioUnitario = lote.PrecioUnitario.Valor,
                    Moneda = lote.PrecioUnitario.Moneda,
                    Vencido = lote.Vencido,
                    DiasHastaVencimiento = lote.FechaVencimiento.DiasHastaVencimiento()
                });
            }

            return resultado.OrderBy(l => l.DiasHastaVencimiento).ToList();
        }
    }
}
