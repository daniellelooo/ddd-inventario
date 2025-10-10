using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventarioDDD.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(InventarioDbContext context)
    {
        if (await context.Categorias.AnyAsync())
        {
            Console.WriteLine("⚠️  La base de datos ya contiene datos.");
            return;
        }

        Console.WriteLine("🌱 Iniciando seed simplificado...");

        // 1. CATEGORÍAS
        var catCarnes = new Categoria("Carnes", "Proteínas cárnicas");
        var catLacteos = new Categoria("Lácteos", "Productos lácteos");
        var catVegetales = new Categoria("Vegetales", "Verduras frescas");

        context.Categorias.AddRange(catCarnes, catLacteos, catVegetales);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ 3 categorías");

        // 2. PROVEEDORES
        var prov1 = new Proveedor(
            "Carnes Premium",
            "900123456-7",
            "3101234567",
            "ventas@carnespremium.com",
            new DireccionProveedor("Calle 50 #23-45", "Bogotá", "Colombia", "110111"),
            "Juan Pérez"
        );

        var prov2 = new Proveedor(
            "Lácteos del Valle",
            "900234567-8",
            "3109876543",
            "pedidos@lacteos.com",
            new DireccionProveedor("Av. Cali #67-89", "Cali", "Colombia", "760001"),
            "María González"
        );

        context.Proveedores.AddRange(prov1, prov2);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ 2 proveedores");

        // 3. INGREDIENTES - UNO POR UNO
        var ing1 = new Ingrediente(
            "Carne Molida",
            "Carne molida de res",
            new UnidadDeMedida("Kilogramo", "kg"),
            new RangoDeStock(20m, 100m),
            catCarnes.Id
        );
        context.Ingredientes.Add(ing1);
        await context.SaveChangesAsync();

        var ing2 = new Ingrediente(
            "Queso Cheddar",
            "Queso cheddar en lonchas",
            new UnidadDeMedida("Kilogramo", "kg"),
            new RangoDeStock(10m, 50m),
            catLacteos.Id
        );
        context.Ingredientes.Add(ing2);
        await context.SaveChangesAsync();

        var ing3 = new Ingrediente(
            "Lechuga",
            "Lechuga fresca",
            new UnidadDeMedida("Kilogramo", "kg"),
            new RangoDeStock(5m, 30m),
            catVegetales.Id
        );
        context.Ingredientes.Add(ing3);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ 3 ingredientes");

        // 4. LOTES
        var lote1 = new Lote(
            "LOTE-001",
            ing1.Id,
            prov1.Id,
            50m,
            new FechaVencimiento(DateTime.Now.AddDays(15)),
            new PrecioConMoneda(18500m, "COP")
        );

        var lote2 = new Lote(
            "LOTE-002",
            ing2.Id,
            prov2.Id,
            30m,
            new FechaVencimiento(DateTime.Now.AddDays(30)),
            new PrecioConMoneda(32000m, "COP")
        );

        context.Lotes.AddRange(lote1, lote2);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ 2 lotes");

        // 5. ÓRDENES DE COMPRA
        var orden1 = new OrdenDeCompra(
            "OC-001",
            ing1.Id,
            prov1.Id,
            new CantidadDisponible(50m),
            new PrecioConMoneda(18500m, "COP"),
            DateTime.Now.AddDays(3),
            "Pedido semanal"
        );

        var orden2 = new OrdenDeCompra(
            "OC-002",
            ing3.Id,
            prov2.Id,
            new CantidadDisponible(20m),
            new PrecioConMoneda(2800m, "COP"),
            DateTime.Now.AddDays(1),
            "Pedido urgente"
        );
        orden2.Aprobar();

        context.OrdenesDeCompra.AddRange(orden1, orden2);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ 2 órdenes");

        // 6. MOVIMIENTOS
        var mov1 = new MovimientoInventario(
            ing1.Id,
            TipoMovimiento.Entrada,
            50m,
            new UnidadDeMedida("Kilogramo", "kg"),
            "Recepción lote LOTE-001",
            lote1.Id,
            "OC-001"
        );

        var mov2 = new MovimientoInventario(
            ing1.Id,
            TipoMovimiento.Salida,
            10m,
            new UnidadDeMedida("Kilogramo", "kg"),
            "Consumo diario",
            lote1.Id
        );

        context.MovimientosInventario.AddRange(mov1, mov2);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ 2 movimientos");

        Console.WriteLine("\n✨ Base de datos lista para pruebas\n");
    }
}
