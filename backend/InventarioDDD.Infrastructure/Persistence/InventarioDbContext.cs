using Microsoft.EntityFrameworkCore;
using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Infrastructure.Configuration.EntityConfigurations;

namespace InventarioDDD.Infrastructure.Persistence
{
    /// <summary>
    /// Manejan la Sesión con la base de datos y el mapeo de 
    /// tablas a través de delegados DbSet.
    /// </summary>
    public class InventarioDbContext : DbContext
    {
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
        {
        }

        // DbSets para entidades principales
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Ingrediente> Ingredientes { get; set; } = null!;
        public DbSet<Lote> Lotes { get; set; } = null!;
        public DbSet<Proveedor> Proveedores { get; set; } = null!;
        public DbSet<OrdenDeCompra> OrdenesDeCompra { get; set; } = null!;
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones usando las clases de configuración
            modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
            modelBuilder.ApplyConfiguration(new IngredienteConfiguration());
            modelBuilder.ApplyConfiguration(new LoteConfiguration());
            modelBuilder.ApplyConfiguration(new ProveedorConfiguration());
            modelBuilder.ApplyConfiguration(new OrdenDeCompraConfiguration());
            modelBuilder.ApplyConfiguration(new MovimientoInventarioConfiguration());

            // Configurar índices y restricciones
            ConfigureIndexes(modelBuilder);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Índices para mejorar rendimiento
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.Nombre)
                .IsUnique();

            modelBuilder.Entity<Ingrediente>()
                .HasIndex(i => i.Nombre)
                .IsUnique();

            modelBuilder.Entity<Ingrediente>()
                .HasIndex(i => i.CategoriaId);

            modelBuilder.Entity<Lote>()
                .HasIndex(l => l.Codigo)
                .IsUnique();

            modelBuilder.Entity<Lote>()
                .HasIndex(l => l.IngredienteId);

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.NIT)
                .IsUnique();

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.Email);

            modelBuilder.Entity<OrdenDeCompra>()
                .HasIndex(o => o.Numero)
                .IsUnique();

            modelBuilder.Entity<OrdenDeCompra>()
                .HasIndex(o => o.ProveedorId);

            modelBuilder.Entity<MovimientoInventario>()
                .HasIndex(m => m.IngredienteId);

            modelBuilder.Entity<MovimientoInventario>()
                .HasIndex(m => m.FechaMovimiento);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Aquí podríamos agregar lógica adicional antes de guardar
            // como auditoría, eventos de dominio, etc.

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}