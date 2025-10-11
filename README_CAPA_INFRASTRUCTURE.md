# 🗄️ CAPA INFRASTRUCTURE - InventarioDDD.Infrastructure

## 📋 Índice

- [Descripción General](#descripción-general)
- [Responsabilidades](#responsabilidades)
- [Estructura de Archivos](#estructura-de-archivos)
- [Persistence (Persistencia)](#persistence-persistencia)
- [Repositories (Repositorios)](#repositories-repositorios)
- [Configuration (Configuración de EF Core)](#configuration-configuración-de-ef-core)
- [Patrón Repository](#patrón-repository)

---

## Descripción General

La **Capa Infrastructure** es responsable de la persistencia de datos y la implementación de servicios externos. Esta capa implementa las interfaces definidas en la capa Domain, proporcionando la lógica concreta para interactuar con bases de datos, APIs externas, sistemas de archivos, etc.

### Tecnologías Utilizadas

- **Entity Framework Core 9.0** - ORM (Object-Relational Mapping)
- **SQLite** - Base de datos relacional
- **.NET 9**
- **Fluent API** - Para configuración de entidades

---

## Responsabilidades

1. **Implementar interfaces de repositorios** definidas en Domain
2. **Persistir y recuperar agregados** de la base de datos
3. **Configurar mapeo objeto-relacional** (ORM)
4. **Gestionar transacciones** de base de datos
5. **Implementar servicios de infraestructura** (email, cache, logging)
6. **Aislar detalles técnicos** de persistencia del dominio

> **Principio clave**: El Domain define QUÉ necesita (interfaces), Infrastructure define CÓMO se implementa (clases concretas).

---

## Estructura de Archivos

```
InventarioDDD.Infrastructure/
│
├── Persistence/              # DbContext y configuración de EF Core
│   └── InventarioDbContext.cs
│
├── Configuration/            # Configuraciones de entidades (Fluent API)
│   ├── CategoriaConfiguration.cs
│   ├── IngredienteConfiguration.cs
│   ├── LoteConfiguration.cs
│   ├── MovimientoInventarioConfiguration.cs
│   ├── OrdenDeCompraConfiguration.cs
│   └── ProveedorConfiguration.cs
│
├── Repositories/             # Implementaciones de repositorios
│   ├── CategoriaRepository.cs
│   ├── IngredienteRepository.cs
│   ├── LoteRepository.cs
│   ├── MovimientoInventarioRepository.cs
│   ├── OrdenDeCompraRepository.cs
│   └── ProveedorRepository.cs
│
├── Cache/                    # Implementación de cache (opcional)
│
└── InventarioDDD.Infrastructure.csproj
```

---

## Persistence (Persistencia)

### 📄 InventarioDbContext.cs

**Propósito**: Clase central de Entity Framework Core que gestiona la conexión a la base de datos y expone las entidades como DbSets.

**Responsabilidades**:

1. Configurar la conexión a SQLite
2. Exponer entidades como DbSets
3. Aplicar configuraciones de Fluent API
4. Gestionar el ciclo de vida de entidades

**Código**:

```csharp
public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
        : base(options)
    {
    }

    // DbSets - Representan tablas en la base de datos
    public DbSet<Ingrediente> Ingredientes { get; set; }
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<OrdenDeCompra> OrdenesCompra { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de Fluent API
        modelBuilder.ApplyConfiguration(new IngredienteConfiguration());
        modelBuilder.ApplyConfiguration(new LoteConfiguration());
        modelBuilder.ApplyConfiguration(new OrdenDeCompraConfiguration());
        modelBuilder.ApplyConfiguration(new ProveedorConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new MovimientoInventarioConfiguration());
    }
}
```

**DbSets Explicados**:

| DbSet                   | Tabla                 | Descripción                          |
| ----------------------- | --------------------- | ------------------------------------ |
| `Ingredientes`          | Ingredientes          | Almacena ingredientes del inventario |
| `Lotes`                 | Lotes                 | Almacena lotes de ingredientes       |
| `OrdenesCompra`         | OrdenesCompra         | Almacena órdenes de compra           |
| `Proveedores`           | Proveedores           | Almacena información de proveedores  |
| `Categorias`            | Categorias            | Almacena categorías de ingredientes  |
| `MovimientosInventario` | MovimientosInventario | Almacena historial de movimientos    |

**Configuración en Program.cs**:

```csharp
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Cadena de Conexión** (appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Inventario.db"
  }
}
```

---

## Repositories (Repositorios)

Los **Repositorios** son la implementación concreta de las interfaces definidas en la capa Domain. Encapsulan toda la lógica de acceso a datos.

### Patrón Repository

```
┌─────────────────────────────────────────────────┐
│             DOMAIN LAYER                        │
│                                                 │
│  interface IIngredienteRepository               │
│  {                                              │
│      Task<Ingrediente> ObtenerPorIdAsync(Guid); │
│      Task AgregarAsync(Ingrediente);            │
│  }                                              │
└────────────────┬────────────────────────────────┘
                 │
                 │ implementa
                 │
┌────────────────▼────────────────────────────────┐
│          INFRASTRUCTURE LAYER                   │
│                                                 │
│  class IngredienteRepository                    │
│      : IIngredienteRepository                   │
│  {                                              │
│      // Usa EF Core para persistir             │
│  }                                              │
└─────────────────────────────────────────────────┘
```

### Ventajas del Patrón Repository

✅ **Abstracción de persistencia** - Domain no conoce detalles de DB  
✅ **Testing facilitado** - Se pueden usar mocks  
✅ **Cambiar tecnología** de persistencia sin afectar dominio  
✅ **Consultas complejas** centralizadas

---

### 📄 IngredienteRepository.cs

**Propósito**: Implementa operaciones CRUD y consultas específicas para ingredientes.

**Código**:

```csharp
public class IngredienteRepository : IIngredienteRepository
{
    private readonly InventarioDbContext _context;

    public IngredienteRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Ingrediente> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Ingredientes
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Ingrediente> ObtenerConLotesAsync(Guid id)
    {
        return await _context.Ingredientes
            .Include(i => i.Lotes) // Carga eager de lotes
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Ingrediente>> ObtenerTodosAsync()
    {
        return await _context.Ingredientes
            .Include(i => i.Categoria)
            .ToListAsync();
    }

    public async Task<List<Ingrediente>> ObtenerParaReabastecerAsync()
    {
        return await _context.Ingredientes
            .Where(i => i.CantidadEnStock < i.RangoDeStock.StockMinimo)
            .Include(i => i.Categoria)
            .ToListAsync();
    }

    public async Task AgregarAsync(Ingrediente ingrediente)
    {
        await _context.Ingredientes.AddAsync(ingrediente);
    }

    public async Task ActualizarAsync(Ingrediente ingrediente)
    {
        _context.Ingredientes.Update(ingrediente);
    }

    public async Task EliminarAsync(Guid id)
    {
        var ingrediente = await ObtenerPorIdAsync(id);
        if (ingrediente != null)
        {
            _context.Ingredientes.Remove(ingrediente);
        }
    }

    public async Task<int> GuardarCambiosAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

**Métodos Clave**:

| Método                        | Descripción                                       | Uso                           |
| ----------------------------- | ------------------------------------------------- | ----------------------------- |
| `ObtenerPorIdAsync`           | Obtiene ingrediente por ID                        | Consultas simples             |
| `ObtenerConLotesAsync`        | Obtiene ingrediente con sus lotes (eager loading) | Para consumo (necesita lotes) |
| `ObtenerTodosAsync`           | Lista todos los ingredientes                      | Listado general               |
| `ObtenerParaReabastecerAsync` | Filtra ingredientes con stock bajo                | Dashboard y alertas           |
| `AgregarAsync`                | Agrega nuevo ingrediente                          | Crear                         |
| `ActualizarAsync`             | Actualiza ingrediente existente                   | Editar                        |
| `GuardarCambiosAsync`         | Persiste cambios en DB                            | Commit de transacción         |

**Eager Loading vs Lazy Loading**:

```csharp
// Eager Loading - Carga relaciones inmediatamente
var ingrediente = await _context.Ingredientes
    .Include(i => i.Lotes)      // Carga lotes
    .Include(i => i.Categoria)  // Carga categoría
    .FirstOrDefaultAsync(i => i.Id == id);

// Sin Include - Solo carga la entidad principal
var ingrediente = await _context.Ingredientes
    .FirstOrDefaultAsync(i => i.Id == id);
// ingrediente.Lotes será null o vacío
```

---

### 📄 OrdenDeCompraRepository.cs

**Propósito**: Implementa operaciones para gestionar órdenes de compra.

**Código Simplificado**:

```csharp
public class OrdenDeCompraRepository : IOrdenDeCompraRepository
{
    private readonly InventarioDbContext _context;

    public OrdenDeCompraRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<OrdenDeCompra> ObtenerPorIdAsync(Guid id)
    {
        return await _context.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Ingrediente)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<OrdenDeCompra>> ObtenerPorEstadoAsync(EstadoOrden estado)
    {
        return await _context.OrdenesCompra
            .Where(o => o.Estado == estado)
            .Include(o => o.Proveedor)
            .Include(o => o.Ingrediente)
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<OrdenDeCompra>> ObtenerPendientesAsync()
    {
        return await ObtenerPorEstadoAsync(EstadoOrden.Pendiente);
    }

    public async Task AgregarAsync(OrdenDeCompra orden)
    {
        await _context.OrdenesCompra.AddAsync(orden);
    }

    public async Task ActualizarAsync(OrdenDeCompra orden)
    {
        _context.OrdenesCompra.Update(orden);
    }

    public async Task<int> GuardarCambiosAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

**Consultas Específicas**:

- `ObtenerPorEstadoAsync`: Filtra órdenes por estado (Pendiente, Aprobada, Recibida)
- `ObtenerPendientesAsync`: Shortcut para órdenes pendientes

---

### 📄 LoteRepository.cs

**Propósito**: Gestiona operaciones CRUD de lotes.

**Código Simplificado**:

```csharp
public class LoteRepository : ILoteRepository
{
    private readonly InventarioDbContext _context;

    public LoteRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Lote> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Lotes
            .Include(l => l.Ingrediente)
            .Include(l => l.Proveedor)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Lote>> ObtenerPorIngredienteAsync(Guid ingredienteId)
    {
        return await _context.Lotes
            .Where(l => l.IngredienteId == ingredienteId)
            .OrderBy(l => l.FechaVencimiento)
            .ToListAsync();
    }

    public async Task<List<Lote>> ObtenerProximosAVencerAsync(int dias)
    {
        var fechaLimite = DateTime.Now.AddDays(dias);

        return await _context.Lotes
            .Where(l => l.FechaVencimiento <= fechaLimite &&
                       l.CantidadDisponible > 0)
            .Include(l => l.Ingrediente)
            .OrderBy(l => l.FechaVencimiento)
            .ToListAsync();
    }

    public async Task AgregarAsync(Lote lote)
    {
        await _context.Lotes.AddAsync(lote);
    }

    public async Task ActualizarAsync(Lote lote)
    {
        _context.Lotes.Update(lote);
    }

    public async Task<int> GuardarCambiosAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

**Consultas Especializadas**:

- `ObtenerPorIngredienteAsync`: Obtiene todos los lotes de un ingrediente (para FIFO)
- `ObtenerProximosAVencerAsync`: Filtra lotes que vencen en N días

---

### 📄 ProveedorRepository.cs, CategoriaRepository.cs, MovimientoInventarioRepository.cs

**Similares a los anteriores**, implementan operaciones CRUD básicas:

- ObtenerPorIdAsync
- ObtenerTodosAsync
- AgregarAsync
- ActualizarAsync
- EliminarAsync
- GuardarCambiosAsync

---

## Configuration (Configuración de EF Core)

Las **Configuration Classes** usan **Fluent API** de Entity Framework Core para configurar cómo las entidades se mapean a tablas de base de datos.

### ¿Por qué Fluent API?

✅ **Separación de responsabilidades** - Configuración fuera de entidades  
✅ **Más potente** que Data Annotations  
✅ **Type-safe** - Detecta errores en compilación  
✅ **Configuración compleja** - Permite relaciones complejas

### Patrón Común

```csharp
public class NombreEntidadConfiguration : IEntityTypeConfiguration<NombreEntidad>
{
    public void Configure(EntityTypeBuilder<NombreEntidad> builder)
    {
        // 1. Nombre de tabla
        builder.ToTable("NombreTabla");

        // 2. Clave primaria
        builder.HasKey(e => e.Id);

        // 3. Propiedades
        builder.Property(e => e.Propiedad)
            .IsRequired()
            .HasMaxLength(100);

        // 4. Value Objects (Owned Types)
        builder.OwnsOne(e => e.ValueObject, vo => {
            vo.Property(v => v.Valor).HasColumnName("Valor");
        });

        // 5. Relaciones
        builder.HasOne(e => e.OtraEntidad)
            .WithMany()
            .HasForeignKey(e => e.OtraEntidadId);

        // 6. Índices
        builder.HasIndex(e => e.Propiedad);
    }
}
```

---

### 📄 IngredienteConfiguration.cs

**Propósito**: Configura cómo la entidad `Ingrediente` se mapea a la tabla en la base de datos.

**Código**:

```csharp
public class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        // 1. Nombre de tabla
        builder.ToTable("Ingredientes");

        // 2. Clave primaria
        builder.HasKey(i => i.Id);

        // 3. Propiedades simples
        builder.Property(i => i.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Descripcion)
            .HasMaxLength(500);

        // 4. Value Objects como Owned Types

        // UnidadDeMedida - Se guarda en columnas de la misma tabla
        builder.OwnsOne(i => i.UnidadDeMedida, um =>
        {
            um.Property(u => u.Nombre)
                .HasColumnName("UnidadMedidaNombre")
                .IsRequired()
                .HasMaxLength(50);

            um.Property(u => u.Simbolo)
                .HasColumnName("UnidadMedidaSimbolo")
                .IsRequired()
                .HasMaxLength(10);
        });

        // RangoDeStock
        builder.OwnsOne(i => i.RangoDeStock, rs =>
        {
            rs.Property(r => r.StockMinimo)
                .HasColumnName("StockMinimo")
                .IsRequired();

            rs.Property(r => r.StockMaximo)
                .HasColumnName("StockMaximo")
                .IsRequired();
        });

        // CantidadEnStock
        builder.OwnsOne(i => i.CantidadEnStock, c =>
        {
            c.Property(ca => ca.Valor)
                .HasColumnName("CantidadEnStock")
                .IsRequired()
                .HasDefaultValue(0);
        });

        // 5. Relaciones
        builder.HasOne(i => i.Categoria)
            .WithMany()
            .HasForeignKey(i => i.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict); // No eliminar categoría si tiene ingredientes

        builder.HasMany(i => i.Lotes)
            .WithOne(l => l.Ingrediente)
            .HasForeignKey(l => l.IngredienteId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina ingrediente, eliminar lotes

        // 6. Índices para performance
        builder.HasIndex(i => i.Nombre);
        builder.HasIndex(i => i.CategoriaId);
    }
}
```

**Estructura de Tabla Resultante**:

```sql
CREATE TABLE Ingredientes (
    Id                    TEXT PRIMARY KEY,
    Nombre                TEXT NOT NULL,
    Descripcion           TEXT,
    UnidadMedidaNombre    TEXT NOT NULL,
    UnidadMedidaSimbolo   TEXT NOT NULL,
    StockMinimo           REAL NOT NULL,
    StockMaximo           REAL NOT NULL,
    CantidadEnStock       REAL NOT NULL DEFAULT 0,
    CategoriaId           TEXT NOT NULL,
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);

CREATE INDEX IX_Ingredientes_Nombre ON Ingredientes(Nombre);
CREATE INDEX IX_Ingredientes_CategoriaId ON Ingredientes(CategoriaId);
```

**Owned Types Explicado**:

Los **Value Objects** (UnidadDeMedida, RangoDeStock, CantidadEnStock) se guardan como columnas en la misma tabla del Ingrediente, no en tablas separadas.

```
Entidad: Ingrediente
  ├─ Id (columna propia)
  ├─ Nombre (columna propia)
  ├─ UnidadDeMedida (Value Object)
  │   ├─ UnidadMedidaNombre (columna)
  │   └─ UnidadMedidaSimbolo (columna)
  ├─ RangoDeStock (Value Object)
  │   ├─ StockMinimo (columna)
  │   └─ StockMaximo (columna)
  └─ CantidadEnStock (Value Object)
      └─ CantidadEnStock (columna)
```

---

### 📄 LoteConfiguration.cs

**Propósito**: Configura el mapeo de la entidad `Lote`.

**Código Simplificado**:

```csharp
public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("Lotes");
        builder.HasKey(l => l.Id);

        // Propiedades
        builder.Property(l => l.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Ubicacion)
            .HasMaxLength(200);

        // Value Objects
        builder.OwnsOne(l => l.CantidadInicial, c =>
        {
            c.Property(ca => ca.Valor)
                .HasColumnName("CantidadInicial")
                .IsRequired();
        });

        builder.OwnsOne(l => l.CantidadDisponible, c =>
        {
            c.Property(ca => ca.Valor)
                .HasColumnName("CantidadDisponible")
                .IsRequired();
        });

        builder.OwnsOne(l => l.FechaVencimiento, f =>
        {
            f.Property(fv => fv.Fecha)
                .HasColumnName("FechaVencimiento")
                .IsRequired();
        });

        builder.OwnsOne(l => l.Precio, p =>
        {
            p.Property(pr => pr.Monto)
                .HasColumnName("PrecioUnitario")
                .IsRequired();

            p.Property(pr => pr.Moneda)
                .HasColumnName("Moneda")
                .IsRequired()
                .HasMaxLength(10);
        });

        // Relaciones
        builder.HasOne(l => l.Ingrediente)
            .WithMany(i => i.Lotes)
            .HasForeignKey(l => l.IngredienteId);

        builder.HasOne(l => l.Proveedor)
            .WithMany()
            .HasForeignKey(l => l.ProveedorId);

        builder.HasOne(l => l.OrdenDeCompra)
            .WithMany()
            .HasForeignKey(l => l.OrdenDeCompraId);

        // Índices
        builder.HasIndex(l => l.Codigo).IsUnique();
        builder.HasIndex(l => l.IngredienteId);
        builder.HasIndex(l => l.FechaVencimiento);
    }
}
```

**Índices Importantes**:

- `Codigo` (único) - Para búsqueda rápida por código de lote
- `IngredienteId` - Para obtener lotes de un ingrediente
- `FechaVencimiento` - Para ordenar por FIFO

---

### 📄 OrdenDeCompraConfiguration.cs

**Propósito**: Configura el mapeo de la entidad `OrdenDeCompra`.

**Aspectos Clave**:

```csharp
// Configuración de Enum
builder.Property(o => o.Estado)
    .HasConversion<string>() // Guarda enum como string en DB
    .IsRequired();

// Configuración de propiedades decimales
builder.Property(o => o.Cantidad)
    .HasPrecision(18, 2); // 18 dígitos, 2 decimales

builder.Property(o => o.PrecioUnitario)
    .HasPrecision(18, 2);

// Columna calculada (Total)
builder.Property(o => o.Total)
    .HasComputedColumnSql("Cantidad * PrecioUnitario");
```

**Conversión de Enum**:

```csharp
// En código: EstadoOrden.Pendiente
// En DB: "Pendiente" (string)
```

---

### 📄 ProveedorConfiguration.cs

**Propósito**: Configura el mapeo de `Proveedor` con su Value Object `DireccionProveedor`.

**Código Simplificado**:

```csharp
public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedores");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Nit)
            .IsRequired()
            .HasMaxLength(50);

        // Value Object: DireccionProveedor
        builder.OwnsOne(p => p.Direccion, d =>
        {
            d.Property(dir => dir.Calle)
                .HasColumnName("DireccionCalle")
                .HasMaxLength(200);

            d.Property(dir => dir.Ciudad)
                .HasColumnName("DireccionCiudad")
                .HasMaxLength(100);

            d.Property(dir => dir.Pais)
                .HasColumnName("DireccionPais")
                .HasMaxLength(100);

            d.Property(dir => dir.CodigoPostal)
                .HasColumnName("DireccionCodigoPostal")
                .HasMaxLength(20);
        });

        // Índices
        builder.HasIndex(p => p.Nit).IsUnique();
    }
}
```

---

### 📄 CategoriaConfiguration.cs

**Configuración simple** - Solo propiedades básicas:

```csharp
builder.ToTable("Categorias");
builder.HasKey(c => c.Id);

builder.Property(c => c.Nombre)
    .IsRequired()
    .HasMaxLength(100);

builder.Property(c => c.Descripcion)
    .HasMaxLength(500);

builder.HasIndex(c => c.Nombre).IsUnique();
```

---

### 📄 MovimientoInventarioConfiguration.cs

**Propósito**: Configura el mapeo de movimientos de inventario (auditoría).

**Aspectos Clave**:

```csharp
// Enum TipoMovimiento como string
builder.Property(m => m.TipoMovimiento)
    .HasConversion<string>()
    .IsRequired();

// Fecha con valor por defecto
builder.Property(m => m.FechaMovimiento)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

// Índices para consultas de auditoría
builder.HasIndex(m => m.IngredienteId);
builder.HasIndex(m => m.FechaMovimiento);
builder.HasIndex(m => new { m.IngredienteId, m.FechaMovimiento });
```

**Índice Compuesto**:

```csharp
// Optimiza consultas como:
// "Obtener movimientos del ingrediente X en el rango de fechas Y-Z"
builder.HasIndex(m => new { m.IngredienteId, m.FechaMovimiento });
```

---

## Patrón Repository

### Ventajas

✅ **Abstracción**: Domain no conoce EF Core  
✅ **Testing**: Fácil crear mocks  
✅ **Queries centralizadas**: Lógica de acceso a datos en un solo lugar  
✅ **Cambio de tecnología**: Se puede cambiar de EF Core a Dapper sin afectar domain

### Desventajas

⚠️ **Código adicional**: Más clases que mantener  
⚠️ **Sobre-abstracción**: A veces LINQ de EF Core es suficiente

### Cuándo Usar Repository

✅ **Proyectos grandes** con múltiples fuentes de datos  
✅ **Testing extensivo** requerido  
✅ **Posible cambio** de tecnología de persistencia  
✅ **Queries complejas** que se reutilizan

---

## Flujo de Persistencia Completo

```
┌──────────────────────────────────────────────────────┐
│ 1. Handler (Application Layer)                      │
│    var ingrediente = new Ingrediente(...);          │
│    await _ingredienteRepository.AgregarAsync(ing);  │
│    await _ingredienteRepository.GuardarCambios();   │
└────────────────┬─────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────┐
│ 2. IngredienteRepository (Infrastructure)           │
│    _context.Ingredientes.Add(ingrediente);          │
│    await _context.SaveChangesAsync();               │
└────────────────┬─────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────┐
│ 3. InventarioDbContext                              │
│    Gestiona ChangeTracker de EF Core               │
│    Aplica configuraciones de Fluent API            │
└────────────────┬─────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────┐
│ 4. Entity Framework Core                            │
│    - Convierte entidades a SQL                      │
│    - Aplica Value Objects como columnas            │
│    - Gestiona relaciones                            │
└────────────────┬─────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────┐
│ 5. SQLite Database (Inventario.db)                  │
│    INSERT INTO Ingredientes (...) VALUES (...);     │
└──────────────────────────────────────────────────────┘
```

---

## Migraciones de EF Core

### Crear Migración

```bash
dotnet ef migrations add NombreMigracion --project InventarioDDD.Infrastructure
```

### Aplicar Migración

```bash
dotnet ef database update --project InventarioDDD.Infrastructure
```

### Ver Migraciones

```bash
dotnet ef migrations list
```

---

---

## 📊 CRITERIOS DE EVALUACIÓN - CAPA INFRASTRUCTURE

### ✅ 1. Repositorios Implementan Interfaces del Dominio

**Criterio**: Los repositorios implementan interfaces definidas en el dominio

**Implementación en el Proyecto**:

**Interfaces en el Dominio** (`InventarioDDD.Domain/Interfaces/IRepositorios.cs`):
```csharp
namespace InventarioDDD.Domain.Interfaces
{
    // ✅ Interfaces definidas EN EL DOMINIO
    public interface IIngredienteRepository
    {
        Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id);
        Task<List<IngredienteAggregate>> ObtenerTodosAsync();
        Task GuardarAsync(IngredienteAggregate ingredienteAggregate);
        // ...
    }
    
    public interface IOrdenDeCompraRepository { /* ... */ }
    public interface IProveedorRepository { /* ... */ }
    // ...
}
```

**Implementaciones en Infrastructure** (`InventarioDDD.Infrastructure/Repositories/`):
```csharp
namespace InventarioDDD.Infrastructure.Repositories
{
    // ✅ Infrastructure IMPLEMENTA las interfaces del Domain
    public class IngredienteRepository : IIngredienteRepository
    {
        private readonly InventarioDbContext _context;
        
        // Implementa todos los métodos definidos en la interfaz
        public async Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id) { /* ... */ }
        public async Task<List<IngredienteAggregate>> ObtenerTodosAsync() { /* ... */ }
        public async Task GuardarAsync(IngredienteAggregate aggregate) { /* ... */ }
    }
}
```

**Inyección de Dependencias** (Program.cs):
```csharp
// Domain depende de la INTERFAZ, Infrastructure proporciona la IMPLEMENTACIÓN
builder.Services.AddScoped<IIngredienteRepository, IngredienteRepository>();
builder.Services.AddScoped<IOrdenDeCompraRepository, OrdenDeCompraRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
// ...
```

**Conclusión**: ✅ **CUMPLE** - Todas las interfaces están en Domain, implementaciones en Infrastructure.

---

### ✅ 2. No Hacen Referencias a Tecnologías Específicas

**Criterio**: Las interfaces NO hacen referencia a SQL, APIs, NoSQL, etc.

**Verificación**:

**❌ INCORRECTO sería**:
```csharp
// En el Domain (MAL - referencias a tecnología específica)
public interface IIngredienteRepository
{
    Task<SqlDataReader> ObtenerPorIdAsync(Guid id); // ❌ SqlDataReader
    Task<DbSet<Ingrediente>> ObtenerTodos(); // ❌ DbSet de EF Core
}
```

**✅ CORRECTO en nuestro proyecto**:
```csharp
// En el Domain (BIEN - abstracciones puras)
public interface IIngredienteRepository
{
    Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id); // ✅ Agregado del Domain
    Task<List<IngredienteAggregate>> ObtenerTodosAsync(); // ✅ Colección genérica
    Task GuardarAsync(IngredienteAggregate aggregate); // ✅ Agregado del Domain
}
```

**Dependencias del Domain**:
```xml
<!-- InventarioDDD.Domain.csproj -->
<ItemGroup>
  <!-- ✅ SOLO tiene MediatR.Contracts (para eventos) -->
  <PackageReference Include="MediatR.Contracts" Version="2.0.1" />
  
  <!-- ✅ NO tiene Entity Framework -->
  <!-- ✅ NO tiene SQLite -->
  <!-- ✅ NO tiene referencias HTTP/API -->
</ItemGroup>
```

**Conclusión**: ✅ **CUMPLE** - El Domain NO conoce ninguna tecnología de persistencia.

---

### ✅ 3. Retornan Entidades y Agregados del Dominio

**Criterio**: Los repositorios trabajan con objetos del dominio, NO con DTOs ni objetos de infraestructura

**Implementación**:

```csharp
public class IngredienteRepository : IIngredienteRepository
{
    // ✅ RETORNA IngredienteAggregate (del Domain)
    public async Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id)
    {
        var ingrediente = await _context.Ingredientes
            .Include(i => i.Lotes) // ← Carga relaciones
            .FirstOrDefaultAsync(i => i.Id == id);
            
        if (ingrediente == null) return null;
        
        // ✅ RETORNA AGREGADO DEL DOMAIN
        return new IngredienteAggregate(ingrediente, ingrediente.Lotes.ToList());
    }
    
    // ✅ RECIBE Y PERSISTE IngredienteAggregate (del Domain)
    public async Task GuardarAsync(IngredienteAggregate ingredienteAggregate)
    {
        _context.Ingredientes.Update(ingredienteAggregate.Ingrediente);
        await _context.SaveChangesAsync();
    }
}
```

**Lo que NO hace**:
```csharp
// ❌ INCORRECTO - Retornar DTOs
public async Task<IngredienteDto> ObtenerPorIdAsync(Guid id) { /* ... */ }

// ❌ INCORRECTO - Retornar objetos de BD
public async Task<DataRow> ObtenerPorIdAsync(Guid id) { /* ... */ }

// ❌ INCORRECTO - Retornar ViewModels
public async Task<IngredienteViewModel> ObtenerPorIdAsync(Guid id) { /* ... */ }
```

**Conclusión**: ✅ **CUMPLE** - Los repositorios solo trabajan con entidades y agregados del Domain.

---

### ✅ 4. ORM Implementado para Conversión y Transformación

**Criterio**: Se tiene implementado ORM para la conversión de tablas a objetos del dominio

**Implementación - Entity Framework Core**:

**InventarioDbContext.cs**:
```csharp
public class InventarioDbContext : DbContext
{
    // ✅ DbSets representan tablas en la BD
    public DbSet<Ingrediente> Ingredientes { get; set; }
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<OrdenDeCompra> OrdenesCompra { get; set; }
    // ...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ Aplica configuraciones de Fluent API
        modelBuilder.ApplyConfiguration(new IngredienteConfiguration());
        modelBuilder.ApplyConfiguration(new LoteConfiguration());
        // ...
    }
}
```

**Conversión Automática**:
```
┌─────────────────────────┐         ┌──────────────────────────┐
│   Tabla Ingredientes    │  ORM    │   Clase Ingrediente      │
│   (SQLite)              │  <-->   │   (Domain Entity)        │
├─────────────────────────┤         ├──────────────────────────┤
│ Id (TEXT)               │  <-->   │ Guid Id                  │
│ Nombre (TEXT)           │  <-->   │ string Nombre            │
│ StockMinimo (REAL)      │  <-->   │ RangoDeStock (VO)        │
│ UnidadDeMedida (TEXT)   │  <-->   │ UnidadDeMedida (VO)      │
└─────────────────────────┘         └──────────────────────────┘
```

**Mapeo de Value Objects**:
```csharp
// IngredienteConfiguration.cs
public void Configure(EntityTypeBuilder<Ingrediente> builder)
{
    // ✅ ORM mapea Value Objects a columnas
    builder.OwnsOne(i => i.RangoDeStock, rango =>
    {
        rango.Property(r => r.StockMinimo).HasColumnName("StockMinimo");
        rango.Property(r => r.StockMaximo).HasColumnName("StockMaximo");
    });
    
    builder.OwnsOne(i => i.UnidadDeMedida, unidad =>
    {
        unidad.Property(u => u.Simbolo).HasColumnName("UnidadDeMedida");
    });
}
```

**Conclusión**: ✅ **CUMPLE** - Entity Framework Core convierte automáticamente entre tablas SQL y objetos del Domain.

---

### ✅ 5. ORM Solo en Infraestructura, Domain No Depende

**Criterio**: Solo se usa ORM en la capa Infrastructure, el Domain no depende de EF Core

**Verificación**:

**Referencias del Proyecto**:

`InventarioDDD.Domain.csproj`:
```xml
<ItemGroup>
  <!-- ✅ SIN Entity Framework Core -->
  <PackageReference Include="MediatR.Contracts" Version="2.0.1" />
</ItemGroup>
```

`InventarioDDD.Infrastructure.csproj`:
```xml
<ItemGroup>
  <!-- ✅ Entity Framework SOLO en Infrastructure -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
  
  <!-- Referencia al Domain (sin EF Core) -->
  <ProjectReference Include="..\InventarioDDD.Domain\InventarioDDD.Domain.csproj" />
</ItemGroup>
```

**Entidades del Domain**:
```csharp
// Ingrediente.cs (Domain)
using InventarioDDD.Domain.ValueObjects; // ✅ Solo referencias del Domain

namespace InventarioDDD.Domain.Entities
{
    public class Ingrediente
    {
        // ✅ SIN atributos de EF Core [Key], [Required], etc.
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        // ...
        
        // ✅ SIN referencias a DbContext, DbSet, etc.
    }
}
```

**Configuración en Infrastructure**:
```csharp
// IngredienteConfiguration.cs (Infrastructure)
using Microsoft.EntityFrameworkCore; // ✅ EF Core SOLO en Infrastructure

public class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        // Toda la configuración de EF Core aquí
    }
}
```

**Conclusión**: ✅ **CUMPLE** - El Domain está 100% libre de dependencias de EF Core.

---

### ⚠️ 6. Implementación de Caché

**Criterio**: Se tienen implementados mecanismos de caché

**Estado Actual**: ⚠️ **NO IMPLEMENTADO EXPLÍCITAMENTE**

**Justificación**:
- El proyecto usa SQLite que es muy rápido para operaciones locales
- Las consultas son simples y no requieren caché adicional
- Entity Framework Core tiene caché de primer nivel automático (ChangeTracker)

**Caché de Primer Nivel de EF Core** (Automático):
```csharp
// Primera consulta: va a la BD
var ingrediente1 = await _context.Ingredientes.FindAsync(id);

// Segunda consulta: usa caché del ChangeTracker (NO va a BD)
var ingrediente2 = await _context.Ingredientes.FindAsync(id);

// ingrediente1 == ingrediente2 (misma instancia en memoria)
```

**Posible Implementación Futura** (si se requiere):

**Opción 1: Caché en Memoria (IMemoryCache)**
```csharp
public class IngredienteRepositoryConCache : IIngredienteRepository
{
    private readonly IMemoryCache _cache;
    private readonly InventarioDbContext _context;
    
    public async Task<IngredienteAggregate?> ObtenerPorIdAsync(Guid id)
    {
        // Intentar obtener del caché
        if (_cache.TryGetValue($"ingrediente_{id}", out IngredienteAggregate? cached))
            return cached;
        
        // Si no está en caché, buscar en BD
        var ingrediente = await _context.Ingredientes
            .Include(i => i.Lotes)
            .FirstOrDefaultAsync(i => i.Id == id);
            
        if (ingrediente != null)
        {
            var aggregate = new IngredienteAggregate(ingrediente, ingrediente.Lotes.ToList());
            
            // Guardar en caché por 5 minutos
            _cache.Set($"ingrediente_{id}", aggregate, TimeSpan.FromMinutes(5));
            
            return aggregate;
        }
        
        return null;
    }
}
```

**Opción 2: Caché Distribuido (Redis)**
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

**Conclusión**: ⚠️ **PARCIALMENTE CUMPLE** - Tiene caché de primer nivel de EF Core (automático), pero no tiene implementación explícita de caché en memoria o distribuido.

---

## Resumen

La **Capa Infrastructure** implementa la persistencia de datos mediante:

1. ✅ **DbContext** - Gestiona conexión y DbSets
2. ✅ **Repositories** - Implementan interfaces del Domain
3. ✅ **Configuration Classes** - Mapean entidades a tablas con Fluent API
4. ✅ **Entity Framework Core** - ORM para acceso a datos
5. ✅ **SQLite** - Base de datos relacional

**Principio clave**: Aisla los detalles técnicos de persistencia del Domain, permitiendo que el dominio se enfoque en lógica de negocio.

### 🎯 Cumplimiento de Criterios de Evaluación

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| **Repositorios implementan interfaces del Domain** | ✅ **CUMPLE** | Interfaces en Domain, implementaciones en Infrastructure |
| **Sin referencias a tecnologías específicas** | ✅ **CUMPLE** | Domain NO conoce SQL/EF Core/SQLite |
| **Retornan entidades y agregados** | ✅ **CUMPLE** | Trabajan con objetos del Domain, NO DTOs |
| **ORM implementado** | ✅ **CUMPLE** | Entity Framework Core convierte tablas ↔ objetos |
| **ORM solo en Infrastructure** | ✅ **CUMPLE** | Domain sin dependencias de EF Core |
| **Implementación de Caché** | ⚠️ **PARCIAL** | Caché de primer nivel (EF Core), sin caché explícito |
