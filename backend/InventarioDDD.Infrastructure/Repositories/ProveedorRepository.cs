using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly InventarioDbContext _context;

        public ProveedorRepository(InventarioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProveedorAggregate?> ObtenerPorIdAsync(Guid id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            return proveedor == null ? null : new ProveedorAggregate(proveedor);
        }

        public async Task<ProveedorAggregate?> ObtenerPorNITAsync(string nit)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.NIT == nit);
            return proveedor == null ? null : new ProveedorAggregate(proveedor);
        }

        public async Task<List<ProveedorAggregate>> ObtenerTodosAsync()
        {
            var proveedores = await _context.Proveedores.ToListAsync();
            return proveedores.Select(p => new ProveedorAggregate(p)).ToList();
        }

        public async Task<List<ProveedorAggregate>> ObtenerActivosAsync()
        {
            var proveedores = await _context.Proveedores
                .Where(p => p.Activo)
                .ToListAsync();
            return proveedores.Select(p => new ProveedorAggregate(p)).ToList();
        }

        public async Task<List<ProveedorAggregate>> ObtenerQueSuministranAsync(Guid ingredienteId)
        {
            var proveedores = await _context.Proveedores
                .Where(p => p.IngredientesSuministrados.Contains(ingredienteId))
                .ToListAsync();
            return proveedores.Select(p => new ProveedorAggregate(p)).ToList();
        }

        public async Task<List<ProveedorAggregate>> ObtenerPorPaisAsync(string pais)
        {
            var proveedores = await _context.Proveedores
                .Where(p => p.Direccion.Pais == pais)
                .ToListAsync();
            return proveedores.Select(p => new ProveedorAggregate(p)).ToList();
        }

        public async Task GuardarAsync(ProveedorAggregate proveedorAggregate)
        {
            var existente = await _context.Proveedores.FindAsync(proveedorAggregate.Id);
            
            if (existente == null)
            {
                _context.Proveedores.Add(proveedorAggregate.Proveedor);
            }
            else
            {
                _context.Entry(existente).CurrentValues.SetValues(proveedorAggregate.Proveedor);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Guid id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            
            if (proveedor != null)
            {
                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.Proveedores.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExisteNITAsync(string nit, Guid? excluirId = null)
        {
            var query = _context.Proveedores.Where(p => p.NIT == nit);
            
            if (excluirId.HasValue)
            {
                query = query.Where(p => p.Id != excluirId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<bool> ExisteEmailAsync(string email, Guid? excluirId = null)
        {
            var query = _context.Proveedores.Where(p => p.Email == email);
            
            if (excluirId.HasValue)
            {
                query = query.Where(p => p.Id != excluirId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
}
