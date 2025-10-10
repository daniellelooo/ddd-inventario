using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Repositories
{
    public class LoteRepository : ILoteRepository
    {
        private readonly InventarioDbContext _context;

        public LoteRepository(InventarioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Lote?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.Lotes.FindAsync(id);
        }

        public async Task<Lote?> ObtenerPorCodigoAsync(string codigo)
        {
            return await _context.Lotes.FirstOrDefaultAsync(l => l.Codigo == codigo);
        }

        public async Task<List<Lote>> ObtenerPorIngredienteAsync(Guid ingredienteId)
        {
            return await _context.Lotes.Where(l => l.IngredienteId == ingredienteId).ToListAsync();
        }

        public async Task<List<Lote>> ObtenerVencidosAsync()
        {
            return await _context.Lotes.Where(l => l.Vencido).ToListAsync();
        }

        public async Task<List<Lote>> ObtenerProximosAVencerAsync(int diasAnticipacion = 7)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(diasAnticipacion);
            return await _context.Lotes
                .Where(l => !l.Vencido && l.FechaVencimiento.Valor <= fechaLimite)
                .OrderBy(l => l.FechaVencimiento.Valor)
                .ToListAsync();
        }

        public async Task<List<Lote>> ObtenerDisponiblesAsync(Guid ingredienteId)
        {
            return await _context.Lotes
                .Where(l => l.IngredienteId == ingredienteId && l.CantidadDisponible > 0 && !l.Vencido)
                .ToListAsync();
        }

        public async Task<List<Lote>> ObtenerPorOrdenCompraAsync(Guid ordenCompraId)
        {
            return await _context.Lotes.Where(l => l.OrdenDeCompraId == ordenCompraId).ToListAsync();
        }

        public async Task GuardarAsync(Lote lote)
        {
            var existente = await _context.Lotes.FindAsync(lote.Id);
            if (existente == null)
            {
                _context.Lotes.Add(lote);
            }
            else
            {
                _context.Entry(existente).CurrentValues.SetValues(lote);
            }
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Guid id)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote != null)
            {
                _context.Lotes.Remove(lote);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.Lotes.AnyAsync(l => l.Id == id);
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, Guid? excluirId = null)
        {
            var query = _context.Lotes.Where(l => l.Codigo == codigo);
            if (excluirId.HasValue)
                query = query.Where(l => l.Id != excluirId.Value);
            return await query.AnyAsync();
        }
    }
}
