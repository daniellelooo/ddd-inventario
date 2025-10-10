namespace InventarioDDD.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa un rango de stock (mínimo y máximo)
    /// </summary>
    public class RangoDeStock
    {
        public decimal StockMinimo { get; private set; }
        public decimal StockMaximo { get; private set; }

        // Constructor privado para Entity Framework
        private RangoDeStock()
        {
        }

        public RangoDeStock(decimal stockMinimo, decimal stockMaximo)
        {
            if (stockMinimo < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo", nameof(stockMinimo));

            if (stockMaximo < 0)
                throw new ArgumentException("El stock máximo no puede ser negativo", nameof(stockMaximo));

            if (stockMinimo > stockMaximo)
                throw new ArgumentException("El stock mínimo no puede ser mayor al stock máximo");

            StockMinimo = stockMinimo;
            StockMaximo = stockMaximo;
        }

        public bool EstaEnRango(decimal cantidad)
        {
            return cantidad >= StockMinimo && cantidad <= StockMaximo;
        }

        public bool EstaPorDebajoDelMinimo(decimal cantidad)
        {
            return cantidad < StockMinimo;
        }

        public bool EstaPorEncimaDelMaximo(decimal cantidad)
        {
            return cantidad > StockMaximo;
        }

        public bool EsStockBajo(decimal cantidad)
        {
            return EstaPorDebajoDelMinimo(cantidad);
        }

        public bool EsStockExcesivo(decimal cantidad)
        {
            return EstaPorEncimaDelMaximo(cantidad);
        }

        public decimal CalcularPorcentajeUso(decimal cantidadActual)
        {
            if (StockMaximo == 0) return 0;
            return Math.Min(100, (cantidadActual / StockMaximo) * 100);
        }

        public decimal CalcularCantidadParaReabastecimiento(decimal cantidadActual)
        {
            if (cantidadActual >= StockMaximo) return 0;
            return StockMaximo - cantidadActual;
        }

        public decimal CalcularPuntoDeReorden()
        {
            // Punto de reorden típicamente entre el mínimo y un 25% adicional
            return StockMinimo + (StockMaximo - StockMinimo) * 0.25m;
        }

        public override bool Equals(object? obj)
        {
            return obj is RangoDeStock other &&
                   StockMinimo == other.StockMinimo &&
                   StockMaximo == other.StockMaximo;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StockMinimo, StockMaximo);
        }

        public override string ToString()
        {
            return $"Stock: {StockMinimo:F2} - {StockMaximo:F2}";
        }

        public static RangoDeStock Crear(decimal minimo, decimal maximo)
        {
            return new RangoDeStock(minimo, maximo);
        }

        public static RangoDeStock CrearConCapacidad(decimal capacidadMaxima, decimal porcentajeMinimo = 10)
        {
            if (porcentajeMinimo < 0 || porcentajeMinimo > 100)
                throw new ArgumentException("El porcentaje mínimo debe estar entre 0 y 100");

            var minimo = capacidadMaxima * (porcentajeMinimo / 100);
            return new RangoDeStock(minimo, capacidadMaxima);
        }
    }
}