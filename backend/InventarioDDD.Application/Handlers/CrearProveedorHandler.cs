using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.ValueObjects;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    public class CrearProveedorHandler : IRequestHandler<CrearProveedorCommand, Guid>
    {
        private readonly IProveedorRepository _proveedorRepository;

        public CrearProveedorHandler(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        public async Task<Guid> Handle(CrearProveedorCommand request, CancellationToken cancellationToken)
        {
            // Validar que no exista un proveedor con el mismo NIT
            var existe = await _proveedorRepository.ExisteNITAsync(request.NIT);
            if (existe)
            {
                throw new InvalidOperationException($"Ya existe un proveedor con el NIT '{request.NIT}'");
            }

            var direccion = new DireccionProveedor(request.Calle, request.Ciudad, request.Pais, request.CodigoPostal);
            
            var proveedor = new Proveedor(
                request.Nombre,
                request.NIT,
                request.Telefono,
                request.Email,
                direccion,
                request.PersonaContacto
            );

            var proveedorAggregate = new ProveedorAggregate(proveedor);

            await _proveedorRepository.GuardarAsync(proveedorAggregate);

            return proveedorAggregate.Id;
        }
    }
}
