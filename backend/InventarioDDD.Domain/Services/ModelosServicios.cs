using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Services
{
    /// <summary>
    /// Requisito de ingrediente para un pedido
    /// </summary>
    public class RequisitoIngrediente
    {
        public Guid IngredienteId { get; set; }
        public decimal CantidadRequerida { get; set; }
        public UnidadDeMedida UnidadDeMedida { get; set; } = null!;
    }

    /// <summary>
    /// Monto total con moneda
    /// </summary>
    public class MontoTotal
    {
        public decimal Valor { get; }
        public string Moneda { get; }

        public MontoTotal(decimal valor, string moneda = "COP")
        {
            Valor = valor;
            Moneda = moneda;
        }

        public override string ToString()
        {
            return $"{Valor:C} {Moneda}";
        }
    }

    /// <summary>
    /// Plan detallado para el consumo de ingredientes
    /// </summary>
    public class PlanConsumo
    {
        private readonly List<ConsumoLotePlanificado> _consumosLote;
        private readonly List<string> _errores;

        public IReadOnlyList<ConsumoLotePlanificado> ConsumosLote => _consumosLote.AsReadOnly();
        public IReadOnlyList<string> Errores => _errores.AsReadOnly();
        public bool EsExitoso { get; private set; }
        public decimal CantidadTotal => _consumosLote.Sum(c => c.CantidadAConsumir);

        public PlanConsumo()
        {
            _consumosLote = new List<ConsumoLotePlanificado>();
            _errores = new List<string>();
            EsExitoso = false;
        }

        public void AgregarConsumoLote(ConsumoLotePlanificado consumo)
        {
            _consumosLote.Add(consumo);
        }

        public void AgregarError(string error)
        {
            _errores.Add(error);
        }

        public void MarcarComoExitoso()
        {
            EsExitoso = true;
        }
    }

    /// <summary>
    /// Información sobre el consumo planificado de un lote específico
    /// </summary>
    public class ConsumoLotePlanificado
    {
        public Guid LoteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal CantidadAConsumir { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public PrecioConMoneda PrecioUnitario { get; set; } = null!;
        public decimal ValorConsumo => CantidadAConsumir * PrecioUnitario.Valor;
    }

    /// <summary>
    /// Resultado de validación con errores y advertencias
    /// </summary>
    public class ResultadoValidacion
    {
        private readonly List<string> _errores;
        private readonly List<string> _advertencias;

        public IReadOnlyList<string> Errores => _errores.AsReadOnly();
        public IReadOnlyList<string> Advertencias => _advertencias.AsReadOnly();
        public bool EsValido { get; private set; }
        public bool TieneAdvertencias => _advertencias.Any();

        public ResultadoValidacion()
        {
            _errores = new List<string>();
            _advertencias = new List<string>();
            EsValido = false;
        }

        public void AgregarError(string error)
        {
            _errores.Add(error);
        }

        public void AgregarAdvertencia(string advertencia)
        {
            _advertencias.Add(advertencia);
        }

        public void MarcarComoExitoso()
        {
            EsValido = true;
        }
    }

    /// <summary>
    /// Alerta para consumo urgente de lotes próximos a vencer
    /// </summary>
    public class AlertaConsumoUrgente
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public Guid LoteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasHastaVencimiento { get; set; }
        public decimal ValorEnRiesgo { get; set; }
    }

    /// <summary>
    /// Impacto financiero de un consumo
    /// </summary>
    public class ImpactoFinancieroConsumo
    {
        public decimal CostoTotal { get; }
        public decimal CostoPromedioPonderado { get; }
        public decimal ValorInventarioConsumido { get; }

        public ImpactoFinancieroConsumo(decimal costoTotal, decimal costoPromedioPonderado, decimal valorInventarioConsumido)
        {
            CostoTotal = costoTotal;
            CostoPromedioPonderado = costoPromedioPonderado;
            ValorInventarioConsumido = valorInventarioConsumido;
        }
    }

    /// <summary>
    /// Recomendación de consumo para optimizar el inventario
    /// </summary>
    public class RecomendacionConsumo
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public Guid LoteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal CantidadRecomendada { get; set; }
        public int Prioridad { get; set; } // 1-5, siendo 5 la máxima prioridad
        public string Motivo { get; set; } = string.Empty;
    }
}