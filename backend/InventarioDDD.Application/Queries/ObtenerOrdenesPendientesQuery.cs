using InventarioDDD.Application.DTOs;
using MediatR;

namespace InventarioDDD.Application.Queries
{
    /// <summary>
    /// Consulta para obtener órdenes de compra pendientes
    /// </summary>
    public class ObtenerOrdenesPendientesQuery : IRequest<List<OrdenDeCompraDto>>
    {
    }
}
