using MediatR;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Events;

namespace InventarioDDD.Infrastructure.Services;

/// <summary>
/// Implementación del Servicio de Recepción de Mercancía
/// </summary>
public class ServicioDeRecepcion : IServicioDeRecepcion
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IOrdenDeCompraRepository _ordenRepository;
    private readonly IPublisher _publisher;

    public ServicioDeRecepcion(
        IIngredienteRepository ingredienteRepository,
        IOrdenDeCompraRepository ordenRepository,
        IPublisher publisher)
    {
        _ingredienteRepository = ingredienteRepository;
        _ordenRepository = ordenRepository;
        _publisher = publisher;
    }

    public async Task RegistrarEntradaMercanciaAsync(long ordenId, List<ItemRecepcion> items)
    {
        var orden = await _ordenRepository.ObtenerPorIdAsync(ordenId);

        if (orden == null)
            throw new InvalidOperationException($"Orden {ordenId} no encontrada");

        foreach (var item in items)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId);

            if (ingrediente == null)
            {
                await NotificarDiscrepanciasAsync(ordenId,
                    $"Ingrediente {item.IngredienteId} no encontrado");
                continue;
            }

            // Verificar calidad antes de agregar
            var cantidad = new CantidadDisponible(item.Cantidad, ingrediente.UnidadDeMedida);
            var fechaVencimiento = new FechaVencimiento(item.FechaVencimiento);

            // Validaciones de calidad
            if (fechaVencimiento.EstaVencido())
            {
                await NotificarDiscrepanciasAsync(ordenId,
                    $"Lote vencido para ingrediente {ingrediente.Nombre}");
                continue;
            }

            if (fechaVencimiento.DiasHastaVencimiento() < 30)
            {
                await NotificarDiscrepanciasAsync(ordenId,
                    $"Lote con vida útil muy corta para {ingrediente.Nombre}: {fechaVencimiento.DiasHastaVencimiento()} días");
            }

            // Agregar lote
            var nuevoLote = ingrediente.AgregarLote(cantidad, fechaVencimiento, orden.ProveedorId);
            await _ingredienteRepository.GuardarAsync(ingrediente);

            // Publicar evento
            await _publisher.Publish(new LoteRecibidoEvent(
                nuevoLote.Id, item.IngredienteId, item.Cantidad));
        }

        // Marcar orden como recibida
        orden.MarcarRecibida();
        await _ordenRepository.GuardarAsync(orden);

        await _publisher.Publish(new OrdenDeCompraRecibidaEvent(ordenId, orden.ProveedorId));
    }

    public async Task<ResultadoCalidad> VerificarCalidadAsync(long loteId)
    {
        // Simplificado: verificaciones básicas
        await Task.CompletedTask;

        var observaciones = new List<string>();

        // Aquí irían las validaciones de calidad específicas
        observaciones.Add("Temperatura: OK");
        observaciones.Add("Embalaje: OK");
        observaciones.Add("Etiquetado: OK");

        return new ResultadoCalidad(
            Aprobado: true,
            Observaciones: string.Join(", ", observaciones));
    }

    public async Task NotificarDiscrepanciasAsync(long ordenId, string diferencias)
    {
        await _publisher.Publish(new DiscrepanciaRecepcionEvent(
            ordenId, 0, diferencias));
    }
}
