using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Infrastructure.Cache;
using InventarioDDD.Infrastructure.Persistence;
using InventarioDDD.Infrastructure.Repositories;

namespace InventarioDDD.Infrastructure.Configuration
{
    /// <summary>
    /// Configuración de ORM, inyección de dependencias
    /// </summary>
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar Entity Framework
            services.AddDbContext<InventarioDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=Inventario.db";

                options.UseSqlite(connectionString);

                // Configuraciones adicionales para desarrollo
                var isDevelopment = configuration["Development"] == "true";
                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Configurar caché en memoria
            services.AddMemoryCache();
            services.AddScoped<MemoryCacheService>();

            // Registrar Unit of Work (comentado - no implementado aún)
            // services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar repositorios
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<ILoteRepository, LoteRepository>();
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<IOrdenDeCompraRepository, OrdenDeCompraRepository>();
            services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();

            return services;
        }

        public static IServiceCollection AddInfrastructureHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Health checks comentados temporalmente - requiere paquete adicional
            // services.AddHealthChecks()
            //     .AddDbContextCheck<InventarioDbContext>("database");

            return services;
        }

        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();

            // Crear la base de datos si no existe
            await context.Database.EnsureCreatedAsync();

            // Aplicar migraciones pendientes
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}