using InventarioDDD.Application.UseCases;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;
using InventarioDDD.Infrastructure.Repositories;
using InventarioDDD.Infrastructure.Services;
using InventarioDDD.API.Jobs;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Sistema de Inventario DDD - Restaurant Management", 
        Version = "v1.0.0",
        Description = "API REST para gestión de inventario con Domain-Driven Design (DDD) y Clean Architecture. " +
                     "Incluye gestión completa de ingredientes, proveedores, órdenes de compra, auditoría, " +
                     "rotación de inventario y alertas automáticas.",
        Contact = new() 
        { 
            Name = "Inventario DDD",
            Url = new Uri("https://github.com/daniellelooo/ddd-inventario")
        },
        License = new()
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Habilitar comentarios XML para documentación
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Agrupar endpoints por tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
    c.DocInclusionPredicate((name, api) => true);
});

// MediatR - Registrar handlers de Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RegistrarIngredienteHandler).Assembly));

// Repositories - Singleton para repositorios en memoria
builder.Services.AddSingleton<IIngredienteRepository, InMemoryIngredienteRepository>();
builder.Services.AddSingleton<IProveedorRepository, InMemoryProveedorRepository>();
builder.Services.AddSingleton<IOrdenDeCompraRepository, InMemoryOrdenDeCompraRepository>();

// Domain Services
builder.Services.AddScoped<IServicioDeInventario, ServicioDeInventario>();
builder.Services.AddScoped<IServicioDeConsumo, ServicioDeConsumo>();
builder.Services.AddScoped<IServicioDeReabastecimiento, ServicioDeReabastecimiento>();
builder.Services.AddScoped<IServicioDeRecepcion, ServicioDeRecepcion>();
builder.Services.AddScoped<IServicioDeAuditoria, ServicioDeAuditoria>();
builder.Services.AddScoped<IServicioDeRotacion, ServicioDeRotacion>();

// Quartz.NET - Scheduled Jobs
builder.Services.AddQuartz(q =>
{
    // Job 1: Alertas de stock bajo (cada 5 minutos)
    var stockBajoJobKey = new JobKey("AlertasStockBajoJob");
    q.AddJob<AlertasStockBajoJob>(opts => opts.WithIdentity(stockBajoJobKey));
    q.AddTrigger(opts => opts
        .ForJob(stockBajoJobKey)
        .WithIdentity("AlertasStockBajoJob-trigger")
        .WithCronSchedule("0 0/5 * * * ?")); // Cada 5 minutos

    // Job 2: Lotes vencidos (cada hora)
    var lotesVencidosJobKey = new JobKey("LotesVencidosJob");
    q.AddJob<LotesVencidosJob>(opts => opts.WithIdentity(lotesVencidosJobKey));
    q.AddTrigger(opts => opts
        .ForJob(lotesVencidosJobKey)
        .WithIdentity("LotesVencidosJob-trigger")
        .WithCronSchedule("0 0 * * * ?")); // Cada hora

    // Job 3: Alertas de vencimiento (diario a las 8 AM)
    var alertasVencimientoJobKey = new JobKey("AlertasVencimientoJob");
    q.AddJob<AlertasVencimientoJob>(opts => opts.WithIdentity(alertasVencimientoJobKey));
    q.AddTrigger(opts => opts
        .ForJob(alertasVencimientoJobKey)
        .WithIdentity("AlertasVencimientoJob-trigger")
        .WithCronSchedule("0 0 8 * * ?")); // Diario a las 8:00 AM

    // Job 4: Limpieza de caché (diario a medianoche)
    var limpiezaCacheJobKey = new JobKey("LimpiezaCacheJob");
    q.AddJob<LimpiezaCacheJob>(opts => opts.WithIdentity(limpiezaCacheJobKey));
    q.AddTrigger(opts => opts
        .ForJob(limpiezaCacheJobKey)
        .WithIdentity("LimpiezaCacheJob-trigger")
        .WithCronSchedule("0 0 0 * * ?")); // Diario a las 00:00
});

// Agregar hosting de Quartz
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventario DDD API v1.0.0");
    c.RoutePrefix = string.Empty; // Swagger UI en la raíz (http://localhost:5261)
    c.DocumentTitle = "Inventario DDD - API Documentation";
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoint de API info (raíz alternativa si Swagger no está en raíz)
app.MapGet("/api", () => Results.Ok(new
{
    Application = "Sistema de Inventario DDD",
    Version = "1.0.0",
    Architecture = "Clean Architecture + Domain-Driven Design",
    Status = "Running ✅",
    Timestamp = DateTime.Now,
    Documentation = "/swagger",
    Endpoints = new
    {
        Ingredientes = "/api/ingredientes",
        Proveedores = "/api/proveedores",
        OrdenesDeCompra = "/api/ordenesdecompra",
        Inventario = "/api/inventario",
        Auditoria = "/api/auditoria",
        Rotacion = "/api/rotacion"
    },
    ScheduledJobs = new[]
    {
        "AlertasStockBajoJob - Cada 5 minutos",
        "LotesVencidosJob - Cada hora",
        "AlertasVencimientoJob - Diario 8 AM",
        "LimpiezaCacheJob - Diario medianoche"
    }
}))
.WithName("APIInfo")
.WithTags("Sistema")
.WithOpenApi();

app.Run();
