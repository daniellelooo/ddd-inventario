namespace InventarioDDD.Application.DTOs
{
    public class OrdenDeCompraDto
    {
        public Guid Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public Guid ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEntregaEsperada { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public List<DetalleOrdenDto> Detalles { get; set; } = new();
    }

    public class DetalleOrdenDto
    {
        public Guid IngredienteId { get; set; }
        public string IngredienteNombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
