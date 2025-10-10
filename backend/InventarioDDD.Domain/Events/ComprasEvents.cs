using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Events
{
    /// <summary>
    /// Evento disparado cuando se detecta que productos están próximos a vencer
    /// </summary>
    public class AlertaVencimiento : DomainEvent
    {
        public List<LoteProximoAVencer> LotesProximosAVencer { get; private set; }
        public int DiasAnticipacion { get; private set; }

        public AlertaVencimiento(List<LoteProximoAVencer> lotesProximosAVencer, int diasAnticipacion)
        {
            LotesProximosAVencer = lotesProximosAVencer ?? new List<LoteProximoAVencer>();
            DiasAnticipacion = diasAnticipacion;
        }
    }

    /// <summary>
    /// Información sobre un lote próximo a vencer
    /// </summary>
    public class LoteProximoAVencer
    {
        public Guid LoteId { get; set; }
        public Guid IngredienteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasHastaVencimiento { get; set; }
    }

    /// <summary>
    /// Evento disparado cuando un lote ha vencido
    /// </summary>
    public class LoteVencido : DomainEvent
    {
        public Guid LoteId { get; private set; }
        public Guid IngredienteId { get; private set; }
        public string CodigoLote { get; private set; }
        public string NombreIngrediente { get; private set; }
        public decimal CantidadVencida { get; private set; }
        public DateTime FechaVencimiento { get; private set; }
        public PrecioConMoneda ValorPerdida { get; private set; }

        public LoteVencido(Guid loteId, Guid ingredienteId, string codigoLote,
                         string nombreIngrediente, decimal cantidadVencida,
                         DateTime fechaVencimiento, PrecioConMoneda valorPerdida)
        {
            LoteId = loteId;
            IngredienteId = ingredienteId;
            CodigoLote = codigoLote;
            NombreIngrediente = nombreIngrediente;
            CantidadVencida = cantidadVencida;
            FechaVencimiento = fechaVencimiento;
            ValorPerdida = valorPerdida;
        }
    }

    /// <summary>
    /// Evento disparado cuando se genera una nueva orden de compra
    /// </summary>
    public class OrdenDeCompraGenerada : DomainEvent
    {
        public Guid OrdenDeCompraId { get; private set; }
        public string NumeroOrden { get; private set; }
        public Guid ProveedorId { get; private set; }
        public string NombreProveedor { get; private set; }
        public DateTime FechaEntregaEsperada { get; private set; }
        public PrecioConMoneda Total { get; private set; }
        public List<LineaOrdenInfo> Lineas { get; private set; }
        public string MotivoGeneracion { get; private set; }

        public OrdenDeCompraGenerada(Guid ordenDeCompraId, string numeroOrden, Guid proveedorId,
                                   string nombreProveedor, DateTime fechaEntregaEsperada,
                                   PrecioConMoneda total, List<LineaOrdenInfo> lineas,
                                   string motivoGeneracion)
        {
            OrdenDeCompraId = ordenDeCompraId;
            NumeroOrden = numeroOrden;
            ProveedorId = proveedorId;
            NombreProveedor = nombreProveedor;
            FechaEntregaEsperada = fechaEntregaEsperada;
            Total = total;
            Lineas = lineas ?? new List<LineaOrdenInfo>();
            MotivoGeneracion = motivoGeneracion;
        }
    }

    /// <summary>
    /// Información sobre una línea de orden de compra
    /// </summary>
    public class LineaOrdenInfo
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public UnidadDeMedida UnidadDeMedida { get; set; } = null!;
        public PrecioConMoneda PrecioUnitario { get; set; } = null!;
    }

    /// <summary>
    /// Evento disparado cuando se recibe una orden de compra
    /// </summary>
    public class OrdenDeCompraRecibida : DomainEvent
    {
        public Guid OrdenDeCompraId { get; private set; }
        public string NumeroOrden { get; private set; }
        public Guid ProveedorId { get; private set; }
        public DateTime FechaRecepcion { get; private set; }
        public List<LoteRecibidoInfo> LotesRecibidos { get; private set; }
        public List<DiscrepanciaInfo> Discrepancias { get; private set; }

        public OrdenDeCompraRecibida(Guid ordenDeCompraId, string numeroOrden, Guid proveedorId,
                                   DateTime fechaRecepcion, List<LoteRecibidoInfo> lotesRecibidos,
                                   List<DiscrepanciaInfo> discrepancias)
        {
            OrdenDeCompraId = ordenDeCompraId;
            NumeroOrden = numeroOrden;
            ProveedorId = proveedorId;
            FechaRecepcion = fechaRecepcion;
            LotesRecibidos = lotesRecibidos ?? new List<LoteRecibidoInfo>();
            Discrepancias = discrepancias ?? new List<DiscrepanciaInfo>();
        }
    }

    /// <summary>
    /// Información sobre un lote recibido
    /// </summary>
    public class LoteRecibidoInfo
    {
        public Guid LoteId { get; set; }
        public Guid IngredienteId { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public decimal CantidadRecibida { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }

    /// <summary>
    /// Información sobre una discrepancia en la recepción
    /// </summary>
    public class DiscrepanciaInfo
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public string TipoDiscrepancia { get; set; } = string.Empty;
        public decimal CantidadEsperada { get; set; }
        public decimal CantidadRecibida { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Evento disparado cuando hay discrepancias en la recepción
    /// </summary>
    public class DiscrepanciaRecepcion : DomainEvent
    {
        public Guid OrdenDeCompraId { get; private set; }
        public string NumeroOrden { get; private set; }
        public Guid ProveedorId { get; private set; }
        public List<DiscrepanciaInfo> Discrepancias { get; private set; }
        public DateTime FechaRecepcion { get; private set; }

        public DiscrepanciaRecepcion(Guid ordenDeCompraId, string numeroOrden, Guid proveedorId,
                                   List<DiscrepanciaInfo> discrepancias, DateTime fechaRecepcion)
        {
            OrdenDeCompraId = ordenDeCompraId;
            NumeroOrden = numeroOrden;
            ProveedorId = proveedorId;
            Discrepancias = discrepancias ?? new List<DiscrepanciaInfo>();
            FechaRecepcion = fechaRecepcion;
        }
    }
}