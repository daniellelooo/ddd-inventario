namespace InventarioDDD.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa un precio con su moneda
    /// </summary>
    public class PrecioConMoneda
    {
        public decimal Valor { get; private set; }
        public string Moneda { get; private set; }

        // Constructor privado para Entity Framework
        private PrecioConMoneda()
        {
            Moneda = string.Empty;
        }

        public PrecioConMoneda(decimal valor, string moneda = "COP")
        {
            if (valor < 0)
                throw new ArgumentException("El precio no puede ser negativo", nameof(valor));

            if (string.IsNullOrWhiteSpace(moneda))
                throw new ArgumentException("La moneda no puede estar vacía", nameof(moneda));

            Valor = valor;
            Moneda = moneda.ToUpperInvariant().Trim();
        }

        public bool EsCero => Valor == 0;
        public bool EsMayorQue(decimal precio) => Valor > precio;
        public bool EsMenorQue(decimal precio) => Valor < precio;

        public PrecioConMoneda Multiplicar(decimal factor)
        {
            if (factor < 0)
                throw new ArgumentException("El factor de multiplicación no puede ser negativo", nameof(factor));

            return new PrecioConMoneda(Valor * factor, Moneda);
        }

        public PrecioConMoneda Sumar(PrecioConMoneda otro)
        {
            if (Moneda != otro.Moneda)
                throw new InvalidOperationException($"No se pueden sumar precios de diferentes monedas: {Moneda} y {otro.Moneda}");

            return new PrecioConMoneda(Valor + otro.Valor, Moneda);
        }

        public PrecioConMoneda Restar(PrecioConMoneda otro)
        {
            if (Moneda != otro.Moneda)
                throw new InvalidOperationException($"No se pueden restar precios de diferentes monedas: {Moneda} y {otro.Moneda}");

            var resultado = Valor - otro.Valor;
            if (resultado < 0)
                throw new InvalidOperationException("La operación resultaría en un precio negativo");

            return new PrecioConMoneda(resultado, Moneda);
        }

        public decimal CalcularTotal(decimal cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentException("La cantidad no puede ser negativa", nameof(cantidad));

            return Valor * cantidad;
        }

        public PrecioConMoneda AplicarDescuento(decimal porcentajeDescuento)
        {
            if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
                throw new ArgumentException("El porcentaje de descuento debe estar entre 0 y 100", nameof(porcentajeDescuento));

            var factor = 1 - (porcentajeDescuento / 100);
            return new PrecioConMoneda(Valor * factor, Moneda);
        }

        public PrecioConMoneda AplicarImpuesto(decimal porcentajeImpuesto)
        {
            if (porcentajeImpuesto < 0)
                throw new ArgumentException("El porcentaje de impuesto no puede ser negativo", nameof(porcentajeImpuesto));

            var factor = 1 + (porcentajeImpuesto / 100);
            return new PrecioConMoneda(Valor * factor, Moneda);
        }

        public override bool Equals(object? obj)
        {
            return obj is PrecioConMoneda other &&
                   Valor == other.Valor &&
                   Moneda == other.Moneda;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Valor, Moneda);
        }

        public override string ToString()
        {
            return $"{Valor:C} {Moneda}";
        }

        public string ToStringConFormato(string formato = "F2")
        {
            return $"{Valor.ToString(formato)} {Moneda}";
        }

        // Precios predefinidos
        public static PrecioConMoneda Zero(string moneda = "COP") => new(0, moneda);
        public static PrecioConMoneda COP(decimal valor) => new(valor, "COP");
        public static PrecioConMoneda USD(decimal valor) => new(valor, "USD");
        public static PrecioConMoneda EUR(decimal valor) => new(valor, "EUR");

        // Operadores
        public static implicit operator decimal(PrecioConMoneda precio)
        {
            return precio.Valor;
        }

        public static bool operator >(PrecioConMoneda left, PrecioConMoneda right)
        {
            if (left.Moneda != right.Moneda)
                throw new InvalidOperationException("No se pueden comparar precios de diferentes monedas");
            return left.Valor > right.Valor;
        }

        public static bool operator <(PrecioConMoneda left, PrecioConMoneda right)
        {
            if (left.Moneda != right.Moneda)
                throw new InvalidOperationException("No se pueden comparar precios de diferentes monedas");
            return left.Valor < right.Valor;
        }

        public static PrecioConMoneda operator +(PrecioConMoneda left, PrecioConMoneda right)
        {
            return left.Sumar(right);
        }

        public static PrecioConMoneda operator -(PrecioConMoneda left, PrecioConMoneda right)
        {
            return left.Restar(right);
        }

        public static PrecioConMoneda operator *(PrecioConMoneda precio, decimal factor)
        {
            return precio.Multiplicar(factor);
        }
    }
}