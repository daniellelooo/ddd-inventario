namespace InventarioDDD.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una cantidad disponible con su unidad de medida
/// </summary>
public record CantidadDisponible
{
    public double Valor { get; init; }
    public UnidadDeMedida UnidadDeMedida { get; init; }

    public CantidadDisponible(double valor, UnidadDeMedida unidadDeMedida)
    {
        if (valor < 0)
            throw new ArgumentException("La cantidad no puede ser negativa", nameof(valor));

        if (unidadDeMedida == null)
            throw new ArgumentNullException(nameof(unidadDeMedida));

        Valor = valor;
        UnidadDeMedida = unidadDeMedida;
    }

    public bool EsPositivo() => Valor > 0;

    public bool EsMayorQue(CantidadDisponible otra) => Valor > otra.Valor;

    public bool EsMenorQue(CantidadDisponible otra) => Valor < otra.Valor;

    public CantidadDisponible Sumar(CantidadDisponible otra)
    {
        if (!UnidadDeMedida.Equals(otra.UnidadDeMedida))
            throw new InvalidOperationException("No se pueden sumar cantidades con diferentes unidades de medida");

        return new CantidadDisponible(Valor + otra.Valor, UnidadDeMedida);
    }

    public CantidadDisponible Restar(CantidadDisponible otra)
    {
        if (!UnidadDeMedida.Equals(otra.UnidadDeMedida))
            throw new InvalidOperationException("No se pueden restar cantidades con diferentes unidades de medida");

        return new CantidadDisponible(Math.Max(0, Valor - otra.Valor), UnidadDeMedida);
    }
}
