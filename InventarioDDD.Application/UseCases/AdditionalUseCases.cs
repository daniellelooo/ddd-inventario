using MediatR;
using InventarioDDD.Domain.Repositories;
using InventarioDDD.Domain.Services;

namespace InventarioDDD.Application.UseCases;

// Commands
public record ConsumirIngredientesCommand(long PedidoId, List<long> IngredientesIds) : IRequest<bool>;
public record GenerarOrdenAutomaticaCommand : IRequest<bool>;
public record RegistrarProveedorCommand(
    string Nombre,
    string Contacto,
    string Telefono,
    string Email,
    string Direccion) : IRequest<long>;
public record CrearOrdenDeCompraCommand(
    long ProveedorId,
    DateTime FechaEntregaEsperada,
    List<ItemOrdenDto> Items) : IRequest<long>;

public record ItemOrdenDto(long IngredienteId, double Cantidad, double PrecioUnitario);

// Queries
public record ObtenerReporteConsumoQuery(DateTime FechaInicio, DateTime FechaFin) : IRequest<List<ConsumoDto>>;
public record ObtenerReporteMermasQuery(DateTime FechaInicio, DateTime FechaFin) : IRequest<List<MermaDto>>;
public record ObtenerValoracionInventarioQuery : IRequest<decimal>;

public record ConsumoDto(long IngredienteId, string Nombre, double CantidadConsumida);
public record MermaDto(long IngredienteId, string Nombre, double CantidadPerdida, string Motivo);

// Handlers
public class ConsumirIngredientesHandler : IRequestHandler<ConsumirIngredientesCommand, bool>
{
    private readonly IServicioDeConsumo _servicio;

    public ConsumirIngredientesHandler(IServicioDeConsumo servicio)
    {
        _servicio = servicio;
    }

    public async Task<bool> Handle(ConsumirIngredientesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _servicio.DescontarIngredientesAsync(request.PedidoId, request.IngredientesIds);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class GenerarOrdenAutomaticaHandler : IRequestHandler<GenerarOrdenAutomaticaCommand, bool>
{
    private readonly IServicioDeReabastecimiento _servicio;

    public GenerarOrdenAutomaticaHandler(IServicioDeReabastecimiento servicio)
    {
        _servicio = servicio;
    }

    public async Task<bool> Handle(GenerarOrdenAutomaticaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _servicio.GenerarOrdenDeCompraAutomaticaAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class RegistrarProveedorHandler : IRequestHandler<RegistrarProveedorCommand, long>
{
    private readonly IProveedorRepository _repository;

    public RegistrarProveedorHandler(IProveedorRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> Handle(RegistrarProveedorCommand request, CancellationToken cancellationToken)
    {
        // Generar un ID temporal (en una implementación real sería auto-generado por la BD)
        var newId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var proveedor = new Domain.Entities.Proveedor(
            newId,
            request.Nombre,
            request.Contacto,
            request.Telefono,
            request.Direccion,
            request.Email);

        await _repository.GuardarAsync(proveedor);
        return proveedor.Id;
    }
}

public class CrearOrdenDeCompraHandler : IRequestHandler<CrearOrdenDeCompraCommand, long>
{
    private readonly IOrdenDeCompraRepository _repository;
    private readonly IIngredienteRepository _ingredienteRepository;

    public CrearOrdenDeCompraHandler(
        IOrdenDeCompraRepository repository,
        IIngredienteRepository ingredienteRepository)
    {
        _repository = repository;
        _ingredienteRepository = ingredienteRepository;
    }

    public async Task<long> Handle(CrearOrdenDeCompraCommand request, CancellationToken cancellationToken)
    {
        // Generar un ID temporal
        var newId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var orden = new Domain.Entities.OrdenDeCompra(
            newId,
            request.ProveedorId,
            request.FechaEntregaEsperada);

        foreach (var item in request.Items)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId);
            if (ingrediente != null)
            {
                var cantidad = new Domain.ValueObjects.CantidadDisponible(item.Cantidad, ingrediente.UnidadDeMedida);
                var precio = new Domain.ValueObjects.PrecioConMoneda((decimal)item.PrecioUnitario, "USD");
                orden.AgregarItem(item.IngredienteId, cantidad, precio);
            }
        }

        await _repository.GuardarAsync(orden);
        return orden.Id;
    }
}

public class ObtenerReporteConsumoHandler : IRequestHandler<ObtenerReporteConsumoQuery, List<ConsumoDto>>
{
    private readonly IServicioDeAuditoria _servicio;

    public ObtenerReporteConsumoHandler(IServicioDeAuditoria servicio)
    {
        _servicio = servicio;
    }

    public async Task<List<ConsumoDto>> Handle(ObtenerReporteConsumoQuery request, CancellationToken cancellationToken)
    {
        var reporte = await _servicio.GenerarReporteConsumoAsync(request.FechaInicio, request.FechaFin);
        return reporte.ConsumosPorIngrediente
            .Select(kvp => new ConsumoDto(kvp.Key, "Ingrediente", kvp.Value))
            .ToList();
    }
}

public class ObtenerReporteMermasHandler : IRequestHandler<ObtenerReporteMermasQuery, List<MermaDto>>
{
    private readonly IServicioDeAuditoria _servicio;

    public ObtenerReporteMermasHandler(IServicioDeAuditoria servicio)
    {
        _servicio = servicio;
    }

    public async Task<List<MermaDto>> Handle(ObtenerReporteMermasQuery request, CancellationToken cancellationToken)
    {
        var reporte = await _servicio.GenerarReporteMermasAsync(request.FechaInicio, request.FechaFin);
        return reporte.MermasPorIngrediente
            .Select(kvp => new MermaDto(kvp.Key, "Ingrediente", kvp.Value, "Vencimiento/Agotado"))
            .ToList();
    }
}

public class ObtenerValoracionInventarioHandler : IRequestHandler<ObtenerValoracionInventarioQuery, decimal>
{
    private readonly IServicioDeInventario _servicio;

    public ObtenerValoracionInventarioHandler(IServicioDeInventario servicio)
    {
        _servicio = servicio;
    }

    public async Task<decimal> Handle(ObtenerValoracionInventarioQuery request, CancellationToken cancellationToken)
    {
        return await _servicio.CalcularValoracionInventarioAsync();
    }
}
