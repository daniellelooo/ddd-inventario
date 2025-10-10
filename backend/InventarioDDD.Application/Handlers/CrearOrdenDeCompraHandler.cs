using InventarioDDD.Application.Commands;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.ValueObjects;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para crear una nueva orden de compra
    /// </summary>
    public class CrearOrdenDeCompraHandler : IRequestHandler<CrearOrdenDeCompraCommand, Guid>
    {
        private readonly IOrdenDeCompraRepository _ordenRepository;
        private readonly IProveedorRepository _proveedorRepository;

        public CrearOrdenDeCompraHandler(
            IOrdenDeCompraRepository ordenRepository,
            IProveedorRepository proveedorRepository)
        {
            _ordenRepository = ordenRepository;
            _proveedorRepository = proveedorRepository;
        }

        public async Task<Guid> Handle(CrearOrdenDeCompraCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el proveedor existe
            var proveedorExiste = await _proveedorRepository.ExisteAsync(request.ProveedorId);
            if (!proveedorExiste)
                throw new ArgumentException($"Proveedor con ID {request.ProveedorId} no encontrado");

            // Nota: En este diseño, cada orden de compra es para un solo ingrediente
            // Si hay múltiples ingredientes, se crean múltiples órdenes

            if (request.Detalles == null || !request.Detalles.Any())
                throw new ArgumentException("La orden debe tener al menos un detalle");

            // Crear una orden por cada ingrediente
            var primeraOrdenId = Guid.Empty;

            foreach (var detalle in request.Detalles)
            {
                // Generar número de orden único
                var numeroOrden = $"OC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                // Crear value objects
                var precio = new PrecioConMoneda(detalle.PrecioUnitario, detalle.Moneda);
                var cantidad = new CantidadDisponible(detalle.Cantidad);

                // Crear la orden de compra
                var ordenEntidad = new OrdenDeCompra(
                    numeroOrden,
                    detalle.IngredienteId,
                    request.ProveedorId,
                    cantidad,
                    precio,
                    request.FechaEntregaEsperada ?? DateTime.UtcNow.AddDays(7)
                );

                // Crear el agregado
                var ordenAggregate = new OrdenDeCompraAggregate(ordenEntidad);

                // Guardar la orden
                await _ordenRepository.GuardarAsync(ordenAggregate);

                // Guardar el ID de la primera orden para retornar
                if (primeraOrdenId == Guid.Empty)
                    primeraOrdenId = ordenAggregate.Id;
            }

            return primeraOrdenId;
        }
    }
}
