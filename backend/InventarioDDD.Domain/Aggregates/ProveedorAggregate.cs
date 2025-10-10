using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Aggregates
{
    public class ProveedorAggregate
    {
        private readonly Proveedor _proveedor;
        private readonly List<OrdenDeCompra> _ordenesDeCompra;

        public Guid Id => _proveedor.Id;
        public Proveedor Proveedor => _proveedor;
        public IReadOnlyList<OrdenDeCompra> OrdenesDeCompra => _ordenesDeCompra.AsReadOnly();

        public ProveedorAggregate(Proveedor proveedor, List<OrdenDeCompra>? ordenesDeCompra = null)
        {
            _proveedor = proveedor ?? throw new ArgumentNullException(nameof(proveedor));
            _ordenesDeCompra = ordenesDeCompra ?? new List<OrdenDeCompra>();
        }

        // Reglas de negocio del agregado Proveedor

        /// <summary>
        /// Actualiza la información del proveedor
        /// </summary>
        public void ActualizarInformacion(string nombre, string telefono, string email,
                                         DireccionProveedor direccion, string personaContacto)
        {
            ValidarInformacionProveedor(nombre, telefono, email, direccion, personaContacto);

            _proveedor.ActualizarInformacion(nombre, telefono, email, direccion, personaContacto);
        }

        /// <summary>
        /// Agrega un ingrediente que el proveedor puede suministrar
        /// </summary>
        public void AgregarIngredienteSuministrado(Guid ingredienteId)
        {
            ValidarIngrediente(ingredienteId);

            _proveedor.AgregarIngredienteSuministrado(ingredienteId);
        }

        /// <summary>
        /// Remueve un ingrediente del catálogo del proveedor
        /// </summary>
        public void RemoverIngredienteSuministrado(Guid ingredienteId)
        {
            if (!_proveedor.PuedeSuministrar(ingredienteId))
                throw new InvalidOperationException("El proveedor no suministra este ingrediente");

            // Verificar que no tenga órdenes pendientes para este ingrediente
            var tieneOrdenesPendientes = _ordenesDeCompra
                .Where(o => o.Estado == Enums.EstadoOrden.Pendiente || o.Estado == Enums.EstadoOrden.Aprobada)
                .Any(o => o.IngredienteId == ingredienteId);

            if (tieneOrdenesPendientes)
                throw new InvalidOperationException("No se puede remover el ingrediente porque tiene órdenes pendientes");

            _proveedor.RemoverIngredienteSuministrado(ingredienteId);
        }

        /// <summary>
        /// Desactiva el proveedor aplicando validaciones de negocio
        /// </summary>
        public void Desactivar()
        {
            ValidarDesactivacion();

            _proveedor.Desactivar();

            // Disparar evento: ProveedorDesactivado
        }

        /// <summary>
        /// Activa el proveedor
        /// </summary>
        public void Activar()
        {
            _proveedor.Activar();

            // Disparar evento: ProveedorActivado
        }

        /// <summary>
        /// Verifica si el proveedor puede suministrar un ingrediente específico
        /// </summary>
        public bool PuedeSuministrar(Guid ingredienteId)
        {
            return _proveedor.PuedeSuministrar(ingredienteId);
        }

        /// <summary>
        /// Obtiene el historial de órdenes de compra del proveedor
        /// </summary>
        public List<OrdenDeCompra> ObtenerHistorialOrdenes(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var ordenes = _ordenesDeCompra.AsQueryable();

            if (fechaDesde.HasValue)
                ordenes = ordenes.Where(o => o.FechaCreacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                ordenes = ordenes.Where(o => o.FechaCreacion <= fechaHasta.Value);

            return ordenes.OrderByDescending(o => o.FechaCreacion).ToList();
        }

        /// <summary>
        /// Obtiene órdenes pendientes del proveedor
        /// </summary>
        public List<OrdenDeCompra> ObtenerOrdenesPendientes()
        {
            return _ordenesDeCompra
                .Where(o => o.Estado == Enums.EstadoOrden.Pendiente ||
                           o.Estado == Enums.EstadoOrden.Aprobada ||
                           o.Estado == Enums.EstadoOrden.EnvioPendiente)
                .OrderBy(o => o.FechaEsperada)
                .ToList();
        }

        /// <summary>
        /// Obtiene órdenes con retraso del proveedor
        /// </summary>
        public List<OrdenDeCompra> ObtenerOrdenesConRetraso()
        {
            return _ordenesDeCompra
                .Where(o => o.EstaVencida())
                .OrderBy(o => o.FechaEsperada)
                .ToList();
        }

        /// <summary>
        /// Calcula el valor total de las órdenes del proveedor en un período
        /// </summary>
        public decimal CalcularValorTotalOrdenes(DateTime fechaDesde, DateTime fechaHasta)
        {
            return _ordenesDeCompra
                .Where(o => o.FechaCreacion >= fechaDesde &&
                           o.FechaCreacion <= fechaHasta &&
                           o.Estado == Enums.EstadoOrden.Recibida)
                .Sum(o => o.CalcularTotal().Valor);
        }

        /// <summary>
        /// Evalúa el desempeño del proveedor
        /// </summary>
        public DesempenoProveedor EvaluarDesempeno(DateTime fechaDesde, DateTime fechaHasta)
        {
            var ordenesEnPeriodo = _ordenesDeCompra
                .Where(o => o.FechaCreacion >= fechaDesde && o.FechaCreacion <= fechaHasta)
                .ToList();

            if (!ordenesEnPeriodo.Any())
                return new DesempenoProveedor(0, 0, 0, 0);

            var totalOrdenes = ordenesEnPeriodo.Count;
            var ordenesATiempo = ordenesEnPeriodo.Count(o => !o.EstaVencida() && o.Estado == Enums.EstadoOrden.Recibida);
            var ordenesCanceladas = ordenesEnPeriodo.Count(o => o.Estado == Enums.EstadoOrden.Cancelada);
            var valorTotal = ordenesEnPeriodo.Where(o => o.Estado == Enums.EstadoOrden.Recibida).Sum(o => o.CalcularTotal().Valor);

            return new DesempenoProveedor(totalOrdenes, ordenesATiempo, ordenesCanceladas, valorTotal);
        }

        // Métodos privados para validaciones

        private void ValidarInformacionProveedor(string nombre, string telefono, string email,
                                               DireccionProveedor direccion, string personaContacto)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del proveedor es requerido");

            if (string.IsNullOrWhiteSpace(telefono))
                throw new ArgumentException("El teléfono del proveedor es requerido");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("El email del proveedor debe ser válido");

            if (direccion == null)
                throw new ArgumentException("La dirección del proveedor es requerida");

            if (string.IsNullOrWhiteSpace(personaContacto))
                throw new ArgumentException("La persona de contacto es requerida");
        }

        private void ValidarIngrediente(Guid ingredienteId)
        {
            if (ingredienteId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente es requerido");
        }

        private void ValidarDesactivacion()
        {
            // Verificar que no tenga órdenes pendientes
            var tieneOrdenesPendientes = _ordenesDeCompra
                .Any(o => o.Estado == Enums.EstadoOrden.Pendiente ||
                         o.Estado == Enums.EstadoOrden.Aprobada ||
                         o.Estado == Enums.EstadoOrden.EnvioPendiente);

            if (tieneOrdenesPendientes)
                throw new InvalidOperationException("No se puede desactivar el proveedor porque tiene órdenes pendientes");
        }
    }

    // Clase auxiliar para el desempeño del proveedor
    public class DesempenoProveedor
    {
        public int TotalOrdenes { get; }
        public int OrdenesATiempo { get; }
        public int OrdenesCanceladas { get; }
        public decimal ValorTotal { get; }
        public double PorcentajeCumplimiento => TotalOrdenes > 0 ? (double)OrdenesATiempo / TotalOrdenes * 100 : 0;
        public double PorcentajeCancelacion => TotalOrdenes > 0 ? (double)OrdenesCanceladas / TotalOrdenes * 100 : 0;

        public DesempenoProveedor(int totalOrdenes, int ordenesATiempo, int ordenesCanceladas, decimal valorTotal)
        {
            TotalOrdenes = totalOrdenes;
            OrdenesATiempo = ordenesATiempo;
            OrdenesCanceladas = ordenesCanceladas;
            ValorTotal = valorTotal;
        }
    }
}