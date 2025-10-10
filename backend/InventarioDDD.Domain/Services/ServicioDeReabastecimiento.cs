using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Domain.Services
{
    /// <summary>
    /// Servicio de dominio para la gestión de reabastecimiento
    /// </summary>
    public class ServicioDeReabastecimiento
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IOrdenDeCompraRepository _ordenDeCompraRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;

        public ServicioDeReabastecimiento(
            IIngredienteRepository ingredienteRepository,
            IProveedorRepository proveedorRepository,
            IOrdenDeCompraRepository ordenDeCompraRepository,
            IMovimientoInventarioRepository movimientoRepository)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _ordenDeCompraRepository = ordenDeCompraRepository ?? throw new ArgumentNullException(nameof(ordenDeCompraRepository));
            _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
        }

        /// <summary>
        /// Genera órdenes de compra automáticas para ingredientes que requieren reabastecimiento
        /// </summary>
        public async Task<List<OrdenDeCompra>> GenerarOrdenDeCompraAutomatica()
        {
            var ingredientesQueRequierenReabastecimiento = await _ingredienteRepository.ObtenerQueRequierenReabastecimientoAsync();
            var ordenesGeneradas = new List<OrdenDeCompra>();

            // Agrupar por proveedor para crear órdenes consolidadas
            var ingredientesPorProveedor = new Dictionary<Guid, List<IngredienteAggregate>>();

            foreach (var ingrediente in ingredientesQueRequierenReabastecimiento)
            {
                var proveedor = await SugerirProveedor(ingrediente.Id);
                if (proveedor != null)
                {
                    if (!ingredientesPorProveedor.ContainsKey(proveedor.Id))
                        ingredientesPorProveedor[proveedor.Id] = new List<IngredienteAggregate>();

                    ingredientesPorProveedor[proveedor.Id].Add(ingrediente);
                }
            }

            foreach (var grupo in ingredientesPorProveedor)
            {
                foreach (var ingrediente in grupo.Value)
                {
                    var numeroOrden = await _ordenDeCompraRepository.GenerarNumeroOrdenAsync();
                    var fechaEntregaEsperada = DateTime.UtcNow.AddDays(7); // 7 días por defecto
                    var cantidadAOrdenar = await CalcularPuntoDeReorden(ingrediente.Id);
                    var cantidadDisponible = new CantidadDisponible(cantidadAOrdenar.Valor);
                    var precioEstimado = ObtenerPrecioEstimado(ingrediente.Id, grupo.Key);

                    var ordenDeCompra = new OrdenDeCompra(
                        numeroOrden,
                        ingrediente.Id,
                        grupo.Key,
                        cantidadDisponible,
                        precioEstimado,
                        fechaEntregaEsperada,
                        "Orden generada automáticamente por reabastecimiento"
                    );

                    ordenesGeneradas.Add(ordenDeCompra);
                }
            }

            return ordenesGeneradas;
        }

        /// <summary>
        /// Calcula el punto de reorden para un ingrediente específico
        /// </summary>
        public async Task<Cantidad> CalcularPuntoDeReorden(Guid ingredienteId)
        {
            var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);
            if (ingredienteAggregate == null)
                throw new ArgumentException("Ingrediente no encontrado");

            // Obtener historial de consumo de los últimos 3 meses
            var fechaDesde = DateTime.UtcNow.AddMonths(-3);
            var historialMovimientos = await _movimientoRepository.ObtenerHistorialAsync(ingredienteId, fechaDesde);

            var movimientosSalida = historialMovimientos.Where(m => m.EsSalida()).ToList();

            if (!movimientosSalida.Any())
            {
                // Si no hay historial, usar el punto de reorden configurado
                return new Cantidad(
                    ingredienteAggregate.Ingrediente.RangoDeStock.CalcularPuntoDeReorden(),
                    ingredienteAggregate.Ingrediente.UnidadDeMedida
                );
            }

            // Calcular consumo promedio diario
            var totalConsumo = movimientosSalida.Sum(m => m.Cantidad);
            var diasPeriodo = (DateTime.UtcNow - fechaDesde).Days;
            var consumoPromedioDiario = totalConsumo / diasPeriodo;

            // Tiempo de entrega promedio (asumimos 7 días)
            var tiempoEntregaDias = 7;

            // Stock de seguridad (50% del consumo en tiempo de entrega)
            var stockSeguridad = (consumoPromedioDiario * tiempoEntregaDias) * 1.5m;

            // Cantidad a ordenar = Diferencia hasta stock máximo
            var stockActual = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;
            var stockMaximo = ingredienteAggregate.Ingrediente.RangoDeStock.StockMaximo;
            var cantidadAOrdenar = Math.Max(0, stockMaximo - stockActual + stockSeguridad);

            return new Cantidad(cantidadAOrdenar, ingredienteAggregate.Ingrediente.UnidadDeMedida);
        }

        /// <summary>
        /// Sugiere el mejor proveedor para un ingrediente
        /// </summary>
        public async Task<Proveedor?> SugerirProveedor(Guid ingredienteId)
        {
            var proveedoresQueSuministran = await _proveedorRepository.ObtenerQueSuministranAsync(ingredienteId);

            if (!proveedoresQueSuministran.Any())
                return null;

            // Evaluar proveedores y seleccionar el mejor
            ProveedorAggregate? mejorProveedor = null;
            var mejorPuntaje = 0.0;

            foreach (var proveedorAggregate in proveedoresQueSuministran)
            {
                if (!proveedorAggregate.Proveedor.Activo)
                    continue;

                // Evaluar desempeño en los últimos 6 meses
                var fechaDesde = DateTime.UtcNow.AddMonths(-6);
                var desempeno = proveedorAggregate.EvaluarDesempeno(fechaDesde, DateTime.UtcNow);

                // Calcular puntaje basado en cumplimiento y experiencia
                var puntaje = CalcularPuntajeProveedor(desempeno, proveedorAggregate.Proveedor.EsNacional());

                if (puntaje > mejorPuntaje)
                {
                    mejorPuntaje = puntaje;
                    mejorProveedor = proveedorAggregate;
                }
            }

            return mejorProveedor?.Proveedor;
        }

        /// <summary>
        /// Identifica ingredientes que están por debajo del punto de reorden
        /// </summary>
        public async Task<List<AlertaReabastecimiento>> IdentificarIngredientesParaReabastecimiento()
        {
            var ingredientesQueRequieren = await _ingredienteRepository.ObtenerQueRequierenReabastecimientoAsync();
            var alertas = new List<AlertaReabastecimiento>();

            foreach (var ingrediente in ingredientesQueRequieren)
            {
                var cantidadOptima = await CalcularPuntoDeReorden(ingrediente.Id);
                var proveedor = await SugerirProveedor(ingrediente.Id);

                alertas.Add(new AlertaReabastecimiento
                {
                    IngredienteId = ingrediente.Id,
                    NombreIngrediente = ingrediente.Ingrediente.Nombre,
                    StockActual = ingrediente.Ingrediente.CantidadEnStock.Valor,
                    PuntoDeReorden = ingrediente.Ingrediente.RangoDeStock.CalcularPuntoDeReorden(),
                    CantidadSugerida = cantidadOptima.Valor,
                    ProveedorSugerido = proveedor,
                    Urgencia = CalcularUrgencia(ingrediente)
                });
            }

            return alertas.OrderByDescending(a => a.Urgencia).ToList();
        }

        // Métodos privados auxiliares

        private PrecioConMoneda ObtenerPrecioEstimado(Guid ingredienteId, Guid proveedorId)
        {
            // En un escenario real, esto consultaría histórico de precios o catálogo
            // Por ahora, retornamos un precio estimado básico
            return new PrecioConMoneda(1000, "COP");
        }

        private double CalcularPuntajeProveedor(DesempenoProveedor desempeno, bool esNacional)
        {
            var puntajeCumplimiento = desempeno.PorcentajeCumplimiento;
            var penalizacionCancelacion = desempeno.PorcentajeCancelacion * 0.5;
            var bonusNacional = esNacional ? 10 : 0; // Preferencia por proveedores nacionales

            return puntajeCumplimiento - penalizacionCancelacion + bonusNacional;
        }

        private int CalcularUrgencia(IngredienteAggregate ingrediente)
        {
            var stockActual = ingrediente.Ingrediente.CantidadEnStock.Valor;
            var stockMinimo = ingrediente.Ingrediente.RangoDeStock.StockMinimo;
            var puntoReorden = ingrediente.Ingrediente.RangoDeStock.CalcularPuntoDeReorden();

            if (stockActual <= stockMinimo) return 5; // Crítico
            if (stockActual <= puntoReorden * 0.5m) return 4; // Alto
            if (stockActual <= puntoReorden * 0.8m) return 3; // Medio
            if (stockActual <= puntoReorden) return 2; // Bajo
            return 1; // Mínimo
        }
    }

    /// <summary>
    /// Cantidad con unidad de medida
    /// </summary>
    public class Cantidad
    {
        public decimal Valor { get; }
        public UnidadDeMedida UnidadDeMedida { get; }

        public Cantidad(decimal valor, UnidadDeMedida unidadDeMedida)
        {
            Valor = valor;
            UnidadDeMedida = unidadDeMedida ?? throw new ArgumentNullException(nameof(unidadDeMedida));
        }

        public override string ToString()
        {
            return $"{Valor} {UnidadDeMedida.Simbolo}";
        }
    }

    /// <summary>
    /// Alerta de reabastecimiento
    /// </summary>
    public class AlertaReabastecimiento
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal PuntoDeReorden { get; set; }
        public decimal CantidadSugerida { get; set; }
        public Proveedor? ProveedorSugerido { get; set; }
        public int Urgencia { get; set; } // 1-5, siendo 5 la máxima urgencia
    }
}