using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    public class CrearCategoriaHandler : IRequestHandler<CrearCategoriaCommand, Guid>
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CrearCategoriaHandler(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Guid> Handle(CrearCategoriaCommand request, CancellationToken cancellationToken)
        {
            // Validar que no exista una categoría con el mismo nombre
            var existe = await _categoriaRepository.ExisteNombreAsync(request.Nombre);
            if (existe)
            {
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{request.Nombre}'");
            }

            var categoria = new Categoria(request.Nombre, request.Descripcion);
            await _categoriaRepository.GuardarAsync(categoria);

            return categoria.Id;
        }
    }
}
