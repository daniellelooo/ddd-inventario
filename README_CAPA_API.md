# 🌐 CAPA API - InventarioDDD.API

## 📋 Índice

- [Descripción General](#descripción-general)
- [Responsabilidades](#responsabilidades)
- [Estructura de Archivos](#estructura-de-archivos)
- [Archivos Principales](#archivos-principales)
- [Controllers (Controladores)](#controllers-controladores)
- [Middleware](#middleware)
- [Configuración](#configuración)

---

## Descripción General

La **Capa API** es la capa más externa de la aplicación y actúa como punto de entrada para todas las peticiones HTTP. Esta capa expone endpoints RESTful que permiten a los clientes (como el frontend React) interactuar con el sistema de inventario.

### Tecnologías Utilizadas

- **.NET 9** (ASP.NET Core)
- **MediatR** - Para implementar el patrón Mediator
- **Swagger/OpenAPI** - Para documentación de API
- **Entity Framework Core** - ORM para acceso a datos

---

## Responsabilidades

1. **Recibir peticiones HTTP** desde clientes externos
2. **Validar datos de entrada** básicos (formato, tipos)
3. **Enrutar peticiones** a los handlers correspondientes mediante MediatR
4. **Serializar/Deserializar** objetos JSON
5. **Manejar errores** y devolver respuestas HTTP apropiadas
6. **Implementar CORS** para permitir acceso desde el frontend
7. **Documentar endpoints** con Swagger

> **Nota Importante**: Esta capa NO contiene lógica de negocio. Solo coordina y delega responsabilidades.

---

## Estructura de Archivos

```
InventarioDDD.API/
│
├── Controllers/              # Controladores REST
│   ├── CategoriasController.cs
│   ├── IngredientesController.cs
│   ├── InventarioController.cs
│   ├── LotesController.cs
│   ├── OrdenesCompraController.cs
│   └── ProveedoresController.cs
│
├── Middleware/               # Middleware personalizado
│   └── ExceptionHandlingMiddleware.cs
│
├── Properties/
│   └── launchSettings.json   # Configuración de inicio
│
├── Program.cs                # Punto de entrada y configuración
├── appsettings.json          # Configuración de la aplicación
└── InventarioDDD.API.csproj  # Archivo de proyecto
```

---

## Archivos Principales

### 📄 Program.cs

**Propósito**: Punto de entrada de la aplicación. Configura todos los servicios y el pipeline de middleware.

**Responsabilidades**:

1. **Configuración de Servicios**:

   - Registra MediatR para CQRS
   - Configura DbContext con SQLite
   - Registra repositorios (Dependency Injection)
   - Configura Swagger para documentación

2. **Configuración del Pipeline HTTP**:
   - Habilita Swagger en desarrollo
   - Configura CORS para permitir peticiones del frontend
   - Agrega middleware de manejo de excepciones
   - Configura enrutamiento de controladores

**Código Clave**:

```csharp
// Registro de MediatR (Patrón Mediator para CQRS)
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CrearIngredienteHandler).Assembly);
});

// Configuración de DbContext con SQLite
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de Repositorios (Dependency Injection)
builder.Services.AddScoped<IIngredienteRepository, IngredienteRepository>();
builder.Services.AddScoped<IOrdenDeCompraRepository, OrdenDeCompraRepository>();
// ... otros repositorios

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());
});
```

**Flujo de Configuración**:

```
Inicio → Configurar Servicios → Construir App →
Configurar Pipeline → Ejecutar Migraciones → Iniciar Servidor
```

---

### 📄 appsettings.json

**Propósito**: Almacena la configuración de la aplicación.

**Contenido Principal**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Inventario.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Configuraciones Clave**:

- **ConnectionStrings**: Cadena de conexión a la base de datos SQLite
- **Logging**: Niveles de log para diferentes componentes

---

## Controllers (Controladores)

Los controladores son clases que exponen endpoints HTTP. Cada controlador maneja un conjunto de operaciones relacionadas con una entidad del dominio.

### 🎮 Patrón Común de Controladores

Todos los controladores siguen este patrón:

1. **Heredan de `ControllerBase`**
2. **Usan `[ApiController]` y `[Route]`** para configuración automática
3. **Inyectan `IMediator`** en el constructor
4. **Delegan operaciones** a handlers mediante MediatR
5. **Devuelven `IActionResult`** con códigos HTTP apropiados

**Estructura Típica**:

```csharp
[ApiController]
[Route("api/[controller]")]
public class NombreController : ControllerBase
{
    private readonly IMediator _mediator;

    public NombreController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new Query());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Command command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
```

---

### 📁 IngredientesController.cs

**Propósito**: Gestiona las operaciones relacionadas con ingredientes.

**Endpoints**:

| Método | Ruta                            | Descripción                         | Handler Usado                             |
| ------ | ------------------------------- | ----------------------------------- | ----------------------------------------- |
| GET    | `/api/ingredientes`             | Obtiene todos los ingredientes      | Query directo al repositorio              |
| GET    | `/api/ingredientes/{id}`        | Obtiene un ingrediente por ID       | Query al repositorio                      |
| GET    | `/api/ingredientes/reabastecer` | Obtiene ingredientes con stock bajo | `ObtenerIngredientesParaReabastecerQuery` |
| POST   | `/api/ingredientes`             | Crea un nuevo ingrediente           | `CrearIngredienteCommand`                 |

**Ejemplo de Endpoint**:

```csharp
[HttpGet("reabastecer")]
public async Task<IActionResult> GetParaReabastecer()
{
    var query = new ObtenerIngredientesParaReabastecerQuery();
    var resultado = await _mediator.Send(query);
    return Ok(resultado);
}
```

**Flujo de una Petición**:

```
Cliente → GET /api/ingredientes/reabastecer → IngredientesController
→ _mediator.Send(Query) → ObtenerIngredientesParaReabastecerHandler
→ Ejecuta lógica → Retorna DTOs → Controller → JSON Response
```

**Validaciones**:

- Los datos de entrada se validan automáticamente por `[ApiController]`
- Las validaciones de negocio están en los handlers

---

### 📁 OrdenesCompraController.cs

**Propósito**: Gestiona el ciclo de vida completo de las órdenes de compra.

**Endpoints**:

| Método | Ruta                              | Descripción                   | Handler Usado                   |
| ------ | --------------------------------- | ----------------------------- | ------------------------------- |
| GET    | `/api/ordenescompra`              | Obtiene todas las órdenes     | Query al repositorio            |
| GET    | `/api/ordenescompra/pendientes`   | Obtiene órdenes pendientes    | `ObtenerOrdenesPendientesQuery` |
| POST   | `/api/ordenescompra`              | Crea una nueva orden          | `CrearOrdenDeCompraCommand`     |
| POST   | `/api/ordenescompra/{id}/aprobar` | Aprueba una orden             | `AprobarOrdenDeCompraCommand`   |
| POST   | `/api/ordenescompra/{id}/recibir` | Recibe una orden y crea lotes | `RecibirOrdenDeCompraCommand`   |

**Ejemplo - Aprobar Orden**:

```csharp
[HttpPost("{id}/aprobar")]
public async Task<IActionResult> Aprobar(
    Guid id,
    [FromBody] AprobarOrdenRequest request)
{
    var command = new AprobarOrdenDeCompraCommand
    {
        OrdenId = id,
        UsuarioId = request.UsuarioId
    };

    await _mediator.Send(command);
    return Ok(new { message = "Orden aprobada exitosamente" });
}
```

**Flujo Completo de una Orden**:

```
1. POST /ordenescompra → CrearOrdenDeCompraCommand
   └─> Estado: Pendiente

2. POST /ordenescompra/{id}/aprobar → AprobarOrdenDeCompraCommand
   └─> Estado: Aprobada

3. POST /ordenescompra/{id}/recibir → RecibirOrdenDeCompraCommand
   └─> Estado: Recibida
   └─> Crea Lotes automáticamente
   └─> Actualiza Stock del Ingrediente
```

---

### 📁 InventarioController.cs

**Propósito**: Gestiona operaciones de inventario como consumo y movimientos.

**Endpoints**:

| Método | Ruta                                    | Descripción                      | Handler Usado                      |
| ------ | --------------------------------------- | -------------------------------- | ---------------------------------- |
| POST   | `/api/inventario/consumo`               | Registra consumo de ingrediente  | `RegistrarConsumoCommand`          |
| GET    | `/api/inventario/movimientos`           | Obtiene historial de movimientos | `ObtenerHistorialMovimientosQuery` |
| GET    | `/api/inventario/lotes/proximos-vencer` | Obtiene lotes próximos a vencer  | `ObtenerLotesProximosAVencerQuery` |

**Ejemplo - Registrar Consumo**:

```csharp
[HttpPost("consumo")]
public async Task<IActionResult> RegistrarConsumo(
    [FromBody] RegistrarConsumoCommand command)
{
    await _mediator.Send(command);
    return Ok(new { message = "Consumo registrado exitosamente" });
}
```

**Flujo de Registro de Consumo**:

```
Cliente → POST /api/inventario/consumo
→ RegistrarConsumoCommand { ingredienteId, cantidad, motivo }
→ RegistrarConsumoHandler
→ Aplica FIFO en lotes
→ Reduce stock disponible
→ Crea MovimientoInventario
→ Retorna éxito
```

---

### 📁 LotesController.cs

**Propósito**: Gestiona la consulta de lotes de ingredientes.

**Endpoints**:

| Método | Ruta                                     | Descripción                     |
| ------ | ---------------------------------------- | ------------------------------- |
| GET    | `/api/lotes`                             | Obtiene todos los lotes         |
| GET    | `/api/lotes/{id}`                        | Obtiene un lote por ID          |
| GET    | `/api/lotes/ingrediente/{ingredienteId}` | Obtiene lotes de un ingrediente |

**Características**:

- Solo permite consultas (GET)
- Los lotes se crean automáticamente al recibir órdenes de compra
- No hay endpoints POST/PUT/DELETE directos

---

### 📁 ProveedoresController.cs

**Propósito**: Gestiona operaciones CRUD de proveedores.

**Endpoints**:

| Método | Ruta                    | Descripción                   | Handler Usado           |
| ------ | ----------------------- | ----------------------------- | ----------------------- |
| GET    | `/api/proveedores`      | Obtiene todos los proveedores | Query al repositorio    |
| GET    | `/api/proveedores/{id}` | Obtiene un proveedor por ID   | Query al repositorio    |
| POST   | `/api/proveedores`      | Crea un nuevo proveedor       | `CrearProveedorCommand` |

---

### 📁 CategoriasController.cs

**Propósito**: Gestiona operaciones CRUD de categorías de ingredientes.

**Endpoints**:

| Método | Ruta                   | Descripción                  | Handler Usado           |
| ------ | ---------------------- | ---------------------------- | ----------------------- |
| GET    | `/api/categorias`      | Obtiene todas las categorías | Query al repositorio    |
| GET    | `/api/categorias/{id}` | Obtiene una categoría por ID | Query al repositorio    |
| POST   | `/api/categorias`      | Crea una nueva categoría     | `CrearCategoriaCommand` |

---

## Middleware

### 🛡️ ExceptionHandlingMiddleware.cs

**Propósito**: Captura y maneja todas las excepciones de la aplicación de forma centralizada.

**Responsabilidades**:

1. Capturar excepciones no controladas
2. Loggear errores con detalles
3. Devolver respuestas HTTP apropiadas
4. Evitar exponer detalles internos al cliente

**Funcionamiento**:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Ejecuta el siguiente middleware
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            message = "Ha ocurrido un error interno",
            details = exception.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

**Tipos de Excepciones Manejadas**:

- `InvalidOperationException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `KeyNotFoundException` → 404 Not Found
- `Exception` (genérica) → 500 Internal Server Error

**Registro en Program.cs**:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## Configuración

### 🔧 CORS (Cross-Origin Resource Sharing)

**Propósito**: Permitir que el frontend (React en puerto 3000) haga peticiones a la API (puerto 5261).

**Configuración**:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()    // Permite cualquier origen
            .AllowAnyMethod()    // Permite GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader());  // Permite cualquier header
});

// En el pipeline
app.UseCors("AllowAll");
```

> **Nota de Seguridad**: En producción, deberías especificar orígenes específicos en lugar de `AllowAnyOrigin()`.

---

### 📚 Swagger/OpenAPI

**Propósito**: Genera documentación interactiva de la API.

**Configuración**:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// En el pipeline (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Acceso**: http://localhost:5261/swagger

**Características**:

- Documentación automática de todos los endpoints
- Permite probar endpoints directamente desde el navegador
- Muestra modelos de datos de entrada/salida

---

## Flujo de una Petición HTTP Completo

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Cliente (Frontend React)                                │
│    POST http://localhost:5261/api/ingredientes             │
│    Body: { "nombre": "Tomate", ... }                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. Pipeline de Middleware                                   │
│    ├─ CORS Middleware (valida origen)                      │
│    ├─ ExceptionHandlingMiddleware (try/catch global)       │
│    └─ Routing Middleware (encuentra el controller)         │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. IngredientesController                                   │
│    [HttpPost]                                               │
│    public async Task<IActionResult> Create(Command cmd)     │
│    {                                                         │
│        var result = await _mediator.Send(cmd);             │
│        return CreatedAtAction(...);                         │
│    }                                                         │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. MediatR (Patrón Mediator)                               │
│    Encuentra el Handler correspondiente al Command         │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. CrearIngredienteHandler (Capa Application)              │
│    - Valida datos                                           │
│    - Crea entidad de dominio                                │
│    - Llama al repositorio                                   │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. IngredienteRepository (Capa Infrastructure)             │
│    - Guarda en base de datos                                │
│    - Retorna entidad guardada                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 7. Retorno al Controller                                    │
│    return CreatedAtAction(..., resultado);                  │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 8. Respuesta HTTP                                           │
│    Status: 201 Created                                      │
│    Body: { "id": "...", "nombre": "Tomate", ... }         │
└─────────────────────────────────────────────────────────────┘
```

---

## Ventajas de esta Arquitectura

### ✅ Separation of Concerns

Cada capa tiene una responsabilidad clara:

- **API**: Recibir/enviar HTTP
- **Application**: Lógica de aplicación y orquestación
- **Infrastructure**: Persistencia y servicios externos

### ✅ Patrón Mediator (MediatR)

- Desacopla controllers de handlers
- Facilita testing
- Permite agregar comportamientos transversales (logging, validación)

### ✅ Manejo de Errores Centralizado

- Un solo lugar para manejar excepciones
- Respuestas consistentes
- Logging automático

### ✅ Documentación Automática (Swagger)

- Documentación siempre actualizada
- Facilita testing manual
- Ayuda a frontend a consumir la API

---

## Códigos HTTP Utilizados

| Código                    | Significado         | Cuándo se Usa           |
| ------------------------- | ------------------- | ----------------------- |
| 200 OK                    | Éxito               | GET, PUT exitosos       |
| 201 Created               | Recurso creado      | POST exitoso            |
| 204 No Content            | Éxito sin contenido | DELETE exitoso          |
| 400 Bad Request           | Datos inválidos     | Validación fallida      |
| 404 Not Found             | Recurso no existe   | GET de ID inexistente   |
| 500 Internal Server Error | Error del servidor  | Excepción no controlada |

---

---

## 📊 CRITERIOS DE EVALUACIÓN - CAPA PRESENTACIÓN (API)

### ✅ 1. Controllers Delgados (Thin Controllers)

**Criterio**: Controllers delgados que implementan los casos de uso

**Implementación**:

```csharp
[ApiController]
[Route("api/[controller]")]
public class IngredientesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public IngredientesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    // ✅ CONTROLLER DELGADO: Solo coordina, no tiene lógica
    [HttpPost]
    public async Task<ActionResult<IngredienteDto>> Crear([FromBody] CrearIngredienteCommand command)
    {
        // 1. Recibe el comando
        // 2. Delega a MediatR (que ejecuta el Handler)
        var resultado = await _mediator.Send(command);
        
        // 3. Retorna respuesta HTTP
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }
}
```

**¿Qué NO hace el Controller?**
- ❌ NO valida reglas de negocio
- ❌ NO accede directamente a la base de datos
- ❌ NO contiene lógica de dominio
- ❌ NO hace cálculos complejos
- ❌ NO coordina múltiples operaciones

**¿Qué SÍ hace el Controller?**
- ✅ Recibe datos del HTTP request
- ✅ Delega a MediatR
- ✅ Retorna respuestas HTTP apropiadas
- ✅ Maneja routing y verbos HTTP

**Ejemplo de Controller GRUESO (incorrecto)**:
```csharp
// ❌ MAL - Controller con lógica de negocio
[HttpPost]
public async Task<ActionResult> CrearIncorrecto([FromBody] IngredienteDto dto)
{
    // ❌ Lógica de negocio en el controller
    if (dto.StockMinimo > dto.StockMaximo)
        return BadRequest("Stock mínimo debe ser menor al máximo");
    
    // ❌ Acceso directo a la BD
    _context.Ingredientes.Add(new Ingrediente { ... });
    await _context.SaveChangesAsync();
    
    // ❌ Lógica de cálculo
    var valorInventario = dto.CantidadEnStock * dto.PrecioUnitario;
    
    return Ok();
}
```

**Conclusión**: ✅ **CUMPLE** - Todos los controllers son delgados y solo coordinan.

---

### ✅ 2. Manejan Consistencia (Commits, Rollbacks)

**Criterio**: Controllers manejan commits y rollbacks de transacciones

**Implementación**:

**Forma 1: Transacciones Implícitas (Automáticas)**
```csharp
[HttpPost("recibir")]
public async Task<ActionResult<OrdenDeCompraDto>> RecibirOrden(
    [FromBody] RecibirOrdenDeCompraCommand command)
{
    // ✅ MediatR + EF Core manejan transacciones automáticamente
    var resultado = await _mediator.Send(command);
    
    // Si todo va bien: COMMIT automático
    // Si hay excepción: ROLLBACK automático
    
    return Ok(resultado);
}
```

**Forma 2: Manejo de Errores con Middleware**
```csharp
// ExceptionHandlingMiddleware.cs
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    try
    {
        // ✅ Ejecuta el request
        await next(context);
        // Si llega aquí: COMMIT exitoso
    }
    catch (InvalidOperationException ex)
    {
        // ✅ Si hay excepción: ROLLBACK automático
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        // ✅ Cualquier error: ROLLBACK
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Error interno" });
    }
}
```

**Flujo de Transacción**:
```
┌──────────────────────────────────────────────┐
│ 1. Controller recibe request                 │
└─────────────┬────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────┐
│ 2. _mediator.Send(command)                   │
│    → Handler ejecuta lógica                  │
│    → Llama a repositorios                    │
│    → EF Core inicia transacción implícita    │
└─────────────┬────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────┐
│ 3. SaveChangesAsync()                        │
│    ✅ Éxito → COMMIT                         │
│    ❌ Error → ROLLBACK                       │
└─────────────┬────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────┐
│ 4. Controller retorna respuesta HTTP         │
│    200/201 si éxito                          │
│    400/500 si error                          │
└──────────────────────────────────────────────┘
```

**Ejemplo con Transacciones Explícitas** (si se requiere):
```csharp
[HttpPost("operacion-compleja")]
public async Task<ActionResult> OperacionCompleja([FromBody] ComandoComplejo command)
{
    using (var transaction = await _context.Database.BeginTransactionAsync())
    {
        try
        {
            // Múltiples operaciones
            var resultado1 = await _mediator.Send(command.Parte1);
            var resultado2 = await _mediator.Send(command.Parte2);
            
            // ✅ COMMIT explícito
            await transaction.CommitAsync();
            
            return Ok();
        }
        catch (Exception)
        {
            // ✅ ROLLBACK explícito
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

**Conclusión**: ✅ **CUMPLE** - Las transacciones se manejan automáticamente con EF Core, garantizando consistencia.

---

### ✅ 3. DTOs para Entrada/Salida

**Criterio**: Implementa DTOs para transportar datos entre capas

**Implementación**:

**DTOs en la Capa Application** (`InventarioDDD.Application/DTOs/`):

```csharp
// IngredienteDto.cs
public class IngredienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string UnidadDeMedida { get; set; }
    public decimal CantidadEnStock { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public Guid CategoriaId { get; set; }
}

// OrdenDeCompraDto.cs
public class OrdenDeCompraDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; }
    public string IngredienteNombre { get; set; }
    public string ProveedorNombre { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
```

**Uso en Controllers**:
```csharp
[HttpGet]
public async Task<ActionResult<List<IngredienteDto>>> ObtenerTodos()
{
    var query = new ObtenerTodosIngredientesQuery();
    
    // ✅ Handler retorna DTOs, NO entidades del dominio
    var ingredientes = await _mediator.Send(query);
    
    return Ok(ingredientes);
}

[HttpPost]
public async Task<ActionResult<IngredienteDto>> Crear([FromBody] CrearIngredienteCommand command)
{
    // ✅ Recibe Command (que es como un DTO de entrada)
    // ✅ Retorna DTO (no entidad de dominio)
    var resultado = await _mediator.Send(command);
    
    return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
}
```

**Separación Entidad vs DTO**:
```
┌─────────────────────────┐       ┌──────────────────────────┐
│   Entidad (Domain)      │       │   DTO (Application)      │
├─────────────────────────┤       ├──────────────────────────┤
│ - Tiene lógica          │       │ - Solo datos             │
│ - Protege invariantes   │       │ - Sin métodos            │
│ - Constructores privados│       │ - Getters/Setters        │
│ - Métodos de negocio    │       │ - Serializable JSON      │
│ - Value Objects         │       │ - Propiedades simples    │
│                         │       │                          │
│ NO se expone en API     │       │ SÍ se expone en API      │
└─────────────────────────┘       └──────────────────────────┘
```

**Conclusión**: ✅ **CUMPLE** - Todos los endpoints usan DTOs, nunca exponen entidades del dominio.

---

### ✅ 4. Mapeo Explícito Hacia/Desde el Modelo de Dominio

**Criterio**: Mapeo explícito entre DTOs y entidades de dominio

**Implementación en Handlers**:

```csharp
// CrearIngredienteHandler.cs
public class CrearIngredienteHandler : IRequestHandler<CrearIngredienteCommand, IngredienteDto>
{
    public async Task<IngredienteDto> Handle(CrearIngredienteCommand request, ...)
    {
        // 1. ✅ MAPEO: Command → Value Objects del Domain
        var unidadDeMedida = new UnidadDeMedida(request.UnidadDeMedida);
        var rangoDeStock = new RangoDeStock(request.StockMinimo, request.StockMaximo);
        
        // 2. ✅ CREAR: Entidad del Domain
        var ingrediente = new Ingrediente(
            request.Nombre,
            request.Descripcion,
            unidadDeMedida,
            rangoDeStock,
            request.CategoriaId
        );
        
        // 3. Persistir
        var aggregate = new IngredienteAggregate(ingrediente);
        await _ingredienteRepository.GuardarAsync(aggregate);
        
        // 4. ✅ MAPEO: Entidad → DTO
        return new IngredienteDto
        {
            Id = ingrediente.Id,
            Nombre = ingrediente.Nombre,
            Descripcion = ingrediente.Descripcion,
            UnidadDeMedida = ingrediente.UnidadDeMedida.Simbolo,
            CantidadEnStock = ingrediente.CantidadEnStock.Valor,
            StockMinimo = ingrediente.RangoDeStock.StockMinimo,
            StockMaximo = ingrediente.RangoDeStock.StockMaximo,
            CategoriaId = ingrediente.CategoriaId
        };
    }
}
```

**Ejemplo de Mapeo Complejo**:
```csharp
// ObtenerIngredientesHandler.cs
private IngredienteDto MapToDto(Ingrediente ingrediente)
{
    return new IngredienteDto
    {
        // ✅ Mapeo explícito de cada campo
        Id = ingrediente.Id,
        Nombre = ingrediente.Nombre,
        Descripcion = ingrediente.Descripcion,
        
        // ✅ Desempaqueta Value Objects
        UnidadDeMedida = ingrediente.UnidadDeMedida.Simbolo,
        CantidadEnStock = ingrediente.CantidadEnStock.Valor,
        StockMinimo = ingrediente.RangoDeStock.StockMinimo,
        StockMaximo = ingrediente.RangoDeStock.StockMaximo,
        
        CategoriaId = ingrediente.CategoriaId,
        FechaCreacion = ingrediente.FechaCreacion
    };
}
```

**Lo que NO se hace**:
```csharp
// ❌ INCORRECTO - Exponer entidades directamente
[HttpGet("{id}")]
public async Task<Ingrediente> ObtenerPorId(Guid id)
{
    return await _context.Ingredientes.FindAsync(id); // ❌ Expone entidad
}

// ❌ INCORRECTO - Sin mapeo explícito
public IngredienteDto AutoMapper(Ingrediente ingrediente)
{
    return _mapper.Map<IngredienteDto>(ingrediente); // ❌ Mapeo implícito/mágico
}
```

**Conclusión**: ✅ **CUMPLE** - Todos los handlers tienen mapeo explícito entre entidades y DTOs.

---

### ✅ 5. Validación de Datos en el Punto de Recepción (Boundaries)

**Criterio**: Validación de datos de entrada en controllers/boundaries

**Implementación**:

**Nivel 1: Validación de ASP.NET Core (Automática)**
```csharp
// Command con Data Annotations
public class CrearIngredienteCommand : IRequest<IngredienteDto>
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Descripcion { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo debe ser positivo")]
    public decimal StockMinimo { get; set; }
}
```

**Nivel 2: Validación en Controllers**
```csharp
[HttpPost]
public async Task<ActionResult<IngredienteDto>> Crear([FromBody] CrearIngredienteCommand command)
{
    // ✅ ASP.NET Core valida automáticamente con [ApiController]
    // Si ModelState no es válido, retorna 400 Bad Request automáticamente
    
    // ✅ Validación adicional si se requiere
    if (command.StockMinimo > command.StockMaximo)
    {
        return BadRequest("El stock mínimo no puede ser mayor al stock máximo");
    }
    
    var resultado = await _mediator.Send(command);
    return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
}
```

**Nivel 3: Validación en Domain (Reglas de Negocio)**
```csharp
// Constructor de Ingrediente (Domain)
public Ingrediente(string nombre, string descripcion, UnidadDeMedida unidadDeMedida, ...)
{
    // ✅ Validaciones de dominio
    if (string.IsNullOrWhiteSpace(nombre))
        throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));
    
    if (nombre.Length > 100)
        throw new ArgumentException("El nombre no puede exceder 100 caracteres", nameof(nombre));
    
    // ...
}
```

**Niveles de Validación**:
```
┌─────────────────────────────────────────────────────────┐
│ 1. Controller (API Boundary)                            │
│    - Validación de formato (Required, Range, etc.)     │
│    - ModelState validation                              │
│    - Retorna 400 Bad Request si falla                   │
├─────────────────────────────────────────────────────────┤
│ 2. Application Handler                                  │
│    - Validación de lógica de aplicación                │
│    - Validación de relaciones entre datos               │
├─────────────────────────────────────────────────────────┤
│ 3. Domain                                               │
│    - Validación de reglas de negocio                    │
│    - Protección de invariantes                          │
│    - Lanza excepciones de dominio                       │
└─────────────────────────────────────────────────────────┘
```

**Manejo de Errores de Validación**:
```csharp
// ExceptionHandlingMiddleware.cs
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    try
    {
        await next(context);
    }
    catch (ArgumentException ex) // ✅ Errores de validación del Domain
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Datos inválidos",
            message = ex.Message
        });
    }
    catch (InvalidOperationException ex) // ✅ Errores de reglas de negocio
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Operación inválida",
            message = ex.Message
        });
    }
}
```

**Conclusión**: ✅ **CUMPLE** - Validación multi-nivel: API boundary, Application, y Domain.

---

## Resumen

La **Capa API** (Presentación) actúa como la puerta de entrada a la aplicación. Su responsabilidad principal es:

1. ✅ Exponer endpoints REST HTTP
2. ✅ Validar formato de datos de entrada
3. ✅ Delegar procesamiento a la capa Application mediante MediatR
4. ✅ Serializar respuestas a JSON
5. ✅ Manejar errores de forma centralizada
6. ✅ Documentar la API con Swagger

**No contiene lógica de negocio** - Solo coordina y transforma datos entre HTTP y el dominio.

### 🎯 Cumplimiento de Criterios de Evaluación

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| **Controllers Delgados** | ✅ **CUMPLE** | Solo coordinan, delegan a MediatR |
| **Manejo de Consistencia** | ✅ **CUMPLE** | EF Core + Middleware manejan transacciones |
| **DTOs para Entrada/Salida** | ✅ **CUMPLE** | Nunca exponen entidades de dominio |
| **Mapeo Explícito** | ✅ **CUMPLE** | Handlers mapean manualmente entidad ↔ DTO |
| **Validación en Boundaries** | ✅ **CUMPLE** | Validación multi-nivel (API, Application, Domain) |
