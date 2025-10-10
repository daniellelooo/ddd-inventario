using InventarioDDD.Application.DTOs;
using InventarioDDD.Application.Queries;
using InventarioDDD.Domain.Interfaces;
using MediatR;

namespace InventarioDDD.Application.Handlers
{
    /// <summary>
    /// Handler para consultar órdenes de compra pendientes
    /// </summary>
    public class ObtenerOrdenesPendientesHandler : IRequestHandler<ObtenerOrdenesPendientesQuery, List<OrdenDeCompraDto>>
    {
        private readonly IOrdenDeCompraRepository _ordenRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IIngredienteRepository _ingredienteRepository;

        public ObtenerOrdenesPendientesHandler(
            IOrdenDeCompraRepository ordenRepository,
            IProveedorRepository proveedorRepository,
            IIngredienteRepository ingredienteRepository)
        {
            _ordenRepository = ordenRepository;
            _proveedorRepository = proveedorRepository;
            _ingredienteRepository = ingredienteRepository;
        }

        public async Task<List<OrdenDeCompraDto>> Handle(ObtenerOrdenesPendientesQuery request, CancellationToken cancellationToken)
        {
            // Obtener órdenes pendientes (son aggregates)
            var ordenesAggregate = await _ordenRepository.ObtenerPendientesAsync();

            var resultado = new List<OrdenDeCompraDto>();

            foreach (var ordenAggregate in ordenesAggregate)
            {
                var orden = ordenAggregate.OrdenDeCompra;
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(orden.ProveedorId);
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(orden.IngredienteId);

                // Cada orden tiene UN solo ingrediente, creamos un detalle único
                var detalles = new List<DetalleOrdenDto>
                {
                    new DetalleOrdenDto
                    {
                        IngredienteId = orden.IngredienteId,
                        IngredienteNombre = ingrediente?.Ingrediente.Nombre ?? "Desconocido",
                        Cantidad = orden.Cantidad.Valor,
                        UnidadMedida = ingrediente?.Ingrediente.UnidadDeMedida.ToString() ?? "UN",
                        PrecioUnitario = orden.PrecioUnitario.Valor,
                        Subtotal = orden.Cantidad.Valor * orden.PrecioUnitario.Valor
                    }
                };

                resultado.Add(new OrdenDeCompraDto
                {
                    Id = orden.Id,
                    NumeroOrden = orden.Numero,
                    ProveedorId = orden.ProveedorId,
                    ProveedorNombre = proveedor?.Proveedor.Nombre ?? "Desconocido",
                    FechaOrden = orden.FechaCreacion,
                    FechaEntregaEsperada = orden.FechaEsperada,
                    Estado = orden.Estado.ToString(),
                    Total = orden.Cantidad.Valor * orden.PrecioUnitario.Valor,
                    Moneda = orden.PrecioUnitario.Moneda,
                    Detalles = detalles
                });
            }

            return resultado.OrderBy(o => o.FechaOrden).ToList();
        }
    }
}
