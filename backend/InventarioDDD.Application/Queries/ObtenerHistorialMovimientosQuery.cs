using InventarioDDD.Application.DTOs;
using MediatR;

namespace InventarioDDD.Application.Queries
{
    /// <summary>
    /// Consulta para obtener el historial de movimientos de un ingrediente
    /// </summary>
    public class ObtenerHistorialMovimientosQuery : IRequest<List<MovimientoInventarioDto>>
    {
        public Guid IngredienteId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}
