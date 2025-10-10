namespace InventarioDDD.Application.DTOs
{
    public class MovimientoInventarioDto
    {
        public Guid Id { get; set; }
        public Guid IngredienteId { get; set; }
        public string IngredienteNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public DateTime FechaMovimiento { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? DocumentoReferencia { get; set; }
        public string? Observaciones { get; set; }
    }
}
