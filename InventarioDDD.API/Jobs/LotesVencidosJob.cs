using Quartz;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.API.Jobs;

/// <summary>
/// Job que verifica cada hora si hay lotes vencidos
/// </summary>
public class LotesVencidosJob : IJob
{
    private readonly IIngredienteRepository _repository;
    private readonly ILogger<LotesVencidosJob> _logger;

    public LotesVencidosJob(
        IIngredienteRepository repository,
        ILogger<LotesVencidosJob> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Ejecutando LotesVencidosJob...");

        try
        {
            var ingredientes = await _repository.ObtenerTodosAsync();
            var totalLotesVencidos = 0;
            var totalCantidadPerdida = 0.0;

            foreach (var ingrediente in ingredientes)
            {
                // Marcar lotes vencidos (el método del agregado publica eventos)
                ingrediente.MarcarLotesVencidos();

                var lotesVencidos = ingrediente.ObtenerLotesDisponibles()
                    .Where(l => l.EstaVencido())
                    .ToList();

                if (lotesVencidos.Any())
                {
                    totalLotesVencidos += lotesVencidos.Count;
                    totalCantidadPerdida += lotesVencidos.Sum(l => l.Cantidad.Valor);

                    _logger.LogWarning(
                        "⚠️ Ingrediente {Nombre}: {Cantidad} lotes vencidos",
                        ingrediente.Nombre,
                        lotesVencidos.Count);

                    foreach (var lote in lotesVencidos)
                    {
                        _logger.LogWarning(
                            "   → Lote {LoteId}: Vencido el {FechaVencimiento}, Cantidad: {Cantidad}",
                            lote.Id,
                            lote.FechaVencimiento.Fecha,
                            lote.Cantidad.Valor);
                    }
                }

                // Los cambios se persisten en memoria automáticamente
                // No es necesario llamar a ActualizarAsync para InMemory repository
            }

            if (totalLotesVencidos > 0)
            {
                _logger.LogWarning(
                    "⚠️ RESUMEN: Total de {Lotes} lotes vencidos, Cantidad total perdida: {Cantidad}",
                    totalLotesVencidos,
                    totalCantidadPerdida);
            }
            else
            {
                _logger.LogInformation("✓ No se encontraron lotes vencidos");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar LotesVencidosJob");
        }
    }
}
