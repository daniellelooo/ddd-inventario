using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;

namespace InventarioDDD.Domain.Services
{
    /// <summary>
    /// Servicio de dominio para la gestión de inventario
    /// </summary>
    public class ServicioDeInventario
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public ServicioDeInventario(IIngredienteRepository ingredienteRepository, ICategoriaRepository categoriaRepository)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
        }

        /// <summary>
        /// Consulta el stock disponible de un ingrediente específico
        /// </summary>
        public async Task<CantidadDisponible?> ConsultarStockDisponible(Guid ingredienteId)
        {
            var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);
            return ingredienteAggregate?.Ingrediente.CantidadEnStock;
        }

        /// <summary>
        /// Verifica la disponibilidad para un pedido completo
        /// </summary>
        public async Task<bool> VerificarDisponibilidadParaPedido(Guid pedidoId, List<RequisitoIngrediente> requisitos)
        {
            foreach (var requisito in requisitos)
            {
                var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(requisito.IngredienteId);
                if (ingredienteAggregate == null)
                    return false;

                if (!VerificarDisponibilidad(ingredienteAggregate, requisito.CantidadRequerida))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Obtiene ingredientes por categoría
        /// </summary>
        public async Task<List<Ingrediente>> ObtenerIngredientesPorCategoria(Guid categoriaId)
        {
            var ingredientesAggregate = await _ingredienteRepository.ObtenerPorCategoriaAsync(categoriaId);
            return ingredientesAggregate.Select(ia => ia.Ingrediente).ToList();
        }

        /// <summary>
        /// Calcula la valoración total del inventario
        /// </summary>
        public async Task<MontoTotal> CalcularValoracionInventario()
        {
            var todosIngredientes = await _ingredienteRepository.ObtenerTodosAsync();
            decimal valorTotal = 0;

            foreach (var ingredienteAggregate in todosIngredientes)
            {
                valorTotal += ingredienteAggregate.CalcularValorInventario();
            }

            return new MontoTotal(valorTotal, "COP");
        }
        /// <summary>
        /// Verifica si hay suficiente stock disponible para un consumo
        /// </summary>
        public bool VerificarDisponibilidad(IngredienteAggregate ingredienteAggregate, decimal cantidadRequerida)
        {
            if (ingredienteAggregate == null)
                throw new ArgumentNullException(nameof(ingredienteAggregate));

            return ingredienteAggregate.Ingrediente.CantidadEnStock.Valor >= cantidadRequerida;
        }

        /// <summary>
        /// Calcula la cantidad óptima a ordenar basada en el consumo histórico
        /// </summary>
        public decimal CalcularCantidadOptima(IngredienteAggregate ingredienteAggregate,
                                            List<MovimientoInventario> historialConsumo,
                                            int diasProyeccion = 30)
        {
            if (ingredienteAggregate == null)
                throw new ArgumentNullException(nameof(ingredienteAggregate));

            var rangoStock = ingredienteAggregate.Ingrediente.RangoDeStock;
            var stockActual = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;

            // Calcular consumo promedio diario
            var consumoPromedioDiario = CalcularConsumoPromedioDiario(historialConsumo);

            // Calcular stock de seguridad (días de stock mínimo)
            var stockSeguridad = consumoPromedioDiario * 7; // 7 días de seguridad

            // Cantidad óptima = Stock máximo - Stock actual + Consumo proyectado
            var consumoProyectado = consumoPromedioDiario * diasProyeccion;
            var cantidadOptima = rangoStock.StockMaximo - stockActual + consumoProyectado + stockSeguridad;

            return Math.Max(0, cantidadOptima);
        }

        /// <summary>
        /// Evalúa si un ingrediente necesita reabastecimiento urgente
        /// </summary>
        public bool RequiereReabastecimientoUrgente(IngredienteAggregate ingredienteAggregate,
                                                   List<MovimientoInventario> historialConsumo)
        {
            if (ingredienteAggregate == null)
                throw new ArgumentNullException(nameof(ingredienteAggregate));

            var stockActual = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;
            var consumoPromedioDiario = CalcularConsumoPromedioDiario(historialConsumo);

            // Si el stock actual no alcanza para 3 días de consumo promedio
            var diasStock = consumoPromedioDiario > 0 ? (decimal)(stockActual / consumoPromedioDiario) : decimal.MaxValue;

            return diasStock <= 3 || ingredienteAggregate.TieneStockBajo();
        }

        /// <summary>
        /// Valida que una recepción de lote sea consistente con las reglas de negocio
        /// </summary>
        public List<string> ValidarRecepcionLote(Lote lote, IngredienteAggregate ingredienteAggregate)
        {
            var errores = new List<string>();

            if (lote == null)
            {
                errores.Add("El lote no puede ser nulo");
                return errores;
            }

            if (ingredienteAggregate == null)
            {
                errores.Add("El agregado de ingrediente no puede ser nulo");
                return errores;
            }

            // Verificar que el lote corresponda al ingrediente
            if (lote.IngredienteId != ingredienteAggregate.Id)
            {
                errores.Add("El lote no corresponde al ingrediente especificado");
            }

            // Verificar que el lote no esté vencido
            if (lote.EstaVencido())
            {
                errores.Add("No se puede recibir un lote que ya está vencido");
            }

            // Verificar que el código del lote sea único
            var loteExistente = ingredienteAggregate.Lotes.FirstOrDefault(l => l.Codigo == lote.Codigo);
            if (loteExistente != null)
            {
                errores.Add($"Ya existe un lote con el código {lote.Codigo}");
            }

            // Verificar que la fecha de vencimiento sea razonable (más de 1 día)
            if (lote.FechaVencimiento.DiasHastaVencimiento() < 1)
            {
                errores.Add("La fecha de vencimiento debe ser al menos 1 día en el futuro");
            }

            return errores;
        }

        /// <summary>
        /// Identifica lotes que deben ser utilizados prioritariamente por FIFO
        /// </summary>
        public List<Lote> ObtenerLotesPrioritarios(IngredienteAggregate ingredienteAggregate)
        {
            if (ingredienteAggregate == null)
                throw new ArgumentNullException(nameof(ingredienteAggregate));

            return ingredienteAggregate.Lotes
                .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido())
                .OrderBy(l => l.FechaVencimiento.Valor)
                .Take(3) // Los 3 más antiguos
                .ToList();
        }

        /// <summary>
        /// Calcula métricas de rotación de inventario
        /// </summary>
        public MetricasRotacion CalcularMetricasRotacion(IngredienteAggregate ingredienteAggregate,
                                                       List<MovimientoInventario> historialMovimientos,
                                                       DateTime fechaInicio, DateTime fechaFin)
        {
            if (ingredienteAggregate == null)
                throw new ArgumentNullException(nameof(ingredienteAggregate));

            var movimientosPeriodo = historialMovimientos
                .Where(m => m.IngredienteId == ingredienteAggregate.Id &&
                           m.FechaMovimiento >= fechaInicio &&
                           m.FechaMovimiento <= fechaFin)
                .ToList();

            var totalConsumo = movimientosPeriodo
                .Where(m => m.EsSalida())
                .Sum(m => m.Cantidad);

            var stockPromedio = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;
            var diasPeriodo = (fechaFin - fechaInicio).Days;

            var rotacionInventario = stockPromedio > 0 ? totalConsumo / stockPromedio : 0;
            var consumoPromedioDiario = diasPeriodo > 0 ? totalConsumo / diasPeriodo : 0;
            var diasStock = consumoPromedioDiario > 0 ? stockPromedio / consumoPromedioDiario : 0;

            return new MetricasRotacion(rotacionInventario, consumoPromedioDiario, diasStock, totalConsumo);
        }

        // Métodos privados auxiliares

        private decimal CalcularConsumoPromedioDiario(List<MovimientoInventario> historialConsumo)
        {
            if (historialConsumo == null || !historialConsumo.Any())
                return 0;

            var movimientosSalida = historialConsumo
                .Where(m => m.EsSalida())
                .OrderBy(m => m.FechaMovimiento)
                .ToList();

            if (!movimientosSalida.Any())
                return 0;

            var totalConsumo = movimientosSalida.Sum(m => m.Cantidad);
            var diasPeriodo = (movimientosSalida.Max(m => m.FechaMovimiento) -
                              movimientosSalida.Min(m => m.FechaMovimiento)).Days;

            return diasPeriodo > 0 ? totalConsumo / diasPeriodo : totalConsumo;
        }
    }

    /// <summary>
    /// Métricas de rotación de inventario
    /// </summary>
    public class MetricasRotacion
    {
        public decimal RotacionInventario { get; }
        public decimal ConsumoPromedioDiario { get; }
        public decimal DiasStock { get; }
        public decimal TotalConsumo { get; }

        public MetricasRotacion(decimal rotacionInventario, decimal consumoPromedioDiario,
                              decimal diasStock, decimal totalConsumo)
        {
            RotacionInventario = rotacionInventario;
            ConsumoPromedioDiario = consumoPromedioDiario;
            DiasStock = diasStock;
            TotalConsumo = totalConsumo;
        }
    }
}