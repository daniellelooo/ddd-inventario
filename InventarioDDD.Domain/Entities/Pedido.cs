namespace InventarioDDD.Domain.Entities;

/// <summary>
/// Entidad Pedido
/// </summary>
public class Pedido
{
    private readonly List<long> _ingredientesIds = new();

    public long Id { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public string Cliente { get; private set; } = string.Empty;
    public IReadOnlyList<long> IngredientesIds => _ingredientesIds.AsReadOnly();

    // Constructor para EF Core
    private Pedido() { }

    public Pedido(long id, string cliente, List<long> ingredientesIds)
    {
        if (string.IsNullOrWhiteSpace(cliente))
            throw new ArgumentException("El cliente es requerido", nameof(cliente));

        Id = id;
        Cliente = cliente;
        FechaCreacion = DateTime.Now;

        if (ingredientesIds != null && ingredientesIds.Any())
            _ingredientesIds.AddRange(ingredientesIds);
    }

    public void AgregarIngrediente(long ingredienteId)
    {
        if (ingredienteId <= 0)
            throw new ArgumentException("El ingrediente ID es inválido", nameof(ingredienteId));

        if (!_ingredientesIds.Contains(ingredienteId))
            _ingredientesIds.Add(ingredienteId);
    }

    public void RemoverIngrediente(long ingredienteId)
    {
        _ingredientesIds.Remove(ingredienteId);
    }
}
