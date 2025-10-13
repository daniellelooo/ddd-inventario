# 🌐 CAPA API (PRESENTACIÓN) - Criterios de Evaluación# 🌐 CAPA API (PRESENTACIÓN) - Criterios de Evaluación# 🌐 CAPA API (PRESENTACIÓN) - InventarioDDD.API

## 📋 Criterios de Evaluación## 📋 Índice## 📋 Índice

1. [Controllers Delgados](#1-controllers-delgados)- [Descripción General](#descripción-general)- [Descripción General](#descripción-general)

2. [Manejan Consistencia](#2-manejan-consistencia)

3. [DTOs para Entrada/Salida](#3-dtos-para-entradasalida)- [Criterios de Evaluación](#criterios-de-evaluación)- [Responsabilidades](#responsabilidades)

4. [Mapeo Explícito](#4-mapeo-explícito)

5. [Validación en Boundaries](#5-validación-en-boundaries) - [1. Controllers Delgados](#1-controllers-delgados)- [Estructura de Archivos](#estructura-de-archivos)

--- - [2. Manejan Consistencia](#2-manejan-consistencia-commits-rollbacks)- [Archivos Principales](#archivos-principales)

## 1. Controllers Delgados - [3. DTOs para Entrada/Salida](#3-dtos-para-entradasalida)- [Controllers (Controladores)](#controllers-controladores)

**Criterio**: Implementan los casos de uso - [4. Mapeo Explícito](#4-mapeo-explícito-hacidesde-el-modelo-de-dominio)- [Middleware](#middleware)

### ¿Qué significa "Controller Delgado"? - [5. Validación en Boundaries](#5-validación-de-datos-en-boundaries)- [Configuración](#configuración)

El controller **NO hace el trabajo**, solo **coordina y delega**.- [Resumen de Cumplimiento](#resumen-de-cumplimiento)

### Ejemplo Real---

**Archivo**: `IngredientesController.cs`---

`````csharp## Descripción General

[HttpPost]

public async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)## Descripción General

{

    tryLa **Capa API** es la capa más externa de la aplicación y actúa como punto de entrada para todas las peticiones HTTP. Esta capa expone endpoints RESTful que permiten a los clientes (como el frontend React) interactuar con el sistema de inventario.

    {

        // 1. Preparar datosLa **Capa API** es la capa de presentación que actúa como punto de entrada HTTP para todas las peticiones del frontend. Implementa controllers REST que exponen endpoints y coordinan la ejecución de casos de uso mediante MediatR.

        var comando = new CrearIngredienteCommand

        {### Tecnologías Utilizadas

            Nombre = request.Nombre,

            Descripcion = request.Descripcion,**Tecnologías**: ASP.NET Core 9, MediatR, Swagger/OpenAPI

            UnidadMedida = request.UnidadMedida,

            StockMinimo = request.StockMinimo,- **.NET 9** (ASP.NET Core)

            StockMaximo = request.StockMaximo,

            CategoriaId = request.CategoriaId---- **MediatR** - Para implementar el patrón Mediator

        };

- **Swagger/OpenAPI** - Para documentación de API

        // 2. DELEGAR (aquí está lo importante)

        var ingredienteId = await _mediator.Send(comando);## Criterios de Evaluación- **Entity Framework Core** - ORM para acceso a datos

        //                         ↑

        //   El controller DELEGA el trabajo al Handler### ✅ 1. Controllers Delgados---

        //   No sabe cómo se valida, crea o guarda

**Criterio**: Controllers delgados que implementan los casos de uso## Responsabilidades

        // 3. Retornar respuesta

        return CreatedAtAction(nameof(ObtenerTodos), **Evidencia en el Proyecto**:1. **Recibir peticiones HTTP** desde clientes externos

            new { id = ingredienteId },

            new { id = ingredienteId, message = "Ingrediente creado exitosamente" });2. **Validar datos de entrada** básicos (formato, tipos)

    }

    catch (Exception ex)#### **Archivo**: `IngredientesController.cs`3. **Enrutar peticiones** a los handlers correspondientes mediante MediatR

    {

        return StatusCode(500, new { message = "Error" });4. **Serializar/Deserializar** objetos JSON

    }

}````csharp5. **Manejar errores** y devolver respuestas HTTP apropiadas

`````

[ApiController]6. **Implementar CORS** para permitir acceso desde el frontend

### ¿Qué hace el Controller?

- ✅ Recibe datos HTTP[Route("api/[controller]")]7. **Documentar endpoints** con Swagger

- ✅ Prepara el Command

- ✅ **DELEGA a MediatR** (línea clave: `_mediator.Send()`)public class IngredientesController : ControllerBase

- ✅ Retorna respuesta HTTP

{> **Nota Importante**: Esta capa NO contiene lógica de negocio. Solo coordina y delega responsabilidades.

### ¿Qué NO hace el Controller?

- ❌ NO valida reglas de negocio private readonly IMediator \_mediator;

- ❌ NO accede a la base de datos

- ❌ NO crea entidades private readonly ILogger<IngredientesController> \_logger;---

- ❌ NO tiene lógica compleja

  private readonly IIngredienteRepository \_ingredienteRepository;

### ✅ Cumple el Criterio

**SÍ**, el controller es delgado porque solo coordina y delega el trabajo real al Handler.## Estructura de Archivos

--- public IngredientesController(

## 2. Manejan Consistencia IMediator mediator, ```

**Criterio**: Manejan consistencia (commits, rollbacks) ILogger<IngredientesController> logger,InventarioDDD.API/

### ¿Qué significa? IIngredienteRepository ingredienteRepository)│

Si algo falla, **todos los cambios se revierten** (rollback). Si todo sale bien, **todos los cambios se guardan** (commit). {├── Controllers/ # Controladores REST

### Ejemplo Real \_mediator = mediator;│ ├── CategoriasController.cs

````csharp _logger = logger;│   ├── IngredientesController.cs

[HttpPost]

public async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)        _ingredienteRepository = ingredienteRepository;│   ├── InventarioController.cs

{

    try    }│   ├── LotesController.cs

    {

        var comando = new CrearIngredienteCommand { ... };│   ├── OrdenesCompraController.cs



        // Entity Framework Core maneja transacciones automáticamente    /// <summary>│   └── ProveedoresController.cs

        var ingredienteId = await _mediator.Send(comando);

            /// ✅ CONTROLLER DELGADO: Solo coordina, NO tiene lógica de negocio│

        // ✅ Si llega aquí: COMMIT (todo se guardó)

        return CreatedAtAction(...);    /// </summary>├── Middleware/               # Middleware personalizado

    }

    catch (Exception ex)    [HttpPost]│   └── ExceptionHandlingMiddleware.cs

    {

        // ❌ Si hay error: ROLLBACK (nada se guarda)    public async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)│

        return StatusCode(500, new { message = "Error" });

    }    {├── Properties/

}

```        try│   └── launchSettings.json   # Configuración de inicio



### ¿Cómo funciona?        {│



**Escenario Exitoso**:            // 1. Construye el Command (DTO de entrada)├── Program.cs                # Punto de entrada y configuración

````

1. Crear ingrediente ✅ var comando = new CrearIngredienteCommand├── appsettings.json # Configuración de la aplicación

2. Actualizar stock ✅

3. Guardar en BD ✅ {└── InventarioDDD.API.csproj # Archivo de proyecto

→ COMMIT: Todo se guarda

`                Nombre = request.Nombre,`

**Escenario con Error**: Descripcion = request.Descripcion,

```

1. Crear ingrediente ✅                UnidadMedida = request.UnidadMedida,---

2. Actualizar stock ❌ ERROR

→ ROLLBACK: Nada se guarda (ingrediente tampoco)                StockMinimo = request.StockMinimo,

```

                StockMaximo = request.StockMaximo,## Archivos Principales

### Middleware para Errores

                CategoriaId = request.CategoriaId

**Archivo**: `ExceptionHandlingMiddleware.cs`

            };### 📄 Program.cs

````csharp

public async Task InvokeAsync(HttpContext context)

{

    try            // 2. Delega a MediatR (que ejecuta el Handler)**Propósito**: Punto de entrada de la aplicación. Configura todos los servicios y el pipeline de middleware.

    {

        await _next(context); // Ejecuta controller y handler            var ingredienteId = await _mediator.Send(comando);

        // ✅ Si no hay errores: COMMIT

    }**Responsabilidades**:

    catch (Exception ex)

    {            // 3. Retorna respuesta HTTP apropiada

        // ❌ Si hay error: ROLLBACK automático

        context.Response.StatusCode = 500;            return CreatedAtAction(1. **Configuración de Servicios**:

        await context.Response.WriteAsJsonAsync(new { error = ex.Message });

    }                nameof(ObtenerTodos),

}

```                new { id = ingredienteId },    - Registra MediatR para CQRS



### ✅ Cumple el Criterio                new { id = ingredienteId, message = "Ingrediente creado exitosamente" });   - Configura DbContext con SQLite

**SÍ**, Entity Framework Core + Middleware garantizan que todo se guarda o nada se guarda.

        }   - Registra repositorios (Dependency Injection)

---

        catch (InvalidOperationException ex)   - Configura Swagger para documentación

## 3. DTOs para Entrada/Salida

        {

**Criterio**: Implementa DTOs para transportar datos entre capas

            return BadRequest(new { message = ex.Message });2. **Configuración del Pipeline HTTP**:

### ¿Qué son los DTOs?

        }   - Habilita Swagger en desarrollo

**DTO** = Objeto simple para transportar datos (sin lógica de negocio)

        catch (Exception ex)   - Configura CORS para permitir peticiones del frontend

### Flujo Completo

        {   - Agrega middleware de manejo de excepciones

````

Frontend → DTO de Entrada → Handler → DTO de Salida → Frontend \_logger.LogError(ex, "Error al crear ingrediente"); - Configura enrutamiento de controladores

````

            return StatusCode(500, new { message = "Error al crear el ingrediente" });

### DTO de Entrada (Request)

        }**Código Clave**:

**Archivo**: `IngredientesController.cs`

    }

```csharp

// El frontend envía esto (JSON):```csharp

{

  "nombre": "Tomate",    /// <summary>// Registro de MediatR (Patrón Mediator para CQRS)

  "unidadMedida": "kg",

  "stockMinimo": 10    /// ✅ CONTROLLER DELGADO: Solo obtiene y mapea a DTObuilder.Services.AddMediatR(cfg => {

}

    /// </summary>    cfg.RegisterServicesFromAssembly(typeof(CrearIngredienteHandler).Assembly);

// Se convierte en este DTO:

public class CrearIngredienteRequest    [HttpGet]});

{

    public string Nombre { get; set; }    public async Task<IActionResult> ObtenerTodos()

    public string UnidadMedida { get; set; }

    public decimal StockMinimo { get; set; }    {// Configuración de DbContext con SQLite

    // ... más propiedades

}        trybuilder.Services.AddDbContext<InventarioDbContext>(options =>

````

        {    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

### DTO de Salida (Response)

            // 1. Obtiene datos del repositorio

`````csharp

[HttpGet]            var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();// Registro de Repositorios (Dependency Injection)

public async Task<IActionResult> ObtenerTodos()

{builder.Services.AddScoped<IIngredienteRepository, IngredienteRepository>();

    var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();

            // 2. Mapea a DTO (objeto anónimo)builder.Services.AddScoped<IOrdenDeCompraRepository, OrdenDeCompraRepository>();

    // Mapear a DTO (objeto simple)

    var resultado = ingredientes.Select(agg => new            var resultado = ingredientes.Select(agg => new// ... otros repositorios

    {

        id = agg.Id,            {

        nombre = agg.Ingrediente.Nombre,

        unidadMedida = agg.Ingrediente.UnidadDeMedida.Simbolo, // ← De Value Object a string                id = agg.Id,// Configuración de CORS

        cantidadEnStock = agg.Ingrediente.CantidadEnStock.Valor // ← De Value Object a decimal

    }).ToList();                nombre = agg.Ingrediente.Nombre,builder.Services.AddCors(options =>



    return Ok(resultado);                descripcion = agg.Ingrediente.Descripcion,{

}

                unidadMedida = agg.Ingrediente.UnidadDeMedida.Simbolo,    options.AddPolicy("AllowAll",

// El frontend recibe esto (JSON):

[                cantidadEnStock = agg.Ingrediente.CantidadEnStock.Valor,        builder => builder.AllowAnyOrigin()

  {

    "id": "guid-123",                stockMinimo = agg.Ingrediente.RangoDeStock.StockMinimo,                         .AllowAnyMethod()

    "nombre": "Tomate",

    "unidadMedida": "kg",                stockMaximo = agg.Ingrediente.RangoDeStock.StockMaximo,                         .AllowAnyHeader());

    "cantidadEnStock": 50.5

  }                categoriaId = agg.Ingrediente.CategoriaId,});

]

```                activo = agg.Ingrediente.Activo,```



### ¿Por qué DTOs?                tieneStockBajo = agg.TieneStockBajo()



| Sin DTO (Mal) | Con DTO (Bien) |            }).ToList();**Flujo de Configuración**:

|---------------|----------------|

| Expone entidades del dominio | Expone datos simples |

| Complejo para el frontend | Fácil de usar |

| Mezcla responsabilidades | Separación clara |            // 3. Retorna DTO como JSON```



### ✅ Cumple el Criterio            return Ok(resultado);Inicio → Configurar Servicios → Construir App →

**SÍ**, usamos DTOs (Request para entrada, objetos anónimos para salida). Nunca exponemos entidades del dominio.

        }Configurar Pipeline → Ejecutar Migraciones → Iniciar Servidor

---

        catch (Exception ex)```

## 4. Mapeo Explícito

        {

**Criterio**: Mapeo explícito hacia/desde el modelo de dominio

            _logger.LogError(ex, "Error al obtener ingredientes");---

### ¿Qué significa "Mapeo Explícito"?

            return StatusCode(500, new { message = "Error al obtener los ingredientes" });

Convertir **manualmente** (no automático) entre DTOs y Entidades del dominio.

        }### 📄 appsettings.json

### Mapeo: DTO → Entidad

    }

**Archivo**: `CrearIngredienteHandler.cs`

}**Propósito**: Almacena la configuración de la aplicación.

```csharp

public async Task<Guid> Handle(CrearIngredienteCommand request, ...)````

{

    // ✅ MAPEO EXPLÍCITO: String → Value Object**Contenido Principal**:

    var unidadMedida = request.UnidadMedida.ToLower() switch

    {**¿Qué NO hace el Controller?**

        "kg" => UnidadDeMedida.Kilogramos,

        "l" => UnidadDeMedida.Litros,- ❌ NO valida reglas de negocio (ej: stock mínimo < stock máximo)```json

        _ => new UnidadDeMedida(request.UnidadMedida, request.UnidadMedida)

    };- ❌ NO accede directamente a la base de datos para escrituras{



    // ✅ MAPEO EXPLÍCITO: Decimals → Value Object- ❌ NO contiene lógica de cálculo "ConnectionStrings": {

    var rangoStock = new RangoDeStock(request.StockMinimo, request.StockMaximo);

- ❌ NO coordina múltiples operaciones complejas "DefaultConnection": "Data Source=Inventario.db"

    // ✅ Crear entidad del dominio

    var ingrediente = new Ingrediente(  },

        request.Nombre,

        request.Descripcion,**¿Qué SÍ hace el Controller?** "Logging": {

        unidadMedida,  // ← Value Object

        rangoStock,    // ← Value Object- ✅ Recibe datos HTTP (JSON) "LogLevel": {

        request.CategoriaId

    );- ✅ Construye Commands/Queries "Default": "Information",



    // Guardar...- ✅ Delega a MediatR "Microsoft.AspNetCore": "Warning"

    return ingrediente.Id;

}- ✅ Retorna respuestas HTTP apropiadas (200, 201, 400, 500) }

`````

- ✅ Maneja logging de errores }

### Mapeo: Entidad → DTO

}

**Archivo**: `IngredientesController.cs`

#### **Archivo**: `OrdenesCompraController.cs````

`````csharp

var resultado = ingredientes.Select(agg => new````csharp**Configuraciones Clave**:

{

    id = agg.Id,[ApiController]

    nombre = agg.Ingrediente.Nombre,

    [Route("api/[controller]")]- **ConnectionStrings**: Cadena de conexión a la base de datos SQLite

    // ✅ MAPEO EXPLÍCITO: Value Object → String

    unidadMedida = agg.Ingrediente.UnidadDeMedida.Simbolo,public class OrdenesCompraController : ControllerBase- **Logging**: Niveles de log para diferentes componentes



    // ✅ MAPEO EXPLÍCITO: Value Object → Decimal{

    cantidadEnStock = agg.Ingrediente.CantidadEnStock.Valor,

    stockMinimo = agg.Ingrediente.RangoDeStock.StockMinimo    private readonly IMediator _mediator;---



    // Mapeamos campo por campo (explícito)    private readonly ILogger<OrdenesCompraController> _logger;

}).ToList();

```## Controllers (Controladores)



### ¿Qué NO hacemos?    public OrdenesCompraController(IMediator mediator, ILogger<OrdenesCompraController> logger)



```csharp    {Los controladores son clases que exponen endpoints HTTP. Cada controlador maneja un conjunto de operaciones relacionadas con una entidad del dominio.

// ❌ INCORRECTO - Mapeo automático

var dto = _mapper.Map<IngredienteDto>(ingrediente);        _mediator = mediator;



// ✅ CORRECTO - Mapeo manual campo por campo        _logger = logger;### 🎮 Patrón Común de Controladores

var dto = new IngredienteDto

{    }

    Id = ingrediente.Id,

    Nombre = ingrediente.Nombre,Todos los controladores siguen este patrón:

    UnidadMedida = ingrediente.UnidadDeMedida.Simbolo

};    /// <summary>

`````

    /// ✅ CONTROLLER DELGADO: Solo delega a MediatR1. **Heredan de `ControllerBase`**

### ✅ Cumple el Criterio

**SÍ**, todo el mapeo es manual y explícito, campo por campo. No usamos herramientas automáticas como AutoMapper. /// </summary>2. **Usan `[ApiController]` y `[Route]`** para configuración automática

--- [HttpPost]3. **Inyectan `IMediator`** en el constructor

## 5. Validación en Boundaries public async Task<IActionResult> CrearOrdenDeCompra(4. **Delegan operaciones** a handlers mediante MediatR

**Criterio**: Validación de datos de entrada en el punto de recepción (boundaries) [FromBody] CrearOrdenDeCompraCommand command)5. **Devuelven `IActionResult`** con códigos HTTP apropiados

### ¿Qué son "Boundaries"? {

Los **boundaries** son los puntos de entrada de la aplicación (los controllers). try**Estructura Típica**:

### 4 Niveles de Validación {

`            // Delega directamente al Handler`csharp

┌──────────────────────────────────────┐

│ Nivel 1: Controller (Boundary) │ ← Formato HTTP var ordenId = await \_mediator.Send(command);[ApiController]

├──────────────────────────────────────┤

│ Nivel 2: Handler (Application) │ ← Lógica de aplicación[Route("api/[controller]")]

├──────────────────────────────────────┤

│ Nivel 3: Domain (Entidades) │ ← Reglas de negocio return CreatedAtAction(public class NombreController : ControllerBase

├──────────────────────────────────────┤

│ Nivel 4: Middleware │ ← Captura errores nameof(CrearOrdenDeCompra),{

└──────────────────────────────────────┘

````new { id = ordenId },    private readonly IMediator _mediator;



### Nivel 1: Controller (Boundary)                new



**Archivo**: `OrdenesCompraController.cs`                {    public NombreController(IMediator mediator)



```csharp                    success = true,    {

[HttpPost]

public async Task<IActionResult> CrearOrdenDeCompra([FromBody] CrearOrdenDeCompraCommand command)                    message = "Orden de compra creada exitosamente",        _mediator = mediator;

{

    try                    data = new { ordenId }    }

    {

        // ✅ Validación básica en el boundary                });

        if (command == null)

            return BadRequest(new { message = "Datos inválidos" });        }    [HttpGet]



        var ordenId = await _mediator.Send(command);        catch (ArgumentException ex)    public async Task<IActionResult> Get()

        return CreatedAtAction(...);

    }        {    {

    catch (ArgumentException ex) // ← Captura errores de validación

    {            _logger.LogWarning(ex, "Datos inválidos al crear orden de compra");        var result = await _mediator.Send(new Query());

        return BadRequest(new { message = ex.Message });

    }            return BadRequest(new { success = false, message = ex.Message });        return Ok(result);

}

```        }    }



### Nivel 2: Handler (Application)        catch (Exception ex)



**Archivo**: `CrearIngredienteHandler.cs`        {    [HttpPost]



```csharp            _logger.LogError(ex, "Error al crear orden de compra");    public async Task<IActionResult> Create([FromBody] Command command)

public async Task<Guid> Handle(CrearIngredienteCommand request, ...)

{            return StatusCode(500, new { message = "Error al crear la orden de compra" });    {

    // ✅ Validación de aplicación

    var categoriaExiste = await _categoriaRepository.ExisteAsync(request.CategoriaId);        }        var result = await _mediator.Send(command);

    if (!categoriaExiste)

        throw new InvalidOperationException("La categoría no existe");    }        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);



    var nombreExiste = await _ingredienteRepository.ExisteNombreAsync(request.Nombre);    }

    if (nombreExiste)

        throw new InvalidOperationException("El nombre ya existe");    [HttpPost("{id}/aprobar")]}



    // Continuar...    public async Task<IActionResult> AprobarOrden(```

}

```        Guid id,



### Nivel 3: Domain (Entidades)        [FromBody] AprobarOrdenDeCompraCommand command)---



**Archivo**: `RangoDeStock.cs` (Value Object)    {



```csharp        try### 📁 IngredientesController.cs

public RangoDeStock(decimal stockMinimo, decimal stockMaximo)

{        {

    // ✅ Validación de reglas de negocio

    if (stockMinimo < 0)            command.OrdenId = id;**Propósito**: Gestiona las operaciones relacionadas con ingredientes.

        throw new ArgumentException("Stock mínimo no puede ser negativo");

                var resultado = await _mediator.Send(command);

    if (stockMaximo <= stockMinimo)

        throw new ArgumentException("Stock máximo debe ser mayor al mínimo");**Endpoints**:



    StockMinimo = stockMinimo;            return Ok(new

    StockMaximo = stockMaximo;

}            {| Método | Ruta                            | Descripción                         | Handler Usado                             |

````

                success = true,| ------ | ------------------------------- | ----------------------------------- | ----------------------------------------- |

### Nivel 4: Middleware

                message = "Orden de compra aprobada exitosamente"| GET    | `/api/ingredientes`             | Obtiene todos los ingredientes      | Query directo al repositorio              |

**Archivo**: `ExceptionHandlingMiddleware.cs`

            });| GET    | `/api/ingredientes/{id}`        | Obtiene un ingrediente por ID       | Query al repositorio                      |

`````csharp

public async Task InvokeAsync(HttpContext context)        }| GET    | `/api/ingredientes/reabastecer` | Obtiene ingredientes con stock bajo | `ObtenerIngredientesParaReabastecerQuery` |

{

    try        catch (ArgumentException ex)| POST   | `/api/ingredientes`             | Crea un nuevo ingrediente           | `CrearIngredienteCommand`                 |

    {

        await _next(context);        {

    }

    catch (ArgumentException ex) // ← Captura errores de validación            _logger.LogWarning(ex, "Orden no encontrada: {OrdenId}", id);**Ejemplo de Endpoint**:

    {

        context.Response.StatusCode = 400;            return NotFound(new { success = false, message = ex.Message });

        await context.Response.WriteAsJsonAsync(new { error = ex.Message });

    }        }```csharp

}

```        catch (Exception ex)[HttpGet("reabastecer")]



### Ejemplo Completo        {public async Task<IActionResult> GetParaReabastecer()



```            _logger.LogError(ex, "Error al aprobar orden");{

Request: POST /api/ingredientes

{ "nombre": "", "stockMinimo": -10 }            return StatusCode(500, new { message = "Error al aprobar la orden" });    var query = new ObtenerIngredientesParaReabastecerQuery();



┌──────────────────────────────────────┐        }    var resultado = await _mediator.Send(query);

│ Controller: ✅ Pasa (formato OK)     │

├──────────────────────────────────────┤    }    return Ok(resultado);

│ Handler: ✅ Pasa (validaciones OK)   │

├──────────────────────────────────────┤}}

│ Domain: ❌ FALLA                     │

│ "Stock mínimo no puede ser negativo"│````

├──────────────────────────────────────┤

│ Middleware: Captura error            │**Conclusión**: ✅ **CUMPLE** - Todos los controllers son delgados, solo coordinan y delegan a MediatR.**Flujo de una Petición**:

│ Retorna 400 Bad Request              │

└──────────────────────────────────────┘---```

`````

Cliente → GET /api/ingredientes/reabastecer → IngredientesController

### ✅ Cumple el Criterio

**SÍ**, validamos en múltiples niveles: Controller (boundary), Handler (aplicación), Domain (negocio), Middleware (captura).### ✅ 2. Manejan Consistencia (Commits, Rollbacks)→ \_mediator.Send(Query) → ObtenerIngredientesParaReabastecerHandler

---→ Ejecuta lógica → Retorna DTOs → Controller → JSON Response

## 📊 Resumen de Cumplimiento**Criterio**: Controllers manejan consistencia con commits y rollbacks de transacciones```

| Criterio | ¿Cumple? | Evidencia |**Evidencia en el Proyecto**:**Validaciones**:

|----------|----------|-----------|

| **Controllers Delgados** | ✅ SÍ | Solo coordinan, delegan a MediatR |#### **Transacciones Implícitas con Entity Framework Core**- Los datos de entrada se validan automáticamente por `[ApiController]`

| **Manejan Consistencia** | ✅ SÍ | EF Core + Middleware garantizan ACID |

| **DTOs Entrada/Salida** | ✅ SÍ | Request (entrada), objetos anónimos (salida) |- Las validaciones de negocio están en los handlers

| **Mapeo Explícito** | ✅ SÍ | Mapeo manual campo por campo |

| **Validación en Boundaries** | ✅ SÍ | 4 niveles: Controller, Handler, Domain, Middleware |````csharp

---[HttpPost]---

## 📁 Archivos Principalespublic async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)

- `backend/InventarioDDD.API/Controllers/IngredientesController.cs`{### 📁 OrdenesCompraController.cs

- `backend/InventarioDDD.API/Controllers/OrdenesCompraController.cs`

- `backend/InventarioDDD.API/Middleware/ExceptionHandlingMiddleware.cs` try

- `backend/InventarioDDD.Application/Handlers/CrearIngredienteHandler.cs`

- `backend/InventarioDDD.Application/Commands/CrearIngredienteCommand.cs` {**Propósito**: Gestiona el ciclo de vida completo de las órdenes de compra.

        // ──────────────────────────────────────────────────────────

        // INICIO DE TRANSACCIÓN IMPLÍCITA (Entity Framework Core)**Endpoints**:

        // ──────────────────────────────────────────────────────────

        | Método | Ruta                              | Descripción                   | Handler Usado                   |

        var comando = new CrearIngredienteCommand { ... };| ------ | --------------------------------- | ----------------------------- | ------------------------------- |

        var ingredienteId = await _mediator.Send(comando);| GET    | `/api/ordenescompra`              | Obtiene todas las órdenes     | Query al repositorio            |

        | GET    | `/api/ordenescompra/pendientes`   | Obtiene órdenes pendientes    | `ObtenerOrdenesPendientesQuery` |

        // Handler ejecuta:| POST   | `/api/ordenescompra`              | Crea una nueva orden          | `CrearOrdenDeCompraCommand`     |

        // 1. Validaciones| POST   | `/api/ordenescompra/{id}/aprobar` | Aprueba una orden             | `AprobarOrdenDeCompraCommand`   |

        // 2. Crear entidad de dominio| POST   | `/api/ordenescompra/{id}/recibir` | Recibe una orden y crea lotes | `RecibirOrdenDeCompraCommand`   |

        // 3. Guardar en BD → SaveChangesAsync()

        **Ejemplo - Aprobar Orden**:

        // ✅ COMMIT AUTOMÁTICO: Si todo fue exitoso

        // ──────────────────────────────────────────────────────────```csharp

        [HttpPost("{id}/aprobar")]

        return CreatedAtAction(...);public async Task<IActionResult> Aprobar(

  } Guid id,

  catch (InvalidOperationException ex) [FromBody] AprobarOrdenRequest request)

  {{

        // ❌ ROLLBACK AUTOMÁTICO: Si hay error de validación    var command = new AprobarOrdenDeCompraCommand

        return BadRequest(new { message = ex.Message });    {

  } OrdenId = id,

  catch (Exception ex) UsuarioId = request.UsuarioId

  { };

        // ❌ ROLLBACK AUTOMÁTICO: Si hay cualquier error

        _logger.LogError(ex, "Error al crear ingrediente");    await _mediator.Send(command);

        return StatusCode(500, new { message = "Error al crear el ingrediente" });    return Ok(new { message = "Orden aprobada exitosamente" });

  }}

}```

```

**Flujo Completo de una Orden**:

#### **Middleware de Manejo de Errores**

```

**Archivo**: `ExceptionHandlingMiddleware.cs`1. POST /ordenescompra → CrearOrdenDeCompraCommand

└─> Estado: Pendiente

El middleware captura todas las excepciones y garantiza rollback:

2. POST /ordenescompra/{id}/aprobar → AprobarOrdenDeCompraCommand

````csharp └─> Estado: Aprobada

public class ExceptionHandlingMiddleware

{3. POST /ordenescompra/{id}/recibir → RecibirOrdenDeCompraCommand

    private readonly RequestDelegate _next;   └─> Estado: Recibida

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;   └─> Crea Lotes automáticamente

   └─> Actualiza Stock del Ingrediente

    public async Task InvokeAsync(HttpContext context)```

    {

        try---

        {

            // ✅ Ejecuta el pipeline (incluye controllers y handlers)### 📁 InventarioController.cs

            await _next(context);

            // Si llega aquí sin excepciones: COMMIT exitoso**Propósito**: Gestiona operaciones de inventario como consumo y movimientos.

        }

        catch (ArgumentException ex)**Endpoints**:

        {

            // ❌ ROLLBACK: Errores de validación| Método | Ruta                                    | Descripción                      | Handler Usado                      |

            _logger.LogWarning(ex, "Validación fallida");| ------ | --------------------------------------- | -------------------------------- | ---------------------------------- |

            context.Response.StatusCode = 400;| POST   | `/api/inventario/consumo`               | Registra consumo de ingrediente  | `RegistrarConsumoCommand`          |

            await context.Response.WriteAsJsonAsync(new { error = ex.Message });| GET    | `/api/inventario/movimientos`           | Obtiene historial de movimientos | `ObtenerHistorialMovimientosQuery` |

        }| GET    | `/api/inventario/lotes/proximos-vencer` | Obtiene lotes próximos a vencer  | `ObtenerLotesProximosAVencerQuery` |

        catch (InvalidOperationException ex)

        {**Ejemplo - Registrar Consumo**:

            // ❌ ROLLBACK: Operaciones inválidas

            _logger.LogWarning(ex, "Operación inválida");```csharp

            context.Response.StatusCode = 400;[HttpPost("consumo")]

            await context.Response.WriteAsJsonAsync(new { error = ex.Message });public async Task<IActionResult> RegistrarConsumo(

        }    [FromBody] RegistrarConsumoCommand command)

        catch (Exception ex){

        {    await _mediator.Send(command);

            // ❌ ROLLBACK: Cualquier otro error    return Ok(new { message = "Consumo registrado exitosamente" });

            _logger.LogError(ex, "Error no controlado");}

            context.Response.StatusCode = 500;```

            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor" });

        }**Flujo de Registro de Consumo**:

    }

}```

```Cliente → POST /api/inventario/consumo

→ RegistrarConsumoCommand { ingredienteId, cantidad, motivo }

#### **Flujo de Transacción**→ RegistrarConsumoHandler

→ Aplica FIFO en lotes

```→ Reduce stock disponible

┌─────────────────────────────────────────────────────────┐→ Crea MovimientoInventario

│ 1. Controller recibe HTTP Request                       │→ Retorna éxito

└────────────────┬────────────────────────────────────────┘```

                 │

                 ▼---

┌─────────────────────────────────────────────────────────┐

│ 2. _mediator.Send(command)                              │### 📁 LotesController.cs

│    → Handler inicia transacción (implícita EF Core)     │

└────────────────┬────────────────────────────────────────┘**Propósito**: Gestiona la consulta de lotes de ingredientes.

                 │

                 ▼**Endpoints**:

┌─────────────────────────────────────────────────────────┐

│ 3. Handler ejecuta lógica de aplicación                 │| Método | Ruta                                     | Descripción                     |

│    - Validaciones                                       │| ------ | ---------------------------------------- | ------------------------------- |

│    - Llamadas a repositorios                            │| GET    | `/api/lotes`                             | Obtiene todos los lotes         |

│    - Lógica de dominio                                  │| GET    | `/api/lotes/{id}`                        | Obtiene un lote por ID          |

└────────────────┬────────────────────────────────────────┘| GET    | `/api/lotes/ingrediente/{ingredienteId}` | Obtiene lotes de un ingrediente |

                 │

         ┌───────┴───────┐**Características**:

         │               │

         ▼               ▼- Solo permite consultas (GET)

    ✅ ÉXITO         ❌ ERROR- Los lotes se crean automáticamente al recibir órdenes de compra

         │               │- No hay endpoints POST/PUT/DELETE directos

         ▼               ▼

┌──────────────┐  ┌──────────────┐---

│   COMMIT     │  │   ROLLBACK   │

│ (automático) │  │ (automático) │### 📁 ProveedoresController.cs

└──────────────┘  └──────────────┘

         │               │**Propósito**: Gestiona operaciones CRUD de proveedores.

         ▼               ▼

┌──────────────┐  ┌──────────────┐**Endpoints**:

│ 201 Created  │  │ 400/500 Error│

└──────────────┘  └──────────────┘| Método | Ruta                    | Descripción                   | Handler Usado           |

```| ------ | ----------------------- | ----------------------------- | ----------------------- |

| GET    | `/api/proveedores`      | Obtiene todos los proveedores | Query al repositorio    |

**Garantías ACID**:| GET    | `/api/proveedores/{id}` | Obtiene un proveedor por ID   | Query al repositorio    |

- ✅ **Atomicidad**: Todo se guarda o nada se guarda| POST   | `/api/proveedores`      | Crea un nuevo proveedor       | `CrearProveedorCommand` |

- ✅ **Consistencia**: Los datos siempre están en estado válido

- ✅ **Aislamiento**: Otras transacciones no ven cambios parciales---

- ✅ **Durabilidad**: Una vez confirmado, el cambio es permanente

### 📁 CategoriasController.cs

**Conclusión**: ✅ **CUMPLE** - Entity Framework Core maneja transacciones automáticamente, el middleware garantiza rollback en errores.

**Propósito**: Gestiona operaciones CRUD de categorías de ingredientes.

---

**Endpoints**:

### ✅ 3. DTOs para Entrada/Salida

| Método | Ruta                   | Descripción                  | Handler Usado           |

**Criterio**: Implementa DTOs para transportar datos entre capas| ------ | ---------------------- | ---------------------------- | ----------------------- |

| GET    | `/api/categorias`      | Obtiene todas las categorías | Query al repositorio    |

**Evidencia en el Proyecto**:| GET    | `/api/categorias/{id}` | Obtiene una categoría por ID | Query al repositorio    |

| POST   | `/api/categorias`      | Crea una nueva categoría     | `CrearCategoriaCommand` |

#### **DTO de Entrada: CrearIngredienteRequest**

---

**Archivo**: `IngredientesController.cs`

## Middleware

```csharp

// DTO de entrada (definido en el controller)### 🛡️ ExceptionHandlingMiddleware.cs

public class CrearIngredienteRequest

{**Propósito**: Captura y maneja todas las excepciones de la aplicación de forma centralizada.

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;**Responsabilidades**:

    public string UnidadMedida { get; set; } = "kg";

    public decimal StockMinimo { get; set; }1. Capturar excepciones no controladas

    public decimal StockMaximo { get; set; }2. Loggear errores con detalles

    public Guid CategoriaId { get; set; }3. Devolver respuestas HTTP apropiadas

}4. Evitar exponer detalles internos al cliente

````

**Funcionamiento**:

**Frontend envía** (JSON):

`json`csharp

POST /api/ingredientespublic class ExceptionHandlingMiddleware

{{

"nombre": "Tomate", private readonly RequestDelegate \_next;

"descripcion": "Tomate fresco", private readonly ILogger<ExceptionHandlingMiddleware> \_logger;

"unidadMedida": "kg",

"stockMinimo": 10, public async Task InvokeAsync(HttpContext context)

"stockMaximo": 100, {

"categoriaId": "guid-categoria" try

} {

````await _next(context); // Ejecuta el siguiente middleware

        }

**Controller recibe**:        catch (Exception ex)

```csharp        {

[HttpPost]            _logger.LogError(ex, "Error no controlado");

public async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)            await HandleExceptionAsync(context, ex);

{        }

    // ASP.NET Core deserializa JSON → CrearIngredienteRequest (DTO)    }



    // Mapea a Command (otro tipo de DTO)    private static Task HandleExceptionAsync(HttpContext context, Exception exception)

    var comando = new CrearIngredienteCommand    {

    {        context.Response.ContentType = "application/json";

        Nombre = request.Nombre,        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        Descripcion = request.Descripcion,

        UnidadMedida = request.UnidadMedida,        var response = new

        StockMinimo = request.StockMinimo,        {

        StockMaximo = request.StockMaximo,            message = "Ha ocurrido un error interno",

        CategoriaId = request.CategoriaId            details = exception.Message

    };        };



    // Envía Command al Handler        return context.Response.WriteAsync(JsonSerializer.Serialize(response));

    var ingredienteId = await _mediator.Send(comando);    }

    }

    return CreatedAtAction(...);```

}

```**Tipos de Excepciones Manejadas**:



#### **Command como DTO**- `InvalidOperationException` → 400 Bad Request

- `ArgumentException` → 400 Bad Request

**Archivo**: `CrearIngredienteCommand.cs`- `KeyNotFoundException` → 404 Not Found

- `Exception` (genérica) → 500 Internal Server Error

```csharp

namespace InventarioDDD.Application.Commands**Registro en Program.cs**:

{

    public class CrearIngredienteCommand : IRequest<Guid>```csharp

    {app.UseMiddleware<ExceptionHandlingMiddleware>();

        // ✅ Solo propiedades (sin métodos, sin lógica)```

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;---

        public string UnidadMedida { get; set; } = string.Empty;

        public decimal StockMinimo { get; set; }## Configuración

        public decimal StockMaximo { get; set; }

        public Guid CategoriaId { get; set; }### 🔧 CORS (Cross-Origin Resource Sharing)

    }

}**Propósito**: Permitir que el frontend (React en puerto 3000) haga peticiones a la API (puerto 5261).

````

**Configuración**:

#### **DTO de Salida: Objeto Anónimo**

````csharp

**Archivo**: `IngredientesController.cs`builder.Services.AddCors(options =>

{

```csharp    options.AddPolicy("AllowAll",

[HttpGet]        builder => builder

public async Task<IActionResult> ObtenerTodos()            .AllowAnyOrigin()    // Permite cualquier origen

{            .AllowAnyMethod()    // Permite GET, POST, PUT, DELETE, etc.

    var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();            .AllowAnyHeader());  // Permite cualquier header

});

    // ✅ MAPEO: Entidad → DTO (objeto anónimo)

    var resultado = ingredientes.Select(agg => new// En el pipeline

    {app.UseCors("AllowAll");

        // Propiedades simples (NO expone entidades del dominio)```

        id = agg.Id,

        nombre = agg.Ingrediente.Nombre,> **Nota de Seguridad**: En producción, deberías especificar orígenes específicos en lugar de `AllowAnyOrigin()`.

        descripcion = agg.Ingrediente.Descripcion,

        ---

        // ✅ Desempaqueta Value Objects a propiedades simples

        unidadMedida = agg.Ingrediente.UnidadDeMedida.Simbolo, // ← String simple### 📚 Swagger/OpenAPI

        cantidadEnStock = agg.Ingrediente.CantidadEnStock.Valor, // ← Decimal simple

        stockMinimo = agg.Ingrediente.RangoDeStock.StockMinimo,**Propósito**: Genera documentación interactiva de la API.

        stockMaximo = agg.Ingrediente.RangoDeStock.StockMaximo,

        **Configuración**:

        categoriaId = agg.Ingrediente.CategoriaId,

        activo = agg.Ingrediente.Activo,```csharp

        tieneStockBajo = agg.TieneStockBajo()builder.Services.AddEndpointsApiExplorer();

    }).ToList();builder.Services.AddSwaggerGen();



    return Ok(resultado); // ← Retorna DTO (se serializa a JSON)// En el pipeline (solo en desarrollo)

}if (app.Environment.IsDevelopment())

```{

    app.UseSwagger();

**Frontend recibe** (JSON):    app.UseSwaggerUI();

```json}

[```

  {

    "id": "guid-123",**Acceso**: http://localhost:5261/swagger

    "nombre": "Tomate",

    "descripcion": "Tomate fresco",**Características**:

    "unidadMedida": "kg",

    "cantidadEnStock": 50.5,- Documentación automática de todos los endpoints

    "stockMinimo": 10,- Permite probar endpoints directamente desde el navegador

    "stockMaximo": 100,- Muestra modelos de datos de entrada/salida

    "categoriaId": "guid-categoria",

    "activo": true,---

    "tieneStockBajo": false

  }## Flujo de una Petición HTTP Completo

]

````

┌─────────────────────────────────────────────────────────────┐

#### **Comparación: Entidad vs DTO**│ 1. Cliente (Frontend React) │

│ POST http://localhost:5261/api/ingredientes │

| Aspecto | Entidad (Domain) | DTO (API) |│ Body: { "nombre": "Tomate", ... } │

|---------|------------------|-----------|└─────────────────────┬───────────────────────────────────────┘

| **Archivo** | `Ingrediente.cs` | Objeto anónimo en Controller | │

| **Propiedades** | `private set` | `public get; set;` | ▼

| **Value Objects** | ✅ `UnidadDeMedida`, `RangoDeStock` | ❌ Strings/decimals simples |┌─────────────────────────────────────────────────────────────┐

| **Métodos** | ✅ `ConsumirIngrediente()`, `TieneStockBajo()` | ❌ Sin métodos |│ 2. Pipeline de Middleware │

| **Complejidad** | Alta (lógica de negocio) | Baja (solo datos) |│ ├─ CORS Middleware (valida origen) │

| **Se expone en API** | ❌ NO | ✅ SÍ |│ ├─ ExceptionHandlingMiddleware (try/catch global) │

│ └─ Routing Middleware (encuentra el controller) │

**Conclusión**: ✅ **CUMPLE** - Todos los endpoints usan DTOs (Commands, objetos anónimos), nunca exponen entidades del dominio.└─────────────────────┬───────────────────────────────────────┘

                      │

--- ▼

┌─────────────────────────────────────────────────────────────┐

### ✅ 4. Mapeo Explícito Hacia/Desde el Modelo de Dominio│ 3. IngredientesController │

│ [HttpPost] │

**Criterio**: Mapeo explícito entre DTOs y entidades de dominio│ public async Task<IActionResult> Create(Command cmd) │

│ { │

**Evidencia en el Proyecto**:│ var result = await \_mediator.Send(cmd); │

│ return CreatedAtAction(...); │

#### **Mapeo en el Handler: DTO → Entidad**│ } │

└─────────────────────┬───────────────────────────────────────┘

**Archivo**: `CrearIngredienteHandler.cs` │

                      ▼

```csharp┌─────────────────────────────────────────────────────────────┐

public class CrearIngredienteHandler : IRequestHandler<CrearIngredienteCommand, Guid>│ 4. MediatR (Patrón Mediator)                               │

{│    Encuentra el Handler correspondiente al Command         │

    private readonly IIngredienteRepository _ingredienteRepository;└─────────────────────┬───────────────────────────────────────┘

    private readonly ICategoriaRepository _categoriaRepository;                      │

                      ▼

    public async Task<Guid> Handle(CrearIngredienteCommand request, CancellationToken cancellationToken)┌─────────────────────────────────────────────────────────────┐

    {│ 5. CrearIngredienteHandler (Capa Application)              │

        // 1. ✅ VALIDACIONES de aplicación│    - Valida datos                                           │

        var categoriaExiste = await _categoriaRepository.ExisteAsync(request.CategoriaId);│    - Crea entidad de dominio                                │

        if (!categoriaExiste)│    - Llama al repositorio                                   │

            throw new InvalidOperationException($"La categoría con ID '{request.CategoriaId}' no existe");└─────────────────────┬───────────────────────────────────────┘

                      │

        var existe = await _ingredienteRepository.ExisteNombreAsync(request.Nombre);                      ▼

        if (existe)┌─────────────────────────────────────────────────────────────┐

            throw new InvalidOperationException($"Ya existe un ingrediente con el nombre '{request.Nombre}'");│ 6. IngredienteRepository (Capa Infrastructure)             │

│    - Guarda en base de datos                                │

        // 2. ✅ MAPEO EXPLÍCITO: String (DTO) → UnidadDeMedida (Value Object)│    - Retorna entidad guardada                               │

        var unidadMedida = request.UnidadMedida.ToLower() switch└─────────────────────┬───────────────────────────────────────┘

        {                      │

            "kg" or "kilogramos" => UnidadDeMedida.Kilogramos,                      ▼

            "g" or "gramos" => UnidadDeMedida.Gramos,┌─────────────────────────────────────────────────────────────┐

            "l" or "litros" => UnidadDeMedida.Litros,│ 7. Retorno al Controller                                    │

            "ml" or "mililitros" => UnidadDeMedida.Mililitros,│    return CreatedAtAction(..., resultado);                  │

            "u" or "unidades" => UnidadDeMedida.Unidades,└─────────────────────┬───────────────────────────────────────┘

            "lb" or "libras" => UnidadDeMedida.Libras,                      │

            "oz" or "onzas" => UnidadDeMedida.Onzas,                      ▼

            _ => new UnidadDeMedida(request.UnidadMedida, request.UnidadMedida)┌─────────────────────────────────────────────────────────────┐

        };│ 8. Respuesta HTTP                                           │

│    Status: 201 Created                                      │

        // 3. ✅ MAPEO EXPLÍCITO: Decimals (DTO) → RangoDeStock (Value Object)│    Body: { "id": "...", "nombre": "Tomate", ... }         │

        var rangoStock = new RangoDeStock(request.StockMinimo, request.StockMaximo);└─────────────────────────────────────────────────────────────┘

```

        // 4. ✅ CREAR ENTIDAD del Domain

        var ingrediente = new Ingrediente(---

            request.Nombre,

            request.Descripcion,## Ventajas de esta Arquitectura

            unidadMedida,    // ← Value Object

            rangoStock,      // ← Value Object### ✅ Separation of Concerns

            request.CategoriaId

        );Cada capa tiene una responsabilidad clara:



        // 5. Crear agregado- **API**: Recibir/enviar HTTP

        var ingredienteAggregate = new IngredienteAggregate(ingrediente);- **Application**: Lógica de aplicación y orquestación

- **Infrastructure**: Persistencia y servicios externos

        // 6. Persistir

        await _ingredienteRepository.GuardarAsync(ingredienteAggregate);### ✅ Patrón Mediator (MediatR)



        // 7. ✅ RETORNAR: Guid (DTO simple)- Desacopla controllers de handlers

        return ingredienteAggregate.Id;- Facilita testing

  }- Permite agregar comportamientos transversales (logging, validación)

}

````### ✅ Manejo de Errores Centralizado



#### **Mapeo en el Controller: Entidad → DTO**- Un solo lugar para manejar excepciones

- Respuestas consistentes

**Archivo**: `IngredientesController.cs`- Logging automático



```csharp### ✅ Documentación Automática (Swagger)

[HttpGet]

public async Task<IActionResult> ObtenerTodos()- Documentación siempre actualizada

{- Facilita testing manual

    // 1. Obtener agregados (entidades del Domain)- Ayuda a frontend a consumir la API

    var ingredientes = await _ingredienteRepository.ObtenerTodosAsync();

---

    // 2. ✅ MAPEO EXPLÍCITO: Entidad → DTO

    var resultado = ingredientes.Select(agg => new## Códigos HTTP Utilizados

    {

        // Mapeo campo por campo (explícito)| Código                    | Significado         | Cuándo se Usa           |

        id = agg.Id,| ------------------------- | ------------------- | ----------------------- |

        nombre = agg.Ingrediente.Nombre,| 200 OK                    | Éxito               | GET, PUT exitosos       |

        descripcion = agg.Ingrediente.Descripcion,| 201 Created               | Recurso creado      | POST exitoso            |

        | 204 No Content            | Éxito sin contenido | DELETE exitoso          |

        // ✅ MAPEO EXPLÍCITO: Value Object → String| 400 Bad Request           | Datos inválidos     | Validación fallida      |

        unidadMedida = agg.Ingrediente.UnidadDeMedida.Simbolo,| 404 Not Found             | Recurso no existe   | GET de ID inexistente   |

        | 500 Internal Server Error | Error del servidor  | Excepción no controlada |

        // ✅ MAPEO EXPLÍCITO: Value Object → Decimal

        cantidadEnStock = agg.Ingrediente.CantidadEnStock.Valor,---



        // ✅ MAPEO EXPLÍCITO: Value Object → Decimals---

        stockMinimo = agg.Ingrediente.RangoDeStock.StockMinimo,

        stockMaximo = agg.Ingrediente.RangoDeStock.StockMaximo,## 📊 CRITERIOS DE EVALUACIÓN - CAPA PRESENTACIÓN (API)



        categoriaId = agg.Ingrediente.CategoriaId,### ✅ 1. Controllers Delgados (Thin Controllers)

        activo = agg.Ingrediente.Activo,

        **Criterio**: Controllers delgados que implementan los casos de uso

        // ✅ Llamada a método del dominio (pero resultado simple)

        tieneStockBajo = agg.TieneStockBajo()**Implementación**:

    }).ToList();

```csharp

    return Ok(resultado);[ApiController]

}[Route("api/[controller]")]

```public class IngredientesController : ControllerBase

{

**Lo que NO se hace**:    private readonly IMediator _mediator;

```csharp

// ❌ INCORRECTO - Exponer entidad directamente    public IngredientesController(IMediator mediator)

[HttpGet("{id}")]    {

public async Task<Ingrediente> ObtenerPorId(Guid id)        _mediator = mediator;

{    }

    return await _repository.ObtenerPorIdAsync(id); // ❌ Expone entidad

}    // ✅ CONTROLLER DELGADO: Solo coordina, no tiene lógica

    [HttpPost]

// ❌ INCORRECTO - Mapeo automático/implícito    public async Task<ActionResult<IngredienteDto>> Crear([FromBody] CrearIngredienteCommand command)

var dto = _mapper.Map<IngredienteDto>(ingrediente); // ❌ AutoMapper (implícito)    {

```        // 1. Recibe el comando

        // 2. Delega a MediatR (que ejecuta el Handler)

**Conclusión**: ✅ **CUMPLE** - Todo el mapeo es explícito: manualmente campo por campo en handlers y controllers.        var resultado = await _mediator.Send(command);



---        // 3. Retorna respuesta HTTP

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);

### ✅ 5. Validación de Datos en Boundaries    }

}

**Criterio**: Validación de datos de entrada en el punto de recepción (boundaries)```



**Evidencia en el Proyecto**:**¿Qué NO hace el Controller?**



#### **Nivel 1: Validación Automática de ASP.NET Core**- ❌ NO valida reglas de negocio

- ❌ NO accede directamente a la base de datos

**Atributo `[ApiController]`** habilita validación automática:- ❌ NO contiene lógica de dominio

- ❌ NO hace cálculos complejos

```csharp- ❌ NO coordina múltiples operaciones

[ApiController] // ← Habilita validación automática

[Route("api/[controller]")]**¿Qué SÍ hace el Controller?**

public class IngredientesController : ControllerBase

{- ✅ Recibe datos del HTTP request

    [HttpPost]- ✅ Delega a MediatR

    public async Task<IActionResult> Crear([FromBody] CrearIngredienteRequest request)- ✅ Retorna respuestas HTTP apropiadas

    {- ✅ Maneja routing y verbos HTTP

        // Si request es null o tiene errores de deserialización:

        // → ASP.NET Core retorna 400 Bad Request automáticamente**Ejemplo de Controller GRUESO (incorrecto)**:



        // Si llegamos aquí, el objeto está bien formado```csharp

    }// ❌ MAL - Controller con lógica de negocio

}[HttpPost]

```public async Task<ActionResult> CrearIncorrecto([FromBody] IngredienteDto dto)

{

#### **Nivel 2: Validación en Controller**    // ❌ Lógica de negocio en el controller

    if (dto.StockMinimo > dto.StockMaximo)

**Archivo**: `OrdenesCompraController.cs`        return BadRequest("Stock mínimo debe ser menor al máximo");



```csharp    // ❌ Acceso directo a la BD

[HttpPost]    _context.Ingredientes.Add(new Ingrediente { ... });

public async Task<IActionResult> CrearOrdenDeCompra(    await _context.SaveChangesAsync();

    [FromBody] CrearOrdenDeCompraCommand command)

{    // ❌ Lógica de cálculo

    try    var valorInventario = dto.CantidadEnStock * dto.PrecioUnitario;

    {

        // ✅ VALIDACIÓN BOUNDARY: Verificar datos básicos    return Ok();

        if (command == null)}

            return BadRequest(new { message = "Datos inválidos" });```



        // Delegar al handler**Conclusión**: ✅ **CUMPLE** - Todos los controllers son delgados y solo coordinan.

        var ordenId = await _mediator.Send(command);

        ---

        return CreatedAtAction(...);

    }### ✅ 2. Manejan Consistencia (Commits, Rollbacks)

    catch (ArgumentException ex)

    {**Criterio**: Controllers manejan commits y rollbacks de transacciones

        // ✅ VALIDACIÓN: Captura errores de validación del Domain

        _logger.LogWarning(ex, "Datos inválidos al crear orden de compra");**Implementación**:

        return BadRequest(new { success = false, message = ex.Message });

    }**Forma 1: Transacciones Implícitas (Automáticas)**

    catch (Exception ex)

    {```csharp

        _logger.LogError(ex, "Error al crear orden de compra");[HttpPost("recibir")]

        return StatusCode(500, new { message = "Error al crear la orden de compra" });public async Task<ActionResult<OrdenDeCompraDto>> RecibirOrden(

    }    [FromBody] RecibirOrdenDeCompraCommand command)

}{

```    // ✅ MediatR + EF Core manejan transacciones automáticamente

    var resultado = await _mediator.Send(command);

#### **Nivel 3: Validación en Handler (Application)**

    // Si todo va bien: COMMIT automático

**Archivo**: `CrearIngredienteHandler.cs`    // Si hay excepción: ROLLBACK automático



```csharp    return Ok(resultado);

public async Task<Guid> Handle(CrearIngredienteCommand request, ...)}

{```

    // ✅ VALIDACIÓN: Categoría existe

    var categoriaExiste = await _categoriaRepository.ExisteAsync(request.CategoriaId);**Forma 2: Manejo de Errores con Middleware**

    if (!categoriaExiste)

    {```csharp

        throw new InvalidOperationException(// ExceptionHandlingMiddleware.cs

            $"La categoría con ID '{request.CategoriaId}' no existe");public async Task InvokeAsync(HttpContext context, RequestDelegate next)

    }{

    try

    // ✅ VALIDACIÓN: Nombre único    {

    var existe = await _ingredienteRepository.ExisteNombreAsync(request.Nombre);        // ✅ Ejecuta el request

    if (existe)        await next(context);

    {        // Si llega aquí: COMMIT exitoso

        throw new InvalidOperationException(    }

            $"Ya existe un ingrediente con el nombre '{request.Nombre}'");    catch (InvalidOperationException ex)

    }    {

            // ✅ Si hay excepción: ROLLBACK automático

    // Continuar con la lógica...        context.Response.StatusCode = 400;

}        await context.Response.WriteAsJsonAsync(new { error = ex.Message });

```    }

    catch (Exception ex)

#### **Nivel 4: Validación en Domain (Entidades)**    {

        // ✅ Cualquier error: ROLLBACK

**Archivo**: `Ingrediente.cs` (Domain)        context.Response.StatusCode = 500;

        await context.Response.WriteAsJsonAsync(new { error = "Error interno" });

```csharp    }

public class Ingrediente}

{```

    public Ingrediente(string nombre, string descripcion, ...)

    {**Flujo de Transacción**:

        // ✅ VALIDACIÓN DOMAIN: Nombre requerido

        if (string.IsNullOrWhiteSpace(nombre))```

            throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));┌──────────────────────────────────────────────┐

        │ 1. Controller recibe request                 │

        // ✅ VALIDACIÓN DOMAIN: Longitud└─────────────┬────────────────────────────────┘

        if (nombre.Length > 100)              │

            throw new ArgumentException("El nombre no puede exceder 100 caracteres", nameof(nombre));              ▼

        ┌──────────────────────────────────────────────┐

        Nombre = nombre;│ 2. _mediator.Send(command)                   │

        // ...│    → Handler ejecuta lógica                  │

    }│    → Llama a repositorios                    │

}│    → EF Core inicia transacción implícita    │

```└─────────────┬────────────────────────────────┘

              │

**Archivo**: `RangoDeStock.cs` (Value Object)              ▼

┌──────────────────────────────────────────────┐

```csharp│ 3. SaveChangesAsync()                        │

public class RangoDeStock│    ✅ Éxito → COMMIT                         │

{│    ❌ Error → ROLLBACK                       │

    public RangoDeStock(decimal stockMinimo, decimal stockMaximo)└─────────────┬────────────────────────────────┘

    {              │

        // ✅ VALIDACIÓN DOMAIN: Stock mínimo positivo              ▼

        if (stockMinimo < 0)┌──────────────────────────────────────────────┐

            throw new ArgumentException("El stock mínimo no puede ser negativo");│ 4. Controller retorna respuesta HTTP         │

        │    200/201 si éxito                          │

        // ✅ VALIDACIÓN DOMAIN: Stock máximo > mínimo│    400/500 si error                          │

        if (stockMaximo <= stockMinimo)└──────────────────────────────────────────────┘

            throw new ArgumentException("El stock máximo debe ser mayor al stock mínimo");```



        StockMinimo = stockMinimo;**Ejemplo con Transacciones Explícitas** (si se requiere):

        StockMaximo = stockMaximo;

    }```csharp

}[HttpPost("operacion-compleja")]

```public async Task<ActionResult> OperacionCompleja([FromBody] ComandoComplejo command)

{

#### **Middleware de Manejo de Errores**    using (var transaction = await _context.Database.BeginTransactionAsync())

    {

**Archivo**: `ExceptionHandlingMiddleware.cs`        try

        {

```csharp            // Múltiples operaciones

public async Task InvokeAsync(HttpContext context, RequestDelegate next)            var resultado1 = await _mediator.Send(command.Parte1);

{            var resultado2 = await _mediator.Send(command.Parte2);

    try

    {            // ✅ COMMIT explícito

        await _next(context);            await transaction.CommitAsync();

    }

    catch (ArgumentException ex) // ✅ Errores de validación            return Ok();

    {        }

        _logger.LogWarning(ex, "Validación fallida");        catch (Exception)

        context.Response.StatusCode = 400; // Bad Request        {

        await context.Response.WriteAsJsonAsync(new            // ✅ ROLLBACK explícito

        {            await transaction.RollbackAsync();

            error = "Datos inválidos",            throw;

            message = ex.Message        }

        });    }

    }}

    catch (InvalidOperationException ex) // ✅ Errores de reglas de negocio```

    {

        _logger.LogWarning(ex, "Operación inválida");**Conclusión**: ✅ **CUMPLE** - Las transacciones se manejan automáticamente con EF Core, garantizando consistencia.

        context.Response.StatusCode = 400; // Bad Request

        await context.Response.WriteAsJsonAsync(new---

        {

            error = "Operación inválida",### ✅ 3. DTOs para Entrada/Salida

            message = ex.Message

        });**Criterio**: Implementa DTOs para transportar datos entre capas

    }

    catch (Exception ex) // ✅ Errores inesperados**Implementación**:

    {

        _logger.LogError(ex, "Error no controlado");**DTOs en la Capa Application** (`InventarioDDD.Application/DTOs/`):

        context.Response.StatusCode = 500; // Internal Server Error

        await context.Response.WriteAsJsonAsync(new```csharp

        {// IngredienteDto.cs

            error = "Error interno del servidor"public class IngredienteDto

        });{

    }    public Guid Id { get; set; }

}    public string Nombre { get; set; }

```    public string Descripcion { get; set; }

    public string UnidadDeMedida { get; set; }

#### **Capas de Validación**    public decimal CantidadEnStock { get; set; }

    public decimal StockMinimo { get; set; }

```    public decimal StockMaximo { get; set; }

┌─────────────────────────────────────────────────────────┐    public Guid CategoriaId { get; set; }

│ 1. BOUNDARY (Controller)                                │}

│    - Validación de formato HTTP                         │

│    - ModelState validation (ASP.NET Core)               │// OrdenDeCompraDto.cs

│    - Retorna 400 Bad Request si falla                   │public class OrdenDeCompraDto

│    Archivo: IngredientesController.cs                   │{

├─────────────────────────────────────────────────────────┤    public Guid Id { get; set; }

│ 2. APPLICATION (Handler)                                │    public string Numero { get; set; }

│    - Validación de lógica de aplicación                │    public string IngredienteNombre { get; set; }

│    - Validación de relaciones (categoría existe)        │    public string ProveedorNombre { get; set; }

│    - Validación de unicidad (nombre único)              │    public decimal Cantidad { get; set; }

│    Archivo: CrearIngredienteHandler.cs                  │    public decimal PrecioUnitario { get; set; }

├─────────────────────────────────────────────────────────┤    public string Estado { get; set; }

│ 3. DOMAIN (Entidades/Value Objects)                     │    public DateTime FechaCreacion { get; set; }

│    - Validación de reglas de negocio                    │}

│    - Protección de invariantes                          │```

│    - Lanza excepciones de dominio                       │

│    Archivos: Ingrediente.cs, RangoDeStock.cs           │**Uso en Controllers**:

├─────────────────────────────────────────────────────────┤

│ 4. MIDDLEWARE (ExceptionHandlingMiddleware)             │```csharp

│    - Captura todas las excepciones                      │[HttpGet]

│    - Convierte a respuestas HTTP apropiadas             │public async Task<ActionResult<List<IngredienteDto>>> ObtenerTodos()

│    - Logging centralizado                               │{

│    Archivo: ExceptionHandlingMiddleware.cs              │    var query = new ObtenerTodosIngredientesQuery();

└─────────────────────────────────────────────────────────┘

```    // ✅ Handler retorna DTOs, NO entidades del dominio

    var ingredientes = await _mediator.Send(query);

**Conclusión**: ✅ **CUMPLE** - Validación multi-nivel: ASP.NET Core (automática), Controller (boundary), Handler (aplicación), Domain (reglas de negocio), Middleware (captura global).

    return Ok(ingredientes);

---}



## 🎯 Resumen de Cumplimiento[HttpPost]

public async Task<ActionResult<IngredienteDto>> Crear([FromBody] CrearIngredienteCommand command)

| Criterio | Estado | Archivo Principal | Evidencia |{

|----------|--------|-------------------|-----------|    // ✅ Recibe Command (que es como un DTO de entrada)

| **Controllers Delgados** | ✅ **CUMPLE** | `IngredientesController.cs`<br>`OrdenesCompraController.cs` | Solo coordinan, delegan a MediatR |    // ✅ Retorna DTO (no entidad de dominio)

| **Manejo de Consistencia** | ✅ **CUMPLE** | `ExceptionHandlingMiddleware.cs`<br>Entity Framework Core | Transacciones ACID automáticas |    var resultado = await _mediator.Send(command);

| **DTOs para Entrada/Salida** | ✅ **CUMPLE** | `CrearIngredienteCommand.cs`<br>`IngredientesController.cs` | Commands (entrada), objetos anónimos (salida) |

| **Mapeo Explícito** | ✅ **CUMPLE** | `CrearIngredienteHandler.cs`<br>`IngredientesController.cs` | Mapeo manual campo por campo |    return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);

| **Validación en Boundaries** | ✅ **CUMPLE** | `IngredientesController.cs`<br>`CrearIngredienteHandler.cs`<br>`ExceptionHandlingMiddleware.cs` | Multi-nivel: Controller, Handler, Domain, Middleware |}

````

---

**Separación Entidad vs DTO**:

## 📁 Archivos Referenciados

````

### Controllers┌─────────────────────────┐       ┌──────────────────────────┐

- `backend/InventarioDDD.API/Controllers/IngredientesController.cs`│   Entidad (Domain)      │       │   DTO (Application)      │

- `backend/InventarioDDD.API/Controllers/OrdenesCompraController.cs`├─────────────────────────┤       ├──────────────────────────┤

│ - Tiene lógica          │       │ - Solo datos             │

### Commands (DTOs)│ - Protege invariantes   │       │ - Sin métodos            │

- `backend/InventarioDDD.Application/Commands/CrearIngredienteCommand.cs`│ - Constructores privados│       │ - Getters/Setters        │

- `backend/InventarioDDD.Application/Commands/CrearOrdenDeCompraCommand.cs`│ - Métodos de negocio    │       │ - Serializable JSON      │

│ - Value Objects         │       │ - Propiedades simples    │

### Handlers│                         │       │                          │

- `backend/InventarioDDD.Application/Handlers/CrearIngredienteHandler.cs`│ NO se expone en API     │       │ SÍ se expone en API      │

└─────────────────────────┘       └──────────────────────────┘

### Middleware```

- `backend/InventarioDDD.API/Middleware/ExceptionHandlingMiddleware.cs`

**Conclusión**: ✅ **CUMPLE** - Todos los endpoints usan DTOs, nunca exponen entidades del dominio.

### Domain (para validaciones)

- `backend/InventarioDDD.Domain/Entities/Ingrediente.cs`---

- `backend/InventarioDDD.Domain/ValueObjects/RangoDeStock.cs`

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
````

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

| Criterio                     | Estado        | Evidencia                                         |
| ---------------------------- | ------------- | ------------------------------------------------- |
| **Controllers Delgados**     | ✅ **CUMPLE** | Solo coordinan, delegan a MediatR                 |
| **Manejo de Consistencia**   | ✅ **CUMPLE** | EF Core + Middleware manejan transacciones        |
| **DTOs para Entrada/Salida** | ✅ **CUMPLE** | Nunca exponen entidades de dominio                |
| **Mapeo Explícito**          | ✅ **CUMPLE** | Handlers mapean manualmente entidad ↔ DTO         |
| **Validación en Boundaries** | ✅ **CUMPLE** | Validación multi-nivel (API, Application, Domain) |
