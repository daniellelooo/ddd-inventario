using MediatR;

namespace InventarioDDD.Application.Commands
{
    /// <summary>
    /// Comando para registrar el consumo de un ingrediente del inventario
    /// </summary>
    public class RegistrarConsumoCommand : IRequest<bool>
    {
        public Guid IngredienteId { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public Guid? UsuarioId { get; set; }
        public string? Observaciones { get; set; }
    }
}
