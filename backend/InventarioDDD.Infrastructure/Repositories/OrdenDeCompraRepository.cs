using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Enums;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Repositories
{
    public class OrdenDeCompraRepository : IOrdenDeCompraRepository
    {
        private readonly InventarioDbContext _context;

        public OrdenDeCompraRepository(InventarioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<OrdenDeCompraAggregate?> ObtenerPorIdAsync(Guid id)
        {
            var orden = await _context.OrdenesDeCompra.FindAsync(id);
            return orden == null ? null : new OrdenDeCompraAggregate(orden);
        }

        public async Task<OrdenDeCompraAggregate?> ObtenerPorNumeroAsync(string numero)
        {
            var orden = await _context.OrdenesDeCompra.FirstOrDefaultAsync(o => o.Numero == numero);
            return orden == null ? null : new OrdenDeCompraAggregate(orden);
        }

        public async Task<List<OrdenDeCompraAggregate>> ObtenerTodosAsync()
        {
            var ordenes = await _context.OrdenesDeCompra.ToListAsync();
            return ordenes.Select(o => new OrdenDeCompraAggregate(o)).ToList();
        }

        public async Task<List<OrdenDeCompraAggregate>> ObtenerPorProveedorAsync(Guid proveedorId)
        {
            var ordenes = await _context.OrdenesDeCompra.Where(o => o.ProveedorId == proveedorId).ToListAsync();
            return ordenes.Select(o => new OrdenDeCompraAggregate(o)).ToList();
        }

        public async Task<List<OrdenDeCompraAggregate>> ObtenerPorEstadoAsync(EstadoOrdenCompra estado)
        {
            // Conversión de EstadoOrdenCompra a EstadoOrden
            var estadoOrden = (EstadoOrden)((int)estado);
            var ordenes = await _context.OrdenesDeCompra.Where(o => o.Estado == estadoOrden).ToListAsync();
            return ordenes.Select(o => new OrdenDeCompraAggregate(o)).ToList();
        }

        public async Task<List<OrdenDeCompraAggregate>> ObtenerPendientesAsync()
        {
            var ordenes = await _context.OrdenesDeCompra
                .Where(o => o.Estado == EstadoOrden.Pendiente || o.Estado == EstadoOrden.Aprobada)
                .ToListAsync();
            return ordenes.Select(o => new OrdenDeCompraAggregate(o)).ToList();
        }

        public async Task<List<OrdenDeCompraAggregate>> ObtenerVencidasAsync()
        {
            var hoy = DateTime.UtcNow;
            var ordenes = await _context.OrdenesDeCompra
                .Where(o => o.FechaEsperada < hoy && o.Estado != EstadoOrden.Recibida && o.Estado != EstadoOrden.Cancelada)
                .ToListAsync();
            return ordenes.Select(o => new OrdenDeCompraAggregate(o)).ToList();
        }

        public async Task<List<OrdenDeCompraAggregate>> ObtenerEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ordenes = await _context.OrdenesDeCompra
                .Where(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin)
                .ToListAsync();
            return ordenes.Select(o => new OrdenDeCompraAggregate(o)).ToList();
        }

        public async Task GuardarAsync(OrdenDeCompraAggregate ordenAggregate)
        {
            var existente = await _context.OrdenesDeCompra.FindAsync(ordenAggregate.Id);
            if (existente == null)
            {
                _context.OrdenesDeCompra.Add(ordenAggregate.OrdenDeCompra);
            }
            else
            {
                _context.Entry(existente).CurrentValues.SetValues(ordenAggregate.OrdenDeCompra);
            }
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Guid id)
        {
            var orden = await _context.OrdenesDeCompra.FindAsync(id);
            if (orden != null)
            {
                _context.OrdenesDeCompra.Remove(orden);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _context.OrdenesDeCompra.AnyAsync(o => o.Id == id);
        }

        public async Task<bool> ExisteNumeroAsync(string numero, Guid? excluirId = null)
        {
            var query = _context.OrdenesDeCompra.Where(o => o.Numero == numero);
            if (excluirId.HasValue)
                query = query.Where(o => o.Id != excluirId.Value);
            return await query.AnyAsync();
        }

        public async Task<string> GenerarNumeroOrdenAsync()
        {
            var ultimaOrden = await _context.OrdenesDeCompra
                .OrderByDescending(o => o.FechaCreacion)
                .FirstOrDefaultAsync();

            var contador = 1;
            if (ultimaOrden != null && ultimaOrden.Numero.StartsWith("OC"))
            {
                var numero = ultimaOrden.Numero.Substring(2);
                if (int.TryParse(numero, out var ultimoNumero))
                {
                    contador = ultimoNumero + 1;
                }
            }

            return $"OC{contador:D6}";
        }
    }
}
