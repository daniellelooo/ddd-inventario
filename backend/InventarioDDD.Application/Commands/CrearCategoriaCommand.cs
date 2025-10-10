using MediatR;

namespace InventarioDDD.Application.Commands
{
    public class CrearCategoriaCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
