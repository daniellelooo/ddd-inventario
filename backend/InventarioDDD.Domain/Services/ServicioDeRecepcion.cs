using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Domain.Services
{
    public class ServicioDeRecepcion
    {
        private readonly IOrdenDeCompraRepository _ordenDeCompraRepository;
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;

        public ServicioDeRecepcion(
            IOrdenDeCompraRepository ordenDeCompraRepository,
            IIngredienteRepository ingredienteRepository,
            ILoteRepository loteRepository,
            IMovimientoInventarioRepository movimientoRepository)
        {
            _ordenDeCompraRepository = ordenDeCompraRepository;
            _ingredienteRepository = ingredienteRepository;
            _loteRepository = loteRepository;
            _movimientoRepository = movimientoRepository;
        }

        /// <summary>
        /// Recibe una orden de compra completa
        /// </summary>
        public async Task<ResultadoRecepcion> RecibirOrdenCompleta(Guid ordenId, DateTime? fechaRecepcion = null)
        {
            var resultado = new ResultadoRecepcion();

            // Obtener la orden
            var ordenAggregate = await _ordenDeCompraRepository.ObtenerPorIdAsync(ordenId);
            if (ordenAggregate == null)
            {
                resultado.AgregarError("Orden de compra no encontrada");
                return resultado;
            }

            if (ordenAggregate.OrdenDeCompra.Estado != Enums.EstadoOrden.EnvioPendiente)
            {
                resultado.AgregarError("Solo se pueden recibir órdenes en estado 'EnvioPendiente'");
                return resultado;
            }

            try
            {
                // Usar el aggregate para recibir la orden
                ordenAggregate.RecibirOrden(fechaRecepcion);

                // Guardar cambios
                await _ordenDeCompraRepository.GuardarAsync(ordenAggregate);

                resultado.EsExitoso = true;
                resultado.FechaRecepcion = fechaRecepcion ?? DateTime.UtcNow;

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.AgregarError($"Error al recibir orden: {ex.Message}");
                return resultado;
            }
        }

        /// <summary>
        /// Recibe una orden parcialmente con cantidades específicas
        /// </summary>
        public async Task<ResultadoRecepcion> RecibirOrdenParcial(Guid ordenId, decimal cantidadRecibida, DateTime? fechaRecepcion = null)
        {
            var resultado = new ResultadoRecepcion();

            // Obtener la orden
            var ordenAggregate = await _ordenDeCompraRepository.ObtenerPorIdAsync(ordenId);
            if (ordenAggregate == null)
            {
                resultado.AgregarError("Orden de compra no encontrada");
                return resultado;
            }

            if (ordenAggregate.OrdenDeCompra.Estado != Enums.EstadoOrden.EnvioPendiente)
            {
                resultado.AgregarError("Solo se pueden recibir órdenes en estado 'EnvioPendiente'");
                return resultado;
            }

            try
            {
                // Para recepción parcial, validar que la cantidad no exceda lo ordenado
                if (cantidadRecibida > ordenAggregate.OrdenDeCompra.Cantidad.Valor)
                {
                    resultado.AgregarError("La cantidad recibida no puede exceder la cantidad ordenada");
                    return resultado;
                }

                // Marcar como recibida
                ordenAggregate.RecibirOrden(fechaRecepcion);

                // Guardar cambios
                await _ordenDeCompraRepository.GuardarAsync(ordenAggregate);

                resultado.EsExitoso = true;
                resultado.FechaRecepcion = fechaRecepcion ?? DateTime.UtcNow;
                resultado.CantidadRecibida = cantidadRecibida;

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.AgregarError($"Error al recibir orden parcial: {ex.Message}");
                return resultado;
            }
        }
    }

    /// <summary>
    /// Resultado de una operación de recepción
    /// </summary>
    public class ResultadoRecepcion
    {
        public bool EsExitoso { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public decimal CantidadRecibida { get; set; }
        public List<string> Errores { get; private set; } = new List<string>();

        public void AgregarError(string error)
        {
            Errores.Add(error);
            EsExitoso = false;
        }
    }
}