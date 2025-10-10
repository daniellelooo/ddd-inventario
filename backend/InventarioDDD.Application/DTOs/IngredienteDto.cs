namespace InventarioDDD.Application.DTOs
{
    public class IngredienteDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Guid CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public decimal CantidadActual { get; set; }
        public bool RequiereReabastecimiento { get; set; }
    }
}
