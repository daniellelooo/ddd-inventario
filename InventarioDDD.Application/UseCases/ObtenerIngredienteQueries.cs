using MediatR;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.Application.UseCases;

// ========================================
// QUERY
// ========================================

/// <summary>
/// Query para obtener un ingrediente por ID
/// </summary>
public record ObtenerIngredientePorIdQuery(long Id) : IRequest<IngredienteDto?>;

/// <summary>
/// Query para obtener todos los ingredientes
/// </summary>
public record ObtenerTodosIngredientesQuery() : IRequest<List<IngredienteDto>>;

// ========================================
// DTO
// ========================================

/// <summary>
/// DTO para transferencia de datos de Ingrediente
/// </summary>
public record IngredienteDto(
    long Id,
    string Nombre,
    string Categoria,
    double StockDisponible,
    string UnidadDeMedida,
    double StockMinimo,
    double StockOptimo,
    double StockMaximo,
    decimal? PrecioReferencia,
    bool Activo,
    bool StockBajo,
    int CantidadLotes
);

// ========================================
// HANDLERS
// ========================================

/// <summary>
/// Handler para obtener ingrediente por ID
/// </summary>
public class ObtenerIngredientePorIdHandler : IRequestHandler<ObtenerIngredientePorIdQuery, IngredienteDto?>
{
    private readonly IIngredienteRepository _repository;

    public ObtenerIngredientePorIdHandler(IIngredienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IngredienteDto?> Handle(ObtenerIngredientePorIdQuery request, CancellationToken cancellationToken)
    {
        var ingrediente = await _repository.ObtenerPorIdAsync(request.Id);

        return ingrediente == null ? null : MapearADto(ingrediente);
    }

    private static IngredienteDto MapearADto(Ingrediente ingrediente)
    {
        return new IngredienteDto(
            ingrediente.Id,
            ingrediente.Nombre,
            ingrediente.Categoria.Nombre,
            ingrediente.CalcularStockDisponible().Valor,
            ingrediente.UnidadDeMedida.Simbolo,
            ingrediente.RangoDeStock.Minimo,
            ingrediente.RangoDeStock.Optimo,
            ingrediente.RangoDeStock.Maximo,
            ingrediente.PrecioReferencia?.Monto,
            ingrediente.Activo,
            ingrediente.EstaEnStockBajo(),
            ingrediente.Lotes.Count
        );
    }
}

/// <summary>
/// Handler para obtener todos los ingredientes
/// </summary>
public class ObtenerTodosIngredientesHandler : IRequestHandler<ObtenerTodosIngredientesQuery, List<IngredienteDto>>
{
    private readonly IIngredienteRepository _repository;

    public ObtenerTodosIngredientesHandler(IIngredienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IngredienteDto>> Handle(ObtenerTodosIngredientesQuery request, CancellationToken cancellationToken)
    {
        var ingredientes = await _repository.ObtenerTodosAsync();

        return ingredientes.Select(MapearADto).ToList();
    }

    private static IngredienteDto MapearADto(Ingrediente ingrediente)
    {
        return new IngredienteDto(
            ingrediente.Id,
            ingrediente.Nombre,
            ingrediente.Categoria.Nombre,
            ingrediente.CalcularStockDisponible().Valor,
            ingrediente.UnidadDeMedida.Simbolo,
            ingrediente.RangoDeStock.Minimo,
            ingrediente.RangoDeStock.Optimo,
            ingrediente.RangoDeStock.Maximo,
            ingrediente.PrecioReferencia?.Monto,
            ingrediente.Activo,
            ingrediente.EstaEnStockBajo(),
            ingrediente.Lotes.Count
        );
    }
}
