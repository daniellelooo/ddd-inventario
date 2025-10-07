namespace InventarioDDD.Domain.ValueObjects;

/// <summary>
/// Value Object que representa la unidad de medida
/// </summary>
public record UnidadDeMedida
{
    public string Nombre { get; init; }
    public string Simbolo { get; init; }

    public UnidadDeMedida(string nombre, string simbolo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la unidad es requerido", nameof(nombre));

        if (string.IsNullOrWhiteSpace(simbolo))
            throw new ArgumentException("El símbolo de la unidad es requerido", nameof(simbolo));

        Nombre = nombre.Trim();
        Simbolo = simbolo.Trim();
    }

    // Unidades predefinidas comunes
    public static readonly UnidadDeMedida Kilogramos = new("Kilogramos", "kg");
    public static readonly UnidadDeMedida Gramos = new("Gramos", "g");
    public static readonly UnidadDeMedida Litros = new("Litros", "L");
    public static readonly UnidadDeMedida Mililitros = new("Mililitros", "ml");
    public static readonly UnidadDeMedida Unidades = new("Unidades", "unid");
}
