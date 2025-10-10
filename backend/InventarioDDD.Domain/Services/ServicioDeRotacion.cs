using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Domain.Services
{
    /// <summary>
    /// Servicio de dominio para análisis de rotación de inventario
    /// </summary>
    public class ServicioDeRotacion
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;

        public ServicioDeRotacion(
            IIngredienteRepository ingredienteRepository,
            IMovimientoInventarioRepository movimientoRepository)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
        }

        /// <summary>
        /// Calcula la rotación de inventario para un ingrediente específico
        /// </summary>
        public async Task<AnalisisRotacion> CalcularRotacionIngrediente(Guid ingredienteId, PeriodoAnalisis periodo)
        {
            var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);
            if (ingredienteAggregate == null)
                throw new ArgumentException($"Ingrediente con ID {ingredienteId} no encontrado");

            var movimientosSalida = await _movimientoRepository.ObtenerPorIngredienteYTipoAsync(
                ingredienteId,
                Enums.TipoMovimiento.Salida,
                periodo.FechaInicio,
                periodo.FechaFin);

            var movimientosEntrada = await _movimientoRepository.ObtenerPorIngredienteYTipoAsync(
                ingredienteId,
                Enums.TipoMovimiento.Entrada,
                periodo.FechaInicio,
                periodo.FechaFin);

            var analisis = new AnalisisRotacion
            {
                IngredienteId = ingredienteId,
                NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                Periodo = periodo,
                FechaAnalisis = DateTime.UtcNow
            };

            // Calcular métricas básicas
            analisis.TotalConsumido = movimientosSalida.Sum(m => m.Cantidad);
            analisis.TotalRecibido = movimientosEntrada.Sum(m => m.Cantidad);
            analisis.StockActual = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;
            analisis.StockPromedio = CalcularStockPromedio(ingredienteAggregate, movimientosEntrada, movimientosSalida, periodo);

            // Calcular índice de rotación
            analisis.IndiceRotacion = CalcularIndiceRotacion(analisis.TotalConsumido, analisis.StockPromedio);

            // Calcular velocidad de rotación (días)
            analisis.VelocidadRotacion = CalcularVelocidadRotacion(analisis.IndiceRotacion, periodo.DiasPeriodo);

            // Clasificar tipo de rotación
            analisis.TipoRotacion = ClasificarRotacion(analisis.IndiceRotacion);

            // Calcular tendencia de consumo
            analisis.TendenciaConsumo = CalcularTendenciaConsumo(movimientosSalida, periodo);

            // Análisis de estacionalidad
            analisis.AnalisisEstacionalidad = CalcularEstacionalidad(movimientosSalida);

            // Cálculo de días de cobertura
            analisis.DiasCobertura = CalcularDiasCobertura(analisis.StockActual, analisis.TotalConsumido, periodo.DiasPeriodo);

            // Recomendaciones
            analisis.Recomendaciones = GenerarRecomendaciones(analisis);

            return analisis;
        }

        /// <summary>
        /// Identifica ingredientes con rotación lenta
        /// </summary>
        public async Task<List<IngredienteLentaRotacion>> IdentificarIngredientesLentaRotacion(
            decimal umbralRotacion = 2.0m,
            int diasUmbral = 90)
        {
            var ingredientesLentaRotacion = new List<IngredienteLentaRotacion>();
            var todosIngredientes = await _ingredienteRepository.ObtenerTodosAsync();

            var periodo = new PeriodoAnalisis
            {
                FechaInicio = DateTime.UtcNow.AddDays(-diasUmbral),
                FechaFin = DateTime.UtcNow
            };

            foreach (var ingredienteAggregate in todosIngredientes)
            {
                var analisisRotacion = await CalcularRotacionIngrediente(ingredienteAggregate.Id, periodo);

                if (analisisRotacion.IndiceRotacion < umbralRotacion &&
                    analisisRotacion.StockActual > 0)
                {
                    var ingredienteLento = new IngredienteLentaRotacion
                    {
                        IngredienteId = ingredienteAggregate.Id,
                        NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                        StockActual = analisisRotacion.StockActual,
                        IndiceRotacion = analisisRotacion.IndiceRotacion,
                        VelocidadRotacion = analisisRotacion.VelocidadRotacion,
                        DiasCobertura = analisisRotacion.DiasCobertura,
                        TipoRotacion = analisisRotacion.TipoRotacion,
                        ValorInventario = CalcularValorInventario(ingredienteAggregate),
                        UltimoMovimiento = await ObtenerFechaUltimoMovimiento(ingredienteAggregate.Id),
                        RiesgoVencimiento = EvaluarRiesgoVencimiento(ingredienteAggregate),
                        AccionesSugeridas = SugerirAcciones(analisisRotacion, ingredienteAggregate)
                    };

                    ingredientesLentaRotacion.Add(ingredienteLento);
                }
            }

            return ingredientesLentaRotacion
                .OrderBy(i => i.IndiceRotacion)
                .ThenByDescending(i => i.ValorInventario)
                .ToList();
        }

        /// <summary>
        /// Proyecta la demanda futura de un ingrediente
        /// </summary>
        public async Task<ProyeccionDemanda> ProyectarDemanda(Guid ingredienteId, int diasProyeccion = 30)
        {
            var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);
            if (ingredienteAggregate == null)
                throw new ArgumentException($"Ingrediente con ID {ingredienteId} no encontrado");

            // Obtener datos históricos (últimos 90 días)
            var fechaInicioHistorico = DateTime.UtcNow.AddDays(-90);
            var movimientosHistoricos = await _movimientoRepository.ObtenerPorIngredienteYTipoAsync(
                ingredienteId,
                Enums.TipoMovimiento.Salida,
                fechaInicioHistorico,
                DateTime.UtcNow);

            var proyeccion = new ProyeccionDemanda
            {
                IngredienteId = ingredienteId,
                NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                FechaProyeccion = DateTime.UtcNow,
                DiasProyeccion = diasProyeccion,
                StockActual = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor
            };

            if (!movimientosHistoricos.Any())
            {
                proyeccion.DemandaProyectada = 0;
                proyeccion.ConfiabilidadProyeccion = 0;
                proyeccion.MetodoProyeccion = "Sin datos históricos";
                return proyeccion;
            }

            // Calcular consumo promedio diario
            var consumoPromedioDiario = CalcularConsumoPromedioDiario(movimientosHistoricos, 90);

            // Análisis de tendencia
            var tendencia = CalcularTendenciaLineal(movimientosHistoricos);

            // Análisis de estacionalidad
            var factorEstacional = CalcularFactorEstacional(movimientosHistoricos, diasProyeccion);

            // Proyección base
            proyeccion.DemandaProyectada = (consumoPromedioDiario * diasProyeccion) * tendencia * factorEstacional;

            // Análisis de variabilidad
            var variabilidad = CalcularVariabilidadDemanda(movimientosHistoricos);
            proyeccion.DesviacionEstandar = variabilidad.DesviacionEstandar;
            proyeccion.CoeficienteVariacion = variabilidad.CoeficienteVariacion;

            // Escenarios de demanda
            proyeccion.EscenarioOptimista = proyeccion.DemandaProyectada * 1.2m;
            proyeccion.EscenarioPesimista = proyeccion.DemandaProyectada * 0.8m;
            proyeccion.EscenarioConservador = proyeccion.DemandaProyectada * 1.1m;

            // Evaluación de confiabilidad
            proyeccion.ConfiabilidadProyeccion = CalcularConfiabilidadProyeccion(movimientosHistoricos, variabilidad);
            proyeccion.MetodoProyeccion = "Promedio móvil con ajuste de tendencia y estacionalidad";

            // Punto de reorden sugerido
            proyeccion.PuntoReordenSugerido = CalcularPuntoReordenOptimo(proyeccion, ingredienteAggregate);

            // Recomendaciones específicas
            proyeccion.Recomendaciones = GenerarRecomendacionesProyeccion(proyeccion, ingredienteAggregate);

            return proyeccion;
        }

        /// <summary>
        /// Genera un reporte completo de rotación para todos los ingredientes
        /// </summary>
        public async Task<ReporteRotacionGeneral> GenerarReporteRotacionGeneral(PeriodoAnalisis periodo)
        {
            var reporte = new ReporteRotacionGeneral
            {
                Periodo = periodo,
                FechaGeneracion = DateTime.UtcNow
            };

            var todosIngredientes = await _ingredienteRepository.ObtenerTodosAsync();
            var analisisCompleto = new List<AnalisisRotacion>();

            foreach (var ingrediente in todosIngredientes)
            {
                var analisis = await CalcularRotacionIngrediente(ingrediente.Id, periodo);
                analisisCompleto.Add(analisis);
            }

            // Clasificar ingredientes por tipo de rotación
            reporte.IngredientesRotacionRapida = analisisCompleto
                .Where(a => a.TipoRotacion == "Rápida")
                .OrderByDescending(a => a.IndiceRotacion)
                .ToList();

            reporte.IngredientesRotacionMedia = analisisCompleto
                .Where(a => a.TipoRotacion == "Media")
                .OrderByDescending(a => a.IndiceRotacion)
                .ToList();

            reporte.IngredientesRotacionLenta = analisisCompleto
                .Where(a => a.TipoRotacion == "Lenta")
                .OrderBy(a => a.IndiceRotacion)
                .ToList();

            // Estadísticas generales
            reporte.EstadisticasGenerales = new EstadisticasRotacion
            {
                RotacionPromedioGeneral = analisisCompleto.Where(a => a.IndiceRotacion > 0).Average(a => a.IndiceRotacion),
                TotalIngredientesAnalizados = analisisCompleto.Count,
                PorcentajeRotacionRapida = (decimal)reporte.IngredientesRotacionRapida.Count / analisisCompleto.Count * 100,
                PorcentajeRotacionMedia = (decimal)reporte.IngredientesRotacionMedia.Count / analisisCompleto.Count * 100,
                PorcentajeRotacionLenta = (decimal)reporte.IngredientesRotacionLenta.Count / analisisCompleto.Count * 100,
                ValorInventarioTotal = analisisCompleto.Sum(a => a.ValorInventario),
                ValorInventarioLentaRotacion = reporte.IngredientesRotacionLenta.Sum(a => a.ValorInventario)
            };

            return reporte;
        }

        // Métodos privados auxiliares

        private decimal CalcularStockPromedio(IngredienteAggregate ingrediente,
            List<MovimientoInventario> entradas,
            List<MovimientoInventario> salidas,
            PeriodoAnalisis periodo)
        {
            var stockInicial = ingrediente.Ingrediente.CantidadEnStock.Valor;
            var stockFinal = stockInicial;

            foreach (var entrada in entradas.OrderBy(e => e.FechaMovimiento))
            {
                stockFinal += entrada.Cantidad;
            }

            foreach (var salida in salidas.OrderBy(s => s.FechaMovimiento))
            {
                stockFinal -= salida.Cantidad;
            }

            return (stockInicial + stockFinal) / 2;
        }

        private decimal CalcularIndiceRotacion(decimal totalConsumido, decimal stockPromedio)
        {
            return stockPromedio > 0 ? totalConsumido / stockPromedio : 0;
        }

        private int CalcularVelocidadRotacion(decimal indiceRotacion, int diasPeriodo)
        {
            return indiceRotacion > 0 ? (int)(diasPeriodo / indiceRotacion) : int.MaxValue;
        }

        private string ClasificarRotacion(decimal indiceRotacion)
        {
            return indiceRotacion switch
            {
                >= 6 => "Muy Rápida",
                >= 4 => "Rápida",
                >= 2 => "Media",
                >= 1 => "Lenta",
                _ => "Muy Lenta"
            };
        }

        private string CalcularTendenciaConsumo(List<MovimientoInventario> movimientos, PeriodoAnalisis periodo)
        {
            if (movimientos.Count < 2) return "Insuficientes datos";

            var primeraMitad = movimientos
                .Where(m => m.FechaMovimiento <= periodo.FechaInicio.AddDays(periodo.DiasPeriodo / 2))
                .Sum(m => m.Cantidad);

            var segundaMitad = movimientos
                .Where(m => m.FechaMovimiento > periodo.FechaInicio.AddDays(periodo.DiasPeriodo / 2))
                .Sum(m => m.Cantidad);

            if (primeraMitad == 0) return segundaMitad > 0 ? "Creciente" : "Estable";

            var cambio = (segundaMitad - primeraMitad) / primeraMitad;

            return cambio switch
            {
                > 0.1m => "Creciente",
                < -0.1m => "Decreciente",
                _ => "Estable"
            };
        }

        private string CalcularEstacionalidad(List<MovimientoInventario> movimientos)
        {
            if (movimientos.Count < 4) return "Datos insuficientes";

            var consumoPorSemana = movimientos
                .GroupBy(m => m.FechaMovimiento.DayOfYear / 7)
                .Select(g => g.Sum(m => m.Cantidad))
                .ToList();

            if (!consumoPorSemana.Any()) return "Sin patrón";

            var promedio = consumoPorSemana.Average();
            var desviacion = Math.Sqrt(consumoPorSemana.Select(c => Math.Pow((double)(c - promedio), 2)).Average());
            var coeficienteVariacion = promedio > 0 ? (decimal)desviacion / promedio : 0;

            return coeficienteVariacion switch
            {
                > 0.3m => "Alta estacionalidad",
                > 0.15m => "Estacionalidad moderada",
                _ => "Baja estacionalidad"
            };
        }

        private int CalcularDiasCobertura(decimal stockActual, decimal totalConsumido, int diasPeriodo)
        {
            if (totalConsumido <= 0) return int.MaxValue;

            var consumoPromedioDiario = totalConsumido / diasPeriodo;
            return consumoPromedioDiario > 0 ? (int)(stockActual / consumoPromedioDiario) : int.MaxValue;
        }

        private List<string> GenerarRecomendaciones(AnalisisRotacion analisis)
        {
            var recomendaciones = new List<string>();

            if (analisis.IndiceRotacion < 1)
            {
                recomendaciones.Add("Evaluar reducción de stock mínimo");
                recomendaciones.Add("Considerar promociones para acelerar consumo");
            }
            else if (analisis.IndiceRotacion > 6)
            {
                recomendaciones.Add("Aumentar frecuencia de pedidos");
                recomendaciones.Add("Revisar niveles de stock de seguridad");
            }

            if (analisis.DiasCobertura > 60)
            {
                recomendaciones.Add("Stock excesivo - optimizar niveles");
            }
            else if (analisis.DiasCobertura < 7)
            {
                recomendaciones.Add("Riesgo de quiebre - aumentar stock");
            }

            return recomendaciones;
        }

        private decimal CalcularConsumoPromedioDiario(List<MovimientoInventario> movimientos, int dias)
        {
            if (!movimientos.Any() || dias <= 0) return 0;

            var totalConsumo = movimientos.Sum(m => m.Cantidad);
            return totalConsumo / dias;
        }

        private decimal CalcularTendenciaLineal(List<MovimientoInventario> movimientos)
        {
            if (movimientos.Count < 2) return 1.0m;

            // Implementación simplificada de regresión lineal
            var consumoPorDia = movimientos
                .GroupBy(m => m.FechaMovimiento.Date)
                .Select(g => new { Fecha = g.Key, Consumo = g.Sum(m => m.Cantidad) })
                .OrderBy(x => x.Fecha)
                .ToList();

            if (consumoPorDia.Count < 2) return 1.0m;

            var primerConsumo = consumoPorDia.Take(consumoPorDia.Count / 2).Average(x => x.Consumo);
            var ultimoConsumo = consumoPorDia.Skip(consumoPorDia.Count / 2).Average(x => x.Consumo);

            return primerConsumo > 0 ? ultimoConsumo / primerConsumo : 1.0m;
        }

        private decimal CalcularFactorEstacional(List<MovimientoInventario> movimientos, int diasProyeccion)
        {
            // Implementación simplificada - en producción se podría usar análisis más sofisticado
            return 1.0m;
        }

        private VariabilidadDemanda CalcularVariabilidadDemanda(List<MovimientoInventario> movimientos)
        {
            if (!movimientos.Any()) return new VariabilidadDemanda();

            var consumosPorDia = movimientos
                .GroupBy(m => m.FechaMovimiento.Date)
                .Select(g => g.Sum(m => m.Cantidad))
                .ToList();

            if (!consumosPorDia.Any()) return new VariabilidadDemanda();

            var promedio = consumosPorDia.Average();
            var varianza = consumosPorDia.Select(c => Math.Pow((double)(c - promedio), 2)).Average();
            var desviacion = (decimal)Math.Sqrt(varianza);

            return new VariabilidadDemanda
            {
                DesviacionEstandar = desviacion,
                CoeficienteVariacion = promedio > 0 ? desviacion / promedio : 0
            };
        }

        private decimal CalcularConfiabilidadProyeccion(List<MovimientoInventario> movimientos, VariabilidadDemanda variabilidad)
        {
            var confiabilidadBase = 0.7m; // 70% base

            // Ajustar por cantidad de datos
            var factorDatos = Math.Min(movimientos.Count / 30.0m, 1.0m);

            // Ajustar por variabilidad (menor variabilidad = mayor confiabilidad)
            var factorVariabilidad = Math.Max(0.3m, 1.0m - variabilidad.CoeficienteVariacion);

            return Math.Min(1.0m, confiabilidadBase * factorDatos * factorVariabilidad);
        }

        private decimal CalcularPuntoReordenOptimo(ProyeccionDemanda proyeccion, IngredienteAggregate ingrediente)
        {
            var consumoDiario = proyeccion.DemandaProyectada / proyeccion.DiasProyeccion;
            var tiempoEntrega = 7; // Asumimos 7 días de tiempo de entrega
            var stockSeguridad = consumoDiario * 3; // 3 días de stock de seguridad

            return (consumoDiario * tiempoEntrega) + stockSeguridad;
        }

        private List<string> GenerarRecomendacionesProyeccion(ProyeccionDemanda proyeccion, IngredienteAggregate ingrediente)
        {
            var recomendaciones = new List<string>();

            if (proyeccion.ConfiabilidadProyeccion < 0.5m)
            {
                recomendaciones.Add("Proyección poco confiable - recopilar más datos históricos");
            }

            if (proyeccion.StockActual < proyeccion.PuntoReordenSugerido)
            {
                recomendaciones.Add("Realizar pedido urgente");
            }

            if (proyeccion.CoeficienteVariacion > 0.5m)
            {
                recomendaciones.Add("Alta variabilidad - considerar múltiples proveedores");
            }

            return recomendaciones;
        }

        private decimal CalcularValorInventario(IngredienteAggregate ingrediente)
        {
            return ingrediente.Lotes.Sum(l => l.CalcularValorInventario());
        }

        private async Task<DateTime?> ObtenerFechaUltimoMovimiento(Guid ingredienteId)
        {
            var ultimoMovimiento = await _movimientoRepository.ObtenerUltimoMovimientoAsync(ingredienteId);
            return ultimoMovimiento?.FechaMovimiento;
        }

        private string EvaluarRiesgoVencimiento(IngredienteAggregate ingrediente)
        {
            var lotesProximosAVencer = ingrediente.ObtenerLotesProximosAVencer(30);

            return lotesProximosAVencer.Count switch
            {
                0 => "Bajo",
                <= 2 => "Medio",
                _ => "Alto"
            };
        }

        private List<string> SugerirAcciones(AnalisisRotacion analisis, IngredienteAggregate ingrediente)
        {
            var acciones = new List<string>();

            if (analisis.IndiceRotacion < 0.5m)
            {
                acciones.Add("Evaluar descontinuación del ingrediente");
                acciones.Add("Reducir punto de reorden");
            }

            if (analisis.DiasCobertura > 90)
            {
                acciones.Add("Implementar promociones especiales");
                acciones.Add("Buscar usos alternativos");
            }

            return acciones;
        }
    }

    // Clases de apoyo para el análisis de rotación

    public class PeriodoAnalisis
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int DiasPeriodo => (FechaFin - FechaInicio).Days;
    }

    public class AnalisisRotacion
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public PeriodoAnalisis Periodo { get; set; } = null!;
        public DateTime FechaAnalisis { get; set; }
        public decimal TotalConsumido { get; set; }
        public decimal TotalRecibido { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockPromedio { get; set; }
        public decimal IndiceRotacion { get; set; }
        public int VelocidadRotacion { get; set; }
        public string TipoRotacion { get; set; } = string.Empty;
        public string TendenciaConsumo { get; set; } = string.Empty;
        public string AnalisisEstacionalidad { get; set; } = string.Empty;
        public int DiasCobertura { get; set; }
        public decimal ValorInventario { get; set; }
        public List<string> Recomendaciones { get; set; } = new List<string>();
    }

    public class IngredienteLentaRotacion
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal IndiceRotacion { get; set; }
        public int VelocidadRotacion { get; set; }
        public int DiasCobertura { get; set; }
        public string TipoRotacion { get; set; } = string.Empty;
        public decimal ValorInventario { get; set; }
        public DateTime? UltimoMovimiento { get; set; }
        public string RiesgoVencimiento { get; set; } = string.Empty;
        public List<string> AccionesSugeridas { get; set; } = new List<string>();
    }

    public class ProyeccionDemanda
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public DateTime FechaProyeccion { get; set; }
        public int DiasProyeccion { get; set; }
        public decimal StockActual { get; set; }
        public decimal DemandaProyectada { get; set; }
        public decimal EscenarioOptimista { get; set; }
        public decimal EscenarioPesimista { get; set; }
        public decimal EscenarioConservador { get; set; }
        public decimal DesviacionEstandar { get; set; }
        public decimal CoeficienteVariacion { get; set; }
        public decimal ConfiabilidadProyeccion { get; set; }
        public string MetodoProyeccion { get; set; } = string.Empty;
        public decimal PuntoReordenSugerido { get; set; }
        public List<string> Recomendaciones { get; set; } = new List<string>();
    }

    public class VariabilidadDemanda
    {
        public decimal DesviacionEstandar { get; set; }
        public decimal CoeficienteVariacion { get; set; }
    }

    public class ReporteRotacionGeneral
    {
        public PeriodoAnalisis Periodo { get; set; } = null!;
        public DateTime FechaGeneracion { get; set; }
        public EstadisticasRotacion EstadisticasGenerales { get; set; } = null!;
        public List<AnalisisRotacion> IngredientesRotacionRapida { get; set; } = new List<AnalisisRotacion>();
        public List<AnalisisRotacion> IngredientesRotacionMedia { get; set; } = new List<AnalisisRotacion>();
        public List<AnalisisRotacion> IngredientesRotacionLenta { get; set; } = new List<AnalisisRotacion>();
    }

    public class EstadisticasRotacion
    {
        public decimal RotacionPromedioGeneral { get; set; }
        public int TotalIngredientesAnalizados { get; set; }
        public decimal PorcentajeRotacionRapida { get; set; }
        public decimal PorcentajeRotacionMedia { get; set; }
        public decimal PorcentajeRotacionLenta { get; set; }
        public decimal ValorInventarioTotal { get; set; }
        public decimal ValorInventarioLentaRotacion { get; set; }
    }
}