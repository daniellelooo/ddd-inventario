using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Events
{
    /// <summary>
    /// Evento disparado cuando un ingrediente es registrado en el sistema
    /// </summary>
    public class IngredienteRegistrado : DomainEvent
    {
        public Guid IngredienteId { get; private set; }
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public UnidadDeMedida UnidadDeMedida { get; private set; }
        public RangoDeStock RangoDeStock { get; private set; }
        public Guid CategoriaId { get; private set; }

        public IngredienteRegistrado(Guid ingredienteId, string nombre, string descripcion,
                                   UnidadDeMedida unidadDeMedida, RangoDeStock rangoDeStock,
                                   Guid categoriaId)
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Descripcion = descripcion;
            UnidadDeMedida = unidadDeMedida;
            RangoDeStock = rangoDeStock;
            CategoriaId = categoriaId;
        }
    }

    /// <summary>
    /// Evento disparado cuando se recibe un lote de ingredientes
    /// </summary>
    public class LoteRecibido : DomainEvent
    {
        public Guid LoteId { get; private set; }
        public Guid IngredienteId { get; private set; }
        public string CodigoLote { get; private set; }
        public decimal CantidadRecibida { get; private set; }
        public FechaVencimiento FechaVencimiento { get; private set; }
        public PrecioConMoneda PrecioUnitario { get; private set; }
        public Guid? OrdenDeCompraId { get; private set; }

        public LoteRecibido(Guid loteId, Guid ingredienteId, string codigoLote,
                          decimal cantidadRecibida, FechaVencimiento fechaVencimiento,
                          PrecioConMoneda precioUnitario, Guid? ordenDeCompraId = null)
        {
            LoteId = loteId;
            IngredienteId = ingredienteId;
            CodigoLote = codigoLote;
            CantidadRecibida = cantidadRecibida;
            FechaVencimiento = fechaVencimiento;
            PrecioUnitario = precioUnitario;
            OrdenDeCompraId = ordenDeCompraId;
        }
    }

    /// <summary>
    /// Evento disparado cuando se consume ingredientes del inventario
    /// </summary>
    public class IngredientesConsumidos : DomainEvent
    {
        public Guid IngredienteId { get; private set; }
        public decimal CantidadConsumida { get; private set; }
        public UnidadDeMedida UnidadDeMedida { get; private set; }
        public string Motivo { get; private set; }
        public List<ConsumoLote> LotesConsumidos { get; private set; }
        public Guid? UsuarioId { get; private set; }

        public IngredientesConsumidos(Guid ingredienteId, decimal cantidadConsumida,
                                    UnidadDeMedida unidadDeMedida, string motivo,
                                    List<ConsumoLote> lotesConsumidos, Guid? usuarioId = null)
        {
            IngredienteId = ingredienteId;
            CantidadConsumida = cantidadConsumida;
            UnidadDeMedida = unidadDeMedida;
            Motivo = motivo;
            LotesConsumidos = lotesConsumidos ?? new List<ConsumoLote>();
            UsuarioId = usuarioId;
        }
    }

    /// <summary>
    /// Información sobre el consumo de un lote específico
    /// </summary>
    public class ConsumoLote
    {
        public Guid LoteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal CantidadConsumida { get; set; }
    }

    /// <summary>
    /// Evento disparado cuando el stock de un ingrediente se actualiza
    /// </summary>
    public class StockActualizado : DomainEvent
    {
        public Guid IngredienteId { get; private set; }
        public decimal StockAnterior { get; private set; }
        public decimal StockNuevo { get; private set; }
        public UnidadDeMedida UnidadDeMedida { get; private set; }
        public string MotivoActualizacion { get; private set; }

        public StockActualizado(Guid ingredienteId, decimal stockAnterior, decimal stockNuevo,
                              UnidadDeMedida unidadDeMedida, string motivoActualizacion)
        {
            IngredienteId = ingredienteId;
            StockAnterior = stockAnterior;
            StockNuevo = stockNuevo;
            UnidadDeMedida = unidadDeMedida;
            MotivoActualizacion = motivoActualizacion;
        }
    }

    /// <summary>
    /// Evento disparado cuando se detecta stock bajo
    /// </summary>
    public class AlertaStockBajo : DomainEvent
    {
        public Guid IngredienteId { get; private set; }
        public string NombreIngrediente { get; private set; }
        public decimal StockActual { get; private set; }
        public decimal StockMinimo { get; private set; }
        public UnidadDeMedida UnidadDeMedida { get; private set; }
        public int NivelUrgencia { get; private set; } // 1: Bajo, 2: Crítico, 3: Agotado

        public AlertaStockBajo(Guid ingredienteId, string nombreIngrediente, decimal stockActual,
                             decimal stockMinimo, UnidadDeMedida unidadDeMedida, int nivelUrgencia)
        {
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
            UnidadDeMedida = unidadDeMedida;
            NivelUrgencia = nivelUrgencia;
        }
    }
}