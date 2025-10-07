namespace InventarioDDD.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una fecha de vencimiento
/// </summary>
public record FechaVencimiento
{
    public DateTime Fecha { get; init; }

    public FechaVencimiento(DateTime fecha)
    {
        if (fecha == default)
            throw new ArgumentException("La fecha de vencimiento no puede estar vacía", nameof(fecha));

        Fecha = fecha.Date; // Solo fecha, sin hora
    }

    public bool EstaVencido() => Fecha < DateTime.Today;

    public bool EstaVencido(DateTime fechaReferencia) => Fecha < fechaReferencia.Date;

    public int DiasHastaVencimiento() => (Fecha - DateTime.Today).Days;

    public bool ProximoAVencer(int dias) => DiasHastaVencimiento() <= dias && !EstaVencido();
}
