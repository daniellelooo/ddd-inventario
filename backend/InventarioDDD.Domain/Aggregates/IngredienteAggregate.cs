using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Aggregates
{
    public class IngredienteAggregate
    {
        private readonly Ingrediente _ingrediente;
        private readonly List<Lote> _lotes;

        public Guid Id => _ingrediente.Id;
        public Ingrediente Ingrediente => _ingrediente;
        public IReadOnlyList<Lote> Lotes => _lotes.AsReadOnly();

        public IngredienteAggregate(Ingrediente ingrediente, List<Lote>? lotes = null)
        {
            _ingrediente = ingrediente ?? throw new ArgumentNullException(nameof(ingrediente));
            _lotes = lotes ?? new List<Lote>();
        }

        // Reglas de negocio del agregado Ingrediente

        /// <summary>
        /// Registra un nuevo lote y aplica las reglas de negocio correspondientes
        /// </summary>
        public void RegistrarLote(Lote lote)
        {
            ValidarLoteParaIngrediente(lote);

            _lotes.Add(lote);
            _ingrediente.AgregarLote(lote);

            // Verificar si necesita generar alertas
            if (_ingrediente.RequiereReabastecimiento())
            {
                // Evento: Se necesita reabastecimiento
                // Esta lógica debería disparar eventos del dominio
            }
        }

        /// <summary>
        /// Consume ingredientes aplicando la regla FIFO
        /// </summary>
        public void ConsumirIngrediente(decimal cantidad, string motivo)
        {
            ValidarConsumo(cantidad);

            // Aplicar FIFO: consumir primero los lotes más antiguos
            var lotesDisponibles = _lotes
                .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido())
                .OrderBy(l => l.FechaVencimiento.Valor)
                .ToList();

            if (!lotesDisponibles.Any())
                throw new InvalidOperationException("No hay lotes disponibles para el consumo");

            decimal cantidadRestante = cantidad;
            var lotesConsumidos = new List<(Lote lote, decimal cantidadConsumida)>();

            foreach (var lote in lotesDisponibles)
            {
                if (cantidadRestante <= 0) break;

                decimal cantidadAConsumir = Math.Min(cantidadRestante, lote.CantidadDisponible);
                lote.Consumir(cantidadAConsumir);
                lotesConsumidos.Add((lote, cantidadAConsumir));
                cantidadRestante -= cantidadAConsumir;
            }

            if (cantidadRestante > 0)
                throw new InvalidOperationException($"Stock insuficiente. Faltaron {cantidadRestante} unidades");

            // Registrar movimiento de inventario
            // Aquí se debería crear un MovimientoInventario y disparar eventos

            // Verificar alertas post-consumo
            VerificarAlertasPostConsumo();
        }

        /// <summary>
        /// Verifica si el ingrediente tiene stock bajo
        /// </summary>
        public bool TieneStockBajo()
        {
            return _ingrediente.TieneStockBajo();
        }

        /// <summary>
        /// Verifica si requiere reabastecimiento
        /// </summary>
        public bool RequiereReabastecimiento()
        {
            return _ingrediente.RequiereReabastecimiento();
        }

        /// <summary>
        /// Obtiene lotes próximos a vencer
        /// </summary>
        public List<Lote> ObtenerLotesProximosAVencer(int diasAnticipacion = 7)
        {
            return _lotes
                .Where(l => l.CantidadDisponible > 0 && l.EstaProximoAVencer(diasAnticipacion))
                .OrderBy(l => l.FechaVencimiento.Valor)
                .ToList();
        }

        /// <summary>
        /// Obtiene lotes vencidos
        /// </summary>
        public List<Lote> ObtenerLotesVencidos()
        {
            return _lotes
                .Where(l => l.CantidadDisponible > 0 && l.EstaVencido())
                .ToList();
        }

        /// <summary>
        /// Calcula el valor total del inventario
        /// </summary>
        public decimal CalcularValorInventario()
        {
            return _lotes
                .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido())
                .Sum(l => l.CalcularValorInventario());
        }

        /// <summary>
        /// Actualiza el rango de stock del ingrediente
        /// </summary>
        public void ActualizarRangoStock(RangoDeStock nuevoRango)
        {
            _ingrediente.ActualizarRangoStock(nuevoRango);

            // Verificar si el nuevo rango genera alertas
            VerificarAlertasPostActualizacion();
        }

        // Métodos privados para validaciones y reglas de negocio

        private void ValidarLoteParaIngrediente(Lote lote)
        {
            if (lote == null)
                throw new ArgumentNullException(nameof(lote));

            if (lote.IngredienteId != _ingrediente.Id)
                throw new InvalidOperationException("El lote no corresponde a este ingrediente");

            if (lote.EstaVencido())
                throw new InvalidOperationException("No se puede agregar un lote ya vencido");

            // Verificar si ya existe un lote con el mismo código
            if (_lotes.Any(l => l.Codigo == lote.Codigo))
                throw new InvalidOperationException($"Ya existe un lote con el código {lote.Codigo}");
        }

        private void ValidarConsumo(decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a consumir debe ser mayor a cero");

            if (cantidad > _ingrediente.CantidadEnStock.Valor)
                throw new InvalidOperationException("Stock insuficiente para el consumo solicitado");
        }

        private void VerificarAlertasPostConsumo()
        {
            if (_ingrediente.TieneStockBajo())
            {
                // Disparar evento: AlertaStockBajo
            }

            if (_ingrediente.RequiereReabastecimiento())
            {
                // Disparar evento: AlertaReabastecimiento
            }
        }

        private void VerificarAlertasPostActualizacion()
        {
            if (_ingrediente.TieneStockBajo())
            {
                // Disparar evento: AlertaStockBajo por nueva configuración
            }
        }
    }
}