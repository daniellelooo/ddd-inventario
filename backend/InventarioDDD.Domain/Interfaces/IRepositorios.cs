using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Aggregates;

namespace InventarioDDD.Domain.Interfaces
{
    /// <summary>
    /// Repositorio para el agregado de Ingrediente
    /// </summary>
    public interface IIngredienteRepository
    {
        Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id);
        Task<IngredienteAggregate?> ObtenerPorNombreAsync(string nombre);
        Task<List<IngredienteAggregate>> ObtenerTodosAsync();
        Task<List<IngredienteAggregate>> ObtenerPorCategoriaAsync(Guid categoriaId);
        Task<List<IngredienteAggregate>> ObtenerConStockBajoAsync();
        Task<List<IngredienteAggregate>> ObtenerQueRequierenReabastecimientoAsync();
        Task<List<IngredienteAggregate>> ObtenerActivosAsync();
        Task GuardarAsync(IngredienteAggregate ingredienteAggregate);
        Task EliminarAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null);
    }

    /// <summary>
    /// Repositorio para el agregado de OrdenDeCompra
    /// </summary>
    public interface IOrdenDeCompraRepository
    {
        Task<OrdenDeCompraAggregate?> ObtenerPorIdAsync(Guid id);
        Task<OrdenDeCompraAggregate?> ObtenerPorNumeroAsync(string numero);
        Task<List<OrdenDeCompraAggregate>> ObtenerTodosAsync();
        Task<List<OrdenDeCompraAggregate>> ObtenerPorProveedorAsync(Guid proveedorId);
        Task<List<OrdenDeCompraAggregate>> ObtenerPorEstadoAsync(Enums.EstadoOrdenCompra estado);
        Task<List<OrdenDeCompraAggregate>> ObtenerPendientesAsync();
        Task<List<OrdenDeCompraAggregate>> ObtenerVencidasAsync();
        Task<List<OrdenDeCompraAggregate>> ObtenerEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task GuardarAsync(OrdenDeCompraAggregate ordenAggregate);
        Task EliminarAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteNumeroAsync(string numero, Guid? excluirId = null);
        Task<string> GenerarNumeroOrdenAsync();
    }

    /// <summary>
    /// Repositorio para el agregado de Proveedor
    /// </summary>
    public interface IProveedorRepository
    {
        Task<ProveedorAggregate?> ObtenerPorIdAsync(Guid id);
        Task<ProveedorAggregate?> ObtenerPorNITAsync(string nit);
        Task<List<ProveedorAggregate>> ObtenerTodosAsync();
        Task<List<ProveedorAggregate>> ObtenerActivosAsync();
        Task<List<ProveedorAggregate>> ObtenerQueSuministranAsync(Guid ingredienteId);
        Task<List<ProveedorAggregate>> ObtenerPorPaisAsync(string pais);
        Task GuardarAsync(ProveedorAggregate proveedorAggregate);
        Task EliminarAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteNITAsync(string nit, Guid? excluirId = null);
        Task<bool> ExisteEmailAsync(string email, Guid? excluirId = null);
    }

    /// <summary>
    /// Repositorio para el agregado de MovimientoInventario
    /// </summary>
    public interface IMovimientoInventarioRepository
    {
        Task<MovimientoInventarioAggregate> ObtenerPorIngredienteAsync(Guid ingredienteId);
        Task<List<MovimientoInventario>> ObtenerHistorialAsync(Guid ingredienteId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<List<MovimientoInventario>> ObtenerPorTipoAsync(Enums.TipoMovimiento tipo, DateTime fechaDesde, DateTime fechaHasta);
        Task<List<MovimientoInventario>> ObtenerPorUsuarioAsync(Guid usuarioId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<List<MovimientoInventario>> ObtenerPorLoteAsync(Guid loteId);
        Task<List<MovimientoInventario>> ObtenerEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<List<MovimientoInventario>> ObtenerPorIngredienteYTipoAsync(Guid ingredienteId, Enums.TipoMovimiento tipo, DateTime fechaDesde, DateTime fechaHasta);
        Task<MovimientoInventario?> ObtenerUltimoMovimientoAsync(Guid ingredienteId);
        Task GuardarMovimientoAsync(MovimientoInventario movimiento);
        Task GuardarAgregadoAsync(MovimientoInventarioAggregate movimientoAggregate);
        Task<bool> ExisteAsync(Guid id);
    }

    /// <summary>
    /// Repositorio para la entidad Categoria
    /// </summary>
    public interface ICategoriaRepository
    {
        Task<Categoria?> ObtenerPorIdAsync(Guid id);
        Task<Categoria?> ObtenerPorNombreAsync(string nombre);
        Task<List<Categoria>> ObtenerTodosAsync();
        Task<List<Categoria>> ObtenerActivasAsync();
        Task GuardarAsync(Categoria categoria);
        Task EliminarAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null);
        Task<bool> TieneIngredientesAsync(Guid categoriaId);
    }

    /// <summary>
    /// Repositorio para la entidad Lote
    /// </summary>
    public interface ILoteRepository
    {
        Task<Lote?> ObtenerPorIdAsync(Guid id);
        Task<Lote?> ObtenerPorCodigoAsync(string codigo);
        Task<List<Lote>> ObtenerTodosAsync();
        Task<List<Lote>> ObtenerPorIngredienteAsync(Guid ingredienteId);
        Task<List<Lote>> ObtenerVencidosAsync();
        Task<List<Lote>> ObtenerProximosAVencerAsync(int diasAnticipacion = 7);
        Task<List<Lote>> ObtenerDisponiblesAsync(Guid ingredienteId);
        Task<List<Lote>> ObtenerPorOrdenCompraAsync(Guid ordenCompraId);
        Task GuardarAsync(Lote lote);
        Task EliminarAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteCodigoAsync(string codigo, Guid? excluirId = null);
    }
}