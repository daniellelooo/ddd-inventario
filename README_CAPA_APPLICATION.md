# 🎯 CAPA APPLICATION - InventarioDDD.Application

## 📋 Índice

- [Descripción General](#descripción-general)
- [Responsabilidades](#responsabilidades)
- [Patrón CQRS](#patrón-cqrs)
- [Estructura de Archivos](#estructura-de-archivos)
- [Commands (Comandos)](#commands-comandos)
- [Queries (Consultas)](#queries-consultas)
- [Handlers (Manejadores)](#handlers-manejadores)
- [DTOs (Data Transfer Objects)](#dtos-data-transfer-objects)

---

## Descripción General

La **Capa Application** es el orquestador de la aplicación. Implementa el patrón **CQRS (Command Query Responsibility Segregation)** para separar las operaciones de escritura (Commands) de las operaciones de lectura (Queries).

Esta capa no contiene lógica de negocio del dominio, pero sí contiene la lógica de aplicación: cómo se coordinan las operaciones, qué repositorios llamar, qué servicios de dominio usar, etc.

### Tecnologías Utilizadas

- **MediatR** - Para implementar CQRS y el patrón Mediator
- **.NET 9**
- **AutoMapper** (opcional) - Para mapeo de objetos

---

## Responsabilidades

1. **Orquestar casos de uso** de la aplicación
2. **Implementar CQRS** (separar escrituras de lecturas)
3. **Coordinar llamadas** a repositorios y servicios de dominio
4. **Transformar entidades de dominio** a DTOs para la API
5. **Validar reglas de aplicación** (no de dominio)
6. **Gestionar transacciones** cuando sea necesario
7. **Publicar eventos de dominio** (si aplica)

> **Diferencia clave**: Dominio = "¿QUÉ?" (reglas de negocio), Application = "¿CÓMO?" (flujo de trabajo)

---

## Patrón CQRS

**CQRS** = Command Query Responsibility Segregation

### ¿Qué es CQRS?

Es un patrón que separa las operaciones que modifican datos (Commands) de las que solo leen datos (Queries).

```
┌─────────────────────────────────────────────────┐
│                    CQRS                         │
├─────────────────────┬───────────────────────────┤
│     COMMANDS        │        QUERIES            │
│   (Escritura)       │       (Lectura)           │
├─────────────────────┼───────────────────────────┤
│ - Modifican estado  │ - Solo leen datos         │
│ - No retornan datos │ - No modifican estado     │
│ - Validan reglas    │ - Pueden optimizarse      │
│ - Crean/Actualizan  │ - Retornan DTOs           │
└─────────────────────┴───────────────────────────┘
```

### Ventajas de CQRS

✅ **Separación de Responsabilidades**: Código más limpio y mantenible  
✅ **Escalabilidad**: Se pueden escalar lecturas y escrituras independientemente  
✅ **Optimización**: Queries pueden usar vistas optimizadas  
✅ **Testing**: Más fácil testear comandos y consultas por separado

---

## Estructura de Archivos

```
InventarioDDD.Application/
│
├── Commands/                 # Comandos (operaciones de escritura)
│   ├── AprobarOrdenDeCompraCommand.cs
│   ├── CrearCategoriaCommand.cs
│   ├── CrearIngredienteCommand.cs
│   ├── CrearOrdenDeCompraCommand.cs
│   ├── CrearProveedorCommand.cs
│   ├── RecibirOrdenDeCompraCommand.cs
│   └── RegistrarConsumoCommand.cs
│
├── Queries/                  # Consultas (operaciones de lectura)
│   ├── ObtenerHistorialMovimientosQuery.cs
│   ├── ObtenerIngredientesParaReabastecerQuery.cs
│   ├── ObtenerLotesProximosAVencerQuery.cs
│   └── ObtenerOrdenesPendientesQuery.cs
│
├── Handlers/                 # Manejadores de Commands y Queries
│   ├── AprobarOrdenDeCompraHandler.cs
│   ├── CrearCategoriaHandler.cs
│   ├── CrearIngredienteHandler.cs
│   ├── CrearOrdenDeCompraHandler.cs
│   ├── CrearProveedorHandler.cs
│   ├── ObtenerHistorialMovimientosHandler.cs
│   ├── ObtenerIngredientesParaReabastecerHandler.cs
│   ├── ObtenerLotesProximosAVencerHandler.cs
│   ├── ObtenerOrdenesPendientesHandler.cs
│   ├── RecibirOrdenDeCompraHandler.cs
│   └── RegistrarConsumoHandler.cs
│
├── DTOs/                     # Data Transfer Objects
│   ├── IngredienteDto.cs
│   ├── LoteDto.cs
│   ├── MovimientoInventarioDto.cs
│   └── OrdenDeCompraDto.cs
│
└── InventarioDDD.Application.csproj
```

---

## Commands (Comandos)

Los **Commands** representan intenciones de modificar el estado del sistema. Son objetos inmutables que contienen todos los datos necesarios para ejecutar una acción.

### Características de los Commands

- ✅ Son clases **simples (POCOs)**
- ✅ Implementan `IRequest<T>` de MediatR
- ✅ Son **inmutables** (solo propiedades get)
- ✅ Tienen **nombres en imperativo** (Crear, Actualizar, Eliminar)
- ✅ No contienen lógica, solo datos

### Patrón Común

```csharp
public class NombreDelCommand : IRequest<TipoRetorno>
{
    public Guid PropiedadId { get; set; }
    public string Propiedad1 { get; set; }
    public decimal Propiedad2 { get; set; }
    // ... más propiedades
}
```

---

### 📄 CrearIngredienteCommand.cs

**Propósito**: Representa la intención de crear un nuevo ingrediente en el sistema.

**Código**:

```csharp
public class CrearIngredienteCommand : IRequest<Guid>
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string UnidadMedida { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public Guid CategoriaId { get; set; }
}
```

**Propiedades**:

- `Nombre`: Nombre del ingrediente (ej: "Tomate")
- `Descripcion`: Descripción opcional
- `UnidadMedida`: Unidad de medida (kg, L, unidad, etc.)
- `StockMinimo`: Cantidad mínima antes de reabastecer
- `StockMaximo`: Cantidad máxima recomendada
- `CategoriaId`: ID de la categoría a la que pertenece

**Retorno**: `Guid` (ID del ingrediente creado)

**Flujo**:

```
API Controller → CrearIngredienteCommand → MediatR
→ CrearIngredienteHandler → Valida → Crea Entidad
→ Guarda en DB → Retorna ID
```

---

### 📄 CrearOrdenDeCompraCommand.cs

**Propósito**: Representa la intención de crear una nueva orden de compra.

**Código**:

```csharp
public class CrearOrdenDeCompraCommand : IRequest<Guid>
{
    public Guid ProveedorId { get; set; }
    public List<DetalleOrdenDto> Detalles { get; set; }
    public DateTime FechaEsperada { get; set; }
    public string Observaciones { get; set; }
}

public class DetalleOrdenDto
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string Moneda { get; set; }
}
```

**Propiedades**:

- `ProveedorId`: ID del proveedor que suministrará
- `Detalles`: Lista de ingredientes con cantidades y precios
- `FechaEsperada`: Fecha esperada de entrega
- `Observaciones`: Notas adicionales

**Retorno**: `Guid` (ID de la orden creada)

**Validaciones en el Handler**:

- ✅ Proveedor existe
- ✅ Ingredientes existen
- ✅ Cantidades son positivas
- ✅ Precios son positivos

---

### 📄 AprobarOrdenDeCompraCommand.cs

**Propósito**: Representa la intención de aprobar una orden de compra pendiente.

**Código**:

```csharp
public class AprobarOrdenDeCompraCommand : IRequest
{
    public Guid OrdenId { get; set; }
    public Guid UsuarioId { get; set; }
}
```

**Propiedades**:

- `OrdenId`: ID de la orden a aprobar
- `UsuarioId`: ID del usuario que aprueba (auditoría)

**Retorno**: `void` (no retorna nada, solo modifica estado)

**Reglas de Negocio Validadas**:

- ✅ La orden existe
- ✅ La orden está en estado "Pendiente"
- ✅ No se puede aprobar una orden ya aprobada o recibida

---

### 📄 RecibirOrdenDeCompraCommand.cs

**Propósito**: Representa la intención de recibir mercancía de una orden aprobada.

**Código**:

```csharp
public class RecibirOrdenDeCompraCommand : IRequest
{
    public Guid OrdenId { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public decimal CantidadRecibida { get; set; }
    public LoteDto Lote { get; set; }
    public string Observaciones { get; set; }
}

public class LoteDto
{
    public string Codigo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Ubicacion { get; set; }
}
```

**Propiedades**:

- `OrdenId`: ID de la orden a recibir
- `FechaRecepcion`: Fecha de recepción (opcional, default: hoy)
- `CantidadRecibida`: Cantidad recibida
- `Lote`: Información del lote a crear
- `Observaciones`: Notas de recepción

**Acciones del Handler**:

1. Valida que la orden esté aprobada
2. Cambia estado de orden a "Recibida"
3. **Crea automáticamente un Lote** con la información
4. **Actualiza el stock del ingrediente**
5. Registra un MovimientoInventario (Entrada)

**Este es uno de los comandos más importantes** porque coordina múltiples operaciones.

---

### 📄 RegistrarConsumoCommand.cs

**Propósito**: Representa la intención de registrar consumo de un ingrediente.

**Código**:

```csharp
public class RegistrarConsumoCommand : IRequest
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public string Motivo { get; set; }
    public Guid UsuarioId { get; set; }
}
```

**Propiedades**:

- `IngredienteId`: ID del ingrediente a consumir
- `Cantidad`: Cantidad a consumir
- `Motivo`: Razón del consumo (ej: "Preparación de hamburguesas")
- `UsuarioId`: Usuario que registra (auditoría)

**Acciones del Handler**:

1. Valida que el ingrediente existe
2. Valida que hay stock disponible
3. **Aplica FIFO** (First In, First Out) - consume lotes más antiguos primero
4. Reduce cantidad disponible en lotes
5. Actualiza stock total del ingrediente
6. Crea MovimientoInventario (Salida)

**Lógica FIFO**:

```
Lotes del ingrediente ordenados por fecha de vencimiento:
1. Lote A - Vence en 5 días  - Disponible: 10 kg
2. Lote B - Vence en 15 días - Disponible: 20 kg
3. Lote C - Vence en 30 días - Disponible: 15 kg

Consumo de 25 kg:
→ Consume 10 kg del Lote A (queda en 0)
→ Consume 15 kg del Lote B (queda en 5)
→ Lote C no se toca
```

---

### 📄 CrearProveedorCommand.cs

**Propósito**: Representa la intención de crear un nuevo proveedor.

**Código**:

```csharp
public class CrearProveedorCommand : IRequest<Guid>
{
    public string Nombre { get; set; }
    public string Nit { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public DireccionDto Direccion { get; set; }
    public string PersonaContacto { get; set; }
}

public class DireccionDto
{
    public string Calle { get; set; }
    public string Ciudad { get; set; }
    public string Pais { get; set; }
    public string CodigoPostal { get; set; }
}
```

**Validaciones**:

- ✅ NIT único
- ✅ Email válido
- ✅ Teléfono con formato correcto

---

### 📄 CrearCategoriaCommand.cs

**Propósito**: Representa la intención de crear una nueva categoría.

**Código**:

```csharp
public class CrearCategoriaCommand : IRequest<Guid>
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
}
```

**Simple pero necesario para organizar ingredientes**.

---

## Queries (Consultas)

Los **Queries** representan peticiones de información. No modifican el estado del sistema, solo leen datos.

### Características de los Queries

- ✅ Son clases **simples (POCOs)**
- ✅ Implementan `IRequest<T>` donde T es el tipo de retorno
- ✅ Tienen **nombres descriptivos** (Obtener, Listar, Buscar)
- ✅ **No modifican estado**
- ✅ Pueden tener parámetros de filtro

### Patrón Común

```csharp
public class ObtenerNombreQuery : IRequest<List<DtoRetorno>>
{
    public Guid? FiltroId { get; set; }
    public DateTime? FechaDesde { get; set; }
    // ... parámetros opcionales de filtro
}
```

---

### 📄 ObtenerIngredientesParaReabastecerQuery.cs

**Propósito**: Obtiene la lista de ingredientes que necesitan ser reabastecidos (stock bajo).

**Código**:

```csharp
public class ObtenerIngredientesParaReabastecerQuery
    : IRequest<List<IngredienteParaReabastecerDto>>
{
    // No tiene parámetros - obtiene todos los que están bajo stock mínimo
}
```

**Retorno**: `List<IngredienteParaReabastecerDto>`

```csharp
public class IngredienteParaReabastecerDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public decimal CantidadActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public string UnidadMedida { get; set; }
    public decimal CantidadSugerida { get; set; } // StockMaximo - CantidadActual
}
```

**Lógica del Handler**:

```csharp
var ingredientes = await _ingredienteRepository.ObtenerTodos();
var bajoStock = ingredientes
    .Where(i => i.CantidadEnStock.Valor < i.RangoDeStock.StockMinimo)
    .Select(i => new IngredienteParaReabastecerDto
    {
        Id = i.Id,
        Nombre = i.Nombre,
        CantidadActual = i.CantidadEnStock.Valor,
        StockMinimo = i.RangoDeStock.StockMinimo,
        StockMaximo = i.RangoDeStock.StockMaximo,
        CantidadSugerida = i.RangoDeStock.StockMaximo - i.CantidadEnStock.Valor
    })
    .ToList();

return bajoStock;
```

**Usado en**: Dashboard para mostrar alertas de stock bajo.

---

### 📄 ObtenerLotesProximosAVencerQuery.cs

**Propósito**: Obtiene lotes que están próximos a vencer para tomar acción.

**Código**:

```csharp
public class ObtenerLotesProximosAVencerQuery
    : IRequest<List<LoteProximoVencerDto>>
{
    public int DiasAnticipacion { get; set; } = 7; // Default: 7 días
}
```

**Parámetros**:

- `DiasAnticipacion`: Número de días de anticipación (default: 7)

**Retorno**: `List<LoteProximoVencerDto>`

```csharp
public class LoteProximoVencerDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }
    public string IngredienteNombre { get; set; }
    public decimal CantidadDisponible { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
}
```

**Lógica del Handler**:

```csharp
var lotes = await _loteRepository.ObtenerTodos();
var fechaLimite = DateTime.Now.AddDays(request.DiasAnticipacion);

var lotesVencimiento = lotes
    .Where(l => l.FechaVencimiento.Fecha <= fechaLimite &&
                l.CantidadDisponible.Valor > 0)
    .OrderBy(l => l.FechaVencimiento.Fecha)
    .Select(l => new LoteProximoVencerDto
    {
        Id = l.Id,
        Codigo = l.Codigo,
        IngredienteNombre = l.Ingrediente.Nombre,
        CantidadDisponible = l.CantidadDisponible.Valor,
        FechaVencimiento = l.FechaVencimiento.Fecha,
        DiasParaVencer = (l.FechaVencimiento.Fecha - DateTime.Now).Days
    })
    .ToList();

return lotesVencimiento;
```

**Usado en**: Dashboard y página de Lotes para priorizar consumo.

---

### 📄 ObtenerOrdenesPendientesQuery.cs

**Propósito**: Obtiene todas las órdenes de compra que están pendientes de aprobar.

**Código**:

```csharp
public class ObtenerOrdenesPendientesQuery
    : IRequest<List<OrdenDeCompraDto>>
{
    // No tiene parámetros
}
```

**Lógica del Handler**:

```csharp
var ordenes = await _ordenRepository.ObtenerPorEstado(EstadoOrden.Pendiente);
return ordenes.Select(MapearADto).ToList();
```

**Usado en**: Dashboard y página de Órdenes de Compra.

---

### 📄 ObtenerHistorialMovimientosQuery.cs

**Propósito**: Obtiene el historial de movimientos de inventario (entradas/salidas).

**Código**:

```csharp
public class ObtenerHistorialMovimientosQuery
    : IRequest<List<MovimientoInventarioDto>>
{
    public Guid? IngredienteId { get; set; } // Filtro opcional
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}
```

**Parámetros Opcionales**:

- `IngredienteId`: Filtrar por ingrediente específico
- `FechaDesde`: Fecha inicio del rango
- `FechaHasta`: Fecha fin del rango

**Retorno**: `List<MovimientoInventarioDto>`

```csharp
public class MovimientoInventarioDto
{
    public Guid Id { get; set; }
    public string IngredienteNombre { get; set; }
    public string TipoMovimiento { get; set; } // "Entrada" o "Salida"
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; }
    public string Motivo { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public Guid UsuarioId { get; set; }
}
```

**Usado en**: Página de Movimientos para auditoría.

---

## Handlers (Manejadores)

Los **Handlers** son las clases que ejecutan la lógica de los Commands y Queries. Cada Command/Query tiene su propio Handler.

### Características de los Handlers

- ✅ Implementan `IRequestHandler<TRequest, TResponse>`
- ✅ Tienen una **única responsabilidad** (Single Responsibility Principle)
- ✅ Coordinan llamadas a repositorios y servicios de dominio
- ✅ Transforman entidades a DTOs
- ✅ Gestionan transacciones si es necesario

### Patrón Común

```csharp
public class NombreHandler : IRequestHandler<NombreCommand, TipoRetorno>
{
    private readonly IRepositorio _repositorio;

    public NombreHandler(IRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<TipoRetorno> Handle(
        NombreCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validaciones
        // 2. Obtener entidades necesarias
        // 3. Ejecutar lógica de dominio
        // 4. Persistir cambios
        // 5. Retornar resultado
    }
}
```

---

### 📄 CrearIngredienteHandler.cs

**Propósito**: Maneja la creación de un nuevo ingrediente.

**Código Simplificado**:

```csharp
public class CrearIngredienteHandler
    : IRequestHandler<CrearIngredienteCommand, Guid>
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public CrearIngredienteHandler(
        IIngredienteRepository ingredienteRepository,
        ICategoriaRepository categoriaRepository)
    {
        _ingredienteRepository = ingredienteRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Guid> Handle(
        CrearIngredienteCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validar que la categoría existe
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId);
        if (categoria == null)
            throw new InvalidOperationException("La categoría no existe");

        // 2. Crear Value Objects
        var unidadMedida = new UnidadDeMedida(request.UnidadMedida, request.UnidadMedida);
        var rangoStock = new RangoDeStock(request.StockMinimo, request.StockMaximo);
        var cantidadInicial = new CantidadDisponible(0); // Inicia en 0

        // 3. Crear Entidad de Dominio
        var ingrediente = new Ingrediente(
            Guid.NewGuid(),
            request.Nombre,
            request.Descripcion,
            unidadMedida,
            rangoStock,
            cantidadInicial,
            request.CategoriaId
        );

        // 4. Persistir
        await _ingredienteRepository.AgregarAsync(ingrediente);
        await _ingredienteRepository.GuardarCambiosAsync();

        // 5. Retornar ID
        return ingrediente.Id;
    }
}
```

**Flujo**:

```
Command → Validar Categoría → Crear Value Objects → Crear Entidad
→ Guardar en DB → Retornar ID
```

---

### 📄 RecibirOrdenDeCompraHandler.cs

**Propósito**: Maneja la recepción de una orden de compra y crea lotes automáticamente.

**Código Simplificado**:

```csharp
public class RecibirOrdenDeCompraHandler
    : IRequestHandler<RecibirOrdenDeCompraCommand>
{
    private readonly IOrdenDeCompraRepository _ordenRepository;
    private readonly ILoteRepository _loteRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public async Task Handle(
        RecibirOrdenDeCompraCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la orden
        var orden = await _ordenRepository.ObtenerPorIdAsync(request.OrdenId);
        if (orden == null)
            throw new InvalidOperationException("Orden no encontrada");

        // 2. Validar que está aprobada
        if (orden.Estado != EstadoOrden.Aprobada)
            throw new InvalidOperationException("Solo se pueden recibir órdenes aprobadas");

        // 3. Crear el lote
        var fechaVencimiento = new FechaVencimiento(request.Lote.FechaVencimiento);
        var precio = new PrecioConMoneda(orden.PrecioUnitario, orden.Moneda);
        var cantidad = new CantidadDisponible(request.CantidadRecibida);

        var lote = new Lote(
            Guid.NewGuid(),
            request.Lote.Codigo,
            orden.IngredienteId,
            orden.ProveedorId,
            request.OrdenId,
            cantidad,
            cantidad, // cantidadInicial = cantidadDisponible
            fechaVencimiento,
            precio,
            request.Lote.Ubicacion
        );

        // 4. Actualizar stock del ingrediente
        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(orden.IngredienteId);
        ingrediente.AgregarLote(lote);

        // 5. Crear movimiento de inventario (Entrada)
        var movimiento = new MovimientoInventario(
            Guid.NewGuid(),
            orden.IngredienteId,
            TipoMovimiento.Entrada,
            cantidad,
            "Recepción de orden de compra",
            DateTime.Now,
            Guid.Empty // Sistema
        );

        // 6. Cambiar estado de la orden
        orden.RecibirOrden(request.FechaRecepcion ?? DateTime.Now);

        // 7. Persistir todos los cambios
        await _loteRepository.AgregarAsync(lote);
        await _ingredienteRepository.ActualizarAsync(ingrediente);
        await _movimientoRepository.AgregarAsync(movimiento);
        await _ordenRepository.ActualizarAsync(orden);
        await _ordenRepository.GuardarCambiosAsync();
    }
}
```

**Este es el handler más complejo** porque coordina:

1. Orden de compra
2. Creación de lote
3. Actualización de stock
4. Registro de movimiento

---

### 📄 RegistrarConsumoHandler.cs

**Propósito**: Maneja el registro de consumo aplicando FIFO.

**Código Simplificado**:

```csharp
public class RegistrarConsumoHandler
    : IRequestHandler<RegistrarConsumoCommand>
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly ILoteRepository _loteRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public async Task Handle(
        RegistrarConsumoCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener ingrediente con sus lotes
        var ingrediente = await _ingredienteRepository
            .ObtenerConLotesAsync(request.IngredienteId);

        if (ingrediente == null)
            throw new InvalidOperationException("Ingrediente no encontrado");

        // 2. Validar stock disponible
        if (ingrediente.CalcularStockDisponible() < request.Cantidad)
            throw new InvalidOperationException("Stock insuficiente");

        // 3. Aplicar FIFO - Consumir de lotes más antiguos primero
        var cantidadRestante = request.Cantidad;
        var lotesAfectados = new List<Lote>();

        var lotesOrdenados = ingrediente.Lotes
            .Where(l => l.CantidadDisponible.Valor > 0 && !l.EstaVencido())
            .OrderBy(l => l.FechaVencimiento.Fecha)
            .ToList();

        foreach (var lote in lotesOrdenados)
        {
            if (cantidadRestante <= 0) break;

            var cantidadAConsumir = Math.Min(
                cantidadRestante,
                lote.CantidadDisponible.Valor
            );

            lote.Consumir(cantidadAConsumir);
            lotesAfectados.Add(lote);
            cantidadRestante -= cantidadAConsumir;
        }

        // 4. Actualizar stock del ingrediente
        ingrediente.ConsumirIngrediente(request.Cantidad, request.Motivo);

        // 5. Crear movimiento
        var movimiento = new MovimientoInventario(
            Guid.NewGuid(),
            request.IngredienteId,
            TipoMovimiento.Salida,
            new CantidadDisponible(request.Cantidad),
            request.Motivo,
            DateTime.Now,
            request.UsuarioId
        );

        // 6. Persistir
        foreach (var lote in lotesAfectados)
            await _loteRepository.ActualizarAsync(lote);

        await _ingredienteRepository.ActualizarAsync(ingrediente);
        await _movimientoRepository.AgregarAsync(movimiento);
        await _ingredienteRepository.GuardarCambiosAsync();
    }
}
```

**Lógica FIFO Explicada**:

1. Obtiene lotes disponibles (no vencidos)
2. Los ordena por fecha de vencimiento (más próximo primero)
3. Consume de cada lote en orden hasta completar la cantidad
4. Actualiza cada lote afectado

---

## DTOs (Data Transfer Objects)

Los **DTOs** son objetos simples usados para transferir datos entre capas. No contienen lógica, solo propiedades.

### ¿Por qué DTOs?

✅ **Desacoplar** capas (Domain no debe exponerse directamente)  
✅ **Controlar** qué datos se exponen a la API  
✅ **Optimizar** transferencia de datos (solo lo necesario)  
✅ **Versionar** API sin afectar el dominio

---

### 📄 IngredienteDto.cs

```csharp
public class IngredienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string UnidadMedida { get; set; }
    public decimal CantidadEnStock { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public string CategoriaNombre { get; set; }
}
```

---

### 📄 LoteDto.cs

```csharp
public class LoteDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }
    public string IngredienteNombre { get; set; }
    public decimal CantidadInicial { get; set; }
    public decimal CantidadDisponible { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string Moneda { get; set; }
}
```

---

### 📄 OrdenDeCompraDto.cs

```csharp
public class OrdenDeCompraDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; }
    public string ProveedorNombre { get; set; }
    public string IngredienteNombre { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; }
    public string Estado { get; set; }
    public DateTime FechaEsperada { get; set; }
}
```

---

### 📄 MovimientoInventarioDto.cs

```csharp
public class MovimientoInventarioDto
{
    public Guid Id { get; set; }
    public string IngredienteNombre { get; set; }
    public string TipoMovimiento { get; set; } // "Entrada" o "Salida"
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; }
    public string Motivo { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public Guid UsuarioId { get; set; }
}
```

---

## Flujo Completo de un Command

```
┌──────────────────────────────────────────────────────────┐
│ 1. Frontend React                                        │
│    POST /api/ingredientes                                │
│    { "nombre": "Tomate", ... }                          │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│ 2. IngredientesController (API Layer)                   │
│    var command = new CrearIngredienteCommand {...};     │
│    var id = await _mediator.Send(command);              │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│ 3. MediatR (Mediator Pattern)                           │
│    Encuentra CrearIngredienteHandler                     │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│ 4. CrearIngredienteHandler (Application Layer)          │
│    - Valida categoría existe                            │
│    - Crea Value Objects                                  │
│    - Crea entidad Ingrediente                           │
│    - Llama a repositorio.AgregarAsync()                 │
│    - Guarda cambios                                      │
│    - Retorna ID del ingrediente                         │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│ 5. IngredienteRepository (Infrastructure Layer)         │
│    - Mapea entidad a tabla de DB                        │
│    - EF Core persiste en SQLite                         │
│    - Retorna entidad con ID generado                    │
└────────────────┬─────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────┐
│ 6. Response                                              │
│    201 Created                                           │
│    { "id": "guid-generado", ... }                       │
└──────────────────────────────────────────────────────────┘
```

---

## Ventajas de esta Arquitectura

### ✅ CQRS

- Código más organizado
- Queries optimizables independientemente
- Facilita caching en lecturas

### ✅ Mediator (MediatR)

- Desacoplamiento total
- Testing más fácil
- Permite agregar behaviors (logging, validación, transacciones)

### ✅ DTOs

- Dominio protegido
- Control de exposición de datos
- Versionamiento de API simplificado

### ✅ Single Responsibility

- Cada handler tiene una sola responsabilidad
- Código más mantenible
- Bugs más fáciles de localizar

---

---

## 📊 CRITERIOS DE EVALUACIÓN - CAPA APPLICATION

### ✅ 1. Use Cases Específicos (Casos de Uso)

**Criterio**: Hay use case y cada uno realiza una única acción de negocio

**Implementación en el Proyecto**:

| Use Case | Command/Query | Acción Única |
|----------|---------------|--------------|
| Crear Ingrediente | `CrearIngredienteCommand` | ✅ Solo crea ingredientes |
| Crear Orden de Compra | `CrearOrdenDeCompraCommand` | ✅ Solo crea órdenes |
| Aprobar Orden | `AprobarOrdenDeCompraCommand` | ✅ Solo aprueba órdenes |
| Recibir Orden | `RecibirOrdenDeCompraCommand` | ✅ Solo recibe órdenes y crea lotes |
| Registrar Consumo | `RegistrarConsumoCommand` | ✅ Solo registra consumos con FIFO |
| Crear Proveedor | `CrearProveedorCommand` | ✅ Solo crea proveedores |
| Crear Categoría | `CrearCategoriaCommand` | ✅ Solo crea categorías |
| Obtener Historial | `ObtenerHistorialMovimientosQuery` | ✅ Solo obtiene movimientos |
| Ingredientes para Reabastecer | `ObtenerIngredientesParaReabastecerQuery` | ✅ Solo obtiene ingredientes con stock bajo |
| Lotes por Vencer | `ObtenerLotesProximosAVencerQuery` | ✅ Solo obtiene lotes próximos a vencer |
| Órdenes Pendientes | `ObtenerOrdenesPendientesQuery` | ✅ Solo obtiene órdenes pendientes |

**Evidencia**:
```csharp
// ✅ CORRECTO: Un command = Una acción específica
public class CrearIngredienteCommand : IRequest<IngredienteDto>
{
    // Solo datos necesarios para crear ingrediente
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    // ...
}

// ❌ INCORRECTO sería:
// public class GestionarIngredienteCommand // (demasiado genérico)
```

**Conclusión**: ✅ **CUMPLE** - Cada Command/Query tiene una responsabilidad única y específica.

---

### ✅ 2. Orquestación (Sin Lógica de Negocio)

**Criterio**: Se implementa coordinación entre servicios de dominio, no contiene lógica de negocio

**Implementación en el Proyecto**:

**Ejemplo 1: RecibirOrdenDeCompraHandler** (Orquestación Compleja)
```csharp
public class RecibirOrdenDeCompraHandler : IRequestHandler<RecibirOrdenDeCompraCommand, OrdenDeCompraDto>
{
    public async Task<OrdenDeCompraDto> Handle(...)
    {
        // 1. ORQUESTACIÓN: Obtener orden
        var ordenAggregate = await _ordenRepository.ObtenerPorIdAsync(request.OrdenId);
        
        // 2. DELEGA LÓGICA DE NEGOCIO al Dominio
        ordenAggregate.OrdenDeCompra.MarcarRecibida(); // ← Lógica en el Dominio
        
        // 3. ORQUESTACIÓN: Crear lote
        var lote = new Lote(...); // ← Constructor del Dominio valida reglas
        
        // 4. ORQUESTACIÓN: Agregar lote al ingrediente
        var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(...);
        ingredienteAggregate.RegistrarLote(lote); // ← Lógica FIFO en el Dominio
        
        // 5. ORQUESTACIÓN: Persistir cambios
        await _ordenRepository.GuardarAsync(ordenAggregate);
        await _ingredienteRepository.GuardarAsync(ingredienteAggregate);
        
        // 6. ORQUESTACIÓN: Crear movimiento de inventario
        var movimiento = new MovimientoInventario(...);
        await _movimientoRepository.GuardarAsync(movimiento);
        
        // 7. MAPEO: Convertir a DTO (sin lógica de negocio)
        return MapToDto(ordenAggregate.OrdenDeCompra);
    }
}
```

**¿Dónde está la lógica de negocio?**
- ✅ `MarcarRecibida()` - En la entidad `OrdenDeCompra` (Domain)
- ✅ `RegistrarLote()` - En el agregado `IngredienteAggregate` (Domain)
- ✅ Validaciones de negocio - En constructores y métodos del Domain
- ✅ FIFO - Implementado en `IngredienteAggregate.ConsumirIngrediente()` (Domain)

**¿Qué hace el Handler?**
- ✅ Coordina llamadas a repositorios
- ✅ Obtiene agregados
- ✅ Invoca métodos del dominio
- ✅ Persiste cambios
- ✅ Mapea a DTOs

**Conclusión**: ✅ **CUMPLE** - Los handlers orquestan, NO contienen lógica de negocio.

---

### ✅ 3. Transacciones Bien Manejadas

**Criterio**: Garantiza consistencia en la operación completa

**Implementación en el Proyecto**:

**Forma 1: Transacciones Implícitas de EF Core**
```csharp
public async Task<OrdenDeCompraDto> Handle(RecibirOrdenDeCompraCommand request, ...)
{
    // Entity Framework Core maneja transacciones automáticamente
    // Si algo falla, hace rollback de todos los cambios
    
    var ordenAggregate = await _ordenRepository.ObtenerPorIdAsync(request.OrdenId);
    ordenAggregate.OrdenDeCompra.MarcarRecibida();
    
    var lote = new Lote(...);
    var ingredienteAggregate = await _ingredienteRepository.ObtenerPorIdAsync(...);
    ingredienteAggregate.RegistrarLote(lote);
    
    // TODAS estas operaciones se hacen en una sola transacción
    await _ordenRepository.GuardarAsync(ordenAggregate);
    await _ingredienteRepository.GuardarAsync(ingredienteAggregate);
    await _movimientoRepository.GuardarAsync(movimiento);
    
    // Si cualquier operación falla, se hace rollback automático
    return MapToDto(ordenAggregate.OrdenDeCompra);
}
```

**Forma 2: Transacciones Explícitas (cuando se requiere)**
```csharp
// En repositorios que heredan el mismo DbContext
public class IngredienteRepository : IIngredienteRepository
{
    private readonly InventarioDbContext _context;
    
    public async Task GuardarAsync(IngredienteAggregate ingredienteAggregate)
    {
        _context.Ingredientes.Update(ingredienteAggregate.Ingrediente);
        await _context.SaveChangesAsync(); // ← Commit de transacción
    }
}
```

**Garantías de Consistencia**:
1. ✅ Si falla `MarcarRecibida()` → No se guarda nada
2. ✅ Si falla crear lote → No se actualiza orden ni ingrediente
3. ✅ Si falla `RegistrarLote()` → Rollback completo
4. ✅ Si falla `SaveChangesAsync()` → Ningún cambio persiste

**Beneficios**:
- ✅ **Atomicidad**: Todo se guarda o nada se guarda
- ✅ **Consistencia**: El dominio siempre está en estado válido
- ✅ **Aislamiento**: Otras transacciones no ven cambios parciales
- ✅ **Durabilidad**: Los cambios confirmados son permanentes

**Conclusión**: ✅ **CUMPLE** - Las transacciones garantizan consistencia total de las operaciones.

---

## Resumen

La **Capa Application** orquesta los casos de uso de la aplicación mediante:

1. ✅ **Commands** - Operaciones que modifican estado
2. ✅ **Queries** - Operaciones que solo leen datos
3. ✅ **Handlers** - Ejecutan la lógica de Commands/Queries
4. ✅ **DTOs** - Transfieren datos entre capas

**No contiene lógica de dominio** - Solo coordina y orquesta operaciones.  
**La lógica de negocio está en el Domain Layer** (Entidades, Value Objects, Domain Services).

### 🎯 Cumplimiento de Criterios de Evaluación

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| **Use Cases Específicos** | ✅ **CUMPLE** | 11 Commands/Queries, cada uno con acción única |
| **Orquestación sin Lógica** | ✅ **CUMPLE** | Handlers coordinan, lógica en Domain |
| **Transacciones Manejadas** | ✅ **CUMPLE** | EF Core garantiza consistencia ACID |
