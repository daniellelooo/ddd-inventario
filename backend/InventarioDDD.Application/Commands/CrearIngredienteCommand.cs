using MediatR;

namespace InventarioDDD.Application.Commands
{
    public class CrearIngredienteCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
