using InventarioDDD.Application.DTOs;
using InventarioDDD.Application.Queries;
using InventarioDDD.Domain.Interfaces;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para consultar ingredientes que necesitan reabastecimiento
    /// </summary>
    public class ObtenerIngredientesParaReabastecerHandler : IRequestHandler<ObtenerIngredientesParaReabastecerQuery, List<IngredienteDto>>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public ObtenerIngredientesParaReabastecerHandler(
            IIngredienteRepository ingredienteRepository,
            ICategoriaRepository categoriaRepository)
        {
            _ingredienteRepository = ingredienteRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<List<IngredienteDto>> Handle(ObtenerIngredientesParaReabastecerQuery request, CancellationToken cancellationToken)
        {
            // Obtener ingredientes que requieren reabastecimiento
            var ingredientes = await _ingredienteRepository.ObtenerQueRequierenReabastecimientoAsync();

            var resultado = new List<IngredienteDto>();

            foreach (var ing in ingredientes)
            {
                var categoria = await _categoriaRepository.ObtenerPorIdAsync(ing.Ingrediente.CategoriaId);

                resultado.Add(new IngredienteDto
                {
                    Id = ing.Ingrediente.Id,
                    Nombre = ing.Ingrediente.Nombre,
                    Descripcion = ing.Ingrediente.Descripcion ?? string.Empty,
                    CategoriaId = ing.Ingrediente.CategoriaId,
                    CategoriaNombre = categoria?.Nombre ?? "Sin categoría",
                    UnidadMedida = ing.Ingrediente.UnidadDeMedida.ToString(),
                    StockMinimo = ing.Ingrediente.RangoDeStock.StockMinimo,
                    StockMaximo = ing.Ingrediente.RangoDeStock.StockMaximo,
                    CantidadActual = ing.Ingrediente.CantidadEnStock.Valor,
                    RequiereReabastecimiento = ing.Ingrediente.RequiereReabastecimiento()
                });
            }

            return resultado;
        }
    }
}
