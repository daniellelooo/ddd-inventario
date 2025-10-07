using MediatR;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Events;

namespace InventarioDDD.Application.UseCases;

// ========================================
// COMANDO
// ========================================

/// <summary>
/// Comando para registrar un nuevo ingrediente
/// </summary>
public record RegistrarIngredienteCommand(
    string Nombre,
    string Categoria,
    List<long> ProveedoresIds,
    string UnidadMedidaNombre,
    string UnidadMedidaSimbolo,
    double StockMinimo,
    double StockOptimo,
    double StockMaximo,
    decimal? PrecioReferencia = null,
    string? Moneda = "BOB"
) : IRequest<long>;

// ========================================
// HANDLER
// ========================================

/// <summary>
/// Handler para registrar un ingrediente
/// </summary>
public class RegistrarIngredienteHandler : IRequestHandler<RegistrarIngredienteCommand, long>
{
    private readonly IIngredienteRepository _repository;
    private readonly IPublisher _publisher;

    public RegistrarIngredienteHandler(IIngredienteRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<long> Handle(RegistrarIngredienteCommand request, CancellationToken cancellationToken)
    {
        // Crear value objects
        var categoria = new Categoria(request.Categoria);
        var unidad = new UnidadDeMedida(request.UnidadMedidaNombre, request.UnidadMedidaSimbolo);
        var rango = new RangoDeStock(request.StockMinimo, request.StockOptimo, request.StockMaximo);

        PrecioConMoneda? precio = null;
        if (request.PrecioReferencia.HasValue && request.PrecioReferencia.Value > 0)
        {
            precio = new PrecioConMoneda(request.PrecioReferencia.Value, request.Moneda ?? "BOB");
        }

        // Crear el agregado
        var ingrediente = new Ingrediente(
            id: 0, // Se asigna al guardar
            nombre: request.Nombre,
            categoria: categoria,
            proveedoresIds: request.ProveedoresIds ?? new List<long>(),
            unidadDeMedida: unidad,
            rangoDeStock: rango,
            precioReferencia: precio,
            activo: true
        );

        // Guardar
        var guardado = await _repository.GuardarAsync(ingrediente);

        // Publicar evento de dominio
        await _publisher.Publish(
            new IngredienteRegistradoEvent(guardado.Id, guardado.Nombre),
            cancellationToken
        );

        return guardado.Id;
    }
}
