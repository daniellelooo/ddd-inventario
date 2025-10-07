using Quartz;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.API.Jobs;

/// <summary>
/// Job que verifica cada 5 minutos si hay ingredientes con stock bajo
/// </summary>
public class AlertasStockBajoJob : IJob
{
    private readonly IIngredienteRepository _repository;
    private readonly ILogger<AlertasStockBajoJob> _logger;

    public AlertasStockBajoJob(
        IIngredienteRepository repository,
        ILogger<AlertasStockBajoJob> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Ejecutando AlertasStockBajoJob...");

        try
        {
            var ingredientes = await _repository.ObtenerTodosAsync();
            var ingredientesStockBajo = ingredientes
                .Where(i => i.EstaEnStockBajo())
                .ToList();

            if (ingredientesStockBajo.Any())
            {
                _logger.LogWarning(
                    "⚠️ ALERTA: {Cantidad} ingredientes con stock bajo",
                    ingredientesStockBajo.Count);

                foreach (var ingrediente in ingredientesStockBajo)
                {
                    var stockActual = ingrediente.CalcularStockDisponible();
                    _logger.LogWarning(
                        "   → {Nombre}: Stock actual {Stock}, Mínimo {Minimo}",
                        ingrediente.Nombre,
                        stockActual.Valor,
                        ingrediente.RangoDeStock.Minimo);

                    // Aquí se pueden implementar acciones adicionales como:
                    // - Enviar notificaciones push
                    // - Enviar emails
                    // - Crear tickets automáticos
                    // - Generar órdenes de compra automáticas
                }
            }
            else
            {
                _logger.LogInformation("✓ Todos los ingredientes tienen stock suficiente");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar AlertasStockBajoJob");
        }
    }
}
