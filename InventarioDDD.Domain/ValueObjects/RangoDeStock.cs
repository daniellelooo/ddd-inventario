namespace InventarioDDD.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el rango de stock (mínimo, óptimo, máximo)
/// </summary>
public record RangoDeStock
{
    public double Minimo { get; init; }
    public double Optimo { get; init; }
    public double Maximo { get; init; }

    public RangoDeStock(double minimo, double optimo, double maximo)
    {
        if (minimo < 0)
            throw new ArgumentException("El stock mínimo no puede ser negativo", nameof(minimo));

        if (optimo <= minimo)
            throw new ArgumentException($"El stock óptimo ({optimo}) debe ser mayor que el mínimo ({minimo})", nameof(optimo));

        if (maximo <= optimo)
            throw new ArgumentException($"El stock máximo ({maximo}) debe ser mayor que el óptimo ({optimo})", nameof(maximo));

        Minimo = minimo;
        Optimo = optimo;
        Maximo = maximo;
    }

    public bool EsStockBajo(double stockActual) => stockActual <= Minimo;

    public bool NecesitaReabastecimiento(double stockActual) => stockActual < Optimo;

    public double CantidadParaReabastecimiento(double stockActual)
    {
        return Math.Max(0, Optimo - stockActual);
    }

    public bool ExcedeMaximo(double stockActual) => stockActual > Maximo;
}
