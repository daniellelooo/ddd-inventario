using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Infrastructure.Services;

/// <summary>
/// Implementación del Servicio de Rotación de Inventario
/// </summary>
public class ServicioDeRotacion : IServicioDeRotacion
{
    private readonly IIngredienteRepository _ingredienteRepository;

    public ServicioDeRotacion(IIngredienteRepository ingredienteRepository)
    {
        _ingredienteRepository = ingredienteRepository;
    }

    public async Task<double> CalcularIndicadorRotacionAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException($"Ingrediente {ingredienteId} no encontrado");

        // Indicador de rotación = Consumo / Stock Promedio
        var consumoDiario = await CalcularConsumoDiarioPromedioAsync(ingredienteId);
        var stockActual = ingrediente.CalcularStockDisponible().Valor;

        if (stockActual == 0)
            return 0;

        // Rotación en días
        return stockActual / Math.Max(consumoDiario, 0.1);
    }

    public async Task<List<IngredienteLentaRotacion>> ObtenerIngredientesLentaRotacionAsync()
    {
        var ingredientes = await _ingredienteRepository.ObtenerActivosAsync();
        var ingredientesLentaRotacion = new List<IngredienteLentaRotacion>();

        foreach (var ingrediente in ingredientes)
        {
            var diasRotacion = await CalcularIndicadorRotacionAsync(ingrediente.Id);

            // Considerar lenta rotación si supera 60 días
            if (diasRotacion > 60)
            {
                ingredientesLentaRotacion.Add(new IngredienteLentaRotacion(
                    ingrediente.Id,
                    ingrediente.Nombre,
                    diasRotacion,
                    ingrediente.CalcularStockDisponible().Valor));
            }
        }

        return ingredientesLentaRotacion.OrderByDescending(i => i.DiasPromedio).ToList();
    }

    public async Task<double> ProyectarDemandaAsync(long ingredienteId, int diasProyeccion)
    {
        var consumoDiario = await CalcularConsumoDiarioPromedioAsync(ingredienteId);
        return consumoDiario * diasProyeccion;
    }

    public async Task<double> CalcularConsumoDiarioPromedioAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            return 0;

        // Simplificado: calcular basado en la diferencia entre stock óptimo y actual
        var diferenciaStock = ingrediente.RangoDeStock.Optimo - ingrediente.CalcularStockDisponible().Valor;

        // Asumir que esta diferencia se consumió en los últimos 30 días
        return Math.Max(diferenciaStock / 30.0, 0);
    }

    public async Task<List<Ingrediente>> ObtenerIngredientesAltaRotacionAsync()
    {
        var ingredientes = await _ingredienteRepository.ObtenerActivosAsync();
        var ingredientesAltaRotacion = new List<Ingrediente>();

        foreach (var ingrediente in ingredientes)
        {
            var diasRotacion = await CalcularIndicadorRotacionAsync(ingrediente.Id);

            // Considerar alta rotación si es menor a 15 días
            if (diasRotacion > 0 && diasRotacion < 15)
            {
                ingredientesAltaRotacion.Add(ingrediente);
            }
        }

        return ingredientesAltaRotacion;
    }

    public async Task<double> CalcularTiempoPromedioEnInventarioAsync(long ingredienteId)
    {
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);

        if (ingrediente == null)
            return 0;

        var lotes = ingrediente.Lotes.Where(l => !l.Agotado).ToList();

        if (!lotes.Any())
            return 0;

        // Calcular promedio de días desde recepción
        var diasPromedio = lotes.Average(l => (DateTime.Now - l.FechaRecepcion).TotalDays);

        return diasPromedio;
    }
}
