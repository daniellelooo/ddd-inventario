using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.ValueObjects;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    public class CrearIngredienteHandler : IRequestHandler<CrearIngredienteCommand, Guid>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CrearIngredienteHandler(
            IIngredienteRepository ingredienteRepository,
            ICategoriaRepository categoriaRepository)
        {
            _ingredienteRepository = ingredienteRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Guid> Handle(CrearIngredienteCommand request, CancellationToken cancellationToken)
        {
            // Validar que la categoría exista
            var categoriaExiste = await _categoriaRepository.ExisteAsync(request.CategoriaId);
            if (!categoriaExiste)
            {
                throw new InvalidOperationException($"La categoría con ID '{request.CategoriaId}' no existe");
            }

            // Validar que no exista un ingrediente con el mismo nombre
            var existe = await _ingredienteRepository.ExisteNombreAsync(request.Nombre);
            if (existe)
            {
                throw new InvalidOperationException($"Ya existe un ingrediente con el nombre '{request.Nombre}'");
            }

            // Mapeo simple de string a UnidadDeMedida predefinida
            var unidadMedida = request.UnidadMedida.ToLower() switch
            {
                "kg" or "kilogramos" => UnidadDeMedida.Kilogramos,
                "g" or "gramos" => UnidadDeMedida.Gramos,
                "l" or "litros" => UnidadDeMedida.Litros,
                "ml" or "mililitros" => UnidadDeMedida.Mililitros,
                "u" or "unidades" => UnidadDeMedida.Unidades,
                "lb" or "libras" => UnidadDeMedida.Libras,
                "oz" or "onzas" => UnidadDeMedida.Onzas,
                _ => new UnidadDeMedida(request.UnidadMedida, request.UnidadMedida)
            };

            var rangoStock = new RangoDeStock(request.StockMinimo, request.StockMaximo);

            var ingrediente = new Ingrediente(
                request.Nombre,
                request.Descripcion,
                unidadMedida,
                rangoStock,
                request.CategoriaId
            );

            var ingredienteAggregate = new IngredienteAggregate(ingrediente);

            await _ingredienteRepository.GuardarAsync(ingredienteAggregate);

            return ingredienteAggregate.Id;
        }
    }
}
