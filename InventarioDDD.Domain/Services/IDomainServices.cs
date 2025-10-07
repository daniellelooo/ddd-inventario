using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Services;

/// <summary>
/// Servicio de dominio para gestionar consultas del inventario
/// </summary>
public interface IServicioDeInventario
{
    Task<CantidadDisponible> ConsultarStockDisponibleAsync(long ingredienteId);
    Task<bool> VerificarDisponibilidadParaPedidoAsync(long pedidoId);
    Task<List<Ingrediente>> ObtenerIngredientesPorCategoriaAsync(string categoria);
    Task<decimal> CalcularValoracionInventarioAsync();
    Task<List<Lote>> ObtenerLotesPorIngredienteAsync(long ingredienteId);
}

/// <summary>
/// Servicio de dominio para gestionar el consumo de ingredientes
/// </summary>
public interface IServicioDeConsumo
{
    Task DescontarIngredientesAsync(long pedidoId, List<long> ingredientesIds);
    Task AplicarFIFOAsync(long ingredienteId, double cantidadRequerida);
    Task<bool> ValidarStockSuficienteAsync(List<long> ingredientesIds);
}

/// <summary>
/// Servicio de dominio para gestionar el reabastecimiento
/// </summary>
public interface IServicioDeReabastecimiento
{
    Task GenerarOrdenDeCompraAutomaticaAsync();
    Task<double> CalcularPuntoDeReordenAsync(long ingredienteId);
    Task<long?> SugerirProveedorAsync(long ingredienteId);
}

/// <summary>
/// Servicio de dominio para gestionar la recepción de mercancía
/// </summary>
public interface IServicioDeRecepcion
{
    Task RegistrarEntradaMercanciaAsync(long ordenId, List<ItemRecepcion> items);
    Task<ResultadoCalidad> VerificarCalidadAsync(long loteId);
    Task NotificarDiscrepanciasAsync(long ordenId, string diferencias);
}

/// <summary>
/// Servicio de dominio para auditoría
/// </summary>
public interface IServicioDeAuditoria
{
    Task<ReporteConsumo> GenerarReporteConsumoAsync(DateTime desde, DateTime hasta);
    Task<ReporteMermas> GenerarReporteMermasAsync(DateTime desde, DateTime hasta);
    Task<List<Discrepancia>> CompararInventarioFisicoAsync(Dictionary<long, double> inventarioFisico);
}

/// <summary>
/// Servicio de dominio para análisis de rotación
/// </summary>
public interface IServicioDeRotacion
{
    Task<double> CalcularIndicadorRotacionAsync(long ingredienteId);
    Task<List<IngredienteLentaRotacion>> ObtenerIngredientesLentaRotacionAsync();
    Task<double> ProyectarDemandaAsync(long ingredienteId, int diasProyeccion);
    Task<double> CalcularConsumoDiarioPromedioAsync(long ingredienteId);
    Task<List<Ingrediente>> ObtenerIngredientesAltaRotacionAsync();
    Task<double> CalcularTiempoPromedioEnInventarioAsync(long ingredienteId);
}

// DTOs para servicios
public record ItemRecepcion(long IngredienteId, double Cantidad, DateTime FechaVencimiento);
public record ResultadoCalidad(bool Aprobado, string Observaciones);
public record ReporteConsumo(DateTime Desde, DateTime Hasta, Dictionary<long, double> ConsumosPorIngrediente);
public record ReporteMermas(DateTime Desde, DateTime Hasta, Dictionary<long, double> MermasPorIngrediente);
public record Discrepancia(long IngredienteId, string Nombre, double StockSistema, double StockFisico, double Diferencia);
public record IngredienteLentaRotacion(long Id, string Nombre, double DiasPromedio, double StockActual);
