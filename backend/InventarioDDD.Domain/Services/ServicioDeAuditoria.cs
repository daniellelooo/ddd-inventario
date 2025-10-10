using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Domain.Services
{
    /// <summary>
    /// Servicio de dominio para auditoría y reportes
    /// </summary>
    public class ServicioDeAuditoria
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;

        public ServicioDeAuditoria(
            IIngredienteRepository ingredienteRepository,
            ILoteRepository loteRepository,
            IMovimientoInventarioRepository movimientoRepository)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _loteRepository = loteRepository ?? throw new ArgumentNullException(nameof(loteRepository));
            _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
        }

        /// <summary>
        /// Genera un reporte detallado de consumo para un período específico
        /// </summary>
        public async Task<ReporteConsumo> GenerarReporteConsumo(PeriodoReporte periodo)
        {
            var movimientosSalida = await _movimientoRepository.ObtenerPorTipoAsync(
                Enums.TipoMovimiento.Salida,
                periodo.FechaInicio,
                periodo.FechaFin);

            var reporte = new ReporteConsumo
            {
                Periodo = periodo,
                FechaGeneracion = DateTime.UtcNow
            };

            // Agrupar por ingrediente
            var consumoPorIngrediente = movimientosSalida
                .GroupBy(m => m.IngredienteId)
                .Select(g => new ConsumoIngrediente
                {
                    IngredienteId = g.Key,
                    CantidadTotal = g.Sum(m => m.Cantidad),
                    NumeroMovimientos = g.Count(),
                    FechaUltimoConsumo = g.Max(m => m.FechaMovimiento),
                    ValorConsumo = 0 // Se calculará después con precios
                }).ToList();

            // Enriquecer con información de ingredientes
            foreach (var consumo in consumoPorIngrediente)
            {
                var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(consumo.IngredienteId);
                if (ingredienteAggregate != null)
                {
                    consumo.NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre;
                    consumo.UnidadMedida = ingredienteAggregate.Ingrediente.UnidadDeMedida.Simbolo;

                    // Calcular valor aproximado del consumo
                    var valorPromedio = CalcularValorPromedioIngrediente(ingredienteAggregate);
                    consumo.ValorConsumo = consumo.CantidadTotal * valorPromedio;
                }
            }

            reporte.ConsumosPorIngrediente = consumoPorIngrediente.OrderByDescending(c => c.CantidadTotal).ToList();
            reporte.TotalMovimientos = movimientosSalida.Count;
            reporte.ValorTotalConsumo = consumoPorIngrediente.Sum(c => c.ValorConsumo);

            return reporte;
        }

        /// <summary>
        /// Calcula las mermas ocurridas en un período
        /// </summary>
        public async Task<ReporteMermas> CalcularMermas(PeriodoReporte periodo)
        {
            var lotesVencidos = await _loteRepository.ObtenerVencidosAsync();
            var lotesVencidosEnPeriodo = lotesVencidos
                .Where(l => l.FechaVencimiento.Valor >= periodo.FechaInicio &&
                           l.FechaVencimiento.Valor <= periodo.FechaFin)
                .ToList();

            var reporte = new ReporteMermas
            {
                Periodo = periodo,
                FechaGeneracion = DateTime.UtcNow
            };

            var mermasPorIngrediente = new List<MermaIngrediente>();

            var ingredientesAfectados = lotesVencidosEnPeriodo
                .GroupBy(l => l.IngredienteId)
                .ToList();

            foreach (var grupo in ingredientesAfectados)
            {
                var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(grupo.Key);
                if (ingredienteAggregate == null) continue;

                var cantidadPerdida = grupo.Sum(l => l.CantidadDisponible);
                var valorPerdido = grupo.Sum(l => l.CalcularValorInventario());

                mermasPorIngrediente.Add(new MermaIngrediente
                {
                    IngredienteId = grupo.Key,
                    NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                    CantidadPerdida = cantidadPerdida,
                    ValorPerdido = valorPerdido,
                    UnidadMedida = ingredienteAggregate.Ingrediente.UnidadDeMedida.Simbolo,
                    NumeroLotesAfectados = grupo.Count(),
                    PorcentajeMerma = CalcularPorcentajeMerma(cantidadPerdida, ingredienteAggregate)
                });
            }

            reporte.MermasPorIngrediente = mermasPorIngrediente.OrderByDescending(m => m.ValorPerdido).ToList();
            reporte.TotalCantidadPerdida = mermasPorIngrediente.Sum(m => m.CantidadPerdida);
            reporte.TotalValorPerdido = mermasPorIngrediente.Sum(m => m.ValorPerdido);
            reporte.TotalLotesAfectados = lotesVencidosEnPeriodo.Count;

            return reporte;
        }

        /// <summary>
        /// Compara el inventario físico con el digital para identificar discrepancias
        /// </summary>
        public async Task<List<DiscrepanciaInventario>> CompararInventarioFisicoVsDigital(List<ConteoFisico> conteosFisicos)
        {
            var discrepancias = new List<DiscrepanciaInventario>();

            foreach (var conteo in conteosFisicos)
            {
                var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(conteo.IngredienteId);
                if (ingredienteAggregate == null) continue;

                var stockDigital = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;
                var diferencia = conteo.CantidadFisica - stockDigital;

                if (Math.Abs(diferencia) > 0.01m) // Tolerancia mínima
                {
                    var discrepancia = new DiscrepanciaInventario
                    {
                        IngredienteId = conteo.IngredienteId,
                        NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                        StockDigital = stockDigital,
                        StockFisico = conteo.CantidadFisica,
                        Diferencia = diferencia,
                        PorcentajeDiferencia = stockDigital != 0 ? (diferencia / stockDigital) * 100 : 0,
                        UnidadMedida = ingredienteAggregate.Ingrediente.UnidadDeMedida.Simbolo,
                        FechaConteo = conteo.FechaConteo,
                        ResponsableConteo = conteo.ResponsableConteo,
                        Observaciones = conteo.Observaciones
                    };

                    // Clasificar severidad
                    discrepancia.Severidad = ClasificarSeveridadDiscrepancia(Math.Abs(discrepancia.PorcentajeDiferencia));

                    discrepancias.Add(discrepancia);
                }
            }

            return discrepancias.OrderByDescending(d => Math.Abs(d.Diferencia)).ToList();
        }

        /// <summary>
        /// Genera alertas de vencimiento próximo
        /// </summary>
        public async Task<List<AlertaVencimiento>> GenerarAlertasVencimiento(int diasAnticipacion = 7)
        {
            var alertas = new List<AlertaVencimiento>();
            var todosIngredientes = await _ingredienteRepository.ObtenerTodosAsync();

            foreach (var ingredienteAggregate in todosIngredientes)
            {
                var lotesProximosAVencer = ingredienteAggregate.ObtenerLotesProximosAVencer(diasAnticipacion);

                foreach (var lote in lotesProximosAVencer)
                {
                    var alerta = new AlertaVencimiento
                    {
                        IngredienteId = ingredienteAggregate.Id,
                        NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                        LoteId = lote.Id,
                        CodigoLote = lote.Codigo,
                        CantidadDisponible = lote.CantidadDisponible,
                        FechaVencimiento = lote.FechaVencimiento.Valor,
                        DiasHastaVencimiento = lote.FechaVencimiento.DiasHastaVencimiento(),
                        ValorEnRiesgo = lote.CalcularValorInventario(),
                        UnidadMedida = ingredienteAggregate.Ingrediente.UnidadDeMedida.Simbolo,
                        Prioridad = CalcularPrioridadAlerta(lote.FechaVencimiento.DiasHastaVencimiento())
                    };

                    alertas.Add(alerta);
                }
            }

            return alertas.OrderBy(a => a.DiasHastaVencimiento).ThenByDescending(a => a.ValorEnRiesgo).ToList();
        }

        /// <summary>
        /// Genera un reporte consolidado de auditoría
        /// </summary>
        public async Task<ReporteAuditoria> GenerarReporteAuditoria(PeriodoReporte periodo)
        {
            var reporte = new ReporteAuditoria
            {
                Periodo = periodo,
                FechaGeneracion = DateTime.UtcNow
            };

            // Recopilar información de múltiples fuentes
            var reporteConsumo = await GenerarReporteConsumo(periodo);
            var reporteMermas = await CalcularMermas(periodo);
            var alertasVencimiento = await GenerarAlertasVencimiento();

            // Estadísticas generales
            var todosMovimientos = await _movimientoRepository.ObtenerEnRangoFechasAsync(periodo.FechaInicio, periodo.FechaFin);
            var todosIngredientes = await _ingredienteRepository.ObtenerTodosAsync();

            reporte.EstadisticasGenerales = new EstadisticasGenerales
            {
                TotalIngredientesActivos = todosIngredientes.Count(i => i.Ingrediente.Activo),
                TotalMovimientos = todosMovimientos.Count,
                TotalConsumo = reporteConsumo.ValorTotalConsumo,
                TotalMermas = reporteMermas.TotalValorPerdido,
                IngredientesConStockBajo = todosIngredientes.Count(i => i.TieneStockBajo()),
                AlertasVencimientoActivas = alertasVencimiento.Count,
                RotacionPromedioInventario = CalcularRotacionPromedio(todosIngredientes, todosMovimientos)
            };

            reporte.ReporteConsumo = reporteConsumo;
            reporte.ReporteMermas = reporteMermas;
            reporte.AlertasVencimiento = alertasVencimiento.Take(10).ToList(); // Top 10 más urgentes

            return reporte;
        }

        // Métodos privados auxiliares

        private decimal CalcularValorPromedioIngrediente(IngredienteAggregate ingredienteAggregate)
        {
            if (!ingredienteAggregate.Lotes.Any()) return 0;

            var lotesConValor = ingredienteAggregate.Lotes.Where(l => l.CantidadDisponible > 0).ToList();
            if (!lotesConValor.Any()) return 0;

            return lotesConValor.Average(l => l.PrecioUnitario.Valor);
        }

        private decimal CalcularPorcentajeMerma(decimal cantidadPerdida, IngredienteAggregate ingredienteAggregate)
        {
            var stockTotal = ingredienteAggregate.Lotes.Sum(l => l.CantidadInicial);
            return stockTotal > 0 ? (cantidadPerdida / stockTotal) * 100 : 0;
        }

        private string ClasificarSeveridadDiscrepancia(decimal porcentajeDiferencia)
        {
            return porcentajeDiferencia switch
            {
                >= 20 => "Crítica",
                >= 10 => "Alta",
                >= 5 => "Media",
                _ => "Baja"
            };
        }

        private int CalcularPrioridadAlerta(int diasHastaVencimiento)
        {
            return diasHastaVencimiento switch
            {
                <= 2 => 5, // Crítica
                <= 5 => 4, // Alta
                <= 10 => 3, // Media
                <= 15 => 2, // Baja
                _ => 1 // Mínima
            };
        }

        private decimal CalcularRotacionPromedio(List<IngredienteAggregate> ingredientes, List<MovimientoInventario> movimientos)
        {
            if (!ingredientes.Any()) return 0;

            var rotaciones = new List<decimal>();

            foreach (var ingrediente in ingredientes)
            {
                var movimientosIngrediente = movimientos.Where(m => m.IngredienteId == ingrediente.Id && m.EsSalida()).ToList();
                if (movimientosIngrediente.Any())
                {
                    var totalConsumo = movimientosIngrediente.Sum(m => m.Cantidad);
                    var stockPromedio = ingrediente.Ingrediente.CantidadEnStock.Valor;

                    if (stockPromedio > 0)
                    {
                        rotaciones.Add(totalConsumo / stockPromedio);
                    }
                }
            }

            return rotaciones.Any() ? rotaciones.Average() : 0;
        }
    }

    /// <summary>
    /// Período para reportes
    /// </summary>
    public class PeriodoReporte
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reporte de consumo
    /// </summary>
    public class ReporteConsumo
    {
        public PeriodoReporte Periodo { get; set; } = null!;
        public DateTime FechaGeneracion { get; set; }
        public List<ConsumoIngrediente> ConsumosPorIngrediente { get; set; } = new List<ConsumoIngrediente>();
        public int TotalMovimientos { get; set; }
        public decimal ValorTotalConsumo { get; set; }
    }

    public class ConsumoIngrediente
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal CantidadTotal { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public int NumeroMovimientos { get; set; }
        public DateTime FechaUltimoConsumo { get; set; }
        public decimal ValorConsumo { get; set; }
    }

    /// <summary>
    /// Reporte de mermas
    /// </summary>
    public class ReporteMermas
    {
        public PeriodoReporte Periodo { get; set; } = null!;
        public DateTime FechaGeneracion { get; set; }
        public List<MermaIngrediente> MermasPorIngrediente { get; set; } = new List<MermaIngrediente>();
        public decimal TotalCantidadPerdida { get; set; }
        public decimal TotalValorPerdido { get; set; }
        public int TotalLotesAfectados { get; set; }
    }

    public class MermaIngrediente
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal CantidadPerdida { get; set; }
        public decimal ValorPerdido { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public int NumeroLotesAfectados { get; set; }
        public decimal PorcentajeMerma { get; set; }
    }

    /// <summary>
    /// Discrepancia entre inventario físico y digital
    /// </summary>
    public class DiscrepanciaInventario
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal StockDigital { get; set; }
        public decimal StockFisico { get; set; }
        public decimal Diferencia { get; set; }
        public decimal PorcentajeDiferencia { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public DateTime FechaConteo { get; set; }
        public string ResponsableConteo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string Severidad { get; set; } = string.Empty;
    }

    /// <summary>
    /// Conteo físico de inventario
    /// </summary>
    public class ConteoFisico
    {
        public Guid IngredienteId { get; set; }
        public decimal CantidadFisica { get; set; }
        public DateTime FechaConteo { get; set; }
        public string ResponsableConteo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Alerta de vencimiento
    /// </summary>
    public class AlertaVencimiento
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public Guid LoteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasHastaVencimiento { get; set; }
        public decimal ValorEnRiesgo { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public int Prioridad { get; set; }
    }

    /// <summary>
    /// Reporte consolidado de auditoría
    /// </summary>
    public class ReporteAuditoria
    {
        public PeriodoReporte Periodo { get; set; } = null!;
        public DateTime FechaGeneracion { get; set; }
        public EstadisticasGenerales EstadisticasGenerales { get; set; } = null!;
        public ReporteConsumo ReporteConsumo { get; set; } = null!;
        public ReporteMermas ReporteMermas { get; set; } = null!;
        public List<AlertaVencimiento> AlertasVencimiento { get; set; } = new List<AlertaVencimiento>();
    }

    public class EstadisticasGenerales
    {
        public int TotalIngredientesActivos { get; set; }
        public int TotalMovimientos { get; set; }
        public decimal TotalConsumo { get; set; }
        public decimal TotalMermas { get; set; }
        public int IngredientesConStockBajo { get; set; }
        public int AlertasVencimientoActivas { get; set; }
        public decimal RotacionPromedioInventario { get; set; }
    }
}