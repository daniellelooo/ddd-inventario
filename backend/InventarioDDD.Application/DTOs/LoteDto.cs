namespace InventarioDDD.Application.DTOs
{
    public class LoteDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public Guid IngredienteId { get; set; }
        public string IngredienteNombre { get; set; } = string.Empty;
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public bool Vencido { get; set; }
        public int DiasHastaVencimiento { get; set; }
    }
}
