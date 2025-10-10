using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Entities
{
    public class MovimientoInventario
    {
        public Guid Id { get; private set; }
        public Guid IngredienteId { get; private set; }
        public Guid? LoteId { get; private set; }
        public TipoMovimiento TipoMovimiento { get; private set; }
        public decimal Cantidad { get; private set; }
        public UnidadDeMedida UnidadDeMedida { get; private set; }
        public DateTime FechaMovimiento { get; private set; }
        public string Motivo { get; private set; }
        public string? DocumentoReferencia { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? Observaciones { get; private set; }

        private MovimientoInventario()
        {
            UnidadDeMedida = null!;
            Motivo = string.Empty;
        }

        public MovimientoInventario(Guid ingredienteId, TipoMovimiento tipoMovimiento,
                                   decimal cantidad, UnidadDeMedida unidadDeMedida,
                                   string motivo, Guid? loteId = null,
                                   string? documentoReferencia = null, Guid? usuarioId = null,
                                   string? observaciones = null)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(cantidad));

            Id = Guid.NewGuid();
            IngredienteId = ingredienteId;
            LoteId = loteId;
            TipoMovimiento = tipoMovimiento;
            Cantidad = cantidad;
            UnidadDeMedida = unidadDeMedida ?? throw new ArgumentNullException(nameof(unidadDeMedida));
            FechaMovimiento = DateTime.UtcNow;
            Motivo = motivo ?? throw new ArgumentNullException(nameof(motivo));
            DocumentoReferencia = documentoReferencia;
            UsuarioId = usuarioId;
            Observaciones = observaciones;
        }

        public bool EsEntrada()
        {
            return TipoMovimiento == TipoMovimiento.Entrada;
        }

        public bool EsSalida()
        {
            return TipoMovimiento == TipoMovimiento.Salida;
        }

        public bool EsAjuste()
        {
            return TipoMovimiento == TipoMovimiento.Ajuste;
        }

        public bool EsTransferencia()
        {
            return TipoMovimiento == TipoMovimiento.Transferencia;
        }

        public decimal ObtenerImpactoEnStock()
        {
            return TipoMovimiento switch
            {
                TipoMovimiento.Entrada => Cantidad,
                TipoMovimiento.Salida => -Cantidad,
                TipoMovimiento.Ajuste => Cantidad, // Puede ser positivo o negativo según el contexto
                TipoMovimiento.Transferencia => -Cantidad, // Salida desde la perspectiva del origen
                _ => 0
            };
        }

        public void AgregarObservacion(string observacion)
        {
            if (string.IsNullOrWhiteSpace(observacion))
                return;

            if (string.IsNullOrEmpty(Observaciones))
            {
                Observaciones = observacion;
            }
            else
            {
                Observaciones += $" | {observacion}";
            }
        }

        public bool FueRealizadoPor(Guid usuarioId)
        {
            return UsuarioId == usuarioId;
        }

        public bool TieneDocumentoReferencia()
        {
            return !string.IsNullOrEmpty(DocumentoReferencia);
        }

        public bool FueRealizadoEn(DateTime fecha)
        {
            return FechaMovimiento.Date == fecha.Date;
        }

        public bool FueRealizadoEntre(DateTime fechaInicio, DateTime fechaFin)
        {
            return FechaMovimiento >= fechaInicio && FechaMovimiento <= fechaFin;
        }
    }
}