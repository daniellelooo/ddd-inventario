using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Repositories
{
    public class IngredienteRepository : IIngredienteRepository
    {
        private readonly InventarioDbContext _context;

        public IngredienteRepository(InventarioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            return ingrediente == null ? null : new IngredienteAggregate(ingrediente);
        }

        public async Task<IngredienteAggregate?> ObtenerPorNombreAsync(string nombre)
        {
            var ingrediente = await _context.Ingredientes.FirstOrDefaultAsync(i => i.Nombre == nombre);
            return ingrediente == null ? null : new IngredienteAggregate(ingrediente);
        }

        public async Task<List<IngredienteAggregate>> ObtenerTodosAsync()
        {
            var ingredientes = await _context.Ingredientes.ToListAsync();
            return ingredientes.Select(i => new IngredienteAggregate(i)).ToList();
        }

        public async Task<List<IngredienteAggregate>> ObtenerPorCategoriaAsync(Guid categoriaId)
        {
            var ingredientes = await _context.Ingredientes.Where(i => i.CategoriaId == categoriaId).ToListAsync();
            return ingredientes.Select(i => new IngredienteAggregate(i)).ToList();
        }

        public async Task<List<IngredienteAggregate>> ObtenerConStockBajoAsync()
        {
            // Implementación básica
            return new List<IngredienteAggregate>();
        }

        public async Task<List<IngredienteAggregate>> ObtenerQueRequierenReabastecimientoAsync()
        {
            // Implementación básica
            return new List<IngredienteAggregate>();
        }

        public async Task<List<IngredienteAggregate>> ObtenerActivosAsync()
        {
            var ingredientes = await _context.Ingredientes.Where(i => i.Activo).ToListAsync();
            return ingredientes.Select(i => new IngredienteAggregate(i)).ToList();
        }

        public async Task GuardarAsync(IngredienteAggregate ingredienteAggregate)
        {
            var existente = await _context.Ingredientes.FindAsync(ingredienteAggregate.Id);
            if (existente == null)
            {
                _context.Ingredientes.Add(ingredienteAggregate.Ingrediente);
            }
            else
            {
                _context.Ingredientes.Update(ingredienteAggregate.Ingrediente);
            }
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Guid id)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente != null)
            {
                _context.Ingredientes.Remove(ingrediente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.Ingredientes.AnyAsync(i => i.Id == id);
        }

        public async Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null)
        {
            var query = _context.Ingredientes.Where(i => i.Nombre == nombre);
            if (excluirId.HasValue)
                query = query.Where(i => i.Id != excluirId.Value);
            return await query.AnyAsync();
        }
    }
}