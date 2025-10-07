namespace InventarioDDD.Domain.Events;

public record IngredienteRegistradoEvent(long IngredienteId, string Nombre, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record LoteRecibidoEvent(long LoteId, long IngredienteId, double Cantidad, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record StockActualizadoEvent(long IngredienteId, double StockAnterior, double StockNuevo, string Motivo, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record AlertaStockBajoEvent(long IngredienteId, string NombreIngrediente, double StockActual, double StockMinimo, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record LoteVencidoEvent(long LoteId, long IngredienteId, double CantidadAfectada, DateTime FechaVencimiento, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record IngredientesConsumidosEvent(long PedidoId, List<long> IngredientesIds, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record OrdenDeCompraGeneradaEvent(long OrdenId, long ProveedorId, List<long> IngredientesIds, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record OrdenDeCompraRecibidaEvent(long OrdenId, long ProveedorId, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record AlertaVencimientoEvent(long IngredienteId, string NombreIngrediente, long LoteId, int DiasHastaVencimiento, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}

public record DiscrepanciaRecepcionEvent(long OrdenId, long IngredienteId, string Descripcion, DateTime OcurridoEn = default) : IDomainEvent
{
    public DateTime OcurridoEn { get; init; } = OcurridoEn == default ? DateTime.Now : OcurridoEn;
}
