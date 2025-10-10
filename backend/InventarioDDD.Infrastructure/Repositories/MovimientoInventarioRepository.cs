using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Repositories
{
    public class MovimientoInventarioRepository : IMovimientoInventarioRepository
    {
        private readonly InventarioDbContext _context;

        public MovimientoInventarioRepository(InventarioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MovimientoInventarioAggregate> ObtenerPorIngredienteAsync(Guid ingredienteId)
        {
            var movimientos = await _context.MovimientosInventario
                .Where(m => m.IngredienteId == ingredienteId)
                .ToListAsync();
            return new MovimientoInventarioAggregate(movimientos);
        }

        public async Task<List<MovimientoInventario>> ObtenerHistorialAsync(Guid ingredienteId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.MovimientosInventario.Where(m => m.IngredienteId == ingredienteId);

            if (fechaDesde.HasValue)
                query = query.Where(m => m.FechaMovimiento >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(m => m.FechaMovimiento <= fechaHasta.Value);

            return await query.OrderByDescending(m => m.FechaMovimiento).ToListAsync();
        }

        public async Task<List<MovimientoInventario>> ObtenerPorTipoAsync(Domain.Enums.TipoMovimiento tipo, DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _context.MovimientosInventario
                .Where(m => m.TipoMovimiento == tipo && m.FechaMovimiento >= fechaDesde && m.FechaMovimiento <= fechaHasta)
                .ToListAsync();
        }

        public async Task<List<MovimientoInventario>> ObtenerPorUsuarioAsync(Guid usuarioId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.MovimientosInventario.Where(m => m.UsuarioId == usuarioId);

            if (fechaDesde.HasValue)
                query = query.Where(m => m.FechaMovimiento >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(m => m.FechaMovimiento <= fechaHasta.Value);

            return await query.ToListAsync();
        }

        public async Task<List<MovimientoInventario>> ObtenerPorLoteAsync(Guid loteId)
        {
            return await _context.MovimientosInventario
                .Where(m => m.LoteId == loteId)
                .ToListAsync();
        }

        public async Task<List<MovimientoInventario>> ObtenerEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.MovimientosInventario
                .Where(m => m.FechaMovimiento >= fechaInicio && m.FechaMovimiento <= fechaFin)
                .ToListAsync();
        }

        public async Task<List<MovimientoInventario>> ObtenerPorIngredienteYTipoAsync(Guid ingredienteId, Domain.Enums.TipoMovimiento tipo, DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _context.MovimientosInventario
                .Where(m => m.IngredienteId == ingredienteId && m.TipoMovimiento == tipo && m.FechaMovimiento >= fechaDesde && m.FechaMovimiento <= fechaHasta)
                .ToListAsync();
        }

        public async Task<MovimientoInventario?> ObtenerUltimoMovimientoAsync(Guid ingredienteId)
        {
            return await _context.MovimientosInventario
                .Where(m => m.IngredienteId == ingredienteId)
                .OrderByDescending(m => m.FechaMovimiento)
                .FirstOrDefaultAsync();
        }

        public async Task GuardarMovimientoAsync(MovimientoInventario movimiento)
        {
            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();
        }

        public async Task GuardarAgregadoAsync(MovimientoInventarioAggregate movimientoAggregate)
        {
            foreach (var movimiento in movimientoAggregate.Movimientos)
            {
                _context.MovimientosInventario.Add(movimiento);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.MovimientosInventario.AnyAsync(m => m.Id == id);
        }
    }
}