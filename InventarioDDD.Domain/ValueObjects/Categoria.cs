namespace InventarioDDD.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una categoría de ingrediente
/// </summary>
public record Categoria
{
    public string Nombre { get; init; }
    public string Descripcion { get; init; }

    public Categoria(string nombre, string descripcion = "")
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la categoría es requerido", nameof(nombre));

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
    }

    // Categorías predefinidas
    public static readonly Categoria Lacteos = new("Lácteos", "Productos derivados de la leche");
    public static readonly Categoria Carnes = new("Carnes", "Carnes rojas y blancas");
    public static readonly Categoria Vegetales = new("Vegetales", "Verduras y hortalizas");
    public static readonly Categoria Granos = new("Granos", "Granos y legumbres");
    public static readonly Categoria Especias = new("Especias", "Condimentos y especias");
    public static readonly Categoria Bebidas = new("Bebidas", "Bebidas y líquidos");
}
