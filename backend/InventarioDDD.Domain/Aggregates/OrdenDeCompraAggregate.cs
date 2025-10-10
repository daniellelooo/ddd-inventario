using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Aggregates
{
    public class OrdenDeCompraAggregate
    {
        private readonly OrdenDeCompra _ordenDeCompra;
        private readonly List<Lote> _lotesRecibidos;

        public Guid Id => _ordenDeCompra.Id;
        public OrdenDeCompra OrdenDeCompra => _ordenDeCompra;
        public IReadOnlyList<Lote> LotesRecibidos => _lotesRecibidos.AsReadOnly();

        public OrdenDeCompraAggregate(OrdenDeCompra ordenDeCompra, List<Lote>? lotesRecibidos = null)
        {
            _ordenDeCompra = ordenDeCompra ?? throw new ArgumentNullException(nameof(ordenDeCompra));
            _lotesRecibidos = lotesRecibidos ?? new List<Lote>();
        }

        /// <summary>
        /// Aprueba la orden de compra aplicando validaciones de negocio
        /// </summary>
        public void Aprobar(Guid usuarioAprobadorId)
        {
            ValidarAprobacion();
            _ordenDeCompra.Aprobar();
        }

        /// <summary>
        /// Marca la orden como envío pendiente
        /// </summary>
        public void MarcarEnvioPendiente()
        {
            ValidarEstadoParaEnvioPendiente();
            _ordenDeCompra.MarcarEnvioPendiente();
        }

        /// <summary>
        /// Recibe la orden y crea los lotes correspondientes
        /// </summary>
        public void RecibirOrden(DateTime? fechaRecepcion = null)
        {
            ValidarRecepcion();

            // Crear lote con la cantidad recibida
            var lote = new Lote(
                GenerarCodigoLote(),
                _ordenDeCompra.IngredienteId,
                _ordenDeCompra.ProveedorId,
                _ordenDeCompra.Cantidad.Valor,
                EstimarFechaVencimiento(),
                _ordenDeCompra.PrecioUnitario,
                _ordenDeCompra.Id
            );

            _lotesRecibidos.Add(lote);
            _ordenDeCompra.MarcarRecibida(fechaRecepcion);
        }

        /// <summary>
        /// Cancela la orden de compra
        /// </summary>
        public void Cancelar(string motivo)
        {
            ValidarCancelacion();
            _ordenDeCompra.Cancelar();
            _ordenDeCompra.ActualizarObservaciones($"{_ordenDeCompra.Observaciones} | Cancelada: {motivo}");
        }

        /// <summary>
        /// Verifica si la orden está vencida
        /// </summary>
        public bool EstaVencida()
        {
            return _ordenDeCompra.EstaVencida();
        }

        /// <summary>
        /// Calcula el total de la orden
        /// </summary>
        public PrecioConMoneda CalcularTotal()
        {
            return _ordenDeCompra.CalcularTotal();
        }

        /// <summary>
        /// Actualiza la fecha esperada de la orden
        /// </summary>
        public void ActualizarFechaEsperada(DateTime nuevaFecha)
        {
            if (!_ordenDeCompra.PuedeSerModificada())
                throw new InvalidOperationException("No se puede actualizar la fecha de una orden que no está pendiente");

            // La orden tendría que tener un método para actualizar la fecha
            // Por ahora solo validamos que se pueda modificar
        }

        // Métodos de validación privados

        private void ValidarAprobacion()
        {
            if (_ordenDeCompra.Estado != EstadoOrden.Pendiente)
                throw new InvalidOperationException("Solo se pueden aprobar órdenes pendientes");

            if (_ordenDeCompra.Cantidad.Valor <= 0)
                throw new InvalidOperationException("No se puede aprobar una orden sin cantidad válida");
        }

        private void ValidarEstadoParaEnvioPendiente()
        {
            if (_ordenDeCompra.Estado != EstadoOrden.Aprobada)
                throw new InvalidOperationException("Solo se pueden marcar como envío pendiente las órdenes aprobadas");
        }

        private void ValidarRecepcion()
        {
            if (_ordenDeCompra.Estado != EstadoOrden.EnvioPendiente && _ordenDeCompra.Estado != EstadoOrden.Aprobada)
                throw new InvalidOperationException("Solo se pueden recibir órdenes en envío pendiente o aprobadas");
        }

        private void ValidarCancelacion()
        {
            if (_ordenDeCompra.Estado == EstadoOrden.Recibida)
                throw new InvalidOperationException("No se puede cancelar una orden ya recibida");
        }

        private string GenerarCodigoLote()
        {
            return $"LT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        private FechaVencimiento EstimarFechaVencimiento()
        {
            // Estimación por defecto: 30 días desde la recepción
            // En un sistema real esto vendría de la configuración del ingrediente
            var fechaEstimada = DateTime.UtcNow.AddDays(30);
            return new FechaVencimiento(fechaEstimada);
        }
    }
}