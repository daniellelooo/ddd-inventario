using InventarioDDD.Application.DTOs;
using MediatR;

namespace InventarioDDD.Application.Queries
{
    /// <summary>
    /// Consulta para obtener ingredientes que requieren reabastecimiento
    /// </summary>
    public class ObtenerIngredientesParaReabastecerQuery : IRequest<List<IngredienteDto>>
    {
    }
}
