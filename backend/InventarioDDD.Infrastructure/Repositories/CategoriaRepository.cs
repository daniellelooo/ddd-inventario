using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly InventarioDbContext _context;

        public CategoriaRepository(InventarioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Categoria?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task<Categoria?> ObtenerPorNombreAsync(string nombre)
        {
            return await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == nombre);
        }

        public async Task<List<Categoria>> ObtenerTodosAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<List<Categoria>> ObtenerActivasAsync()
        {
            // Para simplificar, asumimos que todas están activas
            return await _context.Categorias.ToListAsync();
        }

        public async Task GuardarAsync(Categoria categoria)
        {
            var existente = await _context.Categorias.FindAsync(categoria.Id);
            if (existente == null)
            {
                _context.Categorias.Add(categoria);
            }
            else
            {
                _context.Categorias.Update(categoria);
            }
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Guid id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null)
        {
            var query = _context.Categorias.Where(c => c.Nombre == nombre);
            if (excluirId.HasValue)
                query = query.Where(c => c.Id != excluirId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> TieneIngredientesAsync(Guid categoriaId)
        {
            return await _context.Ingredientes.AnyAsync(i => i.CategoriaId == categoriaId);
        }
    }
}