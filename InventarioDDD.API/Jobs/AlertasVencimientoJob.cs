using Quartz;
using InventarioDDD.Domain.Repositories;

namespace InventarioDDD.API.Jobs;

/// <summary>
/// Job que se ejecuta diariamente a las 8 AM para alertar sobre lotes próximos a vencer
/// </summary>
public class AlertasVencimientoJob : IJob
{
    private readonly IIngredienteRepository _repository;
    private readonly ILogger<AlertasVencimientoJob> _logger;
    private const int DiasAlerta = 7; // Alertar con 7 días de anticipación

    public AlertasVencimientoJob(
        IIngredienteRepository repository,
        ILogger<AlertasVencimientoJob> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Ejecutando AlertasVencimientoJob a las {Hora}...", DateTime.Now);

        try
        {
            var ingredientes = await _repository.ObtenerTodosAsync();
            var totalAlertasGeneradas = 0;

            foreach (var ingrediente in ingredientes)
            {
                var lotesProximosVencer = ingrediente
                    .ObtenerLotesProximosVencer(DiasAlerta)
                    .ToList();

                if (lotesProximosVencer.Any())
                {
                    totalAlertasGeneradas += lotesProximosVencer.Count;

                    _logger.LogWarning(
                        "⚠️ ALERTA VENCIMIENTO: Ingrediente {Nombre} - {Cantidad} lotes próximos a vencer",
                        ingrediente.Nombre,
                        lotesProximosVencer.Count);

                    foreach (var lote in lotesProximosVencer)
                    {
                        var diasRestantes = (lote.FechaVencimiento.Fecha - DateTime.Now).Days;
                        var urgencia = diasRestantes <= 3 ? "🔴 URGENTE" : 
                                     diasRestantes <= 5 ? "🟡 PRIORITARIO" : 
                                     "🟢 ADVERTENCIA";

                        _logger.LogWarning(
                            "   {Urgencia} → Lote {LoteId}: Vence en {Dias} días ({FechaVencimiento}), Cantidad: {Cantidad}",
                            urgencia,
                            lote.Id,
                            diasRestantes,
                            lote.FechaVencimiento.Fecha.ToString("dd/MM/yyyy"),
                            lote.Cantidad.Valor);
                    }

                    // Aquí se pueden implementar acciones adicionales:
                    // - Enviar email diario con resumen
                    // - Crear promociones automáticas
                    // - Ajustar prioridades de producción
                    // - Notificar a responsables de área
                }
            }

            if (totalAlertasGeneradas > 0)
            {
                _logger.LogWarning(
                    "⚠️ RESUMEN DIARIO: {Total} lotes próximos a vencer en los próximos {Dias} días",
                    totalAlertasGeneradas,
                    DiasAlerta);
            }
            else
            {
                _logger.LogInformation(
                    "✓ No hay lotes próximos a vencer en los próximos {Dias} días",
                    DiasAlerta);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar AlertasVencimientoJob");
        }
    }
}
