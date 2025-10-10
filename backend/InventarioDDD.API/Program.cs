using InventarioDDD.API.Middleware;
using InventarioDDD.Domain.Interfaces;
using InventarioDDD.Domain.Services;
using InventarioDDD.Infrastructure.Persistence;
using InventarioDDD.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "InventarioDDD API",
        Version = "v1",
        Description = "API REST para gestión de inventario de restaurante usando DDD",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "contacto@inventario.com"
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Entity Framework y SQLite
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(InventarioDDD.Application.Commands.CrearOrdenDeCompraCommand).Assembly);
});

// Registrar Repositorios
builder.Services.AddScoped<IIngredienteRepository, IngredienteRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IOrdenDeCompraRepository, OrdenDeCompraRepository>();
builder.Services.AddScoped<ILoteRepository, LoteRepository>();
builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

// Registrar Servicios de Dominio
builder.Services.AddScoped<ServicioDeConsumo>();
builder.Services.AddScoped<ServicioDeRecepcion>();
builder.Services.AddScoped<ServicioDeReabastecimiento>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventarioDDD API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

// Middleware personalizado para manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Inicializar base de datos y poblar con datos de prueba
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
    try
    {
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Base de datos inicializada correctamente");

        // Poblar con datos de prueba
        // NOTA: Usar Swagger para crear datos manualmente
        app.Logger.LogInformation("✅ Base de datos inicializada - usar Swagger para crear datos");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

app.Logger.LogInformation("InventarioDDD API iniciada correctamente");

app.Run();
