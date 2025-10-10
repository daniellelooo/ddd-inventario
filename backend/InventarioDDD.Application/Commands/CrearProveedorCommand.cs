using MediatR;

namespace InventarioDDD.Application.Commands
{
    public class CrearProveedorCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = string.Empty;
        public string NIT { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string PersonaContacto { get; set; } = string.Empty;
    }
}
