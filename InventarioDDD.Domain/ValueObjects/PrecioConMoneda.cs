namespace InventarioDDD.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un precio con su moneda
/// </summary>
public record PrecioConMoneda
{
    public decimal Monto { get; init; }
    public string Moneda { get; init; }

    public PrecioConMoneda(decimal monto, string moneda = "BOB")
    {
        if (monto < 0)
            throw new ArgumentException("El monto no puede ser negativo", nameof(monto));

        if (string.IsNullOrWhiteSpace(moneda))
            throw new ArgumentException("La moneda es requerida", nameof(moneda));

        Monto = monto;
        Moneda = moneda.Trim().ToUpperInvariant();
    }

    public PrecioConMoneda MultiplicarPor(double cantidad)
    {
        return new PrecioConMoneda(Monto * (decimal)cantidad, Moneda);
    }

    public static readonly string MonedaBoliviano = "BOB";
    public static readonly string MonedaDolar = "USD";
}
