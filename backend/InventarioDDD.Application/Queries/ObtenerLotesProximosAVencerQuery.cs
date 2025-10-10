using InventarioDDD.Application.DTOs;
using MediatR;

namespace InventarioDDD.Application.Queries
{
    /// <summary>
    /// Consulta para obtener lotes próximos a vencer
    /// </summary>
    public class ObtenerLotesProximosAVencerQuery : IRequest<List<LoteDto>>
    {
        public int DiasAnticipacion { get; set; } = 7;
    }
}
