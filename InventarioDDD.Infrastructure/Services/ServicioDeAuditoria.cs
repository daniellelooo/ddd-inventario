using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Infrastructure.Services;

/// <summary>
/// Implementación del Servicio de Auditoría
/// </summary>
public class ServicioDeAuditoria : IServicioDeAuditoria
{
    private readonly IIngredienteRepository _ingredienteRepository;

    public ServicioDeAuditoria(IIngredienteRepository ingredienteRepository)
    {
        _ingredienteRepository = ingredienteRepository;
    }

    public async Task<ReporteConsumo> GenerarReporteConsumoAsync(DateTime desde, DateTime hasta)
    {
        // Simplificado: en producción se consultarían eventos de consumo
        var consumos = new Dictionary<long, double>();

        var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();

        foreach (var ingrediente in ingredientes)
        {
            // Calcular consumo aproximado
            var consumoEstimado = ingrediente.RangoDeStock.Optimo - ingrediente.CalcularStockDisponible().Valor;
            if (consumoEstimado > 0)
            {
                consumos[ingrediente.Id] = consumoEstimado;
            }
        }

        return new ReporteConsumo(desde, hasta, consumos);
    }

    public async Task<ReporteMermas> GenerarReporteMermasAsync(DateTime desde, DateTime hasta)
    {
        var mermas = new Dictionary<long, double>();

        var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();

        foreach (var ingrediente in ingredientes)
        {
            // Calcular mermas por lotes vencidos/agotados
            var lotesVencidos = ingrediente.Lotes
                .Where(l => l.Agotado || l.EstaVencido())
                .Sum(l => l.Cantidad.Valor);

            if (lotesVencidos > 0)
            {
                mermas[ingrediente.Id] = lotesVencidos;
            }
        }

        return new ReporteMermas(desde, hasta, mermas);
    }

    public async Task<List<Discrepancia>> CompararInventarioFisicoAsync(Dictionary<long, double> inventarioFisico)
    {
        var discrepancias = new List<Discrepancia>();

        foreach (var kvp in inventarioFisico)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(kvp.Key);

            if (ingrediente == null)
                continue;

            var stockSistema = ingrediente.CalcularStockDisponible().Valor;
            var stockFisico = kvp.Value;
            var diferencia = stockFisico - stockSistema;

            if (Math.Abs(diferencia) > 0.01) // Tolerancia mínima
            {
                discrepancias.Add(new Discrepancia(
                    ingrediente.Id,
                    ingrediente.Nombre,
                    stockSistema,
                    stockFisico,
                    diferencia));
            }
        }

        return discrepancias;
    }
}
